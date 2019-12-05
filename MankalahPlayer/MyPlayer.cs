using System;
using System.Collections.Generic;
using System.Linq;
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

        private Position position;

        public MyPlayer(Position pos, int maxTimePerMove) // constructor must match this signature
            : base(pos, "LeBrink", maxTimePerMove) // choose a string other than "MyPlayer"
        {
            this.position = pos;
            this.maxTime = maxTimePerMove;
        }

        public override string getImage()
        {
            return base.getImage();
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

        public DataWrapper minimaxVal(Board b, int d)
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
                    data = minimaxVal(newBoard, d - 1);
                    if (isBetterMove(b.whoseMove(), bestValue, data.getValue()))
                    {
                        bestValue = data.getValue();
                        bestMove = move;
                    }
                }
            }

            return new DataWrapper(bestMove, bestValue);

        }

        public override int evaluate(Board b) 
        {
            if (this.position == Position.Bottom)
            {
                return b.stonesAt(bottomGoal)-b.stonesAt(topGoal);
            }
            else
            {
                return b.stonesAt(topGoal)-b.stonesAt(bottomGoal);
            }
            
        }

        private 

        private int count = 0;

        public override int chooseMove(Board b)
        {
            cancellationToken = new CancellationTokenSource(TimeSpan.FromMilliseconds(maxTime));
           
            var depth = 9;
            int bestMove = minimaxVal(b, depth).getMove();
            depth++;
            try {
                while (true) {
                    bestMove = minimaxVal(b, depth).getMove();
                    depth++;
                }
            } catch (OperationCanceledException e) {
                return bestMove;
            }
        }

    }
}