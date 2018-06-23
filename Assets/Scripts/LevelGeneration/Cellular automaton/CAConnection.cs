using System;

public enum ConnectionType
{
    straight, direct
}

class CAConnection : IComparable<CAConnection>
{
    public CARoom roomA;
    public CARoom roomB;
    public Tile connectionTileA;
    public Tile connectionTileB;
    public Int32 distance;

    public CAConnection(Tile a, Tile b, CARoom ra, CARoom rb, Int32 d)
    {
        connectionTileA = a;
        connectionTileB = b;
        roomA = ra;
        roomB = rb;
        distance = d;
    }
    public CAConnection(Tile a, Tile b, Int32 d)
    {
        connectionTileA = a;
        connectionTileB = b;
        distance = d;
    }

    public int CompareTo(CAConnection otherConnection)
    {
        //return otherRoom.roomSize.CompareTo(roomSize);
        return this.distance.CompareTo(otherConnection.distance);
    }
}

