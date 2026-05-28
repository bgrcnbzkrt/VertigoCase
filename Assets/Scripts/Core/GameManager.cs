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

        public GameState State { get; private set; }
        public int CurrentZone { get; private set; }
        public IReadOnlyList<CollectedReward> Rewards => collected;

        private readonly List<CollectedReward> collected = new List<CollectedReward>();

        public static event Action<GameState> OnStateChanged;
        public static event Action OnSpinRequested;
        public static event Action<int, ZoneType> OnZoneChanged;
        public static event Action<CollectedReward> OnRewardCollected;
        public static event Action OnRewardsCleared;

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void StartGame()
        {
            collected.Clear();
            CurrentZone = 0;
            SetState(GameState.Playing);
            AdvanceZone();
        }

        public void RequestSpin()
        {
            if (State != GameState.Playing) return;
            SetState(GameState.Spinning);
            OnSpinRequested?.Invoke();
        }

        public void ReportSpinResult(SliceData result)
        {
            if (result.isBomb)
            {
                SetState(GameState.BombHit);
                return;
            }

            var reward = new CollectedReward(result.reward, result.amount, CurrentZone);

            int idx = collected.FindIndex(r => r.Reward == result.reward);
            if (idx >= 0)
            {
                var existing = collected[idx];
                existing.Amount += result.amount;
                collected[idx] = existing;
            }
            else
            {
                collected.Add(reward);
            }

            OnRewardCollected?.Invoke(reward);
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
            collected.Clear();
            OnRewardsCleared?.Invoke();
            SetState(GameState.MainMenu);
        }

        public void BackToMenu()
        {
            if (State == GameState.Collecting)
            {
                foreach (var r in collected)
                    if (r.Reward.type == RewardType.Currency)
                        CurrencyManager.Instance.Add(r.Amount);
            }
            SetState(GameState.MainMenu);
        }

        public ZoneType GetZoneType(int zone)
        {
            if (zone > 0 && zone % 30 == 0) return ZoneType.Super;
            if (zone > 0 && zone % 5 == 0) return ZoneType.Safe;
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

        void AdvanceZone()
        {
            CurrentZone++;
            OnZoneChanged?.Invoke(CurrentZone, GetZoneType(CurrentZone));
        }

        void SetState(GameState newState)
        {
            State = newState;
            OnStateChanged?.Invoke(newState);
        }
    }
}
