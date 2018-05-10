using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;


public class BSP : BoardCreator
{
    public string seed;
    public bool useRandomSeed;

    public int minWidth, minHeight, minRoomWidth, minRoomHeight;
    public int offset;

    int[,] map;

    public GameObject[] floorTiles;                           // An array of floor tile prefabs.
    public GameObject[] wallTiles;                            // An array of wall tile prefabs.
    public GameObject[] rubbleTiles;
    public GameObject exit;

    private System.Random pseudoRandom;

    public override void SetupScene(int level)
    {
        this.level = level;
        BoardSetup();
    }

    private void BoardSetup()
    {
        // Create the board holder.
        boardHolder = new GameObject("BoardHolder");

        GenerateMap();
        InstantiateTiles();

        Vector3 playerPos = new Vector3(columns / 2, rows / 2, 0);
        GameManager.instance.GetPlayer().transform.position = playerPos;
        Vector3 exitPos = new Vector3(columns / 2 + 1, rows / 2, 0);
        GameObject tileInstance = Instantiate(exit, exitPos, Quaternion.identity) as GameObject;
        tileInstance.transform.parent = boardHolder.transform;
    }

    private void GenerateMap()
    {
        map = new int[columns, rows];
        if (useRandomSeed)
        {
            seed = Time.time.ToString();
        }
        pseudoRandom = new System.Random(seed.GetHashCode());

        for (int x = 0; x < columns; x++)
            for (int y = 0; y < rows; y++)
                map[x, y] = 1;

        Coord btmLeft = new Coord(1, 1);
        Area mainRoom = new Area(btmLeft, columns - 2, rows - 2);
        BSPLeaf mainLeaf = new BSPLeaf(mainRoom);
        DivideRoom(mainLeaf);
        Connect(mainLeaf);
        DrawRoom(mainLeaf.area.room);
    }

