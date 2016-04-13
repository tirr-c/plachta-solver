using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SophieSolver
{
    class Grid : ICloneable
    {
        private int size;
        private bool[,] shape;
        private int[,] color;
        private int[,] bonusLevel;
        private Dictionary<int, int>[] values;
        private int[] categoryValue;

        private int gridCount;

        private List<PlacedIngredient> ingredients;
        private List<bool> ingredientShadowed;

        private AlchemyPot.IAlchemyPot pot;

        public int Size { get { return size; } }
        public int[] CategoryValue { get { return categoryValue; } }

        public List<PlacedIngredient> Ingredients { get { return ingredients; } }

        public int Potential
        {
            get
            {
                int ret = PlacedCells;
                for (int i = 0; i < size; i++)
                    for (int j = 0; j < size; j++)
                        ret += bonusLevel[i, j];
                return ret;
            }
        }
        private int placedCells;
        public int PlacedCells
        {
            get
            {
                return placedCells;
            }
        }

        private Grid(int size)
        {
            this.size = size;
            this.shape = new bool[size, size];
            this.color = new int[size, size];
            this.bonusLevel = new int[size, size];
            this.values = new Dictionary<int, int>[4];
            for (int i = 0; i < 4; i++) this.values[i] = new Dictionary<int, int>();
            this.categoryValue = new int[4];
            this.gridCount = 0;
            this.placedCells = 0;
            ingredients = new List<PlacedIngredient>();
            ingredientShadowed = new List<bool>();
        }

        public Grid(int size, string[] shape, string[] element, string[] startBonusLevel, AlchemyPot.IAlchemyPot pot)
        {
            this.size = size;
            this.shape = new bool[size, size];
            this.color = new int[size, size];
            this.bonusLevel = new int[size, size];
            this.values = new Dictionary<int, int>[4];
            for (int i = 0; i < 4; i++) this.values[i] = new Dictionary<int, int>();
            this.categoryValue = new int[4];
            this.gridCount = 0;
            this.placedCells = 0;
            this.pot = pot;

            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                {
                    this.shape[i, j] = (shape[i][j] != 'x');
                    this.color[i, j] = (element[i][j] - '0');
                    this.bonusLevel[i, j] = (startBonusLevel[i][j] - '0');
                    if (this.shape[i, j]) this.gridCount++;
                }
            ingredients = new List<PlacedIngredient>();
            ingredientShadowed = new List<bool>();
        }

        public bool IsPlaceable(Ingredient item, int row, int col, ShapeStatus status)
        {
            bool[,] temp = PlacedIngredient.Transform(item.Shape, status);
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                {
                    if (i + row < 0 || j + col < 0 || i + row >= size || j + col >= size)
                    {
                        if (temp[i, j]) return false;
                        continue;
                    }
                    if (!shape[i + row, j + col] && temp[i, j]) return false;
                }
            return true;
        }

        public bool IsPlacedAt(int row, int col)
        {
            for (int i = 0; i < ingredients.Count; i++)
            {
                if (ingredientShadowed[i]) continue;
                if (ingredients[i].IsPlacedAt(row, col)) return true;
            }
            return false;
        }

        public void Place(PlacedIngredient item)
        {
            // Calculate value
            var bonusList = new List<Tuple<int, int>>();
            for (int i = 0; i < 3; i++)
            {
                int nrow = item.Row + i;
                for (int j = 0; j < 3; j++)
                {
                    int ncol = item.Col + j;
                    if (item.Shape[i, j] && bonusLevel[nrow, ncol] > 0)
                    {
                        bonusList.Add(new Tuple<int, int>(color[nrow, ncol], bonusLevel[nrow, ncol]));
                        bonusLevel[nrow, ncol] = 0;
                    }
                }
            }
            if (!values[item.Category].ContainsKey(item.Color)) values[item.Category][item.Color] = 0;
            values[item.Category][item.Color] += pot.CalculateBonus(item, bonusList);
            // Update bonus
            bool[,] chk = new bool[size, size];
            bool[,] mask = new bool[size, size];
            for (int i = 0; i < 3; i++)
            {
                int nrow = item.Row + i;
                for (int j = 0; j < 3; j++)
                {
                    int ncol = item.Col + j;
                    if (item.Shape[i, j])
                    {
                        mask[nrow, ncol] = true;
                        for (int r = -1; r <= 1; r++)
                        {
                            for (int c = -1; c <= 1; c++)
                            {
                                if (r == 0 && c == 0) continue;
                                if (nrow + r < 0 || nrow + r >= size) continue;
                                if (ncol + c < 0 || ncol + c >= size) continue;
                                chk[nrow + r, ncol + c] = true;
                            }
                        }
                    }
                }
            }
            // TODO: Can't optimize this?
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    chk[i, j] ^= mask[i, j];
                }
            }
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if (chk[i, j] && shape[i, j] && !IsPlacedAt(i, j))
                    {
                        bonusLevel[i, j]++;
                        if (bonusLevel[i, j] > 3) bonusLevel[i, j] = 3;
                    }
                }
            }
            // Place
            for (int i = 0; i < ingredients.Count; i++)
            {
                if (ingredientShadowed[i]) continue;
                if (item.IsOverlapped(ingredients[i]))
                {
                    placedCells -= ingredients[i].CellCount;
                    ingredientShadowed[i] = true;
                }
            }
            ingredients.Add(item);
            ingredientShadowed.Add(false);
            placedCells += item.CellCount;
        }

        public void FinalizeValue()
        {
            int[] colorCount = new int[5];
            for (int i = 0; i < ingredients.Count; i++)
            {
                if (ingredientShadowed[i]) continue;
                var item = ingredients[i];
                int c = 0;
                for (int row = 0; row < 3; row++)
                {
                    for (int col = 0; col < 3; col++)
                    {
                        if (item.Shape[row, col]) c++;
                    }
                }
                colorCount[item.Color] += c;
            }
            int max = -1;
            List<int> maxColors = new List<int>();
            for (int i = 0; i < 5; i++)
            {
                if (max < colorCount[i])
                {
                    maxColors.Clear();
                    max = colorCount[i];
                }
                if (max == colorCount[i])
                {
                    maxColors.Add(i);
                }
            }
            foreach (int c in maxColors)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (!values[i].ContainsKey(c)) continue;
                    int v = values[i][c] * (gridCount + max);
                    v /= gridCount;
                    values[i][c] = v;
                }
            }
            UpdateCategoryValue();
        }

        public void UpdateCategoryValue()
        {
            for (int i = 0; i < 4; i++)
            {
                categoryValue[i] = 0;
                foreach (var item in values[i])
                {
                    categoryValue[i] += item.Value;
                }
            }
        }

        public object Clone()
        {
            Grid ret = new Grid(size);
            ret.shape = shape; // does not change
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    ret.color[i, j] = color[i, j];
                    ret.bonusLevel[i, j] = bonusLevel[i, j];
                }
            }
            for (int i = 0; i < 4; i++)
            {
                ret.categoryValue[i] = categoryValue[i];
                ret.values[i] = new Dictionary<int, int>(values[i]);
            }
            ret.gridCount = gridCount;
            ret.placedCells = placedCells;
            ret.pot = pot;
            ret.ingredients.AddRange(ingredients);
            ret.ingredientShadowed.AddRange(ingredientShadowed);
            return ret;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < 4; i++)
            {
                builder.AppendFormat("  {0}: {1}", i, categoryValue[i]);
            }
            return builder.ToString();
        }
    }
}
