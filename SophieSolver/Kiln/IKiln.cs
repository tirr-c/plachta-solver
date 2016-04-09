using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SophieSolver.Kiln
{
    interface IKiln
    {
        int CalculateBonus(PlacedIngredient item, List<Tuple<int, int>> bonusList);
    }
}
