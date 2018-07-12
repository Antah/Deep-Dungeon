using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;


public class BTLevelGenerator : LevelGenerator
{
    public string seed;
    public bool useRandomSeed;

    public int columns = 100;                                 // The number of columns on the board (how wide it will be).
    public int rows = 100;

    public int minAreaWidth, minAreaHeight;
    public int minRoomWidth, minRoomHeight, maxRoomWidth, maxRoomHeight;
    public Boolean alternativeMinCheck;
    public int offset;
    public int maxCorridorLengthX, maxCorridorLengthY;

    int[,] map;

    public GameObject[] floorTiles;                           // An array of floor tile prefabs.
    public GameObject[] wallTiles;                            // An array of wall tile prefabs.
    public GameObject[] rubbleTiles;
    public GameObject exit;

    private System.Random pseudoRandom;
    private int roomCount;
    private float st, mt, et;

    public override void SetupScene(int level)
    {
        this.level = level;
        SetupParameters();
        CreateLevel();
    }

    private void SetupParameters()
    {
        BTSettings settings = UIBTSettings.instance.GetSettings();

        seed = settings.seed;
        useRandomSeed = settings.useRandomSeed;

        columns = settings.levelWidth;
        rows = settings.levelHeight;

        minAreaWidth = settings.minAreaWidth;
        minAreaHeight = settings.minAreaHeight;
        offset = settings.separationOffset;

        minRoomWidth = settings.minRoomWidth;
        minRoomHeight = settings.minRoomHeight;
        maxRoomWidth = settings.maxRoomWidth;
        maxRoomHeight = settings.maxRoomHeight;
        alternativeMinCheck = settings.useOrCheck;

        maxCorridorLengthX = settings.maxConnectionLengthX;
        maxCorridorLengthY = settings.maxConnectionLengthY;
    }

    private void CreateLevel()
    {
        roomCount = 0;
        st = Time.realtimeSinceStartup;

        boardHolder = new GameObject("BoardHolder");
        GenerateMap();
        

        InstantiateTiles();
        

        //Debug.Log("Test number: " + level + " - rooms: " + roomCount + " - generation: " + (mt - st) + " - visualization: " + (et - st));
        //TestLogger.AddLine(level + "\t" + roomCount + "\t" + (mt - st) + "\t" + (et - st));
    }

