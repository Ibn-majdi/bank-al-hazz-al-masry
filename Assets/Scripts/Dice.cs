using UnityEngine;

namespace BankAlHazz
{
    /// <summary>
    /// Two-dice roller. The server (or the solo host) is the only authority
    /// that calls Roll() in online play — clients never roll their own dice.
    /// </summary>
    public class Dice : MonoBehaviour
    {
        public int Die1 { get; private set; }
        public int Die2 { get; private set; }
        public int Total => Die1 + Die2;
        public bool IsDoubles => Die1 == Die2 && Die1 > 0;

        /// <summary>Consecutive doubles rolled this turn (0..3).</summary>
        public int ConsecutiveDoubles { get; private set; }

        private System.Random rng = new System.Random();

        /// <summary>Roll both dice. Returns true if this was the 3rd successive double.</summary>
        public bool Roll()
        {
            Die1 = rng.Next(1, 7);
            Die2 = rng.Next(1, 7);

            if (IsDoubles)
            {
                ConsecutiveDoubles++;
            }
            else
            {
                ConsecutiveDoubles = 0;
            }

            return ConsecutiveDoubles >= 3;
        }

        /// <summary>Reset the doubles counter at the end of a player's turn.</summary>
        public void EndTurn()
        {
            ConsecutiveDoubles = 0;
            Die1 = 0;
            Die2 = 0;
        }

        /// <summary>Server-side roll with an explicit seed (reproducible / auditable).</summary>
        public bool RollWithSeed(int seed)
        {
            rng = new System.Random(seed);
            return Roll();
        }
    }
}
