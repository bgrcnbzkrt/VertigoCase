using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Core;
using Vertigo.Data;
using Vertigo.Wheel;

namespace Vertigo.UI
{
    public class GamePanel : PanelBase
    {
        [Header("Controls")]
        [SerializeField] private Button buttonSpin;
        [SerializeField] private Button buttonLeave;

        [Header("Zone")]
        [SerializeField] private TMP_Text zoneTitleText;
        [SerializeField] private TMP_Text[] zoneLabels;
        [SerializeField] private Image zonePanel;

        [Header("Zone Sprites")]
        [SerializeField] private Sprite spriteZoneNormal;
        [SerializeField] private Sprite spriteZoneSafe;
        [SerializeField] private Sprite spriteZoneSuper;

        [Header("Zone Text Colors")]
        [SerializeField] private Color zoneNormalColor = Color.white;
        [SerializeField] private Color zoneSafeColor = Color.green;
        [SerializeField] private Color zoneSuperColor = Color.yellow;
        [Range(0f, 1f)] [SerializeField] private float passedDim = 0.5f;

        [Header("Wheel")]
        [SerializeField] private WheelController wheelController;

        [Header("Currency")]
        [SerializeField] private TMP_Text goldText;

        [Header("Reward Bar")]
        [SerializeField] private ScrollRect rewardScrollRect;
        [SerializeField] private Transform rewardBarContainer;
        [SerializeField] private RewardSlotUI rewardSlotPrefab;
        [SerializeField] private RectTransform flyingRewardIcon;
        [SerializeField] private GameObject flyingRewardGlow;

        private void Start()
        {
            buttonSpin.onClick.AddListener(() => GameManager.Instance.RequestSpin());
            buttonLeave.onClick.AddListener(() => GameManager.Instance.CollectAndLeave());
        }

        private void OnEnable()
        {
            GameManager.OnZoneChanged += RefreshZone;
            GameManager.OnStateChanged += RefreshButtons;
            GameManager.OnRewardCollected += AddRewardSlot;
            GameManager.OnRewardsCleared += ClearRewardBar;
            CurrencyManager.OnGoldChanged += RefreshGold;
            if (GameManager.Instance != null)
                RefreshButtons(GameManager.Instance.State);
        }

        private void OnDisable()
        {
            GameManager.OnZoneChanged -= RefreshZone;
            GameManager.OnStateChanged -= RefreshButtons;
            GameManager.OnRewardCollected -= AddRewardSlot;
            GameManager.OnRewardsCleared -= ClearRewardBar;
            CurrencyManager.OnGoldChanged -= RefreshGold;
        }

        private void RefreshZone(int zone, ZoneType type)
        {
            wheelController.Setup(GameManager.Instance.GetCurrentWheel(), zone);

            zoneTitleText.text = type switch
            {
                ZoneType.Super => "SUPER ZONE",
                ZoneType.Safe => "SAFE ZONE",
                _ => "ZONE " + zone
            };

            buttonLeave.interactable = GameManager.Instance.CanLeave();
            UpdateZoneIndicator(zone);
        }

        private void UpdateZoneIndicator(int current)
        {
            int center = zoneLabels.Length / 2;

            for (int i = 0; i < zoneLabels.Length; i++)
            {
                int zone = current + (i - center);
                var label = zoneLabels[i];

                if (zone < 1)
                {
                    label.text = "";
                    continue;
                }

                label.text = zone.ToString();
                if (i == center)
                {
                    label.color = Color.white;
                }
                else
                {
                    var type = GameManager.Instance.GetZoneType(zone);
                    label.color = ZoneTextColor(type, zone < current);
                }
            }

            var currentType = GameManager.Instance.GetZoneType(current);
            zonePanel.sprite = currentType switch
            {
                ZoneType.Super => spriteZoneSuper,
                ZoneType.Safe => spriteZoneSafe,
                _ => spriteZoneNormal
            };
        }

        private Color ZoneTextColor(ZoneType type, bool passed)
        {
            Color c = type switch
            {
                ZoneType.Super => zoneSuperColor,
                ZoneType.Safe => zoneSafeColor,
                _ => zoneNormalColor
            };
            if (passed)
                c = new Color(c.r * passedDim, c.g * passedDim, c.b * passedDim, c.a);
            return c;
        }

