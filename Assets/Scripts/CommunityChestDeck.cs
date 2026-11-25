using System;
using System.Collections.Generic;
using UnityEngine;

namespace BankAlHazz
{
    /// <summary>What a Community Chest card does when drawn.</summary>
    public enum CommunityChestEffect
    {
        AdvanceToGo,
        GoToJail,
        CollectMoney,
        PayMoney,
        CollectFromEachPlayer,
        PayRepairs,
        GetOutOfJailFree
    }

    [Serializable]
    public class CommunityChestCard
    {
        public string TextAr;
        public string TextEn;
        public CommunityChestEffect Effect;
        public int Amount;
        public int RepairPerHouse = 40;
        public int RepairPerHotel = 115;

        public CommunityChestCard(string ar, string en, CommunityChestEffect effect, int amount = 0)
        {
            TextAr = ar; TextEn = en; Effect = effect; Amount = amount;
        }
    }

    /// <summary>
    /// Community Chest deck: 16 cards. Mechanics are the Hasbro 2021 rewrite
    /// (same amounts as 2008–2021); flavour is Egyptian community acts:
    /// neighbourhood iftar, street mawlid, helping neighbours, school
    /// fundraiser, rescuing a street cat. One Get Out of Jail Free per deck.
    /// </summary>
    public class CommunityChestDeck : MonoBehaviour
    {
        private readonly List<CommunityChestCard> deck = new List<CommunityChestCard>();
        private readonly System.Random rng = new System.Random();

        private void Awake() => BuildDeck();

        public int Remaining => deck.Count;

        private void BuildDeck()
        {
            deck.Clear();
            deck.Add(new CommunityChestCard("تقدم إلى البداية", "Advance to GO", CommunityChestEffect.AdvanceToGo));
            deck.Add(new CommunityChestCard("اذهب إلى السجن", "Go to Jail", CommunityChestEffect.GoToJail));
            deck.Add(new CommunityChestCard("ساعد جيرانك بعد العاصفة: احصل على 200 جنيه", "Help neighbours after the storm: collect 200 EGP", CommunityChestEffect.CollectMoney, 200));
            deck.Add(new CommunityChestCard("خطأ من البنك: احصل على 200 جنيه", "Bank error in your favour: collect 200 EGP", CommunityChestEffect.CollectMoney, 200));
            deck.Add(new CommunityChestCard("إفطار الحي: اجمع 10 جنيه من كل لاعب", "Neighbourhood iftar: collect 10 EGP from each player", CommunityChestEffect.CollectFromEachPlayer, 10));
            deck.Add(new CommunityChestCard("مولد الشارع: اجمع 10 جنيه من كل لاعب", "Street mawlid: collect 10 EGP from each player", CommunityChestEffect.CollectFromEachPlayer, 10));
            deck.Add(new CommunityChestCard("أنقذت قطة من الشارع: اخرج من السجن مجاناً", "You rescued a street cat: Get Out of Jail Free", CommunityChestEffect.GetOutOfJailFree));
            deck.Add(new CommunityChestCard("تبرع لمستشفى الأطفال: احصل على 100 جنيه", "Children's hospital donation: collect 100 EGP", CommunityChestEffect.CollectMoney, 100));
            deck.Add(new CommunityChestCard("مبادرة المدرسة: احصل على 100 جنيه", "School fundraiser: collect 100 EGP", CommunityChestEffect.CollectMoney, 100));
            deck.Add(new CommunityChestCard("بيع الكحك: احصل على 25 جنيه", "Kahk bake sale: collect 25 EGP", CommunityChestEffect.CollectMoney, 25));
            deck.Add(new CommunityChestCard("تبرع بالدم: احصل على 10 جنيه", "Blood donation: collect 10 EGP", CommunityChestEffect.CollectMoney, 10));
            deck.Add(new CommunityChestCard("فاتورة الدكتور: ادفع 50 جنيه", "Doctor's bill: pay 50 EGP", CommunityChestEffect.PayMoney, 50));
            deck.Add(new CommunityChestCard("مصاريف المدرسة: ادفع 50 جنيه", "School fees: pay 50 EGP", CommunityChestEffect.PayMoney, 50));
            deck.Add(new CommunityChestCard("فاتورة المستشفى: ادفع 100 جنيه", "Hospital bill: pay 100 EGP", CommunityChestEffect.PayMoney, 100));
            deck.Add(new CommunityChestCard("غسيل عربيات لجمع التبرعات: ادفع 100 جنيه", "Car-wash fundraiser: pay 100 EGP", CommunityChestEffect.PayMoney, 100));
            deck.Add(new CommunityChestCard("صيانة البيت: ادفع 40 جنيه لكل بيت و115 لكل فندق", "Home repairs: pay 40 EGP per house, 115 per hotel", CommunityChestEffect.PayRepairs));
            Shuffle();
        }

        public void Shuffle()
        {
            for (int i = deck.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                var tmp = deck[i]; deck[i] = deck[j]; deck[j] = tmp;
            }
        }

        public CommunityChestCard Draw()
        {
            if (deck.Count == 0) BuildDeck();
            var card = deck[0];
            deck.RemoveAt(0);
            if (card.Effect != CommunityChestEffect.GetOutOfJailFree)
                deck.Add(card);
            return card;
        }

        public void ReturnJailFreeCard()
        {
            deck.Add(new CommunityChestCard("أنقذت قطة من الشارع: اخرج من السجن مجاناً",
                "You rescued a street cat: Get Out of Jail Free", CommunityChestEffect.GetOutOfJailFree));
        }
    }
}
