using System;
using System.Windows.Forms;

namespace MiniCactpotSolver
{
    class Predictor
    {
        #region Definitions
        //Once @Init() runs, mirrors the grid of tiles from the game board.
        public int[] grid;              //grid length (9)
        //Associates an alimination yield to each grid tile by index.
        private int[] elim;             //grid length (9)
        //Associates all Minipot potential ratings to each grid tile by index.
        private int[] pots;             //grid length (9)
        //Once @Init() runs, holds the values of all uncovered tiles.
        private int[] values;           //dynamically sized (up to 9)
        //Once @Init() runs, holds the values of all covered tiles.
        private int[] covered_values;   //dynamically sized (up to 9)
        //Once @Init() runs, contains the values from @grid, grouped into the 8 different play lines.
        private int[][] row;            //row length (8)

        private int values_count = 0;
        private bool cactpot_preferred = false;
        private int cactpot_preferred_index = 0;    
        private static int gridLength = 9;
        private static int rowLength = 8;

        //Once this class is instantiated, points to the passed grid so that the caller need not manage anything.
        public System.Windows.Forms.PictureBox[] boxes;        
        public bool cactpot_possible = true;

        //Associates line payouts by line sum. Indexed at 6-24 for conveniece.
        private static int[] payouts = { 0,   0,   0,   0,   0,   0, 10000,   36,  720, 360,   80,  252, 108,
                                        72,  54, 180,  72, 180, 119,    36,  306, 1080, 144, 1800, 3600 };

        //Contains rows whose elements are the grid indices of the tiles in each respective row. The first dimension indices correspond with the indices of
        //each line's respective arrow on the board.
        private static int[][] index = { new int[]{ 0, 1, 2 }, new int[]{ 3, 4, 5 }, new int[]{ 6, 7, 8 }, new int[]{ 0, 3, 6 },
                                         new int[]{ 1, 4, 7 }, new int[]{ 2, 5, 8 }, new int[]{ 0, 4, 8 }, new int[]{ 2, 4, 6 } };


        //Tracks the relationship between each @grid tile and the group of @row's it belongs to, associated firstly by @grid index, and secondly by row index.
        private static int[][] member_index = { new int[]{ 0, 3, 6 }, new int[]{ 0, 4 },        // multi-dimensional, staggered (9)(2-4, values are rowLength)
                                                new int[]{ 0, 5, 7 }, new int[]{ 1, 3 },
                                                new int[]{ 1, 4, 6, 7 },
                                                new int[]{ 1, 5 }, new int[]{ 2, 3, 7 },
                                                new int[]{ 2, 4 }, new int[]{ 2, 5, 6 } };


        #endregion

        public Predictor(ref System.Windows.Forms.PictureBox[] t)
        {
            boxes = t;
            grid = new int[gridLength];
            elim = new int[gridLength];
            pots = new int[gridLength];
            row = new int[rowLength][];
            Init();
        }

        private void Init()
        {
            //Updates the @grid array to the most recent board values.
            for (int x = 0; x < grid.Length; x++)
                grid[x] = (((int)boxes[x].Tag) + 1);

            //Updates @row
            for (int x = 0; x < rowLength; x++)
                row[x] = new int[3] { grid[index[x][0]], grid[index[x][1]], grid[index[x][2]] };

            //Clears @elim and @pots, shifts -1 Tag values to 0, and counts uncovered tiles.
            values_count = 0;
            for (int x = 0; x < gridLength; x++)
            {
                elim[x] = 0;
                pots[x] = 0;
                if (grid[x] < 0) grid[x] = 0;
                else if (grid[x] > 0)
                    values_count++;
            }

            //Populates @values and @covered_values using @values_count.
            int face = 0;
            values = new int[values_count];
            covered_values = new int[gridLength - values_count];
            for (int x = 0, y = 0; x < gridLength; x++)
            {                
                if (grid[x] > 0)
                {
                    values[y] = grid[x];
                    face |= (int)(Math.Pow(2, grid[x]));
                    y++;
                }
            }
            covered_values = Unmask(face ^ 1022);

            //Populates @elim
            Calculate_Elimination_Yield();

            //Populates @pots
            for (int x = 0; x < gridLength; x++)
                pots[x] = Calculate_Minipot_Potential(x);

            //Set possibility flag
            cactpot_possible = IsCactpotPossible();
        }
        public void Display()
        {
            String row1 = "     Predictor grid:\n";
            row1 += grid[0] + "\t" + grid[1] + "\t" + grid[2] + "\n";
            row1 += grid[3] + "\t" + grid[4] + "\t" + grid[5] + "\n";
            row1 += grid[6] + "\t" + grid[7] + "\t" + grid[8] + "\n\n";
            String row2 = "   Elimination yield matrix:\n";
            row2 += elim[0] + "\t" + elim[1] + "\t" + elim[2] + "\n";
            row2 += elim[3] + "\t" + elim[4] + "\t" + elim[5] + "\n";
            row2 += elim[6] + "\t" + elim[7] + "\t" + elim[8] + "\n\n";
            String row3 = "    Minipot potentials:\n";
            row3 += pots[0] + "\t" + pots[1] + "\t" + pots[2] + "\n";
            row3 += pots[3] + "\t" + pots[4] + "\t" + pots[5] + "\n";
            row3 += pots[6] + "\t" + pots[7] + "\t" + pots[8];
            MessageBox.Show(row1 + "\n" + row2 + "\n" + row3);           
        }
        public void Update()
        {
            Init();
        }

