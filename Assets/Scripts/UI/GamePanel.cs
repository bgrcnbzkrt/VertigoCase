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
        [SerializeField] private TMP_Text moneyText;

        [Header("Reward Bar")]
        [SerializeField] private ScrollRect rewardScrollRect;
        [SerializeField] private Transform slotContainer;
        [SerializeField] private RewardSlotUI rewardSlotPrefab;
        [SerializeField] private RectTransform flyingRewardIcon;
        [SerializeField] private GameObject flyingRewardGlow;

        private readonly Dictionary<RewardItemData, RewardSlotUI> rewardSlotMap = new();
        private RewardFlyAnimator flyAnimator;

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
            CurrencyManager.OnMoneyChanged += RefreshMoney;
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
            CurrencyManager.OnMoneyChanged -= RefreshMoney;
        }

        private void RefreshZone(int zone, ZoneType type)
        {
            wheelController.Setup(GameManager.Instance.GetCurrentWheel(), zone);

            if (type == ZoneType.Super)
                zoneTitleText.SetText("SUPER ZONE");
            else if (type == ZoneType.Safe)
                zoneTitleText.SetText("SAFE ZONE");
            else
                zoneTitleText.SetText("ZONE {0}", zone);

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

                label.SetText("{0}", zone);
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
            {
                RefreshGold(CurrencyManager.Instance.Gold);
                RefreshMoney(CurrencyManager.Instance.Money);
            }
        }

        private void RefreshGold(int gold)
        {
            goldText.SetText("{0}", gold);
        }

        private void RefreshMoney(int money)
        {
            moneyText.SetText("{0}", money);
        }

        private void Awake()
        {
            flyAnimator = new RewardFlyAnimator(flyingRewardIcon, flyingRewardGlow);
        }

        private void AddRewardSlot(CollectedReward reward)
        {
            bool isNew = !rewardSlotMap.TryGetValue(reward.Reward, out var targetSlot);

            if (isNew)
            {
                targetSlot = Instantiate(rewardSlotPrefab, slotContainer);
                targetSlot.Setup(reward);
                targetSlot.transform.localScale = Vector3.zero;
                rewardSlotMap[reward.Reward] = targetSlot;
                // rebuild just this container's layout (not every canvas)
                LayoutRebuilder.ForceRebuildLayoutImmediate(slotContainer as RectTransform);
            }

            if (rewardScrollRect != null)
                ScrollToItem(targetSlot.transform as RectTransform);

            flyAnimator.Play(reward.Reward.icon, wheelController.transform.position, targetSlot.transform.position, () =>
            {
                if (isNew)
                    targetSlot.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
                else
                    targetSlot.AddAmount(reward.Amount);
            });
        }

        private void ScrollToItem(RectTransform item)
        {
            if (rewardScrollRect == null || item == null) return;

            var content = slotContainer as RectTransform;
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
            ClearChildren(slotContainer);
            rewardSlotMap.Clear();
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            buttonSpin = FindButton("ui_button_spin");
            buttonLeave = FindButton("ui_button_leave");
            if (wheelController == null)
                wheelController = GetComponentInChildren<WheelController>(true);
        }
    }
}
