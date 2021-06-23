using System;
using NUnit.Framework;
using Stockfish.NET;
using Serilog;

namespace Chess.Core.Tests
{
    public class Tests
    {
        private const string PathToEngine = "/Users/sorokindmitrij/Downloads/stockfish_13_linux_x64/sf_13/src/stockfish";
        private const int Depth = 2;
        private const int Difficulty = 6;

        private const string LogsPath = "logs/log.txt";
        
        private IStockfish _engine;
        
        [SetUp]
        public void Setup()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File(LogsPath)
                .CreateLogger();
            
            _engine = new Stockfish.NET.Stockfish(PathToEngine)
            {
                Depth = Depth,
                SkillLevel = Difficulty
            };
        }

        [Test]
        public void GetVisualOfTheBoard()
        {
            var visual = _engine.GetBoardVisual();
            
            Log.Debug(visual);
            
            Assert.True(string.IsNullOrWhiteSpace(visual) == false);
        }
        
        
    }
}