using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace SophieSolver
{
    class Program
    {
        private static ShapeStatus[][] StatusAvailable = new ShapeStatus[][]
        {
            new ShapeStatus[] { ShapeStatus.None },
            new ShapeStatus[] { ShapeStatus.None, ShapeStatus.VerticallyFlipped },
            new ShapeStatus[] { ShapeStatus.None, ShapeStatus.HorizontallyFlipped },
            new ShapeStatus[] { ShapeStatus.None, ShapeStatus.RotatedOnce, ShapeStatus.RotatedTwice, ShapeStatus.RotatedThreeTimes },
        };

        static void Main(string[] args)
        {
            Stopwatch watch = new Stopwatch();
            watch.Reset();

            TextReader input = Console.In;
            bool inputRedirected = false;
            if (args.Length >= 1)
            {
                input = new StreamReader(args[0], Encoding.UTF8);
                inputRedirected = true;
            }

            Console.Write("Initializing with input... ");
            watch.Start();
            int size = int.Parse(input.ReadLine());
            string[] shape, element, bonus;
            shape = new string[size];
            for (int i = 0; i < size; i++)
                shape[i] = input.ReadLine();
            element = new string[size];
            for (int i = 0; i < size; i++)
                element[i] = input.ReadLine();
            bonus = new string[size];
            for (int i = 0; i < size; i++)
                bonus[i] = input.ReadLine();
            int stateLevel = int.Parse(input.ReadLine());
            Grid grid = new Grid(size, shape, element, bonus, new Kiln.GrandmaKiln(1));

            int cntIngredient = int.Parse(input.ReadLine());
            Ingredient[] ingredients = new Ingredient[cntIngredient];
            for (int i = 0; i < cntIngredient; i++)
            {
                string itemName = input.ReadLine();
                string[] props = input.ReadLine().Split(' ');
                int itemCategory = int.Parse(props[0]);
                int itemColor = int.Parse(props[1]);
                int itemValue = int.Parse(props[2]);
                string[] itemShape = new string[3];
                for (int j = 0; j < 3; j++)
                    itemShape[j] = input.ReadLine();
                ingredients[i] = new Ingredient(itemName, itemShape, itemColor, itemValue, itemCategory);
            }
            if (inputRedirected) input.Close();
            watch.Stop();
            Console.WriteLine("{0}ms", watch.ElapsedMilliseconds);
            watch.Reset();

            Console.Write("Testing placements... ");
            watch.Start();
            var q = from item in ingredients
                    select TestPlacement(grid, item, stateLevel);
            var validPlacement = Task.WhenAll(q.ToArray()).Result;
            watch.Stop();
            Console.WriteLine("{0}ms", watch.ElapsedMilliseconds);
            watch.Reset();

            Console.Write("Placing... ");
            watch.Start();
            var range = new List<int>();
            for (int i = 0; i < ingredients.Length; i++)
            {
                range.Add(i);
            }
            var taskPlace = from i in range
                            select Task.Run(() =>
                            {
                                return Place(grid, validPlacement, i);
                            });
            Grid optimal = null;
            foreach (var g in Task.WhenAll(taskPlace).Result)
            {
                if (optimal == null) optimal = g;
                else
                {
                    if (CompareGrids(optimal, g) < 0) optimal = g;
                }
            }
            watch.Stop();
            Console.WriteLine("{0}ms", watch.ElapsedMilliseconds);
            watch.Reset();

            Console.WriteLine("Solution has the category value of:");
            Console.WriteLine(optimal);
            Console.WriteLine("Placement order:");
            for (int i = 0; i < optimal.Ingredients.Count; i++)
            {
                var item = optimal.Ingredients[i];
                Console.WriteLine("  {0}: {1} at ({2}, {3}) {4}", i, item.Name, item.Row, item.Col, item.Status);
            }
        }

        private static async Task<List<PlacedIngredient>> TestPlacement(Grid grid, Ingredient item, int available)
        {
            return await Task.Run(() =>
            {
                int size = grid.Size;
                List<PlacedIngredient> ret = new List<PlacedIngredient>();
                for (int i = 0; i < size; i++)
                    for (int j = 0; j < size; j++)
                    {
                        var list = item.GetPlacementToCheck(StatusAvailable[available]);
                        foreach (var status in list)
                            if (grid.IsPlaceable(item, i, j, status))
                                ret.Add(new PlacedIngredient(item, i, j, status));
                    }
                return ret;
            });
        }

        private static Grid Place(Grid grid, List<PlacedIngredient>[] list, int startWith)
        {
            bool[] chkItem = new bool[list.Length];
            var l = new List<int>();
            chkItem[startWith] = true;
            l.Add(startWith);
            return BuildOrderAndPlace(grid, list, chkItem, l);
        }

        private static Grid BuildOrderAndPlace(Grid grid, List<PlacedIngredient>[] list, bool[] chk, List<int> order)
        {
            if (order.Count >= list.Length)
            {
                var newlist = new List<PlacedIngredient>[order.Count];
                for (int i = 0; i < order.Count; i++)
                {
                    newlist[i] = list[order[i]];
                }
                var r = SelectShapeAndPlace(grid, newlist, new List<int>());
                Console.WriteLine(r);
                return r;
            }
            Grid g = null;
            for (int i = 0; i < list.Length; i++)
            {
                if (chk[i]) continue;
                chk[i] = true;
                order.Add(i);
                var ret = BuildOrderAndPlace(grid, list, chk, order);
                if (g == null) g = ret;
                else
                {
                    if (CompareGrids(g, ret) < 0) g = ret;
                }
                order.RemoveAt(order.Count - 1);
                chk[i] = false;
            }
            return g;
        }

        private static Grid SelectShapeAndPlace(Grid grid, List<PlacedIngredient>[] list, List<int> selected)
        {
            int current = selected.Count;
            if (current >= list.Length)
            {
                var order = new List<PlacedIngredient>();
                for (int i = 0; i < selected.Count; i++)
                {
                    order.Add(list[i][selected[i]]);
                }
                return PlaceAsTold(grid.Clone() as Grid, order);
            }
            var ingredients = list[current];
            Grid g = null;
            for (int i = 0; i < ingredients.Count; i++)
            {
                selected.Add(i);
                var ret = SelectShapeAndPlace(grid, list, selected);
                if (g == null) g = ret;
                else
                {
                    if (CompareGrids(g, ret) < 0) g = ret;
                }
                selected.RemoveAt(selected.Count - 1);
            }
            return g;
        }

        private static Grid PlaceAsTold(Grid grid, List<PlacedIngredient> order)
        {
            foreach (var item in order)
            {
                grid.Place(item);
            }
            grid.FinalizeValue();
            return grid;
        }

        private static int CompareGrids(Grid lhs, Grid rhs)
        {
            return lhs.CategoryValue[0] - rhs.CategoryValue[0];
        }
    }
}