    private void GenerateMap()
    {
        //Create new empty map
        map = new int[columns, rows];

        //Get random seed or use provided one
        if (useRandomSeed)
        {
            seed = Time.time.ToString();
        }
        pseudoRandom = new System.Random(seed.GetHashCode());

        //Fill map with 1(walls)
        for (int x = 0; x < columns; x++)
            for (int y = 0; y < rows; y++)
                map[x, y] = 1;

        //Create main area
        Tile btmLeft = new Tile(1, 1);
        BTArea mainArea = new BTArea(btmLeft, columns - 2, rows - 2);
        BTNode mainNode = new BTNode(mainArea);

        //Divide recursively main area into smaller ones
        DivideArea(mainNode);

        //Connect recursively all rooms
        ConnectRooms(mainNode);

        //Draw rooms - set 
        DrawRooms(mainNode.area.rooms);
        mt = Time.realtimeSinceStartup;
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
                }
            }
        }

        Vector3 playerPos = new Vector3(columns / 2, rows / 2, 0);
        GameManager.instance.GetPlayer().transform.position = playerPos;
        et = Time.realtimeSinceStartup;
    }

    private void DivideArea(BTNode node)
    {
        int w = node.area.width;
        int h = node.area.height;
        int division; //1 means vertical, 0 means horizontal

        if (w < minAreaWidth)
        {
            if (h < minAreaHeight)
            {
                CreateRoom(node);
                return;
            }
            else
                division = 0;
        }
        else if (h < minAreaHeight)
        {
            division = 1;
        }
        else
        {
            division = pseudoRandom.Next(0, 2);
        }

        BTNode newLeaf1, newLeaf2;
        BTArea newArea1, newArea2;

        if(division == 0) // horizontal bisection
        {
            int lowend = node.area.btmLeft.tileY + offset;
            int highend = node.area.btmLeft.tileY + node.area.height - offset;
            int divisionHeight =  pseudoRandom.Next(lowend, highend);

            newArea1 = new BTArea(node.area.btmLeft, node.area.width, divisionHeight - node.area.btmLeft.tileY);
            Tile newTile = new Tile(node.area.btmLeft.tileX, divisionHeight + 1);
            newArea2 = new BTArea(newTile, node.area.width, node.area.btmLeft.tileY + node.area.height -1 - divisionHeight);

            newLeaf1 = new BTNode(newArea1, node);
            newLeaf2 = new BTNode(newArea2, node);
        }
        else // vertical bisection
        {
            int lowend = node.area.btmLeft.tileX + offset;
            int highend = node.area.btmLeft.tileX + node.area.width - offset;
            int divisionWidth = pseudoRandom.Next(lowend, highend);

            newArea1 = new BTArea(node.area.btmLeft, divisionWidth - node.area.btmLeft.tileX, node.area.height);
            Tile newTile = new Tile(divisionWidth + 1, node.area.btmLeft.tileY);
            newArea2 = new BTArea(newTile, node.area.btmLeft.tileX + node.area.width -1 - divisionWidth, node.area.height);

            newLeaf1 = new BTNode(newArea1, node);
            newLeaf2 = new BTNode(newArea2, node);
        }

        node.child1 = newLeaf1;
        node.child2 = newLeaf2;

        DivideArea(newLeaf1);
        DivideArea(newLeaf2);
    }

    private void CreateRoom(BTNode node) {
        int roomWidth;
        int roomHeight;
        int maxWidth, maxHeight;

        if (alternativeMinCheck)
        {
            if (node.area.width < minRoomWidth && node.area.height < minRoomHeight)
            {
                node = null;
                return;
            }
        }
        else
        {
            if (node.area.width < minRoomWidth || node.area.height < minRoomHeight)
            {
                node = null;
                return;
            }
        }
        roomCount++;

        if (maxRoomWidth > node.area.width)
            maxWidth = node.area.width;
        else
            maxWidth = maxRoomWidth;
        if (maxRoomHeight > node.area.height)
            maxHeight = node.area.height;
        else
            maxHeight = maxRoomHeight;


        if (node.area.width < minRoomWidth)
            roomWidth = node.area.width;
        else
            roomWidth = pseudoRandom.Next(minRoomWidth, maxWidth + 1);
        if (node.area.height < minRoomHeight)
            roomHeight = node.area.height;
        else
            roomHeight = pseudoRandom.Next(minRoomHeight, maxHeight + 1);

        int coordX = node.area.btmLeft.tileX + pseudoRandom.Next(0, node.area.width +1 - roomWidth);
        int coordY = node.area.btmLeft.tileY + pseudoRandom.Next(0, node.area.height +1 - roomHeight);

        BTRoom newRoom = new BTRoom(new Tile(coordX, coordY), roomWidth, roomHeight);
        node.area.rooms.Add(newRoom);
        DrawRooms(node.area.rooms);
    }

    private void DrawRooms(List<BTRoom> rooms)
    {
        foreach(BTRoom room in rooms)
        {
            if (room.corridor)
            {
                foreach (Tile t in room.edgeTiles)
                    map[t.tileX, t.tileY] = 0;
            }
            else
            {
                for (int i = room.btmLeft.tileX; i < room.btmLeft.tileX + room.width; i++)
                    for (int j = room.btmLeft.tileY; j < room.btmLeft.tileY + room.height; j++)
                        map[i, j] = 0;
            }
        }
    }

    private void DrawArea(BTArea area)
    {
        for(int x = area.btmLeft.tileX; x < area.btmLeft.tileX + area.width; x++)
        {
            for (int y = area.btmLeft.tileY; y < area.btmLeft.tileY + area.height; y++)
            {
                map[x, y] = 0;
            }
        }
    }

    private List<BTRoom> ConnectRooms(BTNode node)
    {
        if (node.child1 == null && node.child2 == null)
            return node.area.rooms;
        if (node.child1 == null)
        {
            List<BTRoom> roomComplex = ConnectRooms(node.child2);
            node.area.rooms.AddRange(roomComplex);
            return node.area.rooms;
        }

        if (node.child2 == null)
        {
            List<BTRoom> roomComplex = ConnectRooms(node.child1);
            node.area.rooms.AddRange(roomComplex);
            return node.area.rooms;
        }

        List<BTRoom> roomComplex1 = ConnectRooms(node.child1);
        List<BTRoom> roomComplex2 = ConnectRooms(node.child2);

        node.area.rooms.AddRange(roomComplex1);
        node.area.rooms.AddRange(roomComplex2);

        List<BTConnection> connectionsList = new List<BTConnection>();
        int bestDistance = -1;

        foreach(BTRoom room1 in roomComplex1)
        {
            BTConnection connectionBetweenRooms = null;
            foreach(BTRoom room2 in roomComplex2)
            {
                int tmpBest;
                BTConnection tmpConnection = GetBestConnection(room1, room2, out tmpBest);

                if (tmpBest != -1 && (tmpBest < bestDistance || bestDistance == -1))
                {
                    bestDistance = tmpBest;
                    connectionBetweenRooms = tmpConnection;
                }
            }

            if(connectionBetweenRooms != null)
                connectionsList.Add(connectionBetweenRooms);
            bestDistance = -1;
        }

        int depth = GetTreeDepth(node);
        int connectionsAmount = pseudoRandom.Next(1 + depth/3, 1 + depth/2);

        for (int i = 0; i < connectionsAmount; i++)
        {
            if (connectionsList.Count == 0)
                break;

            int index = pseudoRandom.Next(0, connectionsList.Count);
            BTConnection connection = connectionsList[index];

            List<Tile> corridor = CreateCorridorRoom(connection);
            node.area.rooms.Add(new BTRoom(corridor));

            connectionsList.Remove(connection);
        }
        
        return node.area.rooms;
    }

    private BTConnection GetBestConnection(BTRoom room1, BTRoom room2, out int bestDistance)
    {
        List<BTConnection> connectionsBetweenRooms = new List<BTConnection>();
        bestDistance = -1;

        foreach (Tile t1 in room1.edgeTiles)
        {
            foreach (Tile t2 in room2.edgeTiles)
            {
                if (Math.Abs(t1.tileX - t2.tileX) > maxCorridorLengthX || Math.Abs(t1.tileY - t2.tileY) > maxCorridorLengthY)
                    continue;

                int distanceBetweenRooms = (int)(Mathf.Pow(t1.tileX - t2.tileX, 2) + Mathf.Pow(t1.tileY - t2.tileY, 2));

                if (bestDistance > distanceBetweenRooms || bestDistance == -1)
                {
                    bestDistance = distanceBetweenRooms;
                    connectionsBetweenRooms.Clear();
                    connectionsBetweenRooms.Add(new BTConnection(t1, t2, distanceBetweenRooms));
                }
                else if (bestDistance == distanceBetweenRooms)
                    connectionsBetweenRooms.Add(new BTConnection(t1, t2, distanceBetweenRooms));
            }
        }

        if (bestDistance != -1)
        {
            int index = pseudoRandom.Next(0, connectionsBetweenRooms.Count);
            BTConnection connection = connectionsBetweenRooms[index];
            return connection;
        }

        return null;
    }

    private int GetTreeDepth(BTNode node)
    {
        int depth = 1;

        if (node.child1 == null || node.child2 == null)
            return depth;

        int depth1 = GetTreeDepth(node.child1);
        int depth2 = GetTreeDepth(node.child2);

        if (depth1 < depth2)
            depth += depth1;
        else
            depth += depth2;

        return depth;
    }

    private List<Tile> CreateCorridorRoom(BTConnection connection)
    {
        Tile tile1 = connection.connectionTileA, tile2 = connection.connectionTileB;
        List<Tile> corridorRoom = new List<Tile>();

        Vector2 pos = new Vector2(tile1.tileX, tile1.tileY);

        int xDir = 0, yDir = 0;

        if (tile1.tileX <= tile2.tileX)
            xDir = 1;
        else
            xDir = -1;

        while (pos.x != tile2.tileX)
        {
            corridorRoom.Add(new Tile((int)pos.x, (int)pos.y));

            pos = new Vector2(pos.x + xDir, pos.y);
        }

        if (tile1.tileY <= tile2.tileY)
            yDir = 1;
        else
            yDir = -1;

        while (pos.y != tile2.tileY)
        {
            corridorRoom.Add(new Tile((int)pos.x, (int)pos.y));

            pos = new Vector2(pos.x, pos.y + yDir);
        }

        return corridorRoom;
    }
}

