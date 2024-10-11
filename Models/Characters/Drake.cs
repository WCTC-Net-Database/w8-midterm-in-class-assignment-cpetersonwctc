using W8_assignment_template.Interfaces;

namespace W8_assignment_template.Models.Characters;

public class Drake : CharacterBase, IFlyable, ILootable
{
    public string Treasure { get; set; }

    public Drake()
    {
    }

    public Drake(string name, string type, int level, int hp, string treasure, IRoom startingRoom) : base(name, type, level, hp, startingRoom)
    {
        Treasure = treasure;
    }

    public void Fly()
    {
        OutputManager.WriteLine($"{Name} flies rapidly through the air.", ConsoleColor.Blue);
    }


    public override void UniqueBehavior()
    {
        throw new NotImplementedException();
    }
}
