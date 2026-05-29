using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Core;

namespace Vertigo.UI
{
    public class RewardCollectPanel : PanelBase
    {
        [SerializeField] private Button btnCollect;
        [SerializeField] private Transform rewardGrid;
        [SerializeField] private RewardSlotUI slotPrefab;
        [SerializeField] private RectTransform celebrationGlow;
        [SerializeField] private RectTransform celebrationFlash;

        private Image celebrationFlashImage;
        private bool glowStarted;

        private void Start()
        {
            btnCollect.onClick.AddListener(() => GameManager.Instance.BackToMenu());
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
            PlayCelebration();
        }

        private void PlayCelebration()
        {
            if (celebrationGlow != null && !glowStarted)
            {
                glowStarted = true;
                celebrationGlow.DOScale(1.08f, 1.4f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            }

            if (celebrationFlash == null) return;
            if (celebrationFlashImage == null)
                celebrationFlashImage = celebrationFlash.GetComponent<Image>();

            celebrationFlash.DOKill();
            celebrationFlashImage.DOKill();
            celebrationFlash.localScale = Vector3.one * 0.5f;
            celebrationFlashImage.color = new Color(1f, 1f, 1f, 1f);

            var seq = DOTween.Sequence();
            seq.Append(celebrationFlash.DOScale(1.3f, 0.5f).SetEase(Ease.OutQuad));
            seq.Join(celebrationFlashImage.DOFade(0f, 0.5f));
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            foreach (var btn in GetComponentsInChildren<Button>(true))
                if (btn.name == "ui_btn_collect") btnCollect = btn;
        }
    }
}