        private void RefreshButtons(GameState state)
        {
            buttonSpin.interactable = state == GameState.Playing;
            if (state == GameState.Playing && CurrencyManager.Instance != null)
                RefreshGold(CurrencyManager.Instance.Gold);
        }

        private void RefreshGold(int gold)
        {
            goldText.text = gold.ToString();
        }

        private readonly Dictionary<RewardItemData, RewardSlotUI> rewardSlotMap = new();
        private Image flyingRewardImage;

        private void Awake()
        {
            flyingRewardImage = flyingRewardIcon.GetComponent<Image>();
            flyingRewardImage.preserveAspect = true;
            flyingRewardIcon.gameObject.SetActive(false);
            flyingRewardGlow.SetActive(false);
        }

        private void AddRewardSlot(CollectedReward reward)
        {
            flyingRewardImage.sprite = reward.Reward.icon;
            flyingRewardIcon.position = wheelController.transform.position;
            flyingRewardIcon.localScale = Vector3.zero;
            flyingRewardIcon.gameObject.SetActive(true);
            flyingRewardGlow.SetActive(true);

            bool isNew = !rewardSlotMap.TryGetValue(reward.Reward, out var targetSlot);

            if (isNew)
            {
                targetSlot = Instantiate(rewardSlotPrefab, rewardBarContainer);
                targetSlot.Setup(reward);
                targetSlot.transform.localScale = Vector3.zero;
                rewardSlotMap[reward.Reward] = targetSlot;
                Canvas.ForceUpdateCanvases();
            }

            if (rewardScrollRect != null)
                ScrollToItem(targetSlot.transform as RectTransform);

            var seq = DOTween.Sequence();
            seq.Append(flyingRewardIcon.DOScale(1.5f, 0.3f).SetEase(Ease.OutBack));
            seq.Append(flyingRewardIcon.DOMove(targetSlot.transform.position, 0.5f).SetEase(Ease.InBack));
            seq.Join(flyingRewardIcon.DOScale(0.3f, 0.5f));
            seq.OnComplete(() =>
            {
                flyingRewardIcon.gameObject.SetActive(false);
                flyingRewardGlow.SetActive(false);
                if (isNew)
                    targetSlot.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
                else
                    targetSlot.AddAmount(reward.Amount);
            });
        }

        private void ScrollToItem(RectTransform item)
        {
            if (rewardScrollRect == null || item == null) return;
            Canvas.ForceUpdateCanvases();

            var content = rewardBarContainer as RectTransform;
            var viewport = rewardScrollRect.viewport != null
                ? rewardScrollRect.viewport
                : rewardScrollRect.GetComponent<RectTransform>();

            float scrollable = content.rect.height - viewport.rect.height;
            if (scrollable <= 0f) return;

            float viewportH = viewport.rect.height;
            float itemTopDist = content.rect.yMax - (item.localPosition.y + item.rect.yMax);
            float itemBottomDist = content.rect.yMax - (item.localPosition.y + item.rect.yMin);
            float currentOffset = (1f - rewardScrollRect.verticalNormalizedPosition) * scrollable;

            float targetOffset;
            if (itemTopDist < currentOffset)
                targetOffset = itemTopDist;
            else if (itemBottomDist > currentOffset + viewportH)
                targetOffset = itemBottomDist - viewportH;
            else
                return;

            targetOffset = Mathf.Clamp(targetOffset, 0f, scrollable);

            DOTween.To(() => rewardScrollRect.verticalNormalizedPosition,
                x => rewardScrollRect.verticalNormalizedPosition = x,
                1f - targetOffset / scrollable, 0.3f);
        }

        private void ClearRewardBar()
        {
            foreach (Transform child in rewardBarContainer)
                Destroy(child.gameObject);
            rewardSlotMap.Clear();
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            foreach (var btn in GetComponentsInChildren<Button>(true))
            {
                if (btn.name == "ui_button_spin") buttonSpin = btn;
                if (btn.name == "ui_button_leave") buttonLeave = btn;
            }
            if (wheelController == null)
                wheelController = GetComponentInChildren<WheelController>(true);
        }
    }
}
