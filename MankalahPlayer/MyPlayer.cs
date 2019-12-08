using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Mankalah;

namespace Mankalah
{

    // rename me
    public class LeBrink : Player // class must be public
    {

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);


        private const int bottomGoal = 6;
        private const int topGoal = 13;
        private int maxTime;
        private int turnCount = 0;
        private CancellationTokenSource cancellationToken;
        private int depthMax;

        /* DataWrapper class
         * container for best move, value pair and second best move, second best value pair
         */
        public class DataWrapper
        {
            private int move, value, secondBestMove, secondBestValue;

            public DataWrapper(int move, int value, int secondBestMove, int secondBestValue)
            {
                this.move = move;
                this.value = value;
                this.secondBestMove = secondBestMove;
                this.secondBestValue = secondBestValue;
            }

            public int getMove()
            {
                return this.move;
            }

            public int getValue()
            {
                return this.value;
            }

            public int getSecondBestMove()
            {
                return this.secondBestMove;
            }

            public int getSecondBestValue()
            {
                return this.secondBestValue;
            }

        }

        private Position position;

        /* MyPlayer constructor
         * 
         */
        public MyPlayer(Position pos, int maxTimePerMove) // constructor must match this signature
            : base(pos, "LeBrink", maxTimePerMove) // choose a string other than "MyPlayer"
        {
            this.position = pos;
            this.maxTime = maxTimePerMove;
        }

        /* getImage
         * this returns an image of me
         */
        public override string getImage()
        {
            return "lebrink.png";
        }

