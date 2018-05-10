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
        BSPArea mainArea = new BSPArea(btmLeft, columns - 2, rows - 2);
        BSPNode mainNode = new BSPNode(mainArea);
        DivideArea(mainNode);
        Connect(mainNode);
        DrawRooms(mainNode.area.rooms);
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

    private void DivideArea(BSPNode node)
    {
        int w = node.area.width;
        int h = node.area.height;
        int division; //1 means vertical, 0 means horizontal

        if (w < minWidth)
        {
            if (h < minHeight)
            {
                CreateRoom(node);
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

        BSPNode newLeaf1, newLeaf2;
        BSPArea newArea1, newArea2;

        if(division == 0) // horizontal bisection
        {
            int lowend = node.area.btmLeft.tileY + offset;
            int highend = node.area.btmLeft.tileY + node.area.height - offset;
            int divisionHeight =  pseudoRandom.Next(lowend, highend);

            newArea1 = new BSPArea(node.area.btmLeft, node.area.width, divisionHeight - node.area.btmLeft.tileY);
            Coord newCoord2 = new Coord(node.area.btmLeft.tileX, divisionHeight + 1);
            newArea2 = new BSPArea(newCoord2, node.area.width, node.area.btmLeft.tileY + node.area.height -1 - divisionHeight);

            newLeaf1 = new BSPNode(newArea1, node);
            newLeaf2 = new BSPNode(newArea2, node);
        }
        else // vertical bisection
        {
            int lowend = node.area.btmLeft.tileX + offset;
            int highend = node.area.btmLeft.tileX + node.area.width - offset;
            int divisionWidth = pseudoRandom.Next(lowend, highend);

            newArea1 = new BSPArea(node.area.btmLeft, divisionWidth - node.area.btmLeft.tileX, node.area.height);
            Coord newCoord2 = new Coord(divisionWidth + 1, node.area.btmLeft.tileY);
            newArea2 = new BSPArea(newCoord2, node.area.btmLeft.tileX + node.area.width -1 - divisionWidth, node.area.height);

            newLeaf1 = new BSPNode(newArea1, node);
            newLeaf2 = new BSPNode(newArea2, node);
        }

        node.child1 = newLeaf1;
        node.child2 = newLeaf2;

        DivideArea(newLeaf1);
        DivideArea(newLeaf2);
    }

    private void CreateRoom(BSPNode node) { 
        int roomWidth;
        int roomHeight;
    
        if(node.area.width < minRoomWidth)
            roomWidth = node.area.width;
        else
            roomWidth = pseudoRandom.Next(minRoomWidth, node.area.width + 1);
        if (node.area.height < minRoomHeight)
            roomHeight = node.area.height;
        else
            roomHeight = pseudoRandom.Next(minRoomHeight, node.area.height + 1);

        int coordX = node.area.btmLeft.tileX + pseudoRandom.Next(0, node.area.width +1 - roomWidth);
        int coordY = node.area.btmLeft.tileY + pseudoRandom.Next(0, node.area.height +1 - roomHeight);

        BSPRoom newRoom = new BSPRoom(new Coord(coordX, coordY), roomWidth, roomHeight);
        node.area.rooms.Add(newRoom);
    }

    private void DrawRooms(List<BSPRoom> rooms)
    {
        foreach(BSPRoom room in rooms)
        {
            if (room.corridor)
            {
                foreach (Coord c in room.edgeTiles)
                    map[c.tileX, c.tileY] = 0;
            }
            else
            {
                for (int i = room.btmLeft.tileX; i < room.btmLeft.tileX + room.width; i++)
                    for (int j = room.btmLeft.tileY; j < room.btmLeft.tileY + room.height; j++)
                        map[i, j] = 0;
            }
        }
    }

    private void DrawArea(BSPArea area)
    {
        for(int x = area.btmLeft.tileX; x < area.btmLeft.tileX + area.width; x++)
        {
            for (int y = area.btmLeft.tileY; y < area.btmLeft.tileY + area.height; y++)
            {
                map[x, y] = 0;
            }
        }
    }

    private List<BSPRoom> Connect(BSPNode leaf)
    {
        if (leaf.child1 == null && leaf.child2 == null)
            return leaf.area.rooms;

        List<BSPRoom> roomComplex1 = Connect(leaf.child1);
        List<BSPRoom> roomComplex2 = Connect(leaf.child2);

        List<List<Connection>> connectionsList = new List<List<Connection>>();
        int bestDistance = -1;

        foreach(BSPRoom room1 in roomComplex1)
        {
            List<Connection> connectionsBetweenRooms = new List<Connection>();
            foreach(BSPRoom room2 in roomComplex2)
            {
                int tmpBest;
                List < Connection > tmpList = GetBestConnections(room1, room2, out tmpBest);
                if (tmpBest < bestDistance || bestDistance == -1)
                {
                    bestDistance = tmpBest;
                    connectionsBetweenRooms = tmpList;
                }
                else if (tmpBest == bestDistance)
                    connectionsBetweenRooms.AddRange(tmpList);

            }
            if(bestDistance != -1)
                connectionsList.Add(connectionsBetweenRooms);
            bestDistance = -1;
        }

        leaf.area.rooms.AddRange(roomComplex1);
        leaf.area.rooms.AddRange(roomComplex2);

        int depth = GetTreeDepth(leaf);
        int connectionsAmount = pseudoRandom.Next(1 + depth/3, 1 + depth/2);
        int index = 0;

        for (int i = 0; i < connectionsAmount; i++)
        {
            if (connectionsList.Count == 0)
                break;

            index = pseudoRandom.Next(0, connectionsList.Count);
            List<Connection> chosenConnection = connectionsList[index];

            index = pseudoRandom.Next(0, chosenConnection.Count);
            Connection connection = chosenConnection[index];

            List<Coord> corridor = CreateConnectionRoom(connection);
            leaf.area.rooms.Add(new BSPRoom(corridor));

            connectionsList.Remove(chosenConnection);
        }
        
        return leaf.area.rooms;
    }

    private List<Connection> GetBestConnections(BSPRoom room1, BSPRoom room2, out int bestDistance)
    {
        List<Connection> connectionsBetweenRooms = new List<Connection>();
        bestDistance = -1;

        foreach (Coord c1 in room1.edgeTiles)
        {
            foreach (Coord c2 in room2.edgeTiles)
            {
                int distanceBetweenRooms = (int)(Mathf.Pow(c1.tileX - c2.tileX, 2) + Mathf.Pow(c1.tileY - c2.tileY, 2));
                if (bestDistance > distanceBetweenRooms || bestDistance == -1)
                {
                    bestDistance = distanceBetweenRooms;
                    connectionsBetweenRooms.Clear();
                    connectionsBetweenRooms.Add(new Connection(c1, c2, distanceBetweenRooms));
                }
                else if (bestDistance == distanceBetweenRooms)
                    connectionsBetweenRooms.Add(new Connection(c1, c2, distanceBetweenRooms));
            }
        }

        return connectionsBetweenRooms;
    }

    private int GetTreeDepth(BSPNode leaf)
    {
        int depth = 1;

        if (leaf.child1 == null)
            return depth;

        int depth1 = GetTreeDepth(leaf.child1);
        int depth2 = GetTreeDepth(leaf.child2);

        if (depth1 < depth2)
            depth += depth1;
        else
            depth += depth2;

        return depth;
    }

    private List<Coord> CreateConnectionRoom(Connection connection)
    {
        Coord tile1 = connection.connectionTileA, tile2 = connection.connectionTileB;
        List<Coord> connectionRoom = new List<Coord>();

        Vector2 pos = new Vector2(tile1.tileX, tile1.tileY);

        int xDir = 0, yDir = 0;

        if (tile1.tileX <= tile2.tileX)
            xDir = 1;
        else
            xDir = -1;

        while (pos.x != tile2.tileX)
        {
            connectionRoom.Add(new Coord((int)pos.x, (int)pos.y));

            pos = new Vector2(pos.x + xDir, pos.y);
        }

        if (tile1.tileY <= tile2.tileY)
            yDir = 1;
        else
            yDir = -1;

        while (pos.y != tile2.tileY)
        {
            connectionRoom.Add(new Coord((int)pos.x, (int)pos.y));

            pos = new Vector2(pos.x, pos.y + yDir);
        }

        return connectionRoom;
    }

    class BSPNode
    {
        public BSPArea area;
        public BSPNode parent;
        public BSPNode child1 = null, child2 = null;

        public BSPNode(BSPArea a, BSPNode p)
        {
            area = a;
            parent = p;
        }

        public BSPNode(BSPArea a)
        {
            area = a;
            parent = null;
        }
    }

    class BSPArea
    {
        public Coord btmLeft;
        public int width, height;
        public List<BSPRoom> rooms;

        public BSPArea(Coord c, int w, int h)
        {
            btmLeft = c;
            width = w;
            height = h;
            rooms = new List<BSPRoom>();
        }
    }

    class BSPRoom
    {
        public Coord btmLeft;
        public int width, height;
        public int roomSize;
        public List<Coord> edgeTiles;
        public bool corridor;

        public BSPRoom()
        {
        }

        public BSPRoom(List<Coord> tiles)
        {
            this.corridor = true;
            this.edgeTiles = tiles;
        }

        public BSPRoom(Coord coord, int roomWidth, int roomHeight)
        {
            corridor = false;
            btmLeft = coord;
            width = roomWidth;
            height = roomHeight;

            edgeTiles = new List<Coord>();
            for(int x = 0; x < roomWidth; x++){
                for(int y = 0; y < roomHeight; y++)
                {
                    Coord newCoord = new Coord(coord.tileX + x, coord.tileY + y);
                    if(x == 0 || y == 0 || x == roomWidth - 1 || y == roomHeight - 1)
                        edgeTiles.Add(newCoord);
                }
            }
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

