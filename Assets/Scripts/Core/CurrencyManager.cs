using System;
using UnityEngine;

namespace Vertigo.Core
{
    public class CurrencyManager : MonoBehaviour
    {
        public static CurrencyManager Instance { get; private set; }

        [SerializeField] private int startingGold = 500;
        [SerializeField] private int reviveCost = 100;

        public int Gold { get; private set; }
        public int ReviveCost => reviveCost;

        public static event Action<int> OnGoldChanged;

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            Gold = PlayerPrefs.GetInt("Gold", startingGold);
        }

        public void Add(int amount)
        {
            Gold += amount;
            Save();
            OnGoldChanged?.Invoke(Gold);
        }

        public bool TrySpend(int amount)
        {
            if (Gold < amount) return false;
            Gold -= amount;
            Save();
            OnGoldChanged?.Invoke(Gold);
            return true;
        }

        public bool CanAfford(int amount) => Gold >= amount;

        void Save() => PlayerPrefs.SetInt("Gold", Gold);
    }
}