        /* gloat
         * This took me like 10 minutes to make. Please help me.
         */
        public override string gloat()
        {
            return "░░░░░░░░░░░░░░░░░░░▒▓▓█████████████▓▓▒░░░░░░░░░░░░░░░░░░░░░░\n" +
                   "░░░░░░░░░░░░░░▒████▓▓▒▒░▒▒▒░▒▒▒▒▒▒▓▓████▓▒░░░░░░░░░░░░░░░░░░\n" +
                   "░░░░░░░░░░░▒███▓░░░░░░░░░░░░░░░░░░░░░░▒███████▓▓▒░░░░░░░░░░░\n" +
                   "░░░░░░░░░▒██▓░░░░▒▒███▓▓▒░░░░░░░░░░░░░▓▓▒▒▒▒▒▓██████▓░░░░░░░\n" +
                   "░░░░░░░▒██▓░░░▓███▓▒░░░░░░░░▒▒▒▒▒▒▒▒▒▒░░░░░░▒▓▓███▓████▒▒░░░\n" +
                   "░░░░░░██▓░░▒▓██▓░░▒▓██████▓░░░░░░░▒░░░░░░▒██▓▒░░░▓███▒▓▒░░░░\n" +
                   "░░░░░██░░▓███▒░░▒██▒░░░░▒▒██▓░░░░░░░░░░░██▒░░░░▒████▒█░░░░░░\n" +
                   "░░░░██░▒▓▒▓▓░░░██░░░░░░░░░░░█▓░░░░░░░░░██░░░░░░▒███░░█▒░░░░░\n" +
                   "░░░▓█░░░░░░░░░██░░░░░░░░░░░░▓█░░░░░░░░██░░░░░░░░░░░░░█▒░░░░░\n" +
                   "░░░█▓░░░░░░░░░█▒░░████░░░░░░░█▒░░░░░░░██░░░░░░░░░░░░███░░░░░\n" +
                   "░░▒█░░░░░░░▒▓▒█▓░▓████▓░░░░░▒█░░░░░░░░▒█▒░░░░░░░░░░██░█▒░░░░\n" +
                   "░░██░░░░░▒▓▒▓▒██▒▒▓▓▓░░░░░░░██░░░░░░░░░▒████▓███████▓░█▒░░░░\n" +
                   "░░█▓░░░░░▒░░░▒░▒██▓▒░░░░░▒██▓░░░░░░░░░░░░░░██▓░░░░░░▒██▓░░░░\n" +
                   "░░█░░░░░░░░░▓▒░░░░▒▓██████▓░░░░░░░░░░░░░░▒██░░░▓█▓▓▒░░░██░░░\n" +
                   "░▒█░░░░░░░░░░░░░░░░░░░░░░░░░░▓▒▓▒▒▒▒▒▓▓▓▓██░░▓█▓░▒▒██▒░░██░░\n" +
                   "░▓█░░░░░░░░░░░░░░░░░░░░░░░░░░▒▒▒▒▒▒▓▓▒░░██░░██▓░▒░▒░██░░▒█░░\n" +
                   "░██░░░░░░░░░░░░░░░░░░░░░░░▒▓▒▒▒▒▒▒▒▒░░░██░░▓█░█▓░█▒█▓█▓░░█░░\n" +
                   "░██░░░░░░░░░░░░░░░░░░░░░░░░░▒▒▒▒▒░▒▒░░▓█▓░░██░█▒▒█▒█▒▓█░░█░░\n" +
                   "░██░░░░░░░░░░░░░░░░░░░░░░░░▒░░░░░░░░░░▓█░░░█▒░░░░▒▒░░▒█░▓█░░\n" +
                   "░▒█░░░░░░░░░░░░░░░░░░░░░░░░▒▒▒▒▒▒▒▒▒▒░░█▒░░█▒░░░░░░░░▓█░█▓░░\n" +
                   "░░█▓░▒▒░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░▓█░░█▒░░░░░░░░█░▒█░░░\n" +
                   "░░▓█░░▒░░▒▒░░▒░░░░░░░░░░░░░░░░░░░░░░░░░░█░░█▒░░░░░░░█▓░█▓░░░\n" +
                   "░░░█▒░░▒░░▒░░▒▒░░░░░░░░░░░░░░░░░░░░░░░░░█░░█▒░░░░░░▓█░░█░░░░\n" +
                   "░░░██░░░▒░▒░░░▒▒░░░░░░░░░░░░░░░░░░░░░░░░█░░█▒░░░░░░██░░█░░░░\n" +
                   "░░░░█▓░░░▒░▒░░░░▒▒░░░░░▒▒▒▒▒▒░░░░░░░░░░▒█░▒█░░░░░░░█▒░░█▓░░░\n" +
                   "░░░░▓█░░░░▒▒░░░░░▒▒░░░░░░▒▒▒▓▓▓▒░░░░░░░██░██░░░░░░░██░░▓█░░░\n" +
                   "░░░░░██░░░▒░▒░░░░░▒░░░░░░░░▒░▒▒▓█▒░░░░▒█░░█▓▒▓▓▓▒░░▓█░░░█▒░░\n" +
                   "░░░░░▒█▒░░░▒▒▒░░░░▒░░░░░░░░░░▒▒▒░▒▓░░░██░▒█░░░░▒▓▓░░██░░█▒░░\n" +
                   "░░░░░░▒█▒░░░▒░▒░░░▒░░░░░░▒▒▒░░░░▒▒░░░▒█░░██░░░░░░░█░▒█░░█▒░░\n" +
                   "░░░░░░░▓█░░░▒░▒░░░░▒▒░░░░▓▒▒▓▓▓▒░░▓▒░██░░██▒▒▒▒▓▒▓▓███░░█▒░░\n" +
                   "░░░░░░░░██░░░▒░▒░░░░░▒▒░░░░░░░░▓█▓░░░█▓░░██░▓█░█░█░░█▒░░█▒░░\n" +
                   "░░░░░░░░░██░░░░▒▒░░░░░░▒▒░░░░░░░░▒█▓░█▓░░▓█▒▒█▒█░█▒██░░▒█░░░\n" +
                   "░░░░░░░░░░██▒░░░░▒░░░▒░░░▒▒░░░░░░░░▒▓██░░░██░░░░▒▒██░░░██░░░\n" +
                   "░░░░░░░░░░░▓██░░░░░░░░▒▒░░░▒░░░░░░░░░▓█░░░░▓███▓▓██░░░██░░░░\n" +
                   "░░░░░░░░░░░░░▓██▒░░░░░░▒▒▒▒▒░░░░░░░░░░██░░░░░░▒▒▒░░░░██░░░░░\n" +
                   "░░░░░░░░░░░░░░░▓███▒░░░░░░░▒▒▒▒▒▒▒▒░░░░▓██▒░░░░░░░▒███░░░░░░\n" +
                   "░░░░░░░░░░░░░░░░░▒▓███▓▒░░░░░░░▒░░▒▒▒▒░░░▒██▓██████▓░░░░░░░░\n" +
                   "░░░░░░░░░░░░░░░░░░░░░▒████▓▒▒░░░░░░░░░░░░░░░▓██▒░░░░░░░░░░░░\n" +
                   "░░░░░░░░░░░░░░░░░░░░░░░░░▒▓████▓░░░░░░░▓█████▒░░░░░░░░░░░░░░\n" +
                   "░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░▒█████████▒░░░░░░░░░░░░░░░░░░░\n" +
                   "░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░▒▒▒░░░░░░░░░░░░░░░░░░░░░░░\n";


        }

