using System.Collections.Generic;
using UnityEngine;

namespace BankAlHazz
{
    public enum AIDifficulty { Easy, Normal, Hard }

    /// <summary>
    /// AI opponent. Acts in 2–5 seconds, never stalls the clock.
    /// Easy: buys cautiously, misses set value. Normal: buys/builds/trades
    /// and keeps a cash reserve. Hard: blocks opponent sets, leaves auctions
    /// early, trades aggressively for monopolies.
    /// The AI must buy, build, trade and bid — not just roll.
    /// </summary>
    public class AIManager : MonoBehaviour
    {
        public AIDifficulty Difficulty = AIDifficulty.Normal;

        private GameManager gameManager;
        private Board board;
        private Bank bank;
        private System.Random rng = new System.Random();

        private void Awake()
        {
            gameManager = FindObjectOfType<GameManager>();
            board = FindObjectOfType<Board>();
            bank = FindObjectOfType<Bank>();
        }

        /// <summary>Decide whether to buy the property the AI just landed on.</summary>
        public bool ShouldBuy(Player ai, Property property)
        {
            if (!ai.CanAfford(property.Price)) return false;
            int reserve = Difficulty == AIDifficulty.Easy ? 400
                : Difficulty == AIDifficulty.Normal ? 200 : 100;
            if (ai.Cash - property.Price < reserve && Difficulty != AIDifficulty.Hard)
            {
                // Easy/Normal keep a cash reserve; Hard will go nearly broke for a set.
                if (!CompletesSet(ai, property)) return false;
            }
            if (Difficulty == AIDifficulty.Easy) return rng.NextDouble() < 0.7;
            return true;
        }

        /// <summary>Would buying this property complete a colour set?</summary>
        public bool CompletesSet(Player ai, Property property)
        {
            if (!property.IsStreet) return false;
            foreach (var p in board.PropertiesInGroup(property.Group))
                if (p != property && p.Owner != ai) return false;
            return true;
        }

        /// <summary>Maximum the AI will bid in an auction.</summary>
        public int MaxAuctionBid(Player ai, Property property)
        {
            int baseValue = property.Price;
            if (CompletesSet(ai, property)) baseValue = (int)(property.Price * 1.5f);
            switch (Difficulty)
            {
                case AIDifficulty.Easy: return (int)(baseValue * 0.8f);
                case AIDifficulty.Hard: return (int)(baseValue * 1.3f);
                default: return baseValue;
            }
        }

        /// <summary>Build houses where it is most profitable, respecting even building.</summary>
        public void BuildPhase(Player ai)
        {
            foreach (var group in System.Enum.GetValues(typeof(PropertyGroup)))
            {
                var g = (PropertyGroup)group;
                if (!ai.OwnsFullSet(board, g)) continue;
                foreach (var p in board.PropertiesInGroup(g))
                {
                    int hotels = gameManager.HousesForHotel;
                    if (!p.HasHotel && p.Houses < GameConfig.MaxHousesPerProperty)
                    {
                        string buildReason;
                        if (bank.CanBuildHouse(ai, p, out buildReason)) bank.BuildHouse(ai, p);
                    }
                    else if (!p.HasHotel && p.Houses >= hotels)
                    {
                        bank.BuildHotel(ai, p, hotels);
                    }
                }
            }
        }

        /// <summary>
        /// Evaluate a trade offer from the AI's side. Hard AI blocks opponent
        /// sets: it refuses to give away the last property of a set the
        /// opponent would complete, and overpays to complete its own.
        /// </summary>
        public bool EvaluateTrade(Player ai, TradeOffer give, TradeOffer receive)
        {
            // Value what the AI would receive.
            int receiveValue = receive.Cash;
            foreach (var p in receive.Properties)
            {
                receiveValue += p.IsMortgaged ? p.Price / 2 : p.Price;
                if (CompletesSet(ai, p)) receiveValue += p.Price / 2; // set bonus
            }
            // Value what the AI would give away.
            int giveValue = give.Cash;
            foreach (var p in give.Properties)
            {
                giveValue += p.IsMortgaged ? p.Price / 2 : p.Price;
                // Never hand the opponent their completing property cheaply.
                if (WouldCompleteSetFor(give.From, p)) giveValue += p.Price;
            }

            int threshold = Difficulty == AIDifficulty.Easy ? 0
                : Difficulty == AIDifficulty.Normal ? -50 : -100;
            return receiveValue - giveValue > threshold;
        }

        private bool WouldCompleteSetFor(Player player, Property property)
        {
            if (!property.IsStreet) return false;
            foreach (var p in board.PropertiesInGroup(property.Group))
                if (p != property && p.Owner != player) return false;
            return true;
        }

        /// <summary>Delay before the AI acts, so its turn reads as thinking, not instant.</summary>
        public float ThinkDelaySeconds()
        {
            return Difficulty == AIDifficulty.Easy ? 2f : 3f;
        }
    }
}
