using W8_assignment_template.Data;
using W8_assignment_template.Helpers;
using W8_assignment_template.Interfaces;
using W8_assignment_template.Models.Characters;

namespace W8_assignment_template.Services;

public class GameEngine
{
    private readonly IContext _context;
    private readonly MapManager _mapManager;
    private readonly MenuManager _menuManager;
    private readonly OutputManager _outputManager;

    private readonly IRoomFactory _roomFactory;
    private ICharacter _player;
    private ICharacter _goblin;
    private ICharacter _kobold;
    private ICharacter _drake;

    private List<IRoom> _rooms;

    public GameEngine(IContext context, IRoomFactory roomFactory, MenuManager menuManager, MapManager mapManager, OutputManager outputManager)
    {
        _roomFactory = roomFactory;
        _menuManager = menuManager;
        _mapManager = mapManager;
        _outputManager = outputManager;
        _context = context;
    }

    public void Run()
    {
        if (_menuManager.ShowMainMenu())
        {
            SetupGame();
        }
    }

    private void AttackCharacter()
    {
        // TODO Update this method to allow for attacking a selected monster in the room.
        // TODO e.g. "Which monster would you like to attack?"
        // TODO Right now it just attacks the first monster in the room.
        // TODO It is ok to leave this functionality if there is only one monster in the room.
        _outputManager.Clear();

            while (true)
            {

                var allTargets = _player.CurrentRoom.Characters.FindAll(c => c != _player && c.HP != 0);

                if (allTargets.Count != 0)
                {
                    _outputManager.WriteLine($"You can attack:", ConsoleColor.Green);
                    for (var monsterId = 0; monsterId < allTargets.Count; monsterId++)
                    {
                        _outputManager.WriteLine($"{monsterId + 1}. {allTargets[monsterId].Name}",ConsoleColor.Red);
                    }
                    _outputManager.WriteLine($"{allTargets.Count+1}. Dont Attack", ConsoleColor.Yellow);

                    _outputManager.Display();

                    var choiceAttack = -1;
                    try
                    {
                        choiceAttack = Convert.ToInt32(Console.ReadLine());
                    }
                    catch (Exception ex)
                    {

                    }

                    if (allTargets.ElementAtOrDefault(choiceAttack - 1) != null)
                    {
                        _outputManager.Clear();
                        _player.Attack(allTargets[choiceAttack - 1]);
                    
                    }
                    else if (choiceAttack - 1 == allTargets.Count)
                    {
                        _outputManager.Clear();
                        break;
                    }
                    else
                    {
                        _outputManager.WriteLine($"That is not an option to choose from.");
                    }
            }
            else
            {
                _outputManager.WriteLine("No characters to attack.", ConsoleColor.Red);
                break;
            }
        }
    }

    private void GameLoop()
    {
        while (true)
        {
            _mapManager.DisplayMap();
            _outputManager.WriteLine("Choose an action:", ConsoleColor.Cyan);
            _outputManager.WriteLine("1. Move North");
            _outputManager.WriteLine("2. Move South");
            _outputManager.WriteLine("3. Move East");
            _outputManager.WriteLine("4. Move West");

            // Check if there are characters in the current room to attack
            if (_player.CurrentRoom.Characters.Any(c => c != _player && c.HP != 0))
            {
                _outputManager.WriteLine("5. Attack");
            }

            _outputManager.WriteLine("6. Exit Game");

            _outputManager.Display();

            var input = Console.ReadLine();

            string? direction = null;
            switch (input)
            {
                case "1":
                    direction = "north";
                    break;
                case "2":
                    direction = "south";
                    break;
                case "3":
                    direction = "east";
                    break;
                case "4":
                    direction = "west";
                    break;
                case "5":
                    if (_player.CurrentRoom.Characters.Any(c => c != _player))
                    {
                        AttackCharacter();
                    }
                    else
                    {
                        _outputManager.WriteLine("No characters to attack.", ConsoleColor.Red);
                    }

                    break;
                case "6":
                    _outputManager.WriteLine("Exiting game...", ConsoleColor.Red);
                    _outputManager.Display();
                    Environment.Exit(0);
                    break;
                default:
                    _outputManager.WriteLine("Invalid selection. Please choose a valid option.", ConsoleColor.Red);
                    break;
            }

            // Update map manager with the current room after movement
            if (!string.IsNullOrEmpty(direction))
            {
                _outputManager.Clear();
                _player.Move(direction);
                _mapManager.UpdateCurrentRoom(_player.CurrentRoom);
            }
        }
    }

    private void LoadMonsters()
    {
        _goblin = _context.Characters.OfType<Goblin>().FirstOrDefault();

        var random = new Random();
        var randomRoom = _rooms[random.Next(_rooms.Count)];
        randomRoom.AddCharacter(_goblin); // Use helper method

        _kobold = _context.Characters.OfType<Kobold>().FirstOrDefault();
        _drake = _context.Characters.OfType<Drake>().FirstOrDefault();

        random = new Random();
        randomRoom = _rooms[random.Next(_rooms.Count)];
        randomRoom.AddCharacter(_kobold);
        randomRoom.AddCharacter(_drake);

        // TODO Load your two new monsters here into the same room
    }

    private void SetupGame()
    {
        var startingRoom = SetupRooms();
        _mapManager.UpdateCurrentRoom(startingRoom);

        _player = _context.Characters.OfType<Player>().FirstOrDefault();
        _player.Move(startingRoom);
        _outputManager.WriteLine($"{_player.Name} has entered the game.", ConsoleColor.Green);

        // Load monsters into random rooms 
        LoadMonsters();

        // Pause for a second before starting the game loop
        Thread.Sleep(1000);
        GameLoop();
    }

    private IRoom SetupRooms()
    {
        // TODO Update this method to create more rooms and connect them together

        var entrance = _roomFactory.CreateRoom("entrance", _outputManager);
        var treasureRoom = _roomFactory.CreateRoom("treasure", _outputManager);
        var dungeonRoom = _roomFactory.CreateRoom("dungeon", _outputManager);
        var library = _roomFactory.CreateRoom("library", _outputManager);
        var armory = _roomFactory.CreateRoom("armory", _outputManager);
        var garden = _roomFactory.CreateRoom("garden", _outputManager);
        var shop = _roomFactory.CreateRoom("shop", _outputManager);
        var bedChamber = _roomFactory.CreateRoom("bedchamber", _outputManager);

        entrance.North = treasureRoom;
        entrance.West = library;
        entrance.East = garden;

        treasureRoom.South = entrance;
        treasureRoom.West = dungeonRoom;

        dungeonRoom.East = treasureRoom;

        library.East = entrance;
        library.South = armory;

        armory.North = library;

        garden.West = entrance;
        garden.East = shop;
        garden.South = bedChamber;

        shop.West = garden;

        bedChamber.North = garden;

        // Store rooms in a list for later use
        _rooms = new List<IRoom> { entrance, treasureRoom, dungeonRoom, library, armory, garden };

        return entrance;
    }
}
