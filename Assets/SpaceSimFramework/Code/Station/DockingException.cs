using System;

namespace SpaceSimFramework
{
public abstract class DockingException : Exception
{
    public Station station;
    public string message;
}

public class DockingForbiddenException : DockingException
{
    public DockingForbiddenException(Station station)
    {
        this.station = station;
        message = "[" + station.name + " Approach Control]: Docking denied, please leave the area.";
    }
}

public class DockingQueueException : DockingException
{
    public DockingQueueException(Station station)
    {
        this.station = station;
        message = "[" + station.name + " Approach Control]: Standby, docking pattern is full!";
    }
}

public class MooringUnavailableException : DockingException
{
    public MooringUnavailableException(Station station)
    {
        this.station = station;
        message = "[" + station.name + " Approach Control]: Docking denied - All mooring points occupied!";
    }
}
}