        public int[] GetPrediction()
        {
            Init();

            if (values_count == 4)
            {
                #region Comments
                /* select line
                 * do this by compiling line ratings comprised of all possible payouts for the line
                 * this will need to take into account each tile in the row as if it were played as each possible number,
                 * meaning this section of code will need to manually modify @grid in order to get Calculate_Minipot_Potential() to give
                 * the correct numbers (because it will be called individually on each tile necessary).
                 * or override Calculate_Minipot_Potential to accept rows and call that.
                 * 
                 * as a cheap solution, just borrow the brute loops from Calculate_Minipot_Potential and get the sum of possible payouts
                 * for each line
                 * */
                #endregion

                #region Cheap solution
                int[] row_payout_scores = new int[rowLength];
                int[] highest_payout_indices = new int[rowLength];
                int highest_payout = 0;
                int payout_count = 0;

                for (int x = 0; x < rowLength; x++)
                {
                    row_payout_scores[x] = Calculate_Row_Payout_Sum(x);
                    if (row_payout_scores[x] > highest_payout)
                        highest_payout = row_payout_scores[x];
                }
                for (int x = 0; x < rowLength; x++)
                {
                    if (row_payout_scores[x] == highest_payout)
                    {
                        highest_payout_indices[payout_count] = x;
                        payout_count++;
                    }
                }

                int[] selection_indices = new int[payout_count];
                for (int x = 0; x < payout_count; x++)
                    selection_indices[x] = highest_payout_indices[x];

                return selection_indices;
                #endregion
            }

            int yield_count = 0;
            int candidate_count = 0;
            int highest_yield = 0;
            int highest_potential = 0;
            int[] result_indices;
            int[] highest_yield_indices = new int[gridLength];
            int[] candidate_indices = new int[gridLength];

            if (cactpot_possible && cactpot_preferred)
            {
                //Explicitly suggest to fill in a row with two uncovered cactpot numbers, disregarding highest potential.
                candidate_indices[candidate_count] = cactpot_preferred_index;
                candidate_count++;

                /*
                for (int x = 0; x < gridLength; x++)
                {
                    if (pots[x] > highest_potential)
                        highest_potential = pots[x];
                }
                for (int x = 0; x < gridLength; x++)
                {
                    if (pots[x] == highest_potential)
                    {
                        candidate_indices[candidate_count] = x;
                        candidate_count++;
                    }
                }
                */

            }            
            else
            #region Selection Loops
            {
                for (int x = 0; x < gridLength; x++)
                {
                    if (elim[x] > highest_yield)
                        highest_yield = elim[x];
                }
                for (int x = 0; x < gridLength; x++)
                {
                    if (elim[x] == highest_yield)
                    {
                        highest_yield_indices[yield_count] = x;
                        yield_count++;
                    }
                }

                for (int x = 0; x < yield_count; x++)
                {
                    if (pots[highest_yield_indices[x]] > highest_potential)
                        highest_potential = pots[highest_yield_indices[x]];
                }
                for (int x = 0; x < yield_count; x++)
                {
                    if (pots[highest_yield_indices[x]] == highest_potential)
                    {
                        candidate_indices[candidate_count] = highest_yield_indices[x];
                        candidate_count++;
                    }
                }
            }
            #endregion

            result_indices = new int[candidate_count];
            for (int x = 0; x < candidate_count; x++)
                result_indices[x] = candidate_indices[x];

            return result_indices;
        }
        public bool IsCactpotPossible()
        {
            if (values_count <= 1) return true;

            int count = 0;
            int[] x = new int[3];
            int[] y = new int[3];

            for (int i = 0; i < gridLength; i++)
            {
                if (IsCactpotNumber(grid[i]))
                {
                    GridToMatrix(i, ref x[count], ref y[count]);
                    count++;
                }
            }

            #region (count == 0)
            if (count == 0) 
            {
                cactpot_preferred = false;

                if (values_count < 3)
                    return true;

                else //values_count >= 3
                {
                    
                    int[] down_right_diagonal = row[6];
                    int[] down_left_diagonal = row[7];

                    //if non-cactpot tiles take up the entirety of either diagonal line, cactpot is impossible
                    for (int i = 0, dr_count = 0, dl_count = 0; i < 3; i++)
                    {

                        if (!IsEmpty(down_right_diagonal[i]))
                            dr_count++;
                        if (!IsEmpty(down_left_diagonal[i]))
                            dl_count++;

                        if (IsEqual(dr_count, 3) || IsEqual(dl_count, 3))
                            return false;
                    }

                    //Detect the Y-layout of 4 non-cactpot tiles.
                    if(values_count == 4)
                    {
                        if (IsEmpty(grid[4]))
                            return true;

                        int[] edge_rows = { 0, 2, 3, 5 };
                        int[] opp_rows  = { 2, 0, 5, 3 };

                        //For each edge row, check that the center tile is empty and the corners are uncovered.
                        for(int i = 0; i < 4; i++)
                        {
                            if (!IsEmpty(row[edge_rows[i]][0]) && IsEmpty(row[edge_rows[i]][1]) && !IsEmpty(row[edge_rows[i]][2]))                          
                                if (!IsEmpty(row[opp_rows[i]][1]))
                                    return false;                            
                        }
                    }
                    return true;
                }
            } //count == 0
            #endregion
            #region (count == 1)
            else if (count == 1)
            {
                cactpot_preferred = false;

                if (values_count <= 3)  //cactpot remains possible
                    return true;

                else //if the cactpot number is the only uncovered member of any row, cactpot remains possible
                {
                    for (int i = 0; i < member_index[MatrixToGrid(x[0], y[0])].Length; i++)
                    {
                        int empty_tiles = 0;
                        for (int j = 0; j < 3; j++) //count empty tiles in each row the given tile is a member of
                            if (IsEmpty(row[member_index[MatrixToGrid(x[0], y[0])][i]][j]))
                                empty_tiles++;

                        if (empty_tiles == 2) return true;
                    }
                }
            } //count == 1
            #endregion
            #region (count == 2)
            else if (count == 2)
            {
                cactpot_preferred = true;

                #region (values_count > 2)
                if (values_count > 2)
                {
                    int temp_index = 0;
                    if (IsEqual(x[0], x[1])) //same row
                        temp_index = MatrixToGrid(x[0], (3 - (y[0] + y[1])));

                    else if (IsEqual(y[0], y[1])) //same col
                        temp_index = MatrixToGrid((3 - (x[0] + x[1])), y[0]);

                    else if (IsEqual(x[0], y[0]) && IsEqual(x[1], y[1])) //both down right diagonal
                        temp_index = MatrixToGrid((3 - (x[0] + x[1])), (3 - (y[0] + y[1])));

                    else if (IsEqual(x[0], y[0], 1) || IsEqual(x[1], y[1], 1)) //either is the middle tile
                    {
                        if (HasDifferenceOfTwo(x[0], y[0]) || HasDifferenceOfTwo(x[1], y[1])) //both down left diagonal 
                        {
                            if (IsEqual(x[0], 0) || IsEqual(x[1], 0)) //tile 0 or 1 is (0, 2) so third tile must be (2, 0)                              
                                temp_index = MatrixToGrid(2, 0);

                            else if (y[0] == 0 || y[1] == 0) //tile 0 or 1 is (2, 0) so third tile must be (0, 2)
                                temp_index = MatrixToGrid(0, 2);
                        }
                    }
                    else if (HasDifferenceOfTwo(x[0], x[1]) && HasDifferenceOfTwo(y[0], y[1])) //both coordinate components have a difference of two
                    {
                        if (!IsEqual(x[0], y[0]) && !IsEqual(x[1], y[1])) //both down left diagonal, corners                                       
                            temp_index = MatrixToGrid(1, 1);
                    }
                    else if (HasDifferenceOfTwo(x[0], x[1]) || HasDifferenceOfTwo(y[0], y[1])) //only one component has a difference of two
                        return false;
                    else if (!HasDifferenceOfTwo(x[0], x[1]) && !HasDifferenceOfTwo(y[0], y[1])) //neither component has a difference of two
                        return false;
                    else
                        MessageBox.Show("Unhandled case: ICP_VC2\nProceeding with default values.");

                    if (IsEmpty(grid[temp_index]))
                    {
                        cactpot_preferred_index = temp_index;
                        return true;
                    }
                    else return false;                    
                }
                #endregion //values_count > 2
                #region values_count <= 2
                else
                {
                    if (IsEqual(x[0], y[0], 1) || IsEqual(x[1], y[1], 1)) //either is middle tile
                        return true;

                    else if (HasDifferenceOfTwo(x[0], x[1])) //x0 and x1 have a difference of two [0,2]
                    {
                        if (IsEqual(y[0], y[1])) //same column (column y[0]), with a tile in the middle (tile (1, y[0]))
                            if (IsEmpty(grid[MatrixToGrid(1, y[0])])) //if that tile is empty, cactpot is still possible
                                return true;

                            else if (IsEqual(x[0], y[0]) && IsEqual(x[1], y[1])) //tiles pass downright diagonal test
                                return true;

                            else if (HasDifferenceOfTwo(y[0], y[1])) //tiles pass down left diagonal test, middle tile is covered
                                return true;
                    }
                    else if (HasDifferenceOfTwo(y[0], y[1])) //y0 and y1 have a difference of two [0,2]
                    {
                        if (IsEqual(x[0], x[1])) //tiles are in the same row (rowx[0]), with a tile in the middle (tile (x[0], 1))
                        {
                            if (IsEmpty(grid[MatrixToGrid(x[0], 1)])) //if that tile is empty, cactpot is still possible
                                return true;
                        }

                        else if (IsEqual(x[0], y[0]) && IsEqual(x[1], y[1])) //tiles pass downright diagonal test 
                            return true;

                        else if (HasDifferenceOfTwo(x[0], x[1])) //tiles pass down left diagonal test -> already failed this test to get here, however
                            return true;
                    }
                    else //neither component pairs have a difference of 2
                    {
                        if (IsEqual(x[0], x[1]) || IsEqual(y[0], y[1])) //they are next to one another
                            return true;

                        else //they are kitty corner
                            return false;
                    }
                }
                #endregion //values_count <= 2
            }
            #endregion //count == 2
            #region (count == 3)
            else if (count == 3)
            {
                cactpot_preferred = false;

                if (IsEqual(x[0], x[1], x[2])) //all same row
                    return true;
                else if (IsEqual(y[0], y[1], y[2])) //all same col
                    return true;
                else if (IsEqual(x[0], y[0]) && IsEqual(x[1], y[1]) && IsEqual(x[2], y[2])) // all down right diagonal
                    return true;

                else if (IsEqual(x[0], y[0], 1))  //check 1 for down left diagonal
                    if (HasDifferenceOfTwo(x[1], x[2]))
                        if (HasDifferenceOfTwo(y[1], y[2]))
                            return true;

                        else if (IsEqual(x[1], y[1], 1))    //check 2 for down left diagonal                    
                            if (HasDifferenceOfTwo(x[0], x[2]))
                                if (HasDifferenceOfTwo(y[0], y[2]))
                                    return true;

                                else if (IsEqual(x[2], y[2], 1))    //check 3 for down left diagonal
                                    if (HasDifferenceOfTwo(x[0], x[1]))
                                        if (HasDifferenceOfTwo(y[0], y[1]))
                                            return true;
            }
            #endregion //count == 3
            return false;
        }
        public int[] GetPayoutChances(int row_index)
        {
            int[] payout_chances = new int[25];
            int[] payout_counts = Calculate_Row_Payout_Count(row_index);
            int total_counts = 0;

            for (int x = 0; x < payout_counts.Length; x++)
                total_counts += payout_counts[x];

            for (int x = 0; x < payout_counts.Length; x++)
                payout_chances[x] = ((payout_counts[x] * 100) / total_counts);

            return payout_chances;
        }

