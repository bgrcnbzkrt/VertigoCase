using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Vertigo.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class PanelBase : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;

        public virtual void Show()
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            if (gameObject.activeSelf && canvasGroup.alpha >= 1f) return;
            gameObject.SetActive(true);
            canvasGroup.DOKill();
            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, 0.25f).SetEase(Ease.OutQuad);
        }

        public virtual void Hide()
        {
            if (!gameObject.activeSelf) return;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.DOFade(0f, 0.2f)
                .SetEase(Ease.InQuad)
                .OnComplete(() => gameObject.SetActive(false));
        }

        public void HideImmediate()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
        }

        protected static void ClearChildren(Transform parent)
        {
            foreach (Transform child in parent)
                Destroy(child.gameObject);
        }

        // shared OnValidate helper: find a child button by name 
        protected Button FindButton(string buttonName)
        {
            foreach (var btn in GetComponentsInChildren<Button>(true))
                if (btn.name == buttonName) return btn;
            return null;
        }

        protected virtual void OnValidate()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
        }
    }
}
