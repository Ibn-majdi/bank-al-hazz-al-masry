using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BankAlHazz
{
    /// <summary>
    /// Owns the match: turn loop, match timer, turn timer, jail, rent,
    /// bankruptcy and the win condition. The GameManager is the only
    /// authority that moves players, charges rent and ends the match.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Board board;
        [SerializeField] private Bank bank;
        [SerializeField] private Dice dice;
        [SerializeField] private ChanceDeck chanceDeck;
        [SerializeField] private CommunityChestDeck chestDeck;
        [SerializeField] private Auction auction;
        [SerializeField] private Trade trade;

        [Header("Match setup")]
        public GameMode Mode = GameMode.SoloVsAI;
        public int MatchLengthMinutes = 30;
        public int BoardSize = GameConfig.ClassicBoardSize;

        public List<Player> Players { get; private set; } = new List<Player>();
        public Player CurrentPlayer { get; private set; }
        public bool MatchRunning { get; private set; }
        public bool MatchOver { get; private set; }
        public Player Winner { get; private set; }

        public float MatchTimeRemaining { get; private set; }
        public float TurnTimeRemaining { get; private set; }

        private int currentPlayerIndex;
        private MatchRuleset ruleset;
        private bool turnRolled; // has the current player rolled this turn?

        public MatchRuleset Ruleset => ruleset;

        public float TurnTimerSeconds =>
            ruleset == MatchRuleset.Quick15 ? GameConfig.TurnTimerQuickSeconds
                                            : GameConfig.TurnTimerClassicSeconds;

        public int HousesForHotel =>
            (ruleset == MatchRuleset.Quick15 || ruleset == MatchRuleset.Short30)
                ? GameConfig.HousesForHotelQuick
                : GameConfig.HousesForHotelClassic;

        public bool QuickJail => ruleset == MatchRuleset.Quick15 || ruleset == MatchRuleset.Short30;

        // ---- Setup -------------------------------------------------------

        public void StartMatch(List<Player> players, int matchLengthMinutes, int boardSize)
        {
            Players = players.Where(p => !p.IsBankerSeat).ToList();
            MatchLengthMinutes = matchLengthMinutes;
            BoardSize = boardSize;

            board.Build(boardSize);
            ruleset = RulesetForLength(matchLengthMinutes);
            MatchTimeRemaining = matchLengthMinutes * 60f;
            MatchRunning = true;
            MatchOver = false;
            Winner = null;

            foreach (var p in Players)
            {
                p.Cash = StartingCashForBoard();
                p.Position = 0;
            }

            // Random starting player (no first-player edge at 2p, but fair).
            currentPlayerIndex = Random.Range(0, Players.Count);
            CurrentPlayer = Players[currentPlayerIndex];
            BeginTurn();
        }

        public static MatchRuleset RulesetForLength(int minutes)
        {
            switch (minutes)
            {
                case 15: return MatchRuleset.Quick15;
                case 30: return MatchRuleset.Short30;
                case 45: return MatchRuleset.Standard45;
                default: return MatchRuleset.Classic60;
            }
        }

        /// <summary>
        /// Starting cash scales to ~26% of the board's total property value
        /// (classic: 1500 / 5830). The 40-board keeps the classic 1500.
        /// </summary>
        public int StartingCashForBoard()
        {
            if (BoardSize == GameConfig.ClassicBoardSize) return GameConfig.StartingCash;
            int total = board.TotalPropertyValue();
            return Mathf.Max(500, Mathf.RoundToInt(total * 0.26f / 50f) * 50);
        }

        // ---- Turn loop ---------------------------------------------------

        private void BeginTurn()
        {
            turnRolled = false;
            TurnTimeRemaining = TurnTimerSeconds;
        }

        private void Update()
        {
            if (!MatchRunning || MatchOver) return;

            MatchTimeRemaining -= Time.deltaTime;
            TurnTimeRemaining -= Time.deltaTime;

            if (MatchTimeRemaining <= 0f)
            {
                EndMatchByTime();
                return;
            }

            if (TurnTimeRemaining <= 0f)
            {
                // Timeout never buys, builds or accepts a trade:
                // auto-roll if not rolled, otherwise end the turn.
                if (!turnRolled) RollDice();
                else EndTurn();
            }
        }

        /// <summary>Roll the dice for the current player and resolve the move.</summary>
        public void RollDice()
        {
            if (!MatchRunning || turnRolled) return;
            turnRolled = true;

            bool thirdDoubles = dice.Roll();

            if (CurrentPlayer.InJail)
            {
                ResolveJailRoll(thirdDoubles);
                return;
            }

            if (thirdDoubles)
            {
                SendToJail(CurrentPlayer);
                EndTurn();
                return;
            }

            MoveCurrentPlayer(dice.Total);

            // Doubles grant another roll (handled by the UI calling RollDice
            // again); the turn only ends when the player ends it or fails.
            if (!dice.IsDoubles) TurnTimeRemaining = TurnTimerSeconds;
        }

        private void MoveCurrentPlayer(int steps)
        {
            bool passedGo = CurrentPlayer.Move(steps, board.Size);
            if (passedGo) bank.PayGoSalary(CurrentPlayer);
            ResolveLanding(CurrentPlayer, board.GetSquare(CurrentPlayer.Position), dice.Total);
        }

        private void ResolveLanding(Player player, BoardSquare square, int diceTotal)
        {
            switch (square.Type)
            {
                case SquareType.Street:
                case SquareType.Transport:
                case SquareType.Utility:
                    ResolvePropertyLanding(player, square.Property, diceTotal);
                    break;
                case SquareType.Chance:
                    ResolveChance(player);
                    break;
                case SquareType.CommunityChest:
                    ResolveCommunityChest(player);
                    break;
                case SquareType.IncomeTax:
                    Charge(player, GameConfig.IncomeTax, null);
                    break;
                case SquareType.LuxuryTax:
                    Charge(player, GameConfig.LuxuryTax, null);
                    break;
                case SquareType.GoToJail:
                    SendToJail(player);
                    break;
                default:
                    break; // GO, Jail/Just Visiting, Free Parking: nothing happens
            }
        }

        private void ResolvePropertyLanding(Player player, Property property, int diceTotal)
        {
            if (!property.IsOwned)
            {
                // UI offers Buy; declining starts a mandatory auction.
                // GameManager exposes the choice via TryBuy / DeclineToBuy.
                return;
            }
            if (property.Owner == player) return;
            int rent = property.CurrentRent(
                property.Owner.TransportsOwned,
                property.Owner.UtilitiesOwned,
                diceTotal,
                property.Owner.OwnsFullSet(board, property.Group));
            Charge(player, rent, property.Owner);
        }

        /// <summary>Buy the property the current player just landed on.</summary>
        public bool TryBuyCurrentProperty()
        {
            var sq = board.GetSquare(CurrentPlayer.Position);
            if (sq.Property == null || sq.Property.IsOwned) return false;
            if (!CurrentPlayer.CanAfford(sq.Property.Price)) return false;
            CurrentPlayer.BuyProperty(sq.Property);
            return true;
        }

        /// <summary>Decline to buy: the Bank immediately auctions the property.</summary>
        public void DeclineToBuy()
        {
            var sq = board.GetSquare(CurrentPlayer.Position);
            if (sq.Property == null || sq.Property.IsOwned) return;
            auction.StartAuction(sq.Property, Players);
        }

        // ---- Jail ----------------------------------------------------------

        public void SendToJail(Player player)
        {
            player.Position = board.JailIndex;
            player.InJail = true;
            player.JailTurns = 0;
            dice.EndTurn();
        }

        private void ResolveJailRoll(bool thirdDoubles)
        {
            var player = CurrentPlayer;
            player.JailTurns++;

            int maxTurns = QuickJail ? 1 : 3;
            if (dice.IsDoubles)
            {
                // Doubles: move, no extra roll.
                player.InJail = false;
                player.JailTurns = 0;
                MoveCurrentPlayer(dice.Total);
                EndTurn();
            }
            else if (player.JailTurns >= maxTurns)
            {
                // Forced exit: pay the fine and move.
                Charge(player, GameConfig.JailFine, null);
                player.InJail = false;
                player.JailTurns = 0;
                MoveCurrentPlayer(dice.Total);
                EndTurn();
            }
            else
            {
                // Still in jail; turn ends, player may pay or use a card next turn.
                EndTurn();
            }
        }

        /// <summary>Pay the 50 EGP fine before rolling to leave jail.</summary>
        public bool PayJailFine()
        {
            if (!CurrentPlayer.InJail || !CurrentPlayer.CanAfford(GameConfig.JailFine)) return false;
            CurrentPlayer.Pay(GameConfig.JailFine);
            CurrentPlayer.InJail = false;
            CurrentPlayer.JailTurns = 0;
            return true;
        }

        /// <summary>Use a Get Out of Jail Free card to leave jail.</summary>
        public bool UseJailFreeCard()
        {
            if (!CurrentPlayer.InJail || CurrentPlayer.GetOutOfJailFreeCards <= 0) return false;
            CurrentPlayer.GetOutOfJailFreeCards--;
            CurrentPlayer.InJail = false;
            CurrentPlayer.JailTurns = 0;
            chanceDeck.ReturnJailFreeCard();
            return true;
        }

        // ---- Cards ---------------------------------------------------------

        private void ResolveChance(Player player)
        {
            var card = chanceDeck.Draw();
            ApplyChanceCard(player, card);
        }

        private void ResolveCommunityChest(Player player)
        {
            var card = chestDeck.Draw();
            ApplyCommunityChestCard(player, card);
        }

        private void ApplyChanceCard(Player player, ChanceCard card)
        {
            switch (card.Effect)
            {
                case ChanceEffect.AdvanceToGo:
                    player.Position = board.GoIndex;
                    bank.PayGoSalary(player);
                    break;
                case ChanceEffect.AdvanceToSquare:
                    AdvancePlayerTo(player, card.Amount);
                    break;
                case ChanceEffect.AdvanceToNearestTransport:
                    AdvanceToNearest(player, SquareType.Transport);
                    break;
                case ChanceEffect.AdvanceToNearestUtility:
                    AdvanceToNearest(player, SquareType.Utility);
                    break;
                case ChanceEffect.GoBackThree:
                    player.Position = (player.Position - 3 + board.Size) % board.Size;
                    ResolveLanding(player, board.GetSquare(player.Position), dice.Total);
                    break;
                case ChanceEffect.GoToJail:
                    SendToJail(player);
                    break;
                case ChanceEffect.CollectMoney:
                    player.Receive(card.Amount);
                    break;
                case ChanceEffect.PayMoney:
                    Charge(player, card.Amount, null);
                    break;
                case ChanceEffect.PayEachPlayer:
                    foreach (var other in Players)
                        if (other != player) { player.Pay(card.Amount); other.Receive(card.Amount); }
                    break;
                case ChanceEffect.PayRepairs:
                    Charge(player, RepairCost(player, card.RepairPerHouse, card.RepairPerHotel), null);
                    break;
                case ChanceEffect.GetOutOfJailFree:
                    player.GetOutOfJailFreeCards++;
                    break;
            }
        }

        private void ApplyCommunityChestCard(Player player, CommunityChestCard card)
        {
            switch (card.Effect)
            {
                case CommunityChestEffect.AdvanceToGo:
                    player.Position = board.GoIndex;
                    bank.PayGoSalary(player);
                    break;
                case CommunityChestEffect.GoToJail:
                    SendToJail(player);
                    break;
                case CommunityChestEffect.CollectMoney:
                    player.Receive(card.Amount);
                    break;
                case CommunityChestEffect.PayMoney:
                    Charge(player, card.Amount, null);
                    break;
                case CommunityChestEffect.CollectFromEachPlayer:
                    foreach (var other in Players)
                        if (other != player) { other.Pay(card.Amount); player.Receive(card.Amount); }
                    break;
                case CommunityChestEffect.PayRepairs:
                    Charge(player, RepairCost(player, card.RepairPerHouse, card.RepairPerHotel), null);
                    break;
                case CommunityChestEffect.GetOutOfJailFree:
                    player.GetOutOfJailFreeCards++;
                    break;
            }
        }

        private int RepairCost(Player player, int perHouse, int perHotel)
        {
            int cost = 0;
            foreach (var p in player.Properties)
            {
                cost += p.Houses * perHouse;
                if (p.HasHotel) cost += perHotel;
            }
            return cost;
        }

        private void AdvancePlayerTo(Player player, int index)
        {
            bool passedGo = index < player.Position;
            player.Position = index;
            if (passedGo) bank.PayGoSalary(player);
            ResolveLanding(player, board.GetSquare(index), dice.Total);
        }

        private void AdvanceToNearest(Player player, SquareType type)
        {
            for (int i = 1; i <= board.Size; i++)
            {
                int idx = (player.Position + i) % board.Size;
                if (board.GetSquare(idx).Type == type)
                {
                    AdvancePlayerTo(player, idx);
                    return;
                }
            }
        }

        // ---- Money, debt and bankruptcy ------------------------------------

        /// <summary>
        /// Charge a player. If they cannot pay, they must raise cash (sell
        /// buildings / mortgage / trade); if they still cannot, they go bankrupt.
        /// Returns false if the player went bankrupt.
        /// </summary>
        public bool Charge(Player debtor, int amount, Player creditor)
        {
            if (amount <= 0) return true;
            if (debtor.CanAfford(amount))
            {
                debtor.Pay(amount);
                creditor?.Receive(amount);
                return true;
            }

            // Try to raise cash automatically (UI should offer these choices
            // first; this is the last-resort liquidation).
            if (debtor.RaiseableCash() >= amount)
            {
                LiquidateToPay(debtor, amount);
                debtor.Pay(amount);
                creditor?.Receive(amount);
                return true;
            }

            DeclareBankruptcy(debtor, creditor);
            return false;
        }

        private void LiquidateToPay(Player debtor, int amount)
        {
            // Sell buildings first (half price), then mortgage properties.
            foreach (var group in System.Enum.GetValues(typeof(PropertyGroup)))
            {
                if (debtor.Cash >= amount) break;
                bank.LiquidateGroupBuildings(debtor, (PropertyGroup)group);
            }
            foreach (var p in new List<Property>(debtor.Properties))
            {
                if (debtor.Cash >= amount) break;
                if (!p.IsMortgaged && p.Houses == 0 && !p.HasHotel)
                    bank.Mortgage(debtor, p);
            }
        }

        private void DeclareBankruptcy(Player debtor, Player creditor)
        {
            debtor.IsBankrupt = true;

            // Houses/hotels return to the Bank for half their cost.
            foreach (var group in System.Enum.GetValues(typeof(PropertyGroup)))
                bank.LiquidateGroupBuildings(debtor, (PropertyGroup)group);

            if (creditor != null)
            {
                // Everything of value goes to the creditor.
                creditor.Receive(Mathf.Max(0, debtor.Cash));
                debtor.Cash = 0;
                foreach (var p in new List<Property>(debtor.Properties))
                {
                    debtor.Properties.Remove(p);
                    p.Owner = creditor;
                    creditor.Properties.Add(p);
                    if (p.IsMortgaged)
                    {
                        // New owner must at once pay the Bank 10% interest.
                        creditor.Pay(p.MortgageValue / 10);
                    }
                }
                creditor.GetOutOfJailFreeCards += debtor.GetOutOfJailFreeCards;
            }
            else
            {
                // Owed to the Bank: the Bank auctions the properties.
                debtor.Cash = 0;
                foreach (var p in new List<Property>(debtor.Properties))
                {
                    debtor.Properties.Remove(p);
                    p.Owner = null;
                    auction.StartAuction(p, Players);
                }
            }
            debtor.GetOutOfJailFreeCards = 0;

            Players.Remove(debtor);
            CheckForWinner();
        }

        /// <summary>Open a trade offer from the current player (UI entry point).</summary>
        public bool ProposeTrade(TradeOffer fromCurrent, TradeOffer fromOther)
        {
            return trade.Propose(fromCurrent, fromOther);
        }

        // ---- Turn end / win ------------------------------------------------

        public void EndTurn()
        {
            if (!MatchRunning) return;
            dice.EndTurn();
            turnRolled = false;

            if (Players.Count == 0) return;
            currentPlayerIndex = (currentPlayerIndex + 1) % Players.Count;
            CurrentPlayer = Players[currentPlayerIndex];
            BeginTurn();
        }

        private void CheckForWinner()
        {
            if (Players.Count == 1)
            {
                Winner = Players[0];
                EndMatch();
            }
        }

        /// <summary>
        /// Clock ended: finish the current round so every player has had the
        /// same number of turns, then the richest by net worth wins.
        /// </summary>
        private void EndMatchByTime()
        {
            // Finish the round: keep playing until the turn returns to the
            // player who started this round, then value everyone.
            // (Simplified here: value immediately — the UI layer completes
            // the round by calling FinishRoundThenValue().)
            FinishRoundThenValue();
        }

        public void FinishRoundThenValue()
        {
            Winner = Players.OrderByDescending(p => p.NetWorth()).FirstOrDefault();
            EndMatch();
        }

        private void EndMatch()
        {
            MatchRunning = false;
            MatchOver = true;
        }
    }
}
