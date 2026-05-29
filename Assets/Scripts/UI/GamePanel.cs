using System;
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
        [SerializeField] private Button btnSpin;
        [SerializeField] private Button btnLeave;

        [Header("Zone")]
        [SerializeField] private TMP_Text zoneTitleText;
        [SerializeField] private ZoneSlot[] zoneSlots;

        [Header("Zone Sprites")]
        [SerializeField] private Sprite spriteZoneNormal;
        [SerializeField] private Sprite spriteZoneSafe;
        [SerializeField] private Sprite spriteZoneSuper;
        [SerializeField] private Sprite spriteZonePassed;
        [SerializeField] private Sprite spriteZoneCurrent;

        [Header("Wheel")]
        [SerializeField] private WheelController wheelController;

        [Header("Currency")]
        [SerializeField] private TMP_Text goldText;

        [Header("Reward Bar")]
        [SerializeField] private ScrollRect rewardScrollRect;
        [SerializeField] private Transform rewardBarContainer;
        [SerializeField] private RewardSlotUI rewardSlotPrefab;
        [SerializeField] private RectTransform flyingRewardIcon;

        [Serializable]
        public class ZoneSlot
        {
            public Image background;
            public TMP_Text text;
        }

        private void Start()
        {
            btnSpin.onClick.AddListener(() => GameManager.Instance.RequestSpin());
            btnLeave.onClick.AddListener(() => GameManager.Instance.CollectAndLeave());
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

            btnLeave.interactable = GameManager.Instance.CanLeave();
            UpdateZoneIndicator(zone);
        }

        private void UpdateZoneIndicator(int current)
        {
            int[] display = { current - 2, current - 1, current, current + 1, current + 2 };

            for (int i = 0; i < zoneSlots.Length && i < display.Length; i++)
            {
                int zone = display[i];
                var slot = zoneSlots[i];

                if (zone < 1)
                {
                    slot.background.gameObject.SetActive(true);
                    slot.background.color = Color.clear;
                    slot.text.text = "";
                    slot.background.transform.localScale = Vector3.one;
                    continue;
                }

                slot.background.color = Color.white;
                slot.background.gameObject.SetActive(true);
                slot.text.text = zone.ToString();

                if (zone < current)
                {
                    slot.background.sprite = spriteZonePassed;
                    slot.background.transform.localScale = Vector3.one;
                }
                else if (zone == current)
                {
                    slot.background.sprite = spriteZoneCurrent;
                    slot.background.transform.localScale = Vector3.one * 1.15f;
                }
                else
                {
                    var t = GameManager.Instance.GetZoneType(zone);
                    slot.background.sprite = t switch
                    {
                        ZoneType.Super => spriteZoneSuper,
                        ZoneType.Safe => spriteZoneSafe,
                        _ => spriteZoneNormal
                    };
                    slot.background.transform.localScale = Vector3.one;
                }
            }
        }

        private void RefreshButtons(GameState state)
        {
            btnSpin.interactable = state == GameState.Playing;
            if (state == GameState.Playing && CurrencyManager.Instance != null)
                RefreshGold(CurrencyManager.Instance.Gold);
        }

        private void RefreshGold(int gold)
        {
            if (goldText != null)
                goldText.text = gold.ToString();
        }

        private readonly Dictionary<RewardItemData, RewardSlotUI> rewardSlotMap = new();
        private Image flyingRewardImage;

        private void Awake()
        {
            flyingRewardImage = flyingRewardIcon.GetComponent<Image>();
            flyingRewardImage.preserveAspect = true;
            flyingRewardIcon.gameObject.SetActive(false);
        }

        private void AddRewardSlot(CollectedReward reward)
        {
            flyingRewardImage.sprite = reward.Reward.icon;
            flyingRewardIcon.position = wheelController.transform.position;
            flyingRewardIcon.localScale = Vector3.zero;
            flyingRewardIcon.gameObject.SetActive(true);

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

            var slot = targetSlot;
            var seq = DOTween.Sequence();
            seq.Append(flyingRewardIcon.DOScale(1.5f, 0.3f).SetEase(Ease.OutBack));
            seq.Append(flyingRewardIcon.DOMove(slot.transform.position, 0.5f).SetEase(Ease.InBack));
            seq.Join(flyingRewardIcon.DOScale(0.3f, 0.5f));
            seq.OnComplete(() =>
            {
                flyingRewardIcon.gameObject.SetActive(false);
                if (isNew)
                    slot.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
                else
                    slot.AddAmount(reward.Amount);
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
                if (btn.name == "ui_btn_spin") btnSpin = btn;
                if (btn.name == "ui_btn_leave") btnLeave = btn;
            }
            if (wheelController == null)
                wheelController = GetComponentInChildren<WheelController>(true);
        }
    }
}
