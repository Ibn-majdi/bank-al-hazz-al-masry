using UnityEngine;

namespace BankAlHazz
{
    /// <summary>
    /// Shared constants for Bank Al-Hazz Al-Masry.
    /// All money values are in EGP and follow the Hasbro classic numerals
    /// (1500 start, 200 GO) reskinned from dollars to Egyptian pounds.
    /// </summary>
    public static class GameConfig
    {
        // Money (EGP)
        public const int StartingCash = 1500;
        public const int GoSalary = 200;
        public const int JailFine = 50;
        public const int IncomeTax = 200;
        public const int LuxuryTax = 100;

        // Board sizes selectable by the room creator
        public static readonly int[] BoardSizes = { 28, 32, 40 };
        public const int ClassicBoardSize = 40;

        // Match lengths (minutes) offered in solo timed mode
        public static readonly int[] SoloMatchLengthsMinutes = { 15, 30, 45, 60 };

        // Turn timers (seconds)
        public const float TurnTimerClassicSeconds = 60f;
        public const float TurnTimerQuickSeconds = 35f;
        public const float AuctionTimerSeconds = 30f;
        public const float TradeResponseSeconds = 20f;

        // Building supply (Hasbro classic)
        public const int TotalHouses = 32;
        public const int TotalHotels = 12;
        public const int MaxHousesPerProperty = 4;
        public const int HousesForHotelClassic = 4;
        public const int HousesForHotelQuick = 3;

        // Players
        public const int MaxPlayingSeats = 4;
        public const int MinPlayingSeats = 2;
        public const int MaxRoomOccupants = 5; // 4 players + 1 human banker

        // Mortgage
        public const float MortgageInterestRate = 0.10f; // +10% to lift

        // Auction
        public const int AuctionMinIncrement = 10;

        // House-cost tiers by colour group (EGP per house)
        public static int HouseCostForGroup(PropertyGroup group)
        {
            switch (group)
            {
                case PropertyGroup.Brown:
                case PropertyGroup.LightBlue:
                    return 50;
                case PropertyGroup.Pink:
                case PropertyGroup.Orange:
                    return 100;
                case PropertyGroup.Red:
                case PropertyGroup.Yellow:
                    return 150;
                case PropertyGroup.Green:
                case PropertyGroup.DarkBlue:
                    return 200;
                default:
                    return 0;
            }
        }
    }

    /// <summary>Colour groups of street properties (classic 8 groups).</summary>
    public enum PropertyGroup
    {
        None,
        Brown,
        LightBlue,
        Pink,
        Orange,
        Red,
        Yellow,
        Green,
        DarkBlue
    }

    /// <summary>What kind of square a board position is.</summary>
    public enum SquareType
    {
        Go,
        Street,
        Transport,
        Utility,
        Chance,
        CommunityChest,
        IncomeTax,
        LuxuryTax,
        JailJustVisiting,
        FreeParking,
        GoToJail
    }

    /// <summary>Match ruleset derived from the chosen match length.</summary>
    public enum MatchRuleset
    {
        Quick15,   // 15 min: dealt deeds, 3-house hotels, Quick Jail, 35 s turns
        Short30,   // 30 min: short hybrid
        Standard45,// 45 min: classic building, 60 s turns
        Classic60  // 60 min: full classic
    }

    /// <summary>How the match is being played.</summary>
    public enum GameMode
    {
        SoloVsAI,
        OnlineRoom
    }
}
