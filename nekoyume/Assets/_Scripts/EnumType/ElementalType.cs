using System;
using System.Collections.Generic;

namespace Nekoyume.EnumType
{
    public enum ElementalType
    {
        Normal,
        Fire,
        Water,
        Land,
        Wind,
    }

    public enum ElementalResult
    {
        Win,
        Draw,
        Lose,
    }

    public class ElementalTypeComparer : IEqualityComparer<ElementalType>
    {
        public static readonly ElementalTypeComparer Instance = new ElementalTypeComparer();

        public bool Equals(ElementalType x, ElementalType y)
        {
            return x == y;
        }

        public int GetHashCode(ElementalType obj)
        {
            return (int) obj;
        }
    }

    public static class ElementalTypeExtension
    {
        public const decimal Multiplier = .5m;
        public static bool TryGetWinCase(this ElementalType win, out ElementalType lose)
        {
            switch (win)
            {
                case ElementalType.Normal:
                    lose = ElementalType.Normal;
                    return false;
                case ElementalType.Fire:
                    lose = ElementalType.Wind;
                    return true;
                case ElementalType.Water:
                    lose = ElementalType.Fire;
                    return true;
                case ElementalType.Land:
                    lose = ElementalType.Water;
                    return true;
                case ElementalType.Wind:
                    lose = ElementalType.Land;
                    return true;
                default:
                    throw new ArgumentOutOfRangeException(nameof(win), win, null);
            }
        }

        public static bool TryGetLoseCase(this ElementalType lose, out ElementalType win)
        {
            switch (lose)
            {
                case ElementalType.Normal:
                    win = ElementalType.Normal;
                    return false;
                case ElementalType.Fire:
                    win = ElementalType.Water;
                    return true;
                case ElementalType.Water:
                    win = ElementalType.Land;
                    return true;
                case ElementalType.Land:
                    win = ElementalType.Wind;
                    return true;
                case ElementalType.Wind:
                    win = ElementalType.Fire;
                    return true;
                default:
                    throw new ArgumentOutOfRangeException(nameof(lose), lose, null);
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns>
        /// 1: Win
        /// 0: Draw
        /// -1: Lose
        /// </returns>
        public static ElementalResult GetBattleResult(this ElementalType from, ElementalType to)
        {
            if (from == ElementalType.Normal)
                return ElementalResult.Draw;

            if (from == to)
                return ElementalResult.Draw;

            if (from.TryGetWinCase(out var lose) &&
                lose == to)
                return ElementalResult.Win;

            if (from.TryGetLoseCase(out var win) &&
                win == to)
                return ElementalResult.Lose;

            return ElementalResult.Draw;
        }

        public static int GetDamage(this ElementalType from, ElementalType to, int damage)
        {
            return Convert.ToInt32(damage * GetMultiplier(from, to));
        }

        private static decimal GetMultiplier(this ElementalType from, ElementalType to)
        {
            var battleResult = from.GetBattleResult(to);
            var multiplier = 0;
            switch (battleResult)
            {
                case ElementalResult.Win:
                    multiplier = 1;
                    break;
                case ElementalResult.Lose:
                    multiplier = -1;
                    break;
            }

            return 1 + multiplier * Multiplier;
        }
    }
}
