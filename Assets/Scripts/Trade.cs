using System.Collections.Generic;
using UnityEngine;

namespace BankAlHazz
{
    /// <summary>One side of a trade offer: cash + deeds + jail-free cards.</summary>
    [System.Serializable]
    public class TradeOffer
    {
        public Player From;
        public int Cash;
        public List<Property> Properties = new List<Property>();
        public int JailFreeCards;

        public bool IsValid()
        {
            if (From == null) return false;
            if (Cash < 0 || Cash > From.Cash) return false;
            if (JailFreeCards < 0 || JailFreeCards > From.GetOutOfJailFreeCards) return false;
            foreach (var p in Properties)
            {
                if (p.Owner != From) return false;
                if (p.Houses > 0 || p.HasHotel) return false; // sell buildings first
            }
            return true;
        }
    }

    /// <summary>
    /// Structured trading (Hasbro): money, properties and Get Out of Jail Free
    /// cards only — never buildings, never player loans, no unenforceable
    /// promises (e.g. rent immunity). Offers have a ~20 s response timer and
    /// a counter-offer must change the offer. Trading is allowed while in
    /// debt — it is the last chance to avoid bankruptcy.
    /// </summary>
    public class Trade : MonoBehaviour
    {
        public bool IsOpen { get; private set; }
        public TradeOffer OfferA { get; private set; }
        public TradeOffer OfferB { get; private set; }

        private float responseTimer;

        public event System.Action OnTradeAccepted;
        public event System.Action OnTradeDeclined;
        public event System.Action<TradeOffer, TradeOffer> OnCounterOffer;

        public bool Propose(TradeOffer fromA, TradeOffer fromB)
        {
            if (IsOpen) return false;
            if (!fromA.IsValid() || !fromB.IsValid()) return false;
            OfferA = fromA;
            OfferB = fromB;
            IsOpen = true;
            responseTimer = GameConfig.TradeResponseSeconds;
            return true;
        }

        private void Update()
        {
            if (!IsOpen) return;
            responseTimer -= Time.deltaTime;
            if (responseTimer <= 0f) Decline(); // timeout never accepts a trade
        }

        public void Accept()
        {
            if (!IsOpen) return;
            Execute(OfferA, OfferB);
            Execute(OfferB, OfferA);
            IsOpen = false;
            OnTradeAccepted?.Invoke();
        }

        public void Decline()
        {
            if (!IsOpen) return;
            IsOpen = false;
            OnTradeDeclined?.Invoke();
        }

        /// <summary>Counter-offer must differ from the current offer.</summary>
        public bool Counter(TradeOffer newA, TradeOffer newB)
        {
            if (!IsOpen) return false;
            if (!newA.IsValid() || !newB.IsValid()) return false;
            bool same = newA.Cash == OfferA.Cash && newB.Cash == OfferB.Cash
                && newA.Properties.Count == OfferA.Properties.Count
                && newB.Properties.Count == OfferB.Properties.Count
                && newA.JailFreeCards == OfferA.JailFreeCards
                && newB.JailFreeCards == OfferB.JailFreeCards;
            if (same) return false;
            OfferA = newA;
            OfferB = newB;
            responseTimer = GameConfig.TradeResponseSeconds;
            OnCounterOffer?.Invoke(newA, newB);
            return true;
        }

        private static void Execute(TradeOffer give, TradeOffer receive)
        {
            // give.From gives cash/deeds/cards to receive.From
            give.From.Pay(give.Cash);
            receive.From.Receive(give.Cash);
            foreach (var p in give.Properties)
            {
                give.From.Properties.Remove(p);
                p.Owner = receive.From;
                receive.From.Properties.Add(p);
            }
            give.From.GetOutOfJailFreeCards -= give.JailFreeCards;
            receive.From.GetOutOfJailFreeCards += give.JailFreeCards;
        }
    }
}
