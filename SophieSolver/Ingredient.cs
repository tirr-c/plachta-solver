using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SophieSolver
{
    class Ingredient
    {
        protected bool[,] shape;
        protected int color;
        protected int value;
        protected int category;
        private string name;
        private int count;
        public string Name { get { return name; } }
        public bool[,] Shape { get { return shape; } }
        public int Color { get { return color; } }
        public int Value { get { return value; } }
        public int Category { get { return category; } }
        public int CellCount { get { return count; } }

        protected Ingredient(Ingredient item)
        {
            this.shape = new bool[3, 3];
            for (int i = 0; i < 3; i++) for (int j = 0; j < 3; j++) this.shape[i, j] = item.shape[i, j];
            this.color = item.color;
            this.value = item.value;
            this.name = item.name;
            this.category = item.category;
            this.count = item.count;
        }

        public Ingredient(string name, string[] shape, int color, int value, int category)
        {
            this.name = name;
            this.shape = new bool[3, 3];
            this.color = color;
            this.value = value;
            this.category = category;
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                {
                    this.shape[i, j] = (shape[i][j] == '*');
                    count += this.shape[i, j] ? 1 : 0;
                }
        }

        public List<ShapeStatus> GetPlacementToCheck(ShapeStatus[] statusToTest)
        {
            var ret = new List<ShapeStatus>();
            var placements = new bool[statusToTest.Length][,];
            int cnt = 0;
            foreach (var status in statusToTest)
            {
                placements[cnt++] = PlacedIngredient.Transform(shape, status);
            }
            for (int i = cnt - 1; i >= 0; i--)
            {
                bool masterSucceeded = true;
                bool succeeded = true;
                for (int j = i - 1; j >= 0; j--)
                {
                    succeeded = false;
                    for (int r = 0; r < 3; r++)
                    {
                        for (int c = 0; c < 3; c++)
                            if (placements[i][r, c] != placements[j][r, c])
                            {
                                succeeded = true;
                                break;
                            }
                        if (succeeded) break;
                    }
                    if (!succeeded)
                    {
                        masterSucceeded = false;
                        break;
                    }
                }
                if (masterSucceeded)
                {
                    ShapeStatus status = statusToTest[i];
                    ret.Add(status);
                }
            }
            return ret;
        }

        public override string ToString()
        {
            return name;
        }
    }
}
