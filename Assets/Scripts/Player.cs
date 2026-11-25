using System.Collections.Generic;
using UnityEngine;

namespace BankAlHazz
{
    /// <summary>
    /// A player in the match: human, AI, or the human banker seat.
    /// The banker seat (IsBankerSeat) never owns property and never rolls.
    /// </summary>
    public class Player : MonoBehaviour
    {
        public string PlayerId;
        public string DisplayName;
        public bool IsAI;
        public bool IsBankerSeat;
        public int SeatIndex;

        // Cosmetic only — transport token, never affects gameplay.
        public string TokenId = "bicycle"; // bicycle | car | luxury_car | scooter | tuktuk | plane
        public string AvatarId;

        public int Cash = GameConfig.StartingCash;
        public int Position;
        public bool IsBankrupt;

        // Jail state
        public bool InJail;
        public int JailTurns;
        public int GetOutOfJailFreeCards;

        public readonly List<Property> Properties = new List<Property>();

        public int TransportsOwned
        {
            get
            {
                int n = 0;
                foreach (var p in Properties) if (p.IsTransport) n++;
                return n;
            }
        }

        public int UtilitiesOwned
        {
            get
            {
                int n = 0;
                foreach (var p in Properties) if (p.IsUtility) n++;
                return n;
            }
        }

        /// <summary>Does this player own every street in the colour group?</summary>
        public bool OwnsFullSet(Board board, PropertyGroup group)
        {
            var groupProps = board.PropertiesInGroup(group);
            if (groupProps.Count == 0) return false;
            foreach (var p in groupProps)
                if (p.Owner != this) return false;
            return true;
        }

        /// <summary>
        /// Hasbro Short-Game net-worth valuation (used to decide timed matches):
        /// cash + unmortgaged at printed price + mortgaged at half price
        /// + houses at purchase price + hotels at purchase price (incl. houses turned in).
        /// </summary>
        public int NetWorth()
        {
            int worth = Cash;
            foreach (var p in Properties)
            {
                worth += p.IsMortgaged ? p.Price / 2 : p.Price;
                worth += p.Houses * p.HouseCost;
                if (p.HasHotel) worth += p.HouseCost; // hotel price incl. houses turned in
            }
            return worth;
        }

        /// <summary>
        /// Total cash the player can raise right now by selling buildings
        /// (half price) and mortgaging everything unmortgaged.
        /// </summary>
        public int RaiseableCash()
        {
            int total = Cash;
            foreach (var p in Properties)
            {
                total += p.SellBuildingsValue();
                if (!p.IsMortgaged) total += p.MortgageValue;
            }
            return total;
        }

        public bool CanAfford(int amount) => Cash >= amount;

        public void Pay(int amount) => Cash -= amount;
        public void Receive(int amount) => Cash += amount;

        public void BuyProperty(Property property)
        {
            Pay(property.Price);
            property.Owner = this;
            Properties.Add(property);
        }

        public void LoseProperty(Property property)
        {
            Properties.Remove(property);
            property.Owner = null;
            property.IsMortgaged = false;
            property.Houses = 0;
            property.HasHotel = false;
        }

        /// <summary>Move forward N squares, wrapping around the board.</summary>
        /// <returns>True if the move passed GO.</returns>
        public bool Move(int steps, int boardSize)
        {
            int old = Position;
            Position = (Position + steps) % boardSize;
            return Position < old; // wrapped past GO
        }
    }
}
