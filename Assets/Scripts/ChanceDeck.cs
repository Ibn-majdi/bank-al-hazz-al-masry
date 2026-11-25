using System;
using System.Collections.Generic;
using UnityEngine;

namespace BankAlHazz
{
    /// <summary>What a Chance card does when drawn.</summary>
    public enum ChanceEffect
    {
        AdvanceToGo,
        AdvanceToNearestTransport,
        AdvanceToNearestUtility,
        AdvanceToSquare,   // named square, e.g. Giza Pyramids
        GoBackThree,
        GoToJail,
        CollectMoney,
        PayMoney,
        PayEachPlayer,
        CollectFromEachPlayer,
        PayRepairs,        // per house / per hotel
        GetOutOfJailFree
    }

    [Serializable]
    public class ChanceCard
    {
        public string TextAr;
        public string TextEn;
        public ChanceEffect Effect;
        public int Amount;       // money / target square index, per effect
        public int RepairPerHouse = 25;
        public int RepairPerHotel = 100;

        public ChanceCard(string ar, string en, ChanceEffect effect, int amount = 0)
        {
            TextAr = ar; TextEn = en; Effect = effect; Amount = amount;
        }
    }

    /// <summary>
    /// Chance deck: 16 cards (Hasbro 2008–2021 mechanics, Egyptian flavour).
    /// Drawn from the top, returned face-down to the bottom — except
    /// Get Out of Jail Free, which is kept until used or traded.
    /// </summary>
    public class ChanceDeck : MonoBehaviour
    {
        private readonly List<ChanceCard> deck = new List<ChanceCard>();
        private readonly System.Random rng = new System.Random();

        private void Awake() => BuildDeck();

        public int Remaining => deck.Count;

        private void BuildDeck()
        {
            deck.Clear();
            deck.Add(new ChanceCard("تقدم إلى البداية", "Advance to GO", ChanceEffect.AdvanceToGo));
            deck.Add(new ChanceCard("تقدم إلى أقرب وسيلة مواصلات", "Advance to nearest transport", ChanceEffect.AdvanceToNearestTransport));
            deck.Add(new ChanceCard("تقدم إلى أقرب وسيلة مواصلات", "Advance to nearest transport", ChanceEffect.AdvanceToNearestTransport));
            deck.Add(new ChanceCard("تقدم إلى أقرب مرفق", "Advance to nearest utility", ChanceEffect.AdvanceToNearestUtility));
            deck.Add(new ChanceCard("تقدم إلى الجيزة — الأهرامات", "Advance to Giza — Pyramids", ChanceEffect.AdvanceToSquare, 39));
            deck.Add(new ChanceCard("ارجع 3 خانات", "Go back 3 spaces", ChanceEffect.GoBackThree));
            deck.Add(new ChanceCard("اذهب إلى السجن", "Go to Jail", ChanceEffect.GoToJail));
            deck.Add(new ChanceCard("أرباح البنك: احصل على 50 جنيه", "Bank dividend: collect 50 EGP", ChanceEffect.CollectMoney, 50));
            deck.Add(new ChanceCard("قرض البناء: احصل على 150 جنيه", "Building loan: collect 150 EGP", ChanceEffect.CollectMoney, 150));
            deck.Add(new ChanceCard("رئيس مجلس الإدارة: ادفع 50 جنيه لكل لاعب", "Chairman: pay 50 EGP to each player", ChanceEffect.PayEachPlayer, 50));
            deck.Add(new ChanceCard("مخالفة سرعة: ادفع 15 جنيه", "Speeding fine: pay 15 EGP", ChanceEffect.PayMoney, 15));
            deck.Add(new ChanceCard("صيانة: ادفع 25 جنيه لكل بيت و100 لكل فندق", "Repairs: pay 25 EGP per house, 100 per hotel", ChanceEffect.PayRepairs));
            deck.Add(new ChanceCard("اخرج من السجن مجاناً", "Get Out of Jail Free", ChanceEffect.GetOutOfJailFree));
            deck.Add(new ChanceCard("تقدم إلى القاهرة — الزمالك", "Advance to Cairo — Zamalek", ChanceEffect.AdvanceToSquare, 37));
            deck.Add(new ChanceCard("زحمة كوبري: ادفع 20 جنيه", "Traffic jam: pay 20 EGP", ChanceEffect.PayMoney, 20));
            deck.Add(new ChanceCard("عيدية: احصل على 100 جنيه", "Eid gift: collect 100 EGP", ChanceEffect.CollectMoney, 100));
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

        /// <summary>Draw the top card. Jail-free cards are NOT returned to the deck here.</summary>
        public ChanceCard Draw()
        {
            if (deck.Count == 0) BuildDeck();
            var card = deck[0];
            deck.RemoveAt(0);
            if (card.Effect != ChanceEffect.GetOutOfJailFree)
                deck.Add(card); // back to the bottom
            return card;
        }

        /// <summary>Return a used Get Out of Jail Free card to the bottom of the deck.</summary>
        public void ReturnJailFreeCard()
        {
            deck.Add(new ChanceCard("اخرج من السجن مجاناً", "Get Out of Jail Free", ChanceEffect.GetOutOfJailFree));
        }
    }
}
