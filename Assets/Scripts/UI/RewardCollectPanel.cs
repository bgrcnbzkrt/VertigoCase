using UnityEngine;
using UnityEngine.UI;
using Vertigo.Core;

namespace Vertigo.UI
{
    public class RewardCollectPanel : PanelBase
    {
        [SerializeField] private Button buttonCollect;
        [SerializeField] private Transform slotContainer;
        [SerializeField] private RewardSlotUI rewardSlotPrefab;

        private void Start()
        {
            buttonCollect.onClick.AddListener(() => GameManager.Instance.BackToMenu());
        }

        public override void Show()
        {
            ClearChildren(slotContainer);

            foreach (var r in GameManager.Instance.Rewards)
            {
                var slot = Instantiate(rewardSlotPrefab, slotContainer);
                slot.Setup(r);
            }

            base.Show();
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            foreach (var btn in GetComponentsInChildren<Button>(true))
                if (btn.name == "ui_button_collect") buttonCollect = btn;
        }
    }
}