        //Yummy broot loops
        private int Calculate_Minipot_Potential(int tile)
        {
            if (grid[tile] != 0) return 0;

            int[] line_totals = new int[rowLength];                 
            
            for (int x = 0; x < member_index[tile].Length; x++)         
                line_totals[member_index[tile][x]] = Calculate_Row_Payout_Sum(member_index[tile][x]); ;

            int overall_total = 0;
            for (int x = 0; x < rowLength; x++)
                overall_total += line_totals[x];

            return overall_total;
        }
        private int[] Calculate_Row_Payout_Count(int row_index)
        {
            int[] payout_counts = new int[25];
            int line_sum = 0;
            int count = 0;

            for (int y = 0; y < 3; y++)
            {
                if (row[row_index][y] == 0)
                    count++;
                else
                    line_sum += row[row_index][y];
            }

            #region Combination Loops by Case
            if (count == 0)
            {
                payout_counts[line_sum]++;
            }
            else if (count == 1)
            {
                for (int i = 0; i < covered_values.Length; i++)
                {
                    int temp = line_sum + covered_values[i];
                    payout_counts[temp]++;
                }
            }
            else if (count == 2)
            {
                for (int i = 0; i < (covered_values.Length - 1); i++)
                {
                    for (int j = i + 1; j < covered_values.Length; j++)
                    {
                        int temp = line_sum + covered_values[i] + covered_values[j];
                        payout_counts[temp]++;
                    }
                }
            }
            else if (count == 3)
            {
                for (int i = 0; i < (covered_values.Length - 2); i++)
                {
                    for (int j = i + 1; j < (covered_values.Length - 1); j++)
                    {
                        for (int k = j + 1; k < covered_values.Length; k++)
                        {
                            int temp = line_sum + covered_values[i] + covered_values[j] + covered_values[k];
                            payout_counts[temp]++;
                        }
                    }
                }
            }
            #endregion

            return payout_counts;
        }
        private int Calculate_Row_Payout_Sum(int row_index)
        {
            int[] payout_counts = Calculate_Row_Payout_Count(row_index);

            int total = 0;
            for (int i = 6; i < payout_counts.Length; i++)
                total += (payouts[i] * payout_counts[i]);

            return total;
        }
        private void Calculate_Elimination_Yield()
        {
            for (int x = 0; x < rowLength; x++)
            {
                if (IsTransparent(row[x][0]) && IsTransparent(row[x][1]) && IsTransparent(row[x][2]))
                {
                    elim[index[x][0]]++;
                    elim[index[x][1]]++;
                    elim[index[x][2]]++;
                }
                for (int y = 0; y < 3; y++)
                    if (!IsEmpty(row[x][y])) elim[index[x][y]] = 0;
            }
        }


