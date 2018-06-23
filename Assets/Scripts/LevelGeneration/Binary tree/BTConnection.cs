using System;
using System.Collections.Generic;

class BTConnection : IComparable<BTConnection>
{
    public Tile connectionTileA;
    public Tile connectionTileB;
    public Int32 distance;

    public BTConnection(Tile a, Tile b, Int32 d)
    {
        connectionTileA = a;
        connectionTileB = b;
        distance = d;
    }

    public int CompareTo(BTConnection otherConnection)
    {
        return this.distance.CompareTo(otherConnection.distance);
    }
}