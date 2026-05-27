using DG.Tweening;
using UnityEngine;
using Vertigo.Core;

namespace Vertigo.UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private MainMenuPanel mainMenu;
        [SerializeField] private GamePanel game;
        [SerializeField] private BombPanel bomb;
        [SerializeField] private RewardCollectPanel rewardCollect;

        private void Awake()
        {
            DG.Tweening.DOTween.Init();
        }

        private void Start()
        {
            game.HideImmediate();
            bomb.HideImmediate();
            rewardCollect.HideImmediate();
            mainMenu.Show();
        }

        private void OnEnable()
        {
            GameManager.OnStateChanged += HandleStateChange;
        }

        private void OnDisable()
        {
            GameManager.OnStateChanged -= HandleStateChange;
        }

        private void HandleStateChange(GameState state)
        {
            switch (state)
            {
                case GameState.MainMenu:
                    game.Hide();
                    bomb.Hide();
                    rewardCollect.Hide();
                    mainMenu.Show();
                    break;
                case GameState.Playing:
                    mainMenu.Hide();
                    bomb.Hide();
                    rewardCollect.Hide();
                    game.Show();
                    break;
                case GameState.BombHit:
                    bomb.Show();
                    break;
                case GameState.Collecting:
                    rewardCollect.Show();
                    break;
            }
        }
    }

    [RequireComponent(typeof(CanvasGroup))]
    public abstract class PanelBase : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;

        public virtual void Show()
        {
            gameObject.SetActive(true);
            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, 0.25f).SetEase(Ease.OutQuad);
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
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

        protected virtual void OnValidate()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
        }
    }
}
