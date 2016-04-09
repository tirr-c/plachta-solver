using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SophieSolver.Kiln
{
    class GrandmaKiln : IKiln
    {
        private static int[][] BonusTable = new int[][] {
            new int[] { 0, 2, 4, 6 },
            new int[] { 0, 3, 5, 7 },
        };

        private int[] bonusTable;

        public GrandmaKiln(int kilnLevel)
        {
            bonusTable = BonusTable[kilnLevel];
        }

        public int CalculateBonus(PlacedIngredient item, List<Tuple<int, int>> bonusList)
        {
            int bonusCount = 0;
            int sameColorCount = 0;
            foreach (var bonus in bonusList)
            {
                bonusCount += bonusTable[bonus.Item2];
                if (bonus.Item1 == item.Color)
                {
                    bonusCount += bonus.Item2;
                    sameColorCount++;
                }
            }
            return item.Value + bonusCount + sameColorCount / 2;
        }
    }
}