        /* isBetterMove
         * desc: returns true if minValue or maxValue should equal currentValue (does min when needed and max when needed)
         */
        public bool isBetterMove(Position currentPosition, int bestValue, int currentValue, int depth)
        {
            //if bestValue is minValue, which is default, set it to currentValue
            if (bestValue == int.MinValue)
                return true;

            //if it is my turn, maximize value by returning true if it is greater than that, else do minimize
            if (currentPosition == this.position)
                return currentValue > bestValue;
            else
                return bestValue > currentValue;
        }

        /* minimaxVal with Alpha-Beta Pruning
         * returns evaluation if it reaches maxDepth, else calculates min or max if it is opponent's turn or my turn respectively
         * 
         */
        public DataWrapper minimaxVal(Board b, int d, int alpha, int beta)
        {
            //when time runs out, this evaluates to true
            //when the cancelled execption is thrown, the last best move will be returned in the chooseMove() try->catch statement
            if (cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException();

            //return if reached gameover or max depth
            if (b.gameOver() || d == 0)
                return new DataWrapper(0, evaluate(b), 0 , 0);

            DataWrapper data;

            //look through right positions for moves
            int start = 0;
            int end = 5;

            if (b.whoseMove() == Position.Top)
            {
                start = 7;
                end = 12;
            }

            //initialize variables
            int bestMove = -1;
            int secondBestMove = -1;

            int bestValue = int.MinValue;
            int secondBestValue = int.MinValue;

            //loop through all possible moves
            for (int move = start; move <= end; move++)
            {
                //if it is a legal move, check the value of it
                if (b.legalMove(move))
                {
                    //create copy of board, make move, and evaluate it by recursing
                    Board newBoard = new Board(b);
                    newBoard.makeMove(move, false);
                    data = minimaxVal(newBoard, d - 1, alpha, beta);
                    
                    //if value, move pair is best or worst, set bestValue and bestMove to it
                    //also set second best value, move pair to previous value
                    if (isBetterMove(b.whoseMove(), bestValue, data.getValue(), d))
                    {
                        secondBestValue = bestValue;
                        bestValue = data.getValue();
                        
                        secondBestMove = bestMove;
                        bestMove = move;

                        //alpha beta pruning
                        if (b.whoseMove() == this.position)
                        {
                            alpha = Math.Max(alpha, bestValue);
                        }
                        else
                        {
                            beta = Math.Min(beta, bestValue);
                        }

                    }
                    
                    //if alpha is greater than or equal to beta, we can skip other moves (pruning part of alpha beta pruning)
                    if (alpha >= beta)
                        break;
                }
            }

            //return move pairs
            return new DataWrapper(bestMove, bestValue, secondBestMove, secondBestValue);

        }

        /* (unused) pointsOnMyTop
         * gives high score for having number of marbles to be evenly distributed on your side
         * gives estimate for top (reason this is top is because it makes the evalute function easy to make since it just does a negative value of this if i am bottom)
         */
        public int pointsOnMyTop(Board b)
        {
            int bottomPoints = 0;
            int bottomMin = int.MaxValue;
            int bottomMax = 0;
            for (int pos = 0; pos <= 5; pos++)
            {
                bottomPoints += b.stonesAt(pos);
                if (b.stonesAt(pos) < bottomMin)
                    bottomMin = b.stonesAt(pos);
                if (b.stonesAt(pos) > bottomMax)
                    bottomMax = b.stonesAt(pos);
            }

            int topPoints = 0;
            int topMin = int.MaxValue;
            int topMax = int.MaxValue;
            for (int pos = 7; pos <= 12; pos++)
            {
                topPoints += b.stonesAt(pos);
                if (b.stonesAt(pos) < topMin)
                    topMin = b.stonesAt(pos);
                if (b.stonesAt(pos) > topMax)
                    topMax = b.stonesAt(pos);
            }

            return (topMin - topMax)- (bottomMin - bottomMax);
        }

        /* capturesForTop
         * estimates how many captures can be made in future board states from current game state
         * gives estimate for favoring top
         */
        public int capturesForTop(Board b)
        {
            int bottomCaptures = 0;
            for (int pos = 0; pos <= 5; pos++)
            {
                if (b.stonesAt(pos) == 0)
                {
                    for (int supplyBefore = 0; supplyBefore < pos; supplyBefore++)
                    {
                        if (b.stonesAt(supplyBefore) == (pos - supplyBefore))
                        {
                            bottomCaptures = Math.Max(b.stonesAt(12 - pos), bottomCaptures);
                        }
                    }
                }
            }

            int topCaptures = 0;
            for (int pos = 7; pos <= 12; pos++)
            {
                if (b.stonesAt(pos) == 0)
                {
                    for (int supplyBefore = 7; supplyBefore < pos; supplyBefore++)
                    {
                        if (b.stonesAt(supplyBefore) == (pos - supplyBefore))
                        {
                            topCaptures = Math.Max(b.stonesAt(12 - pos), topCaptures);
                        }
                    }
                }
            }
            return (topCaptures - bottomCaptures);
        }

        /* moveAgainsTop
         * estimates how many move agains can be made in the next move from current board game state
         * gives estimate for favoring top
         */
        public int moveAgainsTop(Board b)
        {
            int top = 0;
            for (int pos = 12; pos >= 7; pos--)
            {
                if (b.stonesAt(pos) == topGoal - pos)
                {
                    top += 1;
                }
            }

            int bottom = 0;
            for (int pos = 5; pos >= 0; pos--)
            {
                if (b.stonesAt(pos) == bottomGoal - pos)
                {
                    bottom += 1;
                }
            }

            return (top - bottom);

        }

        /* evaluate
         * returns score of my player
         */
        public override int evaluate(Board b)
        {
            if (this.position == Position.Top)
            {
                return b.stonesAt(topGoal) - b.stonesAt(bottomGoal) + (pointsOnMyTop(b)+moveAgainsTop(b)) / depthMax + capturesForTop(b);
            }
            else
            {
                return b.stonesAt(bottomGoal) - b.stonesAt(topGoal) - (pointsOnMyTop(b) + moveAgainsTop(b)) / depthMax - capturesForTop(b);
            }

        }

        /* chooseMove
         * Description: Goes through every depth starting at 1, until time runs out at which point it will return last bestMove
         */
        public override int chooseMove(Board b)
        {

            cancellationToken = new CancellationTokenSource(TimeSpan.FromMilliseconds(maxTime));
            depthMax = 1;
            DataWrapper data;
            data = minimaxVal(b, depthMax, int.MinValue, int.MaxValue);

            turnCount++;
            depthMax++;



            try
            {
                while (true)
                {
                    data = minimaxVal(b, depthMax, int.MinValue, int.MaxValue);
                    depthMax++;
                }
            }
            catch (OperationCanceledException e)
            {
                if (data.getSecondBestValue() - data.getValue() == 1 && turnCount % 9 == 0)
                    return data.getSecondBestMove();
                return data.getMove();
            }

        }

    }
}