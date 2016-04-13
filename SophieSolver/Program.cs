using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

        private static bool stop = false;
        private static int totalCells = 0;
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
            string[] gridProp = input.ReadLine().Split(' ');
            int stateLevel = int.Parse(gridProp[0]);
            int potColor = int.Parse(gridProp[1]);
            var pot = new AlchemyPot.PracticeAlchemyPot(1, potColor);
            Grid grid = new Grid(size, shape, element, bonus, pot);

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
                totalCells += ingredients[i].CellCount;
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
            for (int i = 0; i < 8; i++) range.Add(i);
            var queue = new PriorityQueue<GridQueueItem>(8);
            var resultQueue = new ConcurrentQueue<Grid>();
            var signal = new ManualResetEventSlim();
            queue.Enqueue(new GridQueueItem(grid, validPlacement));
            var taskPlace = from i in range
                            select PlaceTask(i, queue, resultQueue, signal);
            var task = Task.WhenAll(taskPlace);
            Grid optimal = null;
            while (true)
            {
                if (!signal.Wait(1 * 10 * 1000)) break;
                signal.Reset();
                while (!resultQueue.IsEmpty)
                {
                    Grid g;
                    if (!resultQueue.TryDequeue(out g)) continue;
                    if (optimal == null)
                    {
                        optimal = g;
                        Console.WriteLine("Initial optimal: {0}", optimal);
                    }
                    else if (CompareGrids(optimal, g) <= 0)
                    {
                        optimal = g;
                        Console.WriteLine("New optimal: {0}", optimal);
                    }
                }
            }
            stop = true;
            task.Wait();
            watch.Stop();
            Console.WriteLine("{0}ms", watch.ElapsedMilliseconds);
            watch.Reset();

            int quality = 0;
            for (int i = 0; i < optimal.Size; i++)
            {
                for (int j = 0; j < optimal.Size; j++)
                {
                    if (optimal.IsPlacedAt(i, j)) quality++;
                }
            }
            Console.WriteLine("Solution has the quality of {0}", quality);
            Console.WriteLine("Solution has the category value of:");
            Console.WriteLine(optimal);
            Console.WriteLine("Placement order:");
            for (int i = 0; i < optimal.Ingredients.Count; i++)
            {
                var item = optimal.Ingredients[i];
                Console.WriteLine("  {0}: {1} at ({2}, {3}) {4}", i, item.Name, item.Row, item.Col, item.Status);
            }
            Console.WriteLine("Simulation...");
            grid = new Grid(size, shape, element, bonus, pot);
            foreach (var item in optimal.Ingredients)
            {
                grid.Place(item);
                grid.UpdateCategoryValue();
                Console.WriteLine(grid);
            }
            grid.FinalizeValue();
            Console.WriteLine("==> {0}", grid);
        }

        private static async Task<Grid> PlaceTask(int id, PriorityQueue<GridQueueItem> queue, ConcurrentQueue<Grid> resultQueue, ManualResetEventSlim signal)
        {
            Grid ret = null;
            try
            {
                while (!stop)
                {
                    GridQueueItem item = await queue.BlockingDequeueAsync();
                    await Task.Yield();
                    if (item.Done)
                    {
                        Grid doneGrid = item.Grid;
                        doneGrid.FinalizeValue();
                        if (ret == null)
                        {
                            ret = doneGrid;
                            resultQueue.Enqueue(ret);
                            signal.Set();
                        }
                        else
                        {
                            if (CompareGrids(ret, doneGrid) < 0)
                            {
                                ret = doneGrid;
                                resultQueue.Enqueue(ret);
                                signal.Set();
                            }
                        }
                    }
                    else
                    {
                        item.EnqueueCandidates(queue);
                    }
                }
            }
            catch (QueueExhaustedException)
            {
            }
            return ret;
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

        private static int[][] levels = new int[][]
        {
            new int[] { 15, },
            new int[] { 30, 60, 90, },
            new int[] { 50, },
            new int[] { 40, 80, },
        };
        private static int[] targetLevel = new int[] { 1, 3, 0, 2, };
        private static bool[] critical = new bool[] { true, false, false, true };
        private static double[] coeff = new double[] { 1, 2, 0, 1 };

        private static int CompareGrids(Grid lhs, Grid rhs)
        {
            double lhssum = 0, rhssum = 0;
            bool lcritical = true, rcritical = true;
            for (int i = 0; i < 4; i++)
            {
                if (levels[i].Length == 0) continue;
                if (targetLevel[i] == 0) continue;
                int llevel;
                for (llevel = 0; llevel < levels[i].Length && levels[i][llevel] < lhs.CategoryValue[i]; llevel++)
                {
                }
                if (llevel > targetLevel[i]) llevel = targetLevel[i];
                int rlevel;
                for (rlevel = 0; rlevel < levels[i].Length && levels[i][rlevel] < rhs.CategoryValue[i]; rlevel++)
                {
                }
                if (rlevel > targetLevel[i]) rlevel = targetLevel[i];
                if (critical[i] && llevel < targetLevel[i]) lcritical = false;
                if (critical[i] && rlevel < targetLevel[i]) rcritical = false;
                lhssum += Math.Pow(2, llevel) * coeff[i] / Math.Pow(2, targetLevel[i]);
                rhssum += Math.Pow(2, rlevel) * coeff[i] / Math.Pow(2, targetLevel[i]);
            }
            lhssum += lhs.PlacedCells / (double) totalCells;
            rhssum += rhs.PlacedCells / (double) totalCells;
            lhssum -= rhssum;
            if (lcritical && !rcritical) return 1;
            else if (!lcritical && rcritical) return -1;
            if (lhssum < 0) return -1;
            else if (lhssum == 0) return 0;
            else return 1;
        }
    }
}