    private void InstantiateTiles()
    {
        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                if (map[i, j] == 1)
                {
                    InstantiateFromArray(wallTiles, i, j);
                }
                else
                {
                    InstantiateFromArray(floorTiles, i, j);
                    if(map[i,j] == 2)
                        InstantiateFromArray(rubbleTiles, i, j);
                }
            }
        }
    }

    private void DivideRoom(BSPLeaf leaf)
    {
        int w = leaf.area.width;
        int h = leaf.area.height;
        int division; //1 means vertical, 0 means horizontal

        if (w < minWidth)
        {
            if (h < minHeight)
            {
                CreateRoom(leaf);
                return;
            }
            else
                division = 0;
        }
        else if (h < minHeight)
        {
            division = 1;
        }
        else
        {
            division = pseudoRandom.Next(0, 2);
        }

        BSPLeaf newLeaf1, newLeaf2;
        Area newRoom1, newRoom2;

        if(division == 0) // horizontal bisection
        {
            int lowend = leaf.area.btmLeft.tileY + offset;
            int highend = leaf.area.btmLeft.tileY + leaf.area.height - offset;
            int divisionHeight =  pseudoRandom.Next(lowend, highend);

            newRoom1 = new Area(leaf.area.btmLeft, leaf.area.width, divisionHeight - leaf.area.btmLeft.tileY);
            Coord newCoord2 = new Coord(leaf.area.btmLeft.tileX, divisionHeight + 1);
            newRoom2 = new Area(newCoord2, leaf.area.width, leaf.area.btmLeft.tileY + leaf.area.height -1 - divisionHeight);

            newLeaf1 = new BSPLeaf(newRoom1, leaf);
            newLeaf2 = new BSPLeaf(newRoom2, leaf);
        }
        else // vertical bisection
        {
            int lowend = leaf.area.btmLeft.tileX + offset;
            int highend = leaf.area.btmLeft.tileX + leaf.area.width - offset;
            int divisionWidth = pseudoRandom.Next(lowend, highend);

            newRoom1 = new Area(leaf.area.btmLeft, divisionWidth - leaf.area.btmLeft.tileX, leaf.area.height);
            Coord newCoord2 = new Coord(divisionWidth + 1, leaf.area.btmLeft.tileY);
            newRoom2 = new Area(newCoord2, leaf.area.btmLeft.tileX + leaf.area.width -1 - divisionWidth, leaf.area.height);

            newLeaf1 = new BSPLeaf(newRoom1, leaf);
            newLeaf2 = new BSPLeaf(newRoom2, leaf);
        }

        leaf.child1 = newLeaf1;
        leaf.child2 = newLeaf2;

        DivideRoom(newLeaf1);
        DivideRoom(newLeaf2);
    }

    private void CreateRoom(BSPLeaf leaf) { 
        int roomWidth;
        int roomHeight;
    
        if(leaf.area.width < minRoomWidth)
            roomWidth = leaf.area.width;
        else
            roomWidth = pseudoRandom.Next(minRoomWidth, leaf.area.width + 1);
        if (leaf.area.height < minRoomHeight)
            roomHeight = leaf.area.height;
        else
            roomHeight = pseudoRandom.Next(minRoomHeight, leaf.area.height + 1);

        int coordX = leaf.area.btmLeft.tileX + pseudoRandom.Next(0, leaf.area.width +1 - roomWidth);
        int coordY = leaf.area.btmLeft.tileY + pseudoRandom.Next(0, leaf.area.height +1 - roomHeight);

        Room newRoom = new Room(new Coord(coordX, coordY), roomWidth, roomHeight);
        leaf.area.room = newRoom;
        //DrawArea(leaf.area);
        //DrawRoom(leaf.area.room);
    }

    private void DrawRoom(Room room)
    {
        foreach(Coord c in room.tiles)
        {
            map[c.tileX, c.tileY] = 0;
        }
    }

    private void DrawArea(Area area)
    {
        for(int x = area.btmLeft.tileX; x < area.btmLeft.tileX + area.width; x++)
        {
            for (int y = area.btmLeft.tileY; y < area.btmLeft.tileY + area.height; y++)
            {
                map[x, y] = 0;
            }
        }
    }

    private Room Connect(BSPLeaf leaf)
    {
        if (leaf.child1 == null && leaf.child2 == null)
            return leaf.area.room;

        Room roomComplex1 = Connect(leaf.child1);
        Room roomComplex2 = Connect(leaf.child2);

        List<Connection> connections = new List<Connection>();
        List<Connection> straightConnections = new List<Connection>();
        int bestDistance = -1;

        foreach (Coord c1 in roomComplex1.edgeTiles)
        {
            foreach (Coord c2 in roomComplex2.edgeTiles)
            {
                int distanceBetweenRooms = (int)(Mathf.Pow(c1.tileX - c2.tileX, 2) + Mathf.Pow(c1.tileY - c2.tileY, 2));
                if(c1.tileX == c2.tileX || c1.tileY == c2.tileY)
                {
                    straightConnections.Add(new Connection(c1, c2, distanceBetweenRooms));
                }
                if (bestDistance > distanceBetweenRooms || bestDistance == -1)
                {
                    bestDistance = distanceBetweenRooms;
                    connections.Clear();
                    connections.Add(new Connection(c1, c2, distanceBetweenRooms));
                } else if(bestDistance == distanceBetweenRooms)
                    connections.Add(new Connection(c1, c2, distanceBetweenRooms));
            }
        }

        List<Coord> connectedRoom = new List<Coord>();
        connectedRoom.AddRange(roomComplex1.tiles);
        connectedRoom.AddRange(roomComplex2.tiles);

        List<Coord> connectedRoomEdges = new List<Coord>();
        connectedRoomEdges.AddRange(roomComplex1.edgeTiles);
        connectedRoomEdges.AddRange(roomComplex2.edgeTiles);

        int depth = GetTreeDepth(leaf);
        int corridorsAmount = pseudoRandom.Next(1 + depth/3, 1 + depth/2);
        int index = 0;
        connections.AddRange(straightConnections);

        for (int i = 0; i < corridorsAmount; i++)
        {
            if(connections.Count > 0)
                 index = pseudoRandom.Next(0, connections.Count);

            Connection chosenConnection = connections[index];
            List<Coord> corridor = CreateCorridor(chosenConnection.connectionTileA, chosenConnection.connectionTileB);
            connectedRoom.AddRange(corridor);
            connectedRoomEdges.AddRange(corridor);
        }
        
        leaf.area.room = new Room();
        leaf.area.room.tiles = connectedRoom;
        leaf.area.room.edgeTiles = connectedRoomEdges;

        return leaf.area.room;
    }

    private int GetTreeDepth(BSPLeaf leaf)
    {
        int depth = 1;
        while(leaf.child1 != null)
        {
            leaf = leaf.child1;
            depth++;
        }
        return depth;
    }

    private List<Coord> CreateCorridor(Coord tile1, Coord tile2)
    {
        List<Coord> passage = new List<Coord>();

        Coord nextTile = new Coord();
        int xTmp = tile1.tileX, yTmp = tile1.tileY;
        int xAdd = 1, yAdd = 1;

        if (tile1.tileX > tile2.tileX)
            xAdd = -1;
        if (tile1.tileY > tile2.tileY)
            yAdd = -1;

        if (tile1.tileX == tile2.tileX)
            yTmp += yAdd;
        else if (yTmp == tile2.tileY)
            xTmp += xAdd;
        else if (pseudoRandom.Next(0, 2) == 0)
            xTmp += xAdd;
        else
            yTmp += yAdd;

        nextTile = new Coord(xTmp, yTmp);

        while (nextTile.tileX != tile2.tileX || nextTile.tileY != tile2.tileY)
        {
            passage.Add(nextTile);

            if (xTmp == tile2.tileX)
                yTmp += yAdd;
            else if (yTmp == tile2.tileY)
                xTmp += xAdd;
            else if (pseudoRandom.Next(0, 2) == 0)
                xTmp += xAdd;
            else
                yTmp += yAdd;

            nextTile = new Coord(xTmp, yTmp);
        }

        return passage;
    }

    class BSPLeaf
    {
        public Area area;
        public BSPLeaf parent;
        public BSPLeaf child1 = null, child2 = null;

        public BSPLeaf(Area a, BSPLeaf p)
        {
            area = a;
            parent = p;
        }

        public BSPLeaf(Area a)
        {
            area = a;
            parent = null;
        }
    }

    class Area
    {
        public Coord btmLeft;
        public int width, height;
        public Room room;

        public Area(Coord c, int w, int h)
        {
            btmLeft = c;
            width = w;
            height = h;
        }
    }

    class Room : IComparable<Room>
    {
        public List<Coord> tiles;
        public List<Coord> edgeTiles;
        public int roomSize;

        public Room()
        {
        }

        public Room(List<Coord> roomTiles, int[,] map)
        {
            tiles = roomTiles;
            roomSize = tiles.Count;

            edgeTiles = new List<Coord>();
            foreach (Coord tile in tiles)
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

        public Room(Coord coord, int roomWidth, int roomHeight)
        {
            tiles = new List<Coord>();
            edgeTiles = new List<Coord>();
            for(int x = 0; x < roomWidth; x++){
                for(int y = 0; y < roomHeight; y++)
                {
                    Coord newCoord = new Coord(coord.tileX + x, coord.tileY + y);
                    tiles.Add(newCoord);
                    if(x == 0 || y == 0 || x == roomWidth - 1 || y == roomHeight - 1)
                        edgeTiles.Add(newCoord);
                }
            }
        }

        public int CompareTo(Room otherRoom)
        {
            return this.roomSize.CompareTo(otherRoom.roomSize);
        }
    }

    struct Coord
    {
        public int tileX;
        public int tileY;

        public Coord(int x, int y)
        {
            tileX = x;
            tileY = y;
        }
    }

    class Connection : IComparable<Connection>
    {
        public Coord connectionTileA;
        public Coord connectionTileB;
        public Int32 distance;

        public Connection(Coord a, Coord b, Int32 d)
        {
            connectionTileA = a;
            connectionTileB = b;
            distance = d;
        }

        public int CompareTo(Connection otherConnection)
        {
            //return otherRoom.roomSize.CompareTo(roomSize);
            return this.distance.CompareTo(otherConnection.distance);
        }
    }
}

