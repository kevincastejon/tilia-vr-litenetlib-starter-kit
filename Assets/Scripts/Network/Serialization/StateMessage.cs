using System.Collections.Generic;

public class StateMessage
{
    public PlayerState[] Players { get; set; }
    public EntityState[] Bullets { get; set; }
    public EntityState[] Guns { get; set; }
}
