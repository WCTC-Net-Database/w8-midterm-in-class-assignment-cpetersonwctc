using Microsoft.Extensions.DependencyInjection;
using W8_assignment_template.Helpers;
using W8_assignment_template.Interfaces;
using W8_assignment_template.Models.Rooms;

namespace W8_assignment_template.Models.Characters;

public abstract class CharacterBase : ICharacter
{
    public int HP { get; set; }
    public int Level { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }

    protected OutputManager OutputManager;
    private IRoom _currentRoom;

    // Explicit interface implementation for CurrentRoom to prevent access from outside the class
    // This is to ensure that the CurrentRoom property is only set by the Move method
    public IRoom CurrentRoom => _currentRoom;

    // Parameterless constructor for deserialization
    protected CharacterBase()
    {
    }

    protected CharacterBase(string name, string type, int level, int hp, IRoom startingRoom)
    {
        Name = name;
        Type = type;
        Level = level;
        HP = hp;

        _currentRoom = startingRoom;
        _currentRoom.Enter();
    }

    public void Attack(ICharacter target)
    {
        // OK.. SOOO- the enemies dont actually get removed; but instead their health is set to 0, and they wont appear for attacking anymore

        OutputManager.WriteLine($"{Name} attacks {target.Name} with a chilling touch.", ConsoleColor.Blue);

        if (this is Player player && target is ILootable targetWithTreasure && !string.IsNullOrEmpty(targetWithTreasure.Treasure))
        {
            OutputManager.WriteLine($"{Name} takes {targetWithTreasure.Treasure} from {target.Name}", ConsoleColor.Blue);
            player.Gold += 10; // Assuming each treasure is worth 10 gold
            targetWithTreasure.Treasure = null; // Treasure is taken
            OutputManager.WriteLine($"{Name} now has {player.Gold} gold", ConsoleColor.Blue);

            target.HP = 0;
        }
        else if (this is Player playerWithGold && target is Player targetWithGold && targetWithGold.Gold > 0)
        {
            // we can't attack other players, but if we could we could take their gold here
            OutputManager.WriteLine($"{Name} takes gold from {target.Name}", ConsoleColor.Blue);
            playerWithGold.Gold += targetWithGold.Gold;
            targetWithGold.Gold = 0; // Gold is taken
        }
    }

    public void Move(IRoom? nextRoom)
    {
        if (nextRoom != null)
        {
            _currentRoom = nextRoom;
            OutputManager.WriteLine($"{Name} has entered {_currentRoom.Name}. {_currentRoom.Description}", ConsoleColor.Green);
            foreach (var character in _currentRoom.Characters)
            {
                //added if statement so monsters wont display as "here" if they are dead...
                if(character.HP > 0)
                {
                    OutputManager.WriteLine($"{character.Name} is here.", ConsoleColor.Red);
                }
            }
        }
        else
        {
            OutputManager.WriteLine($"{Name} cannot move to the specified room.", ConsoleColor.Yellow);
        }
    }

    public void Move(string? direction)
    {
        var nextRoom = direction?.ToLower() switch
        {
            "north" => _currentRoom.North,
            "south" => _currentRoom.South,
            "east" => _currentRoom.East,
            "west" => _currentRoom.West,
            _ => null
        };

        Move(nextRoom);
    }

    // Method to set the OutputManager called by the CharacterBaseConverter so that the character can write to the console
    // This method is called after deserialization
    public void SetOutputManager(OutputManager outputManager)
    {
        OutputManager = outputManager;
    }

    public abstract void UniqueBehavior();
}