        private bool IsEmpty(int x)
        {
            if (x == 0) return true;
            else return false;
        }
        private bool IsEqual(int x, int y)
        {
            if (x == y) return true;
            else return false;
        }
        private bool IsEqual(int x, int y, int z)
        {
            if (x == y && y == z) return true;
            else return false;
        }
        private bool IsTransparent(int x)
        {
            if (x >= 0 && x <= 3) return true;
            else return false;
        }
        private bool IsCactpotNumber(int x)
        {
            if (x == 1 || x == 2 || x == 3) return true;
            else return false;
        }
        private int[] Unmask(int mask)
        {
            int temp = 0;
            int count = 0;
            int[] ret = new int[gridLength - values_count];

            for (int x = 0; (mask > 0) && (x <= 10); x++)
            {
                temp = ((int)Math.Pow(2, x));
                if ((mask & temp) == temp)
                {
                    ret[count] = x;
                    mask -= temp;
                    count++;
                }
            }
            return ret;
        }

        /* Description
         * 
         * Accepts an integer and two integer pointers. Converts the integer @grid_index, which is an index
         * to a 1-dimensional array (@grid), into the matrix coordinate pair that corresponds to that index
         * in a 3-column matrix. The resulting coordinate pair is stored in @matrix_x and @matrix_y. This
         * function assumes in all cases that the array index provided is in the range of 0 to 8, and that
         * the resulting matrix coordinates are in the range of [0, 0] to [2, 2] (a 3x3 matrix). 
         * 
         * Returns @grid_index.
         * */
        private int GridToMatrix(int grid_index, ref int matrix_x, ref int matrix_y)
        {
            matrix_x = grid_index / 3;
            matrix_y = grid_index % 3;
            return grid_index;
        }

        /* Description
         * 
         * Accepts two integers representing a matrix coordinate pair and computes a single
         * array index by reversing the process in GridToMatrix(). This function assumes in all
         * cases that matrix coordinates range from [0, 0] to [2, 2] (a 3x3 matrix) and that the
         * resulting array index will be in the range of 0 to 8.
         * 
         * Returns the resulting array index.
         * */
        private int MatrixToGrid(int matrix_x, int matrix_y)
        {
            return (matrix_x * 3) + matrix_y;
        }

        /* Description
         * 
         * Accepts two integers and checks the difference between them. This is useful for using on 3x3 matrix
         * components because, if the components are from two separate elements and represent the same dimension, a
         * true value indicates that these elements are on opposite sides of the matrix (along the opposite edges)
         * across the tested dimension.
         * 
         * Returns true if the difference is 2, and false otherwise.
         * */
        private bool HasDifferenceOfTwo(int x, int y)
        {
            if (x > y)
                if (x - y == 2) return true;
                else return false;
            else
                if (y - x == 2) return true;
                else return false;
        }
    }
}
