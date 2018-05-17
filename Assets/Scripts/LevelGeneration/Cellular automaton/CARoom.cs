using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class CARoom : IComparable<CARoom>
{
    public List<Tile> tiles;
    public List<Tile> edgeTiles;
    public List<CARoom> connectedRooms;
    public int roomSize;
    public bool isAccessibleFromMainRoom;
    public bool isMainRoom;
    public int passages;
    public int maxPassages;

    public CARoom()
    {
    }

    public CARoom(List<Tile> roomTiles, int[,] map, int connectionThreshold, int sizeFactor)
    {
        tiles = roomTiles;
        roomSize = tiles.Count;
        connectedRooms = new List<CARoom>();

        int sizeToPassages = 1 + roomSize / sizeFactor;
        if (sizeToPassages > connectionThreshold)
            sizeToPassages = connectionThreshold;
        maxPassages = sizeToPassages;
        passages = 0;

        edgeTiles = new List<Tile>();
        foreach (Tile tile in tiles)
        {
            for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
            {
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                {
                    if (x == tile.tileX || y == tile.tileY)
                    {
                        if (map[x, y] == 1)
                        {
                            edgeTiles.Add(tile);
                        }
                    }
                }
            }
        }
    }

    public void SetAccessibleFromMainRoom()
    {
        if (!isAccessibleFromMainRoom)
        {
            isAccessibleFromMainRoom = true;
            foreach (CARoom connectedRoom in connectedRooms)
            {
                connectedRoom.SetAccessibleFromMainRoom();
            }
        }
    }

    public static void ConnectRooms(CARoom roomA, CARoom roomB)
    {
        roomA.passages++;
        roomB.passages++;

        if (roomA.isAccessibleFromMainRoom)
        {
            roomB.SetAccessibleFromMainRoom();
        }
        else if (roomB.isAccessibleFromMainRoom)
        {
            roomA.SetAccessibleFromMainRoom();
        }
        roomA.connectedRooms.Add(roomB);
        roomB.connectedRooms.Add(roomA);
    }

    public bool IsConnected(CARoom otherRoom)
    {
        return connectedRooms.Contains(otherRoom);
    }

    public int CompareTo(CARoom otherRoom)
    {
        //return otherRoom.roomSize.CompareTo(roomSize);
        return this.roomSize.CompareTo(otherRoom.roomSize);
    }

    internal bool checkMaxPassages()
    {
        if (passages >= maxPassages)
            return false;
        return true;
    }
}
