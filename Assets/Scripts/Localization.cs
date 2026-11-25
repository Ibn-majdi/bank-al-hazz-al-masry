using System.Collections.Generic;
using UnityEngine;

namespace BankAlHazz
{
    public enum Language { Arabic, English }

    /// <summary>
    /// Arabic + English UI strings. Amounts are identical in both languages;
    /// only the wording changes. Arabic UI must use an RTL layout.
    /// Add new strings here — never hard-code UI text in gameplay scripts.
    /// </summary>
    public static class Localization
    {
        public static Language Current = Language.Arabic;

        private sealed class LocalizedString
        {
            public string Ar;
            public string En;
            public LocalizedString(string ar, string en) { Ar = ar; En = en; }
        }

        private static readonly Dictionary<string, LocalizedString> strings =
            new Dictionary<string, LocalizedString>
            {
                { "app_name", new LocalizedString("بنك الحظ المصري", "Bank Al-Hazz Al-Masry") },
                { "play", new LocalizedString("العب", "Play") },
                { "solo", new LocalizedString("فردي ضد الكمبيوتر", "Solo vs AI") },
                { "online", new LocalizedString("أونلاين", "Online") },
                { "create_room", new LocalizedString("إنشاء روم", "Create Room") },
                { "join_room", new LocalizedString("دخول روم", "Join Room") },
                { "room_code", new LocalizedString("كود الروم", "Room Code") },
                { "invite", new LocalizedString("دعوة", "Invite") },
                { "roll_dice", new LocalizedString("ارمِ الزهر", "Roll Dice") },
                { "buy", new LocalizedString("شراء", "Buy") },
                { "auction", new LocalizedString("مزاد", "Auction") },
                { "pass", new LocalizedString("تخطَّ", "Pass") },
                { "trade", new LocalizedString("تبديل", "Trade") },
                { "mortgage", new LocalizedString("رهن", "Mortgage") },
                { "unmortgage", new LocalizedString("فك الرهن", "Lift Mortgage") },
                { "build_house", new LocalizedString("ابنِ بيتاً", "Build House") },
                { "build_hotel", new LocalizedString("ابنِ فندقاً", "Build Hotel") },
                { "end_turn", new LocalizedString("إنهاء الدور", "End Turn") },
                { "bankrupt", new LocalizedString("أفلست!", "Bankrupt!") },
                { "winner", new LocalizedString("الفائز", "Winner") },
                { "net_worth", new LocalizedString("صافي الثروة", "Net Worth") },
                { "cash", new LocalizedString("النقد", "Cash") },
                { "go_to_jail", new LocalizedString("اذهب إلى السجن", "Go to Jail") },
                { "get_out_jail_free", new LocalizedString("اخرج من السجن مجاناً", "Get Out of Jail Free") },
                { "chance", new LocalizedString("فرصة", "Chance") },
                { "community_chest", new LocalizedString("صندوق المجتمع", "Community Chest") },
                { "time_left", new LocalizedString("الوقت المتبقي", "Time Left") },
                { "your_turn", new LocalizedString("دورك", "Your Turn") },
                { "waiting", new LocalizedString("بانتظار اللاعبين…", "Waiting for players…") },
                { "banker", new LocalizedString("البنك", "Banker") },
                { "bicycle", new LocalizedString("بسكلتة", "Bicycle") },
                { "car", new LocalizedString("عربية", "Car") },
                { "luxury_car", new LocalizedString("عربية فاخرة", "Luxury Car") },
                { "scooter", new LocalizedString("سكوتر كهرباء", "Electric Scooter") },
                { "tuktuk", new LocalizedString("توك توك", "Tuk-Tuk") },
                { "plane", new LocalizedString("طيارة خاصة", "Private Plane") },
            };

        public static string Get(string key)
        {
            LocalizedString pair;
            if (strings.TryGetValue(key, out pair))
                return Current == Language.Arabic ? pair.Ar : pair.En;
            return key;
        }

        /// <summary>Format money identically in both languages: "1,500 ج.م" / "1,500 EGP".</summary>
        public static string Money(int amount)
        {
            string formatted = amount.ToString("N0");
            return Current == Language.Arabic ? $"{formatted} ج.م" : $"{formatted} EGP";
        }
    }
}
