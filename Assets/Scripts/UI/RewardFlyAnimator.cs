using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Vertigo.UI
{
    public class RewardFlyAnimator
    {
        private readonly RectTransform icon;
        private readonly Image iconImage;
        private readonly GameObject glow;
        private readonly CanvasGroup glowGroup;

        public RewardFlyAnimator(RectTransform icon, GameObject glow)
        {
            this.icon = icon;
            iconImage = icon.GetComponent<Image>();
            iconImage.preserveAspect = true;
            this.glow = glow;
            glowGroup = glow.GetComponent<CanvasGroup>();
            ResetState();
        }

        public void ResetState()
        {
            icon.gameObject.SetActive(false);
            glowGroup.alpha = 0f;
        }

        public void Play(Sprite sprite, Vector3 startPos, Vector3 targetPos, Action onArrive)
        {
            iconImage.sprite = sprite;
            icon.position = startPos;
            icon.localScale = Vector3.zero;
            icon.gameObject.SetActive(true);

            glowGroup.DOKill();
            glowGroup.alpha = 0f;
            glowGroup.DOFade(1f, 0.2f);
            glow.transform.DOKill();
            glow.transform.DOLocalRotate(new Vector3(0f, 0f, -360f), 4f, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart);

            var seq = DOTween.Sequence();
            seq.Append(icon.DOScale(1.5f, 0.3f).SetEase(Ease.OutBack));
            seq.Append(icon.DOMove(targetPos, 0.5f).SetEase(Ease.InBack));
            seq.Join(icon.DOScale(0.3f, 0.5f));
            seq.OnComplete(() =>
            {
                icon.gameObject.SetActive(false);
                glow.transform.DOKill();
                glowGroup.DOFade(0f, 0.25f);
                onArrive?.Invoke();
            });
        }
    }
}
