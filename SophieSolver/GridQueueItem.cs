using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SophieSolver
{
    class GridQueueItem : IComparable<GridQueueItem>
    {
        private readonly Grid grid;
        private readonly List<PlacedIngredient>[] list;
        private readonly List<bool> check;

        private int PlacedItemCount
        {
            get
            {
                int cnt = 0;
                foreach (bool i in check) cnt++;
                return cnt;
            }
        }
        public bool Done
        {
            get
            {
                bool ret = true;
                foreach (bool i in check) ret &= i;
                return ret;
            }
        }
        public Grid Grid => grid;

        public GridQueueItem(Grid grid, List<PlacedIngredient>[] list)
        {
            this.grid = grid;
            this.list = list;
            this.check = new List<bool>(list.Length);
            for (int i = 0; i < list.Length; i++) check.Add(false);
        }
        private GridQueueItem(Grid grid, List<PlacedIngredient>[] list, List<bool> check)
        {
            this.grid = grid;
            this.list = list;
            this.check = check;
        }

        public int CompareTo(GridQueueItem other)
        {
            //int deltaCnt = PlacedItemCount - other.PlacedItemCount;
            //if (deltaCnt != 0) return -deltaCnt;
            return grid.Potential - other.grid.Potential;
        }

        public void EnqueueCandidates(PriorityQueue<GridQueueItem> q)
        {
            var ingredientToPlace = new List<int>();
            for (int i = 0; i < check.Count; i++)
            {
                if (!check[i]) ingredientToPlace.Add(i);
            }
            foreach (var i in ingredientToPlace)
            {
                var newCheck = new List<bool>(check);
                newCheck[i] = true;
                var itemList = list[i];
                foreach (var item in itemList)
                {
                    Grid newGrid = grid.Clone() as Grid;
                    newGrid.Place(item);
                    newGrid.UpdateCategoryValue();
                    q.Enqueue(new GridQueueItem(newGrid, list, newCheck));
                }
            }
        }
    }
}
