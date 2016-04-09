using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SophieSolver.AlchemyPot
{
    interface IAlchemyPot
    {
        int CalculateBonus(PlacedIngredient item, List<Tuple<int, int>> bonusList);
    }
}
