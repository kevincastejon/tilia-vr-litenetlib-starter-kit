public class InitMessage
{
    public int OwnId { get; set; }
    public PlayerAddMessage[] Players { get; set; }
    public EntityAddMessage[] Entities { get; set; }
    public byte ColoredCube { get; set; }
}
