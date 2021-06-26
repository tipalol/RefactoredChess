using System;
using System.Collections.Generic;
using Stockfish.NET;

namespace Chess.Core
{
    public class AIGame
    {
        public const string PlayerDefaultSide = "white";
        
        private const string PathToEngine = "/app/sf_13/src/stockfish";
        private const int Depth = 2;
        private const int Difficulty = 15;

        private readonly IStockfish _engine;
        private readonly List<string> _moves = new ();

        public AIGame()
        {
            _engine = new Stockfish.NET.Stockfish(PathToEngine)
            {
                Depth = Depth,
                SkillLevel = Difficulty
            };
        }

        public string Move(string from, string to)
        {
            if (_engine.IsMoveCorrect(from + to) == false)
                throw new Exception($"Move is incorrect! From {from} To {to}");
            
            _moves.Add(from + to);
            _engine.SetPosition(_moves.ToArray());
            
            return GetAiMove();
        }

        public bool IsMoveCorrect(string from, string to) => _engine.IsMoveCorrect(from + to);

        public string GetFen()
        {
            return _engine.GetFenPosition();
        }

        private string GetAiMove()
        {
            var aiMove = _engine.GetBestMove();
            
            _moves.Add(aiMove);
            _engine.SetPosition(_moves.ToArray());
            
            return aiMove;
        }
    }
}