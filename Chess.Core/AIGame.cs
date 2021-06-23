using System;
using System.Collections.Generic;
using System.IO;
using Stockfish.NET;

namespace Chess.Core
{
    public class AIGame
    {
        public const string PlayerDefaultSide = "white";
        
        private const string PathToEngine = "/sf_13/src/stockfish";
        private const int Depth = 2;
        private const int Difficulty = 6;

        private readonly IStockfish _engine;
        private string GetFullPath => Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location).ToString() + "/sf_13/src/stockfish");

        private List<string> _moves = new List<string>();

        public AIGame()
        {
            _engine = new Stockfish.NET.Stockfish(GetFullPath)
            {
                Depth = Depth,
                SkillLevel = Difficulty
            };
        }
///Users/sorokindmitrij/RiderProjects/RefactoredChess/Chess.Web/bin/Debug/net5.0/sf_13/src/stockfish
        public string Move(string from, string to)
        {
            if (_engine.IsMoveCorrect(from + to) == false)
                throw new Exception("Move is incorrect!");
            
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