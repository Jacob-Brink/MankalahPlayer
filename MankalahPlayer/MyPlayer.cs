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
    public class MyPlayer : Player // class must be public
    {

        private const int bottomGoal = 6;
        private const int topGoal = 13;
        private int maxTime;
        private int turnCount = 0;
        private CancellationTokenSource cancellationToken;



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

        public class WeightsPackage
        {
            public double weight1 { get; }
            public double weight2 { get; }
            public double weight3 { get; }
            public double weight4 { get; }

            public WeightsPackage(double w1, double w2, double w3, double w4)
            {
                weight1 = w1;
                weight2 = w2;
                weight3 = w3;
                weight4 = w4;
            }

        }

        private Position position;
        private WeightsPackage weightsPackage;

        public MyPlayer(Position pos, int maxTimePerMove) // constructor must match this signature
            : base(pos, "LeBrink", maxTimePerMove) // choose a string other than "MyPlayer"
        {
            this.position = pos;
            this.maxTime = maxTimePerMove;
            //this.weightsPackage = weightsPackage;
        }

        public override string getImage()
        {
            return "lebrink.png";
        }

        public bool isBetterMove(Position currentPosition, int bestValue, int currentValue)
        {
            if (bestValue == int.MinValue)
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

            int bestValue = int.MinValue;
            int bestMove = -1;

            for (int move = start; move <= end; move++)
            {
                if (b.legalMove(move))
                {

                    Board newBoard = new Board(b);
                    newBoard.makeMove(move, false);//test
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

        public double pointsOnMyTop(Board b)//asdf
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

            return (topPoints - bottomPoints);
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
            return (topCaptures - bottomCaptures);
        }

        public double moveAgainsTop(Board b)
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

        public double topCala(Board b)
        {
            return (b.stonesAt(topGoal) - b.stonesAt(bottomGoal));
        }

        public int calculationTopValue(Board b)
        {
            int evaluation = (int) (capturesForTop(b)*5 + 10 * topCala(b) + (b.stonesAt(topGoal) + b.stonesAt(bottomGoal))*pointsOnMyTop(b));
            Console.WriteLine("top Cala" + topCala(b));
            Console.WriteLine("top Cala * 10000 cast" + (int)(10000 * topCala(b)));
            return
               evaluation;
            //weightsPackage.weight1 * topCala(b) + weightsPackage.weight2 * moveAgainsTop(b) +
            // weightsPackage.weight3 * capturesForTop(b)));// + weightsPackage.weight4 * pointsOnMyTop(b)));
        }

        public override int evaluate(Board b)
        {
            if (this.position == Position.Top)
            {
                return calculationTopValue(b);
            }
            else
            {
                return -1 * calculationTopValue(b);
            }

        }

        public override int chooseMove(Board b)
        {
            cancellationToken = new CancellationTokenSource(TimeSpan.FromMilliseconds(maxTime));
            var depth = 1;
            int bestMove = minimaxVal(b, depth, int.MinValue, int.MaxValue).getMove();
            turnCount++;
            depth++;
            try
            {
                while (true)
                {
                    bestMove = minimaxVal(b, depth, int.MinValue, int.MaxValue).getMove();
                    depth++;
                }
            }
            catch (OperationCanceledException e)
            {
                Console.WriteLine(depth);
                return bestMove;
            }

        }

    }
}