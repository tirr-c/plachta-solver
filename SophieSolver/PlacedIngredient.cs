using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SophieSolver
{
    enum ShapeStatus
    {
        None = 0,
        VerticallyFlipped,
        HorizontallyFlipped,
        RotatedOnce,
        RotatedTwice,
        RotatedThreeTimes
    }

    class PlacedIngredient : Ingredient
    {
        protected int row, col;
        protected ShapeStatus status;

        public int Row { get { return row; } }
        public int Col { get { return col; } }
        public ShapeStatus Status { get { return status; } }

        public PlacedIngredient(Ingredient item, int row, int col, ShapeStatus status)
            : base(item)
        {
            this.row = row;
            this.col = col;
            this.shape = Transform(this.shape, status);
            this.status = status;
        }

        public bool IsOverlapped(PlacedIngredient item)
        {
            int cntrow, cntcol;
            int lrowstart, rrowstart;
            int lcolstart, rcolstart;
            cntrow = row - item.row;
            cntcol = col - item.col;
            if (cntrow < 0) cntrow = -cntrow;
            if (cntcol < 0) cntcol = -cntcol;
            if (cntrow >= 3 || cntcol >= 3) return false;
            cntrow = 3 - cntrow;
            cntcol = 3 - cntcol;

            if (row < item.row)
            {
                lrowstart = 3 - cntrow;
                rrowstart = 0;
            }
            else
            {
                lrowstart = 0;
                rrowstart = 3 - cntrow;
            }
            if (col < item.col)
            {
                lcolstart = 3 - cntcol;
                rcolstart = 0;
            }
            else
            {
                lcolstart = 0;
                rcolstart = 3 - cntcol;
            }

            for (int i = 0; i < cntrow; i++)
            {
                for (int j = 0; j < cntcol; j++)
                {
                    if (shape[lrowstart + i, lcolstart + j] && item.shape[rrowstart + i, rcolstart + j]) return true;
                }
            }
            return false;
        }

        public bool IsPlacedAt(int row, int col)
        {
            row -= this.row;
            col -= this.col;
            if (row < 0 || row >= 3 || col < 0 || col >= 3) return false;
            return shape[row, col];
        }

        public static bool[,] Transform(bool[,] shape, ShapeStatus status)
        {
            bool[,] ret = new bool[3, 3];
            int cntrow = 0, cntcol = 0;
            for (int i = 0; i < 3; i++)
            {
                bool isrow = false;
                bool iscol = false;
                for (int j = 0; j < 3; j++)
                {
                    isrow = isrow || shape[i, j];
                    iscol = iscol || shape[j, i];
                }
                if (isrow) cntrow = i;
                if (iscol) cntcol = i;
            }
            switch (status)
            {
                case ShapeStatus.None:
                    for (int i = 0; i <= cntrow; i++)
                        for (int j = 0; j <= cntcol; j++)
                            ret[i, j] = shape[i, j];
                    break;
                case ShapeStatus.VerticallyFlipped:
                    for (int i = 0; i <= cntrow; i++)
                        for (int j = 0; j <= cntcol; j++)
                            ret[cntrow - i, j] = shape[i, j];
                    break;
                case ShapeStatus.HorizontallyFlipped:
                    for (int i = 0; i <= cntrow; i++)
                        for (int j = 0; j <= cntcol; j++)
                            ret[i, cntcol - j] = shape[i, j];
                    break;
                case ShapeStatus.RotatedOnce:
                    for (int i = 0; i <= cntrow; i++)
                        for (int j = 0; j <= cntcol; j++)
                            ret[j, cntrow - i] = shape[i, j];
                    break;
                case ShapeStatus.RotatedTwice:
                    for (int i = 0; i <= cntrow; i++)
                        for (int j = 0; j <= cntcol; j++)
                            ret[cntrow - i, cntcol - j] = shape[i, j];
                    break;
                case ShapeStatus.RotatedThreeTimes:
                    for (int i = 0; i <= cntrow; i++)
                        for (int j = 0; j <= cntcol; j++)
                            ret[cntcol - j, i] = shape[i, j];
                    break;
            }
            return ret;
        }
    }
}
