using System.Collections.Generic;
using UnityEngine;

namespace BankAlHazz
{
    /// <summary>
    /// Mandatory auction (Hasbro): when a player lands on an unowned property
    /// and declines to buy it at the printed price, the Bank immediately
    /// auctions it. Any player may bid, including the decliner.
    /// 20–30 s timer, minimum increment 10 EGP. Pass = out.
    /// </summary>
    public class Auction : MonoBehaviour
    {
        public Property Property { get; private set; }
        public bool IsRunning { get; private set; }
        public int CurrentBid { get; private set; }
        public Player CurrentBidder { get; private set; }

        private readonly List<Player> activeBidders = new List<Player>();
        private float timerSeconds;

        public event System.Action<Player, int> OnBidPlaced;
        public event System.Action<Player, int> OnAuctionFinished; // winner (null = no sale), price

        public void StartAuction(Property property, List<Player> players, float durationSeconds = GameConfig.AuctionTimerSeconds)
        {
            Property = property;
            IsRunning = true;
            CurrentBid = 0;
            CurrentBidder = null;
            timerSeconds = durationSeconds;
            activeBidders.Clear();
            activeBidders.AddRange(players);
        }

        private void Update()
        {
            if (!IsRunning) return;
            timerSeconds -= Time.deltaTime;
            if (timerSeconds <= 0f) Finish();
        }

        /// <summary>Place a bid. Must exceed the current bid by at least the minimum increment.</summary>
        public bool PlaceBid(Player player, int amount)
        {
            if (!IsRunning || !activeBidders.Contains(player)) return false;
            if (amount < CurrentBid + GameConfig.AuctionMinIncrement) return false;
            if (!player.CanAfford(amount)) return false;
            CurrentBid = amount;
            CurrentBidder = player;
            timerSeconds = GameConfig.AuctionTimerSeconds; // reset the clock on a new bid
            OnBidPlaced?.Invoke(player, amount);
            return true;
        }

        public void Pass(Player player)
        {
            activeBidders.Remove(player);
            if (activeBidders.Count <= 1 && CurrentBidder != null) Finish();
        }

        private void Finish()
        {
            IsRunning = false;
            if (CurrentBidder != null)
            {
                CurrentBidder.Pay(CurrentBid);
                CurrentBidder.Properties.Add(Property);
                Property.Owner = CurrentBidder;
            }
            // If nobody bid, the property stays with the Bank.
            OnAuctionFinished?.Invoke(CurrentBidder, CurrentBid);
        }
    }
}
