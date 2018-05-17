using System;
using System.Collections.Generic;
using UnityEngine;

public struct Tile
{
    public int tileX;
    public int tileY;

    public Tile(int x, int y)
    {
        tileX = x;
        tileY = y;
    }
}
static class TileTools
{
    public static List<Tile> GetLine(Tile from, Tile to)
    {
        List<Tile> line = new List<Tile>();

        int x = from.tileX;
        int y = from.tileY;

        int dx = to.tileX - from.tileX;
        int dy = to.tileY - from.tileY;

        bool inverted = false;
        int step = Math.Sign(dx);
        int gradientStep = Math.Sign(dy);

        int longest = Mathf.Abs(dx);
        int shortest = Mathf.Abs(dy);

        if (longest < shortest)
        {
            inverted = true;
            longest = Mathf.Abs(dy);
            shortest = Mathf.Abs(dx);

            step = Math.Sign(dy);
            gradientStep = Math.Sign(dx);
        }

        int gradientAccumulation = longest / 2;
        for (int i = 0; i < longest; i++)
        {
            line.Add(new Tile(x, y));

            if (inverted)
            {
                y += step;
            }
            else
            {
                x += step;
            }

            gradientAccumulation += shortest;
            if (gradientAccumulation >= longest)
            {
                if (inverted)
                {
                    x += gradientStep;
                }
                else
                {
                    y += gradientStep;
                }
                gradientAccumulation -= longest;
            }
        }

        return line;
    }

    public static Vector3 CoordToWorldPoint(Tile tile, int columns, int rows)
    {
        return new Vector3(-columns / 2 + .5f + tile.tileX, 2, -rows / 2 + .5f + tile.tileY);
    }
}
