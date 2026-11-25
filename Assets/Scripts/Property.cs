using System;
using UnityEngine;

namespace BankAlHazz
{
    /// <summary>
    /// A buyable property: street, transport, or utility.
    /// Rent tiers follow the classic Hasbro deed values (see GAME_DESIGN.md §4.2).
    /// Mortgage value is always half the printed price.
    /// </summary>
    [Serializable]
    public class Property
    {
        public string NameAr;
        public string NameEn;
        public PropertyGroup Group;
        public int Price;
        public int BaseRent;
        public bool IsTransport;
        public bool IsUtility;

        public Player Owner;
        public bool IsMortgaged;
        public int Houses;      // 0..4
        public bool HasHotel;

        // Rent with 1..4 houses and hotel, for streets. Index: houses count
        // (0 = site rent, 4 = 4 houses, 5 = hotel). Filled from the classic
        // ladder by price band; see RentTable below.
        private readonly int[] rentWithHouses = new int[6];

        public Property(string nameAr, string nameEn, PropertyGroup group,
            int price, int baseRent, bool isTransport = false, bool isUtility = false)
        {
            NameAr = nameAr;
            NameEn = nameEn;
            Group = group;
            Price = price;
            BaseRent = baseRent;
            IsTransport = isTransport;
            IsUtility = isUtility;
            RentTable.Fill(this);
        }

        public int HouseCost => GameConfig.HouseCostForGroup(Group);
        public int MortgageValue => Price / 2;
        public int UnmortgageCost => MortgageValue + Mathf.RoundToInt(MortgageValue * GameConfig.MortgageInterestRate);
        public bool IsOwned => Owner != null;
        public bool IsStreet => !IsTransport && !IsUtility;

        /// <summary>
        /// Current rent, given how many transports/utilities the owner holds
        /// and the dice total (utilities charge a multiple of the dice roll).
        /// </summary>
        public int CurrentRent(int transportsOwned, int utilitiesOwned, int diceTotal, bool ownerHasFullSet)
        {
            if (IsMortgaged || Owner == null) return 0;

            if (IsTransport)
            {
                // 25 / 50 / 100 / 200 for 1..4 owned.
                return 25 * (1 << (Mathf.Clamp(transportsOwned, 1, 4) - 1));
            }

            if (IsUtility)
            {
                int multiplier = utilitiesOwned >= 2 ? 10 : 4;
                return multiplier * diceTotal;
            }

            if (HasHotel) return rentWithHouses[5];
            if (Houses > 0) return rentWithHouses[Houses];

            // No buildings: double base rent on a full colour set.
            return ownerHasFullSet ? BaseRent * 2 : BaseRent;
        }

        public int RentAtLevel(int houses, bool hotel)
        {
            if (hotel) return rentWithHouses[5];
            return rentWithHouses[Mathf.Clamp(houses, 0, 4)];
        }

        public void SetRentTier(int houses, int rent)
        {
            rentWithHouses[Mathf.Clamp(houses, 0, 5)] = rent;
        }

        public int SellBuildingsValue()
        {
            // Buildings sell back to the Bank at half the price paid.
            int value = Houses * (HouseCost / 2);
            if (HasHotel) value += HouseCost / 2;
            return value;
        }
    }

    /// <summary>
    /// Classic rent tiers per price band. The full 22-street ladder is
    /// encoded by price so placeholder governorates keep correct ratios.
    /// Values are the Hasbro deed rents: site / 1 / 2 / 3 / 4 houses / hotel.
    /// </summary>
    public static class RentTable
    {
        public static void Fill(Property p)
        {
            if (!p.IsStreet) return;

            // Keyed by (price, baseRent) — the classic ladder.
            int[] tiers;
            if (p.Price == 60 && p.BaseRent == 2)       tiers = new[] { 2, 10, 30, 90, 160, 250 };
            else if (p.Price == 60)                     tiers = new[] { 4, 20, 60, 180, 320, 450 };
            else if (p.Price == 100)                    tiers = new[] { 6, 30, 90, 270, 400, 550 };
            else if (p.Price == 120)                    tiers = new[] { 8, 40, 100, 300, 450, 600 };
            else if (p.Price == 140)                    tiers = new[] { 10, 50, 150, 450, 625, 750 };
            else if (p.Price == 160)                    tiers = new[] { 12, 60, 180, 500, 700, 900 };
            else if (p.Price == 180)                    tiers = new[] { 14, 70, 200, 550, 750, 950 };
            else if (p.Price == 200)                    tiers = new[] { 16, 80, 220, 600, 800, 1000 };
            else if (p.Price == 220)                    tiers = new[] { 18, 90, 250, 700, 875, 1050 };
            else if (p.Price == 240)                    tiers = new[] { 20, 100, 300, 750, 925, 1100 };
            else if (p.Price == 260)                    tiers = new[] { 22, 110, 330, 800, 975, 1150 };
            else if (p.Price == 280)                    tiers = new[] { 24, 120, 360, 850, 1025, 1200 };
            else if (p.Price == 300)                    tiers = new[] { 26, 130, 390, 900, 1100, 1275 };
            else if (p.Price == 320)                    tiers = new[] { 28, 150, 450, 1000, 1200, 1400 };
            else if (p.Price == 350)                    tiers = new[] { 35, 175, 500, 1100, 1300, 1500 };
            else if (p.Price == 400)                    tiers = new[] { 50, 200, 600, 1400, 1700, 2000 };
            else
            {
                // Fallback: scale from base rent (keeps ratios sane).
                tiers = new[] { p.BaseRent, p.BaseRent * 5, p.BaseRent * 15,
                    p.BaseRent * 45, p.BaseRent * 80, p.BaseRent * 100 };
            }

            for (int i = 0; i < 6; i++) p.SetRentTier(i, tiers[i]);
        }
    }
}
