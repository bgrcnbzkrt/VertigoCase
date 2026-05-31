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

        private PanelBase[] allPanels;

        private void Awake()
        {
            // recycle tweens so the spin/reward animations don't keep allocating
            DOTween.Init(recycleAllByDefault: true);
            allPanels = new PanelBase[] { mainMenu, game, bomb, rewardCollect };
        }

        private void Start()
        {
            foreach (var panel in allPanels)
                if (panel != mainMenu) panel.HideImmediate();
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
                    ShowOnly(mainMenu);
                    break;
                case GameState.Playing:
                    ShowOnly(game);
                    break;
                // bomb/collect sit on top of the game panel, leave the rest as is
                case GameState.BombHit:
                    bomb.Show();
                    break;
                case GameState.Collecting:
                    rewardCollect.Show();
                    break;
            }
        }

        private void ShowOnly(PanelBase target)
        {
            foreach (var panel in allPanels)
            {
                if (panel == target) panel.Show();
                else panel.Hide();
            }
        }
    }
}
