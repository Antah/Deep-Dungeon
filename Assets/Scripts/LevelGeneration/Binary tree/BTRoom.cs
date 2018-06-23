using System;
using System.Collections.Generic;

class BTRoom
{
    public Tile btmLeft;
    public int width, height;
    public List<Tile> edgeTiles;
    public bool corridor;

    public BTRoom()
    {
    }

    public BTRoom(List<Tile> tiles)
    {
        this.corridor = true;
        this.edgeTiles = tiles;
    }

    public BTRoom(Tile tile, int roomWidth, int roomHeight)
    {
        corridor = false;
        btmLeft = tile;
        width = roomWidth;
        height = roomHeight;

        edgeTiles = new List<Tile>();
        for (int x = 0; x < roomWidth; x++)
        {
            for (int y = 0; y < roomHeight; y++)
            {
                Tile newTile = new Tile(tile.tileX + x, tile.tileY + y);
                if (x == 0 || y == 0 || x == roomWidth - 1 || y == roomHeight - 1)
                    edgeTiles.Add(newTile);
            }
        }
    }
}