using System;
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

        [Header("Reward Bar")]
        [SerializeField] private Transform rewardBarContainer;
        [SerializeField] private RewardSlotUI rewardSlotPrefab;

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
        }

        private void OnDisable()
        {
            GameManager.OnZoneChanged -= RefreshZone;
            GameManager.OnStateChanged -= RefreshButtons;
            GameManager.OnRewardCollected -= AddRewardSlot;
            GameManager.OnRewardsCleared -= ClearRewardBar;
        }

        private void RefreshZone(int zone, ZoneType type)
        {
            wheelController.Setup(GameManager.Instance.GetCurrentWheel());

            zoneTitleText.text = type switch
            {
                ZoneType.Super => "SUPER ZONE",
                ZoneType.Safe => "SAFE ZONE",
                _ => "ZONE " + zone
            };

            btnLeave.gameObject.SetActive(GameManager.Instance.CanLeave());
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
                    slot.background.gameObject.SetActive(false);
                    continue;
                }

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
        }

        private void AddRewardSlot(CollectedReward reward)
        {
            var slot = Instantiate(rewardSlotPrefab, rewardBarContainer);
            slot.Setup(reward);
        }

        private void ClearRewardBar()
        {
            foreach (Transform child in rewardBarContainer)
                Destroy(child.gameObject);
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
