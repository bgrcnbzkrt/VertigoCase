using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Core;
using Vertigo.Data;

namespace Vertigo.Wheel
{
    public class WheelController : MonoBehaviour
    {
        [SerializeField] private Image baseImage;
        [SerializeField] private Image indicatorImage;
        [SerializeField] private List<SliceRef> sliceRefs;

        private WheelConfig config;
        private bool spinning;

        [System.Serializable]
        public class SliceRef
        {
            public Image icon;
            public TMP_Text amountText;
            public GameObject root;
        }

        private void OnEnable()
        {
            GameManager.OnSpinRequested += Spin;
        }

        private void OnDisable()
        {
            GameManager.OnSpinRequested -= Spin;
        }

        public void Setup(WheelConfig wheelConfig, int zone)
        {
            bool sameWheel = config == wheelConfig;
            config = wheelConfig;
            spinning = false;
            DOTween.Kill(baseImage.rectTransform);
            baseImage.sprite = config.baseSprite;
            indicatorImage.sprite = config.indicatorSprite;

            for (int i = 0; i < sliceRefs.Count; i++)
            {
                if (i >= config.slices.Count)
                {
                    sliceRefs[i].root.SetActive(false);
                    continue;
                }

                sliceRefs[i].root.SetActive(true);
                var slice = config.slices[i];
                sliceRefs[i].icon.sprite = slice.reward.icon;
                sliceRefs[i].icon.preserveAspect = true;
                if (slice.reward.type == RewardType.Bomb)
                    sliceRefs[i].amountText.SetText("");
                else
                    sliceRefs[i].amountText.SetText("x{0}", config.ScaleAmount(slice.amount, zone));
            }

            // only reset rotation when switching wheels, otherwise it jumps mid-game
            if (!sameWheel)
                baseImage.rectTransform.localRotation = Quaternion.identity;
        }

        // target slice is decided by GameManager; the wheel only plays the animation
        private void Spin(int target)
        {
            if (spinning || config == null) return;
            spinning = true;

            float sliceAngle = 360f / config.slices.Count;
            int rotations = Random.Range(config.minRotations, config.maxRotations + 1);
            float totalAngle = 360f * rotations + (target * sliceAngle);

            var targetRotation = new Vector3(0, 0, totalAngle);

            // FastBeyond360 -> actually spin the turns, don't snap the short way
            baseImage.rectTransform
                .DORotate(targetRotation, config.spinDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.OutQuart)
                .OnComplete(() =>
                {
                    spinning = false;
                    GameManager.Instance.OnSpinComplete();
                });
        }
    }
}
