using System.Collections.Generic;
using UnityEngine;

namespace BankAlHazz
{
    /// <summary>Host settings chosen before the match starts.</summary>
    [System.Serializable]
    public class RoomSettings
    {
        public int BoardSize = GameConfig.ClassicBoardSize;
        public int MatchLengthMinutes = 60; // 0 = classic open (no clock)
        public float TurnTimerSeconds = GameConfig.TurnTimerClassicSeconds;
        public int StartingCash = GameConfig.StartingCash;
        public int GoSalary = GameConfig.GoSalary;
        public bool AuctionsEnabled = true;
        public bool HumanBankerSeat = false;
    }

    /// <summary>
    /// Online room: create / join by code or invite link, host settings,
    /// seats (up to 4 players + 1 optional human banker), spectators.
    /// This class owns the LOBBY. The authoritative match state lives on
    /// the server (GameManager + Bank); the human banker seat is an
    /// auctioneer/announcer only and never touches the ledger.
    /// Transport note: plug a real transport (Unity Netcode / Photon / Mirror)
    /// behind IRoomTransport; the lobby logic below is transport-agnostic.
    /// </summary>
    public class NetworkRoom : MonoBehaviour
    {
        public string RoomCode { get; private set; }
        public string InviteLink { get; private set; }
        public RoomSettings Settings { get; private set; } = new RoomSettings();
        public bool IsHost { get; private set; }

        public readonly List<Player> SeatedPlayers = new List<Player>();
        public Player HumanBanker;
        public readonly List<Player> Spectators = new List<Player>();

        public event System.Action<Player> OnPlayerJoined;
        public event System.Action<Player> OnPlayerLeft;
        public event System.Action OnMatchStarted;

        private readonly System.Random rng = new System.Random();

        /// <summary>Create a room. Returns the 6-character join code.</summary>
        public string CreateRoom(Player host, RoomSettings settings)
        {
            Settings = settings;
            IsHost = true;
            RoomCode = GenerateCode();
            InviteLink = $"bankalhazz://room/{RoomCode}";
            SeatedPlayers.Clear();
            SeatedPlayers.Add(host);
            return RoomCode;
        }

        /// <summary>Join by code. Returns false if the room is full or the code is wrong.</summary>
        public bool JoinByCode(string code, Player player)
        {
            if (code != RoomCode) return false;
            if (SeatedPlayers.Count >= GameConfig.MaxPlayingSeats) return false;
            SeatedPlayers.Add(player);
            OnPlayerJoined?.Invoke(player);
            return true;
        }

        public bool JoinAsBanker(Player player)
        {
            if (HumanBanker != null) return false;
            if (!Settings.HumanBankerSeat) return false;
            HumanBanker = player;
            player.IsBankerSeat = true;
            OnPlayerJoined?.Invoke(player);
            return true;
        }

        public void JoinAsSpectator(Player player)
        {
            Spectators.Add(player);
        }

        public void Leave(Player player)
        {
            SeatedPlayers.Remove(player);
            Spectators.Remove(player);
            if (HumanBanker == player) HumanBanker = null;
            OnPlayerLeft?.Invoke(player);
        }

        /// <summary>
        /// Disconnect handling: the seat is held for rejoin; if the player
        /// does not return, an AI of the same difficulty takes the seat so
        /// the match can finish. Handled by GameManager + AIManager.
        /// </summary>
        public bool CanRejoin(Player player)
        {
            return SeatedPlayers.Contains(player) || HumanBanker == player;
        }

        public bool CanStartMatch()
        {
            return SeatedPlayers.Count >= GameConfig.MinPlayingSeats;
        }

        public void StartMatch()
        {
            if (!CanStartMatch()) return;
            OnMatchStarted?.Invoke();
        }

        private string GenerateCode()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // no 0/O/1/I
            var code = new char[6];
            for (int i = 0; i < 6; i++) code[i] = chars[rng.Next(chars.Length)];
            return new string(code);
        }
    }
}
