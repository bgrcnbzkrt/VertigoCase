using System;
using UnityEngine;

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

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            Gold = PlayerPrefs.GetInt("Gold", startingGold);
            Money = PlayerPrefs.GetInt("Money", startingMoney);
        }

        public void AddGold(int amount)
        {
            Gold += amount;
            PlayerPrefs.SetInt("Gold", Gold);
            OnGoldChanged?.Invoke(Gold);
        }

        public void AddMoney(int amount)
        {
            Money += amount;
            PlayerPrefs.SetInt("Money", Money);
            OnMoneyChanged?.Invoke(Money);
        }

        public bool TrySpend(int amount)
        {
            if (Gold < amount) return false;
            Gold -= amount;
            PlayerPrefs.SetInt("Gold", Gold);
            OnGoldChanged?.Invoke(Gold);
            return true;
        }

        public bool CanAfford(int amount) => Gold >= amount;
    }
}
