using System.Collections.Generic;
using UnityEngine;

namespace BankAlHazz
{
    /// <summary>
    /// The Bank. Holds the money supply (never goes broke), the finite
    /// building supply (32 houses, 12 hotels), and runs GO salary,
    /// mortgages and building sales. In online play the server IS the bank;
    /// a human banker seat may announce auctions but never touches the ledger.
    /// </summary>
    public class Bank : MonoBehaviour
    {
        public int HousesRemaining = GameConfig.TotalHouses;
        public int HotelsRemaining = GameConfig.TotalHotels;

        private Board board;

        private void Awake()
        {
            board = GetComponent<Board>();
            if (board == null) board = FindObjectOfType<Board>();
        }

        /// <summary>Pay GO salary (200 EGP) when a player passes or lands on GO.</summary>
        public void PayGoSalary(Player player)
        {
            player.Receive(GameConfig.GoSalary);
        }

        /// <summary>
        /// Can this player build a house on this property right now?
        /// Full set, nothing in the set mortgaged, even building, supply left.
        /// </summary>
        public bool CanBuildHouse(Player player, Property property, out string reason)
        {
            reason = null;
            if (!property.IsStreet) { reason = "not_street"; return false; }
            if (property.Owner != player) { reason = "not_owner"; return false; }
            if (property.HasHotel) { reason = "already_hotel"; return false; }
            if (property.Houses >= GameConfig.MaxHousesPerProperty) { reason = "max_houses"; return false; }
            if (!player.OwnsFullSet(board, property.Group)) { reason = "no_full_set"; return false; }
            foreach (var p in board.PropertiesInGroup(property.Group))
                if (p.IsMortgaged) { reason = "group_mortgaged"; return false; }
            // Even building: never more than one house ahead of the lowest in the group.
            int min = int.MaxValue;
            foreach (var p in board.PropertiesInGroup(property.Group))
                min = Mathf.Min(min, p.HasHotel ? GameConfig.MaxHousesPerProperty : p.Houses);
            if (property.Houses > min) { reason = "uneven_building"; return false; }
            if (HousesRemaining <= 0) { reason = "no_houses_left"; return false; }
            if (!player.CanAfford(property.HouseCost)) { reason = "cannot_afford"; return false; }
            return true;
        }

        public bool BuildHouse(Player player, Property property)
        {
            string unusedReason;
            if (!CanBuildHouse(player, property, out unusedReason)) return false;
            player.Pay(property.HouseCost);
            property.Houses++;
            HousesRemaining--;
            return true;
        }

        /// <summary>Hotel only after 4 houses (3 in Quick) on EVERY property in the set.</summary>
        public bool CanBuildHotel(Player player, Property property, int housesForHotel, out string reason)
        {
            reason = null;
            if (!property.IsStreet) { reason = "not_street"; return false; }
            if (property.Owner != player) { reason = "not_owner"; return false; }
            if (property.HasHotel) { reason = "already_hotel"; return false; }
            foreach (var p in board.PropertiesInGroup(property.Group))
            {
                if (p.IsMortgaged) { reason = "group_mortgaged"; return false; }
                if (p.Houses < housesForHotel) { reason = "not_enough_houses"; return false; }
            }
            if (HotelsRemaining <= 0) { reason = "no_hotels_left"; return false; }
            if (!player.CanAfford(property.HouseCost)) { reason = "cannot_afford"; return false; }
            return true;
        }

        public bool BuildHotel(Player player, Property property, int housesForHotel)
        {
            string unusedReason2;
            if (!CanBuildHotel(player, property, housesForHotel, out unusedReason2)) return false;
            player.Pay(property.HouseCost);
            foreach (var p in board.PropertiesInGroup(property.Group))
            {
                // Only the target property converts; others keep their houses
                // until they are upgraded in turn (classic rule: hotel per property).
                if (p == property)
                {
                    HousesRemaining += p.Houses; // 4 houses return to the Bank
                    p.Houses = 0;
                    p.HasHotel = true;
                }
            }
            HotelsRemaining--;
            return true;
        }

        /// <summary>Sell one house back to the Bank at half price, evenly (reverse of building).</summary>
        public bool SellHouse(Player player, Property property)
        {
            if (property.Owner != player || property.Houses <= 0) return false;
            int max = 0;
            foreach (var p in board.PropertiesInGroup(property.Group))
                max = Mathf.Max(max, p.HasHotel ? GameConfig.MaxHousesPerProperty : p.Houses);
            if (property.Houses < max) return false; // sell from the highest first
            property.Houses--;
            HousesRemaining++;
            player.Receive(property.HouseCost / 2);
            return true;
        }

        /// <summary>Mortgage an unimproved property for half its printed price.</summary>
        public bool Mortgage(Player player, Property property)
        {
            if (property.Owner != player || property.IsMortgaged) return false;
            if (property.Houses > 0 || property.HasHotel) return false;
            property.IsMortgaged = true;
            player.Receive(property.MortgageValue);
            return true;
        }

        /// <summary>Lift a mortgage: value + 10% interest.</summary>
        public bool Unmortgage(Player player, Property property)
        {
            if (property.Owner != player || !property.IsMortgaged) return false;
            if (!player.CanAfford(property.UnmortgageCost)) return false;
            player.Pay(property.UnmortgageCost);
            property.IsMortgaged = false;
            return true;
        }

        /// <summary>Sell ALL buildings in a colour group at half price (used when mortgaging / going bankrupt).</summary>
        public int LiquidateGroupBuildings(Player player, PropertyGroup group)
        {
            int gained = 0;
            foreach (var p in board.PropertiesInGroup(group))
            {
                if (p.Owner != player) continue;
                if (p.HasHotel)
                {
                    gained += p.HouseCost / 2;
                    HotelsRemaining++;
                    HousesRemaining += GameConfig.MaxHousesPerProperty;
                    p.HasHotel = false;
                }
                gained += p.Houses * (p.HouseCost / 2);
                HousesRemaining += p.Houses;
                p.Houses = 0;
            }
            player.Receive(gained);
            return gained;
        }
    }
}
