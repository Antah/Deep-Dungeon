using System;
using System.Collections.Generic;

class BTArea
{
    public Tile btmLeft;
    public int width, height;
    public List<BTRoom> rooms;

    public BTArea(Tile c, int w, int h)
    {
        btmLeft = c;
        width = w;
        height = h;
        rooms = new List<BTRoom>();
    }
}