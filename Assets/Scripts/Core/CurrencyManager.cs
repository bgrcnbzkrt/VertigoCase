using System;
using UnityEngine;
using Vertigo.Data;

namespace Vertigo.Core
{
    public class CurrencyManager : MonoBehaviour
    {
        public static CurrencyManager Instance { get; private set; }

        [SerializeField] private int startingGold = 500;
        [SerializeField] private int startingMoney = 0;
        [SerializeField] private int reviveCost = 100;

        public int Gold { get; private set; }
        public int Money { get; private set; }
        public int ReviveCost => reviveCost;

        public static event Action<int> OnGoldChanged;
        public static event Action<int> OnMoneyChanged;

        private const string PrefsGold = "Gold";
        private const string PrefsMoney = "Money";

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            // load saved balances, fall back to starting values on first run
            Gold = PlayerPrefs.GetInt(PrefsGold, startingGold);
            Money = PlayerPrefs.GetInt(PrefsMoney, startingMoney);
        }

        public void AddGold(int amount)
        {
            Gold += amount;
            PlayerPrefs.SetInt(PrefsGold, Gold);
            OnGoldChanged?.Invoke(Gold);
        }

        public void AddMoney(int amount)
        {
            Money += amount;
            PlayerPrefs.SetInt(PrefsMoney, Money);
            OnMoneyChanged?.Invoke(Money);
        }

        public bool TrySpend(int amount)
        {
            if (Gold < amount) return false;
            Gold -= amount;
            PlayerPrefs.SetInt(PrefsGold, Gold);
            OnGoldChanged?.Invoke(Gold);
            return true;
        }

        public bool CanAfford(int amount) => Gold >= amount;


        private void OnApplicationPause(bool paused)
        {
            if (paused) PlayerPrefs.Save();
        }

        private void OnApplicationQuit()
        {
            PlayerPrefs.Save();
        }

        // gold/money go back to balances, other reward types just stay in the list
        public void Deposit(RewardType type, int amount)
        {
            switch (type)
            {
                case RewardType.Gold: AddGold(amount); break;
                case RewardType.Money: AddMoney(amount); break;
            }
        }
    }
}
