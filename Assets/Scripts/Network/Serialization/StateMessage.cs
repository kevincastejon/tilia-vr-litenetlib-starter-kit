using System.Collections.Generic;

public class StateMessage
{
    public int Sequence { get; set; }
    public PlayerState[] Players { get; set; }
    public EntityState[] Entities { get; set; }
    public byte ColoredCube { get; set; }

    public StateMessage Clone()
    {
        PlayerState[] plrs = new PlayerState[Players.Length];
        for (int i = 0; i < Players.Length; i++)
        {
            plrs[i] = Players[i].Clone();
        }
        EntityState[] ent = new EntityState[Entities.Length];
        for (int i = 0; i < Entities.Length; i++)
        {
            ent[i] = Entities[i].Clone();
        }
        return new StateMessage()
        {
            Sequence = Sequence,
            Players = plrs,
            Entities = ent,
            ColoredCube = ColoredCube
        };
    }
}
