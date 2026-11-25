using System;
using System.Collections.Generic;
using UnityEngine;

namespace BankAlHazz
{
    /// <summary>
    /// A single square on the board. Pure data — no Unity behaviour.
    /// </summary>
    [Serializable]
    public class BoardSquare
    {
        public int Index;
        public SquareType Type;
        public string NameAr;   // Arabic display name (governorate / place)
        public string NameEn;   // English display name
        public Property Property; // null for non-buyable squares

        public BoardSquare(int index, SquareType type, string nameAr, string nameEn)
        {
            Index = index;
            Type = type;
            NameAr = nameAr;
            NameEn = nameEn;
        }

        public bool IsBuyable => Property != null;
    }

    /// <summary>
    /// Board data model for 28 / 32 / 40 squares.
    /// The full 40-square board is the classic layout; smaller boards are
    /// derived by removing whole colour groups (never splitting a group).
    /// Governorate names here are PLACEHOLDERS — swap them later without
    /// touching the price ladder (see Property.cs / GAME_DESIGN.md §4.2).
    /// </summary>
    public class Board : MonoBehaviour
    {
        [SerializeField] private int boardSize = GameConfig.ClassicBoardSize;

        public int Size => squares.Count;
        public IReadOnlyList<BoardSquare> Squares => squares;

        private readonly List<BoardSquare> squares = new List<BoardSquare>();

        private void Awake()
        {
            Build(boardSize);
        }

        /// <summary>Builds the board for the requested size (28, 32 or 40).</summary>
        public void Build(int size)
        {
            if (size != 28 && size != 32 && size != 40)
                throw new ArgumentException($"Unsupported board size {size}. Use 28, 32 or 40.");

            boardSize = size;
            squares.Clear();

            // Full 40-square classic layout (placeholders for governorates).
            var full = BuildClassic40();

            if (size == 40)
            {
                squares.AddRange(full);
                return;
            }

            // Smaller boards: drop whole colour groups, keep 4 corners,
            // transports, utilities and 2 Chance + 2 Community Chest spaces.
            var dropGroups = size == 32
                ? new HashSet<PropertyGroup> { PropertyGroup.Brown, PropertyGroup.LightBlue }
                : new HashSet<PropertyGroup>
                    { PropertyGroup.Brown, PropertyGroup.LightBlue, PropertyGroup.Pink, PropertyGroup.Orange };

            var kept = new List<BoardSquare>();
            foreach (var sq in full)
            {
                if (sq.Property != null && dropGroups.Contains(sq.Property.Group))
                    continue;
                kept.Add(sq);
            }

            // Re-index after removal so movement stays contiguous.
            for (int i = 0; i < kept.Count; i++)
                kept[i].Index = i;

            squares.AddRange(kept);
        }

        public BoardSquare GetSquare(int index)
        {
            int wrapped = ((index % squares.Count) + squares.Count) % squares.Count;
            return squares[wrapped];
        }

        /// <summary>Index of the GO square (always 0).</summary>
        public int GoIndex => 0;

        /// <summary>Index of the Jail / Just Visiting square.</summary>
        public int JailIndex
        {
            get
            {
                for (int i = 0; i < squares.Count; i++)
                    if (squares[i].Type == SquareType.JailJustVisiting) return i;
                return 10;
            }
        }

        /// <summary>All street properties in one colour group.</summary>
        public List<Property> PropertiesInGroup(PropertyGroup group)
        {
            var list = new List<Property>();
            foreach (var sq in squares)
                if (sq.Property != null && sq.Property.Group == group)
                    list.Add(sq.Property);
            return list;
        }

        /// <summary>Total printed value of every buyable property on this board.</summary>
        public int TotalPropertyValue()
        {
            int total = 0;
            foreach (var sq in squares)
                if (sq.Property != null) total += sq.Property.Price;
            return total;
        }

        private static void AddSquare(List<BoardSquare> list, int i, SquareType t,
            string ar, string en, Property p = null)
        {
            var sq = new BoardSquare(i, t, ar, en) { Property = p };
            list.Add(sq);
        }

