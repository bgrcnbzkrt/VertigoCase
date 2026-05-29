using UnityEngine;
using UnityEngine.UI;
using Vertigo.Core;

namespace Vertigo.UI
{
    public class RewardCollectPanel : PanelBase
    {
        [SerializeField] private Button buttonCollect;
        [SerializeField] private Transform rewardGrid;
        [SerializeField] private RewardSlotUI slotPrefab;

        private void Start()
        {
            buttonCollect.onClick.AddListener(() => GameManager.Instance.BackToMenu());
        }

        public override void Show()
        {
            foreach (Transform child in rewardGrid)
                Destroy(child.gameObject);

            foreach (var r in GameManager.Instance.Rewards)
            {
                var slot = Instantiate(slotPrefab, rewardGrid);
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
