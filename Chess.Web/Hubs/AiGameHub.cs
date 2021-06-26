using System;
using System.IO;
using System.Threading.Tasks;
using Chess.Core;
using Microsoft.AspNetCore.SignalR;
using Serilog;

namespace Chess.Web.Hubs
{
    public class AiGameHub : Hub
    {
        private const string GameContextKey = "Game";
        private const string UserInfoContextKey = "User";
        private const string LogsPath = "logs/log.txt";

        private ILogger Logger { get; } = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File(LogsPath, rollingInterval: RollingInterval.Day)
            .CreateLogger();

        private AIGame Game => (AIGame) Context.Items[GameContextKey];
        private string User => (string) Context.Items[UserInfoContextKey];

        public async Task Login(string login)
        {
            Context.Items.Add(UserInfoContextKey, login);

            //TODO: connect to wordpress db
            Logger.Information($"New client logged in {login}", login);
            await Clients.Caller.SendAsync("LoggedIn", login);
        }

        public async Task Play()
        {
            var game = new AIGame();

            if (Context.Items.ContainsKey(GameContextKey))
                Context.Items[GameContextKey] = game;
            else
                Context.Items.Add(GameContextKey, game);

            Logger.Information($"Player {User} started new game vs AI");
            await Clients.Caller.SendAsync("GameStarted", AIGame.PlayerDefaultSide);
        }

        public async Task MakeMove(string from, string to)
        {
            if (!Game.IsMoveCorrect(from, to)) throw new Exception("Move is incorrect!");

            Logger.Debug($"Player {User} made move {from} to {to}");
            var aiMove = Game.Move(from, to);

            var aiFrom = (aiMove.Substring(0, 2));
            var aiTo = (aiMove.Substring(2, 2));
            
            Logger.Debug($"AI plays {aiMove} {aiFrom} {aiTo}");
            
            

            await Clients.Caller.SendAsync("ReceiveMove", aiFrom, aiTo);
        }

        public async Task EndGame(string fen)
        {
            Logger.Information($"The game just ended. {User} FEN {fen}");
            await Clients.Caller.SendAsync("GameEnded");
        }
    }
}