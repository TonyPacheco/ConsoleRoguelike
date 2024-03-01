using Roguelike;
using Roguelike.Runners;

ConsoleRunner.Init();
var player = DataRunner.LoadPlayerFromDisk("tony.plr");
//var player = Player.ConsoleCreate();
var world = DataRunner.LoadWorldFromDisk("test.wld");
var game = new Game(player, world);
//var game = DataRunner.LoadGameStateFromDisk("test.sav");
game.Start();
