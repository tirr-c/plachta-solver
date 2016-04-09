using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SophieSolver.AlchemyPot
{
    class PracticeAlchemyPot : IAlchemyPot
    {
        private static int[][] BonusTable = new int[][] {
            new int[] { 0, 2, 4, 6 },
            new int[] { 0, 3, 5, 7 },
        };

        private int[] bonusTable;
        private int potColor;

        public PracticeAlchemyPot(int potLevel, int potColor)
        {
            bonusTable = BonusTable[potLevel];
            this.potColor = potColor;
        }

        public int CalculateBonus(PlacedIngredient item, List<Tuple<int, int>> bonusList)
        {
            int bonusCount = 0;
            int potColorCount = 0;
            int sameColorCount = 0;
            foreach (var bonus in bonusList)
            {
                int currentBonus = bonusTable[bonus.Item2];
                if (bonus.Item1 == item.Color)
                {
                    currentBonus += bonus.Item2;
                    sameColorCount++;
                }
                if (bonus.Item2 == potColor)
                {
                    potColorCount += currentBonus;
                }
                else
                {
                    bonusCount += currentBonus;
                }
            }
            int ret = item.Value + bonusCount;
            if (item.Color == potColor)
            {
                ret += (2 * potColorCount + sameColorCount) * 3 / 4;
            }
            else
            {
                ret += (3 * potColorCount + sameColorCount) / 2;
            }
            return ret;
        }
    }
}