        /// <summary>Classic 40-square layout with placeholder governorates.</summary>
        private static List<BoardSquare> BuildClassic40()
        {
            var list = new List<BoardSquare>();

            AddSquare(list, 0, SquareType.Go, "البداية", "GO");
            AddSquare(list, 1, SquareType.Street, "الفيوم — وادي الريان", "Fayoum — Wadi El Rayyan",
                new Property("الفيوم", "Fayoum", PropertyGroup.Brown, 60, 2));
            AddSquare(list, 2, SquareType.CommunityChest, "صندوق المجتمع", "Community Chest");
            AddSquare(list, 3, SquareType.Street, "بني سويف", "Beni Suef",
                new Property("بني سويف", "Beni Suef", PropertyGroup.Brown, 60, 4));
            AddSquare(list, 4, SquareType.IncomeTax, "ضريبة الدخل", "Income Tax");
            AddSquare(list, 5, SquareType.Transport, "محطة رمسيس", "Ramses Station",
                new Property("محطة رمسيس", "Ramses Station", PropertyGroup.None, 200, 25, isTransport: true));
            AddSquare(list, 6, SquareType.Street, "المنيا", "Minya",
                new Property("المنيا", "Minya", PropertyGroup.LightBlue, 100, 6));
            AddSquare(list, 7, SquareType.Chance, "فرصة", "Chance");
            AddSquare(list, 8, SquareType.Street, "أسيوط", "Assiut",
                new Property("أسيوط", "Assiut", PropertyGroup.LightBlue, 100, 6));
            AddSquare(list, 9, SquareType.Street, "سوهاج — أبيدوس", "Sohag — Abydos",
                new Property("سوهاج", "Sohag", PropertyGroup.LightBlue, 120, 8));
            AddSquare(list, 10, SquareType.JailJustVisiting, "السجن / زيارة", "Jail / Just Visiting");
            AddSquare(list, 11, SquareType.Street, "المنصورة", "Mansoura",
                new Property("المنصورة", "Mansoura", PropertyGroup.Pink, 140, 10));
            AddSquare(list, 12, SquareType.Utility, "الكهرباء", "Electricity",
                new Property("الكهرباء", "Electricity", PropertyGroup.None, 150, 0, isUtility: true));
            AddSquare(list, 13, SquareType.Street, "طنطا", "Tanta",
                new Property("طنطا", "Tanta", PropertyGroup.Pink, 140, 10));
            AddSquare(list, 14, SquareType.Street, "الزقازيق", "Zagazig",
                new Property("الزقازيق", "Zagazig", PropertyGroup.Pink, 160, 12));
            AddSquare(list, 15, SquareType.Transport, "مطار القاهرة", "Cairo Airport",
                new Property("مطار القاهرة", "Cairo Airport", PropertyGroup.None, 200, 25, isTransport: true));
            AddSquare(list, 16, SquareType.Street, "بورسعيد", "Port Said",
                new Property("بورسعيد", "Port Said", PropertyGroup.Orange, 180, 14));
            AddSquare(list, 17, SquareType.CommunityChest, "صندوق المجتمع", "Community Chest");
            AddSquare(list, 18, SquareType.Street, "الإسماعيلية", "Ismailia",
                new Property("الإسماعيلية", "Ismailia", PropertyGroup.Orange, 180, 14));
            AddSquare(list, 19, SquareType.Street, "السويس", "Suez",
                new Property("السويس", "Suez", PropertyGroup.Orange, 200, 16));
            AddSquare(list, 20, SquareType.FreeParking, "موقف مجاني", "Free Parking");
            AddSquare(list, 21, SquareType.Street, "دمياط — رأس البر", "Damietta — Ras El Bar",
                new Property("دمياط", "Damietta", PropertyGroup.Red, 220, 18));
            AddSquare(list, 22, SquareType.Chance, "فرصة", "Chance");
            AddSquare(list, 23, SquareType.Street, "الإسكندرية — قلعة قايتباي", "Alexandria — Qaitbay Citadel",
                new Property("الإسكندرية", "Alexandria", PropertyGroup.Red, 220, 18));
            AddSquare(list, 24, SquareType.Street, "مرسى مطروح", "Marsa Matrouh",
                new Property("مرسى مطروح", "Marsa Matrouh", PropertyGroup.Red, 240, 20));
            AddSquare(list, 25, SquareType.Transport, "ميناء الإسكندرية", "Alexandria Port",
                new Property("ميناء الإسكندرية", "Alexandria Port", PropertyGroup.None, 200, 25, isTransport: true));
            AddSquare(list, 26, SquareType.Street, "الأقصر — الكرنك", "Luxor — Karnak",
                new Property("الأقصر", "Luxor", PropertyGroup.Yellow, 260, 22));
            AddSquare(list, 27, SquareType.Street, "أسوان — السد العالي", "Aswan — High Dam",
                new Property("أسوان", "Aswan", PropertyGroup.Yellow, 260, 22));
            AddSquare(list, 28, SquareType.Utility, "المياه", "Water",
                new Property("المياه", "Water", PropertyGroup.None, 150, 0, isUtility: true));
            AddSquare(list, 29, SquareType.Street, "قنا — دندرة", "Qena — Dendera",
                new Property("قنا", "Qena", PropertyGroup.Yellow, 280, 24));
            AddSquare(list, 30, SquareType.GoToJail, "إلى السجن", "Go To Jail");
            AddSquare(list, 31, SquareType.Street, "الغردقة", "Hurghada",
                new Property("الغردقة", "Hurghada", PropertyGroup.Green, 300, 26));
            AddSquare(list, 32, SquareType.Street, "شرم الشيخ", "Sharm El Sheikh",
                new Property("شرم الشيخ", "Sharm El Sheikh", PropertyGroup.Green, 300, 26));
            AddSquare(list, 33, SquareType.CommunityChest, "صندوق المجتمع", "Community Chest");
            AddSquare(list, 34, SquareType.Street, "دهب", "Dahab",
                new Property("دهب", "Dahab", PropertyGroup.Green, 320, 28));
            AddSquare(list, 35, SquareType.Transport, "محطة سيدي جابر", "Sidi Gaber Station",
                new Property("محطة سيدي جابر", "Sidi Gaber Station", PropertyGroup.None, 200, 25, isTransport: true));
            AddSquare(list, 36, SquareType.Chance, "فرصة", "Chance");
            AddSquare(list, 37, SquareType.Street, "القاهرة — الزمالك", "Cairo — Zamalek",
                new Property("القاهرة", "Cairo", PropertyGroup.DarkBlue, 350, 35));
            AddSquare(list, 38, SquareType.LuxuryTax, "ضريبة فاخرة", "Luxury Tax");
            AddSquare(list, 39, SquareType.Street, "الجيزة — الأهرامات", "Giza — Pyramids",
                new Property("الجيزة", "Giza", PropertyGroup.DarkBlue, 400, 50));

            return list;
        }
    }
}
