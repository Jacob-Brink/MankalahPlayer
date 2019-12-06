using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Mankalah;

namespace Mankalah
{
    
    // rename me
    public class MyPlayer : Player // class must be public
    {

        private const int bottomGoal = 6;
        private const int topGoal = 13;
        private int maxTime;
        private int turnCount = 0;
        private CancellationTokenSource cancellationToken;

        private double w1, w2, w3, w4;

        public class DataWrapper
        {
            private int move, value;

            public DataWrapper(int move, int value)
            {
                this.move = move;
                this.value = value;
            }

            public int getMove()
            {
                return this.move;
            }

            public int getValue()
            {
                return this.value;
            }

        }

        private Position position;

        public MyPlayer(Position pos, int maxTimePerMove) // constructor must match this signature
            : base(pos, "LeBrink", maxTimePerMove) // choose a string other than "MyPlayer"
        {
            this.position = pos;
            this.maxTime = maxTimePerMove;
            w1 = .6;
            w2 = 0;
            w3 = 0;
            w4 = .4;
        }

        public override string getImage()
        {
            return "lebrink.png";
        }

        public bool isBetterMove(Position currentPosition, int bestValue, int currentValue)
        {
            if (bestValue == -1)
                return true;

            if (currentPosition == this.position)
                return currentValue > bestValue;
            else
                return bestValue > currentValue;
        }

        public DataWrapper minimaxVal(Board b, int d, int alpha, int beta)
        {

            if (cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException();

            if (b.gameOver() || d == 0)
                return new DataWrapper(0, evaluate(b));

            DataWrapper data;

            int start = 0;
            int end = 5;

            if (b.whoseMove() == Position.Top)
            {
                start = 7;
                end = 12;
            }

            int bestValue = -1;
            int bestMove = -1;

            for (int move = start; move <= end; move++)
            {
                if (b.legalMove(move))
                {

                    Board newBoard = new Board(b);
                    newBoard.makeMove(move, true);//test
                    data = minimaxVal(newBoard, d - 1, alpha, beta);
                    
                    if (b.whoseMove() == this.position)
                    {
                        alpha = Math.Max(alpha, data.getValue());
                    }
                    else
                    {
                        beta = Math.Min(beta, data.getValue());
                    }

                    if (alpha >= beta)
                        break;

                    if (isBetterMove(b.whoseMove(), bestValue, data.getValue()))
                    {
                        bestValue = data.getValue();
                        bestMove = move;
                    }
                }
            }

            return new DataWrapper(bestMove, bestValue);

        }

        public double pointsOnMyTop(Board b)
        {
            int bottomPoints = 0;
            for (int pos = 0; pos <= 5; pos++)
            {
                bottomPoints += b.stonesAt(pos);
            }

            int topPoints = 0;
            for (int pos = 7; pos <= 12; pos++)
            {
                topPoints += b.stonesAt(pos);
            }

            return (topPoints - bottomPoints) / 12.0;
        }

        public double capturesForTop(Board b)
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
            //todo
            return (topCaptures - bottomCaptures)/24.0;
        }

        public double moveAgainsTop(Board b)
        {
            int top = 0;
            for (int pos = 0; pos <= 5; pos++)
            {
                if (b.stonesAt(pos) == topGoal - pos)
                {
                    top = 1;
                    break;
                }
            }

            int bottom = 0;
            for (int pos = 7; pos <= 12; pos++)
            {
                if (b.stonesAt(pos) == bottomGoal - pos)
                {
                    bottom = 1;
                    break;
                }
            }

            return (top - bottom);

        }

        public double topCala(Board b)
        {
            return (b.stonesAt(topGoal) - b.stonesAt(bottomGoal))/46.0;
        }

        public int calculationTopValue(Board b)
        {
            return (int)(10000 * (w1 * topCala(b) + w2 * moveAgainsTop(b) + w3 * capturesForTop(b) + w4 * pointsOnMyTop(b)));
        }

        public override int evaluate(Board b) 
        {
            if (this.position == Position.Top)
            {
                return calculationTopValue(b);
            }
            else
            {
                return -1* calculationTopValue(b);
            }
            
        }

        public override int chooseMove(Board b)
        {
            cancellationToken = new CancellationTokenSource(TimeSpan.FromMilliseconds(maxTime));
            turnCount++;
            var depth = 8;
            int bestMove = minimaxVal(b, depth, int.MinValue, int.MaxValue).getMove();
            depth++;
            try {
                while (true) {
                    bestMove = minimaxVal(b, depth, int.MinValue, int.MaxValue).getMove();
                    depth++;
                }
            } catch (OperationCanceledException e)
            {
                return bestMove;
            }
            
        }

    }
}