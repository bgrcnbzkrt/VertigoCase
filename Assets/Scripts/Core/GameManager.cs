using System;
using System.Collections.Generic;
using UnityEngine;
using Vertigo.Data;

namespace Vertigo.Core
{
    public enum GameState { MainMenu, Playing, Spinning, BombHit, Collecting }
    public enum ZoneType { Normal, Safe, Super }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Wheel Configs")]
        [SerializeField] private WheelConfig bronzeWheel;
        [SerializeField] private WheelConfig silverWheel;
        [SerializeField] private WheelConfig goldenWheel;

        [Header("Zone Rules")]
        [SerializeField] private int safeZoneInterval = 5;
        [SerializeField] private int superZoneInterval = 30;

        public GameState State { get; private set; }
        public int CurrentZone { get; private set; }
        public IReadOnlyList<CollectedReward> Rewards => rewards.Items;

        private readonly RewardCollection rewards = new();
        private int pendingSpinTarget;

        public static event Action<GameState> OnStateChanged;
        public static event Action<int> OnSpinRequested;
        public static event Action<int, ZoneType> OnZoneChanged;
        public static event Action<CollectedReward> OnRewardCollected;
        public static event Action OnRewardsCleared;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; } // kill dupes
            Instance = this;
        }

        public void StartGame()
        {
            rewards.Clear();
            CurrentZone = 0;
            SetState(GameState.Playing);
            AdvanceZone();
        }

        public void RequestSpin()
        {
            if (State != GameState.Playing) return;
            // the model rolls the result here; the wheel only animates to this slice
            pendingSpinTarget = UnityEngine.Random.Range(0, GetCurrentWheel().slices.Count);
            SetState(GameState.Spinning);
            OnSpinRequested?.Invoke(pendingSpinTarget);
        }

        // the wheel view calls this once its landing animation finishes
        public void OnSpinComplete()
        {
            if (State != GameState.Spinning) return;
            ReportSpinResult(GetCurrentWheel().slices[pendingSpinTarget]);
        }

        private void ReportSpinResult(SliceData result)
        {
            if (result.reward.type == RewardType.Bomb)
            {
                SetState(GameState.BombHit);
                return;
            }

            int amount = GetCurrentWheel().ScaleAmount(result.amount, CurrentZone);
            rewards.Add(result.reward, amount);

            // event carries this spin's amount so the UI can animate the increment
            OnRewardCollected?.Invoke(new CollectedReward(result.reward, amount));
            AdvanceZone();
            SetState(GameState.Playing);
        }

        public void CollectAndLeave()
        {
            if (State != GameState.Playing || !CanLeave()) return;
            SetState(GameState.Collecting);
        }

        public void Revive()
        {
            if (State != GameState.BombHit) return;
            if (CurrencyManager.Instance == null) return;
            if (!CurrencyManager.Instance.TrySpend(CurrencyManager.Instance.ReviveCost)) return;
            SetState(GameState.Playing);
            OnZoneChanged?.Invoke(CurrentZone, GetZoneType(CurrentZone));
        }

        public void GiveUp()
        {
            rewards.Clear();
            OnRewardsCleared?.Invoke();
            SetState(GameState.MainMenu);
        }

        public void BackToMenu()
        {
            if (State == GameState.Collecting && CurrencyManager.Instance != null)
            {
                foreach (var r in rewards.Items)
                    CurrencyManager.Instance.Deposit(r.Reward.type, r.Amount);
            }
            rewards.Clear();
            OnRewardsCleared?.Invoke();
            SetState(GameState.MainMenu);
        }

        public ZoneType GetZoneType(int zone)
        {
            // intervals are designer-tunable from the inspector (default 5 / 30)
            if (zone > 0 && zone % superZoneInterval == 0) return ZoneType.Super;
            if (zone > 0 && zone % safeZoneInterval == 0) return ZoneType.Safe;
            return ZoneType.Normal;
        }

        public WheelConfig GetCurrentWheel() => GetZoneType(CurrentZone) switch
        {
            ZoneType.Super => goldenWheel,
            ZoneType.Safe => silverWheel,
            _ => bronzeWheel
        };

        public bool CanLeave()
        {
            var t = GetZoneType(CurrentZone);
            return t == ZoneType.Safe || t == ZoneType.Super;
        }

        private void AdvanceZone()
        {
            CurrentZone++;
            OnZoneChanged?.Invoke(CurrentZone, GetZoneType(CurrentZone));
        }

        private void SetState(GameState newState)
        {
            State = newState;
            OnStateChanged?.Invoke(newState);
        }
    }
}
