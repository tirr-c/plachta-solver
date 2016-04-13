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
            int[] colorBonus = new int[5];
            int sameColorCount = 0;
            foreach (var bonus in bonusList)
            {
                colorBonus[bonus.Item1] += bonusTable[bonus.Item2] * 2;
                if (bonus.Item1 == item.Color)
                {
                    colorBonus[bonus.Item1] += bonus.Item2 * 2;
                    sameColorCount++;
                }
            }
            colorBonus[item.Color] += sameColorCount;
            colorBonus[potColor] = colorBonus[potColor] * 3 / 2;
            int ret = item.Value * 2;
            for (int i = 0; i < 5; i++)
            {
                ret += colorBonus[i];
            }
            return ret / 2;
        }
    }
}
