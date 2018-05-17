using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class CABoardCreator : BoardCreator
{
    public string seed;
    public bool useRandomSeed;

    public int columns = 100;
    public int rows = 100;

    [Range(0, 100)]
    public int wallsFillPercent;

    public int smoothingIterations;
    public NeighbourhoodType neighbourhoodType;
    public int surviveMin, surviveMax, newMin, newMax;

    public bool hybridMode;
    public int smoothingIterationsHybrid;
    public NeighbourhoodType neighbourhoodTypeHybrid;
    public int surviveMinHybrid, surviveMaxHybrid, newMinHybrid, newMaxHybrid;

    public int roomThreshold;
    public int wallThreshold;
    public int roomUpperThreshold;
    public int wallUpperThreshold;

    public bool connectRooms;
    public int connectionUpperThreshold;
    public ConnectionType connectionType;
    public int connectionSizeFactor;
    public int maxCorridorLengthX, maxCorridorLengthY;



    public GameObject[] floorTiles;                           // An array of floor tile prefabs.
    public GameObject[] wallTiles;                            // An array of wall tile prefabs.
    public GameObject[] rubbleTiles;
    public GameObject exit;

    int[,] map;
    private System.Random pseudoRandom;

    public override void SetupScene(int level)
    {
        this.level = level;
        BoardSetup();
    }
 
    private void BoardSetup()
    {
        //float start = Time.realtimeSinceStartup;
        boardHolder = new GameObject("BoardHolder");

        GenerateMap();

        List<CARoom> survivingRooms = OptimizeRooms();

        if (connectRooms && survivingRooms.Count > 0)
        {
            survivingRooms.Sort();
            survivingRooms.Reverse();
            survivingRooms[0].isMainRoom = true;
            survivingRooms[0].isAccessibleFromMainRoom = true;
            ConnectRooms(survivingRooms);
        }

        //float midpoint = Time.realtimeSinceStartup;
        //StartCoroutine(DelayedInitialisation());
        InstantiateTiles();

        Vector3 playerPos = new Vector3(columns/2, rows/2, 0);
        GameManager.instance.GetPlayer().transform.position = playerPos;
        Vector3 exitPos = new Vector3(columns / 2 + 1, rows / 2, 0);
        GameObject tileInstance = Instantiate(exit, exitPos, Quaternion.identity) as GameObject;
        tileInstance.transform.parent = boardHolder.transform;

        //float end = Time.realtimeSinceStartup;
        //Debug.Log("Finish time: " + (midpoint - start) + "\nWith initialization: " + (end - start));
    }

    IEnumerator DelayedInitialisation()
    {
        print(Time.time);
        yield return new WaitForSeconds(2);
        print(Time.time);
        InstantiateTiles();
    }

    private void InstantiateTiles()
    {
        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                if (map[i, j] == 0)
                {
                    InstantiateFromArray(floorTiles, i, j);
                }
                else
                {
                    InstantiateFromArray(wallTiles, i, j);
                }
            }
        }
    }

    void GenerateMap()
    {
        map = new int[columns, rows];
        if (useRandomSeed)
        {
            seed = Time.time.ToString();
        }

        RandomFillMap();

        Ruleset ruleset = new Ruleset(neighbourhoodType, surviveMin, surviveMax, newMin, newMax);
        for (int i = 0; i < smoothingIterations; i++)
        {
            SmoothMap(ruleset);
        }

        if (hybridMode)
        {
            Ruleset ruleset2 = new Ruleset(neighbourhoodTypeHybrid, surviveMinHybrid, surviveMaxHybrid, newMinHybrid, newMaxHybrid);
            for (int i = 0; i < smoothingIterationsHybrid; i++)
            {
                SmoothMap(ruleset2);
            }
        }
    }
    void GenerateArea(List<Tile> area)
    {
        RandomFillArea(area);

        Ruleset ruleset = new Ruleset(neighbourhoodType, surviveMin, surviveMax, newMin, newMax);
        for (int i = 0; i < smoothingIterations; i++)
        {
            SmoothArea(ruleset, area);
        }
        if (hybridMode)
        {
            Ruleset ruleset2 = new Ruleset(neighbourhoodTypeHybrid, surviveMinHybrid, surviveMaxHybrid, newMinHybrid, newMaxHybrid);
            for (int i = 0; i < smoothingIterationsHybrid; i++)
            {
                SmoothArea(ruleset2, area);
            }
        }
    }

    void RandomFillMap()
    {
        pseudoRandom = new System.Random(seed.GetHashCode());

        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                if (x == 0 || x == columns - 1 || y == 0 || y == rows - 1)
                {
                    map[x, y] = 1;
                }
                else
                {
                    if (pseudoRandom.Next(0, 100) < wallsFillPercent)
                    {
                        map[x, y] = 1;
                    }
                    else
                    {
                        map[x, y] = 0;
                    }

                }
            }
        }
    }
    void RandomFillArea(List<Tile> area)
    {
        System.Random pseudoRandom = new System.Random(seed.GetHashCode());

        foreach (Tile c in area)
        {
            if (c.tileX == 0 || c.tileX == columns - 1 || c.tileY == 0 || c.tileY == rows - 1)
            {
                map[c.tileX, c.tileY] = 1;
            }
            else
            {
                if (pseudoRandom.Next(0, 100) < wallsFillPercent)
                {
                    map[c.tileX, c.tileY] = 1;
                }
                else
                {
                    map[c.tileX, c.tileY] = 0;
                }

            }
        }
    }

    void AddBorderToMap(int borderSize)
    {
        int[,] borderedMap = new int[columns + borderSize * 2, rows + borderSize * 2];

        for (int x = 0; x < borderedMap.GetLength(0); x++)
        {
            for (int y = 0; y < borderedMap.GetLength(1); y++)
            {
                if (x >= borderSize && x < columns + borderSize && y >= borderSize && y < rows + borderSize)
                {
                    borderedMap[x, y] = map[x - borderSize, y - borderSize];
                }
                else
                {
                    borderedMap[x, y] = 1;
                }
            }
        }
        map = borderedMap;
    }

    void SmoothMap(Ruleset ruleset)
    {
        int[,] tmpMap = new int[map.GetLength(0), map.GetLength(1)];
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                tmpMap[x, y] = map[x, y];
                if (x == 0 || y == 0 || x == columns -1 || y == rows -1)
                    continue;

                int neighbourRoomTiles = CheckNeighbourhood(x, y, ruleset.neighbourhoodType);

                if (tmpMap[x, y] == 1 && neighbourRoomTiles >= ruleset.newMin && neighbourRoomTiles <= ruleset.newMax)
                {
                    tmpMap[x, y] = 0;
                }
                else if (tmpMap[x, y] == 0 && neighbourRoomTiles >= ruleset.survMin && neighbourRoomTiles <= ruleset.survMax)
                {
                    tmpMap[x, y] = 0;
                }
                else
                {
                    tmpMap[x, y] = 1;
                }
            }
        }
        map = tmpMap;
    }
    void SmoothArea(Ruleset ruleset, List<Tile> area)
    {
        int[,] tmpMap = new int[map.GetLength(0), map.GetLength(1)];
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                tmpMap[x, y] = map[x, y];
            }
        }

        foreach (Tile c in area)
        {
            int neighbourRoomTiles = CheckNeighbourhood(c.tileX, c.tileY, ruleset.neighbourhoodType);

            if (tmpMap[c.tileX, c.tileY] == 1 && neighbourRoomTiles >= ruleset.newMin && neighbourRoomTiles <= ruleset.newMax)
            {
                tmpMap[c.tileX, c.tileY] = 0;
            }
            else if (tmpMap[c.tileX, c.tileY] == 0 && neighbourRoomTiles >= ruleset.survMin && neighbourRoomTiles <= ruleset.survMax)
            {
                tmpMap[c.tileX, c.tileY] = 0;
            }
            else
            {
                tmpMap[c.tileX, c.tileY] = 1;
            }
        }
        map = tmpMap;
    }

    private int CheckNeighbourhood(int x, int y, NeighbourhoodType nt)
    {
        if (nt == NeighbourhoodType.Moore)
            return MooreNeighbourhood(x, y);
        else if (nt == NeighbourhoodType.Neuman)
            return NeumanNeighbourhood(x, y);

        return 0;
    }

    List<CARoom> OptimizeRooms()
    {
        List<List<Tile>> wallRegions = GetRegions(1);

        foreach (List<Tile> wallRegion in wallRegions)
        {
            if (wallRegion.Count < wallThreshold)
            {
                foreach (Tile tile in wallRegion)
                {
                    map[tile.tileX, tile.tileY] = 0;
                }
            }
        }

        List<List<Tile>> roomRegions = GetRegions(0);
        List<CARoom> survivingRooms = new List<CARoom>();

        foreach (List<Tile> roomRegion in roomRegions)
        {
            if (roomRegion.Count < roomThreshold)
            {
                foreach (Tile tile in roomRegion)
                {
                    map[tile.tileX, tile.tileY] = 1;
                }
            } else if(roomRegion.Count > roomUpperThreshold)
            {
                GenerateArea(roomRegion);
                List<CARoom> newSurvivingRooms = OptimizeRoomsInArea(roomRegion);
                survivingRooms.AddRange(newSurvivingRooms);
            }
            else
            {
                survivingRooms.Add(new CARoom(roomRegion, map, connectionUpperThreshold, connectionSizeFactor));
            }
        }

        return survivingRooms;
    }
    List<CARoom> OptimizeRoomsInArea(List<Tile> area)
    {
        List<List<Tile>> wallRegions = GetRegionsInArea(1, area);

        foreach (List<Tile> wallRegion in wallRegions)
        {
            if (wallRegion.Count < wallThreshold)
            {
                foreach (Tile tile in wallRegion)
                {
                    map[tile.tileX, tile.tileY] = 0;
                }
            }
        }

        List<List<Tile>> roomRegions = GetRegionsInArea(0, area);
        List<CARoom> survivingRooms = new List<CARoom>();

        foreach (List<Tile> roomRegion in roomRegions)
        {
            if (roomRegion.Count < roomThreshold)
            {
                foreach (Tile tile in roomRegion)
                {
                    map[tile.tileX, tile.tileY] = 1;
                }
            }
            else
            {
                survivingRooms.Add(new CARoom(roomRegion, map, connectionUpperThreshold, connectionSizeFactor));
            }
        }

        return survivingRooms;
    }

    List<List<Tile>> GetRegionsInArea(int tileType, List<Tile> area)
    {
        List<List<Tile>> regions = new List<List<Tile>>();
        int[,] mapFlags = new int[columns, rows];

        foreach(Tile c in area) {
            if (mapFlags[c.tileX, c.tileY] == 0 && map[c.tileX, c.tileY] == tileType)
            {
                List<Tile> newRegion = GetRegionTiles(c.tileX, c.tileY);
                regions.Add(newRegion);

                foreach (Tile tile in newRegion)
                {
                    mapFlags[tile.tileX, tile.tileY] = 1;
                }
            }
        }

        return regions;
    }
    List<List<Tile>> GetRegions(int tileType)
    {
        List<List<Tile>> regions = new List<List<Tile>>();
        int[,] mapFlags = new int[columns, rows];

        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                if (mapFlags[x, y] == 0 && map[x, y] == tileType)
                {
                    List<Tile> newRegion = GetRegionTiles(x, y);
                    regions.Add(newRegion);

                    foreach (Tile tile in newRegion)
                    {
                        mapFlags[tile.tileX, tile.tileY] = 1;
                    }
                }
            }
        }

        return regions;
    }
    List<Tile> GetRegionTiles(int startX, int startY)
    {
        List<Tile> tiles = new List<Tile>();
        int[,] mapFlags = new int[columns, rows];
        int tileType = map[startX, startY];

        Queue<Tile> queue = new Queue<Tile>();
        queue.Enqueue(new Tile(startX, startY));
        mapFlags[startX, startY] = 1;

        while (queue.Count > 0)
        {
            Tile tile = queue.Dequeue();
            tiles.Add(tile);

            for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
            {
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                {
                    if (IsInMapRange(x, y) && (y == tile.tileY || x == tile.tileX))
                    {
                        if (mapFlags[x, y] == 0 && map[x, y] == tileType)
                        {
                            mapFlags[x, y] = 1;
                            queue.Enqueue(new Tile(x, y));
                        }
                    }
                }
            }
        }

        return tiles;
    }
    bool IsInMapRange(int x, int y)
    {
        return x >= 0 && x < columns && y >= 0 && y < rows;
    }

    void ConnectRooms(List<CARoom> allRooms)
    {
        List<CARoom> roomListA = allRooms;
        List<CARoom> roomListB = allRooms;

        foreach (CARoom room1 in roomListA)
        {
            if (!room1.checkMaxPassages())
                continue;
            List<CAConnection> connectionsList = new List<CAConnection>();
            foreach (CARoom room2 in roomListB)
            {
                if (room1.connectedRooms.Contains(room2))
                    continue;

                int bestDistance;
                CAConnection connectionBetweenRooms = GetBestConnection(room1, room2, false, out bestDistance);
                if (connectionBetweenRooms != null)
                    connectionsList.Add(connectionBetweenRooms);
            }

            for (int i = room1.passages; i <= room1.maxPassages; i++)
            {
                if (connectionsList.Count == 0)
                    break;

                int index = pseudoRandom.Next(0, connectionsList.Count);
                CAConnection connection = connectionsList[index];

                if (connectionType == ConnectionType.straight)
                    CreateStraightCorridor(connection.roomA, connection.roomB, connection.connectionTileA, connection.connectionTileB, false);
                else
                    CreateDirectCorridor(connection.roomA, connection.roomB, connection.connectionTileA, connection.connectionTileB, false);

                connectionsList.Remove(connection);
            }

        }

        ConnectRoomsToMain(allRooms);
    }
    private void ConnectRoomsToMain(List<CARoom> allRooms)
    {
        List<CARoom> roomListA = new List<CARoom>();
        List<CARoom> roomListB = new List<CARoom>();
        foreach (CARoom room in allRooms)
        {
            if (room.isAccessibleFromMainRoom)
            {
                roomListB.Add(room);
            }
            else
            {
                roomListA.Add(room);
            }
        }

        foreach (CARoom room1 in roomListA)
        {
            CAConnection connection = null;
            int bestDistance = -1;
            foreach (CARoom room2 in roomListB)
            {
                int tmp;
                CAConnection connectionBetweenRooms = GetBestConnection(room1, room2, true, out tmp);

                if (connectionBetweenRooms != null)
                {
                    if (tmp < bestDistance || bestDistance == -1)
                    {
                        bestDistance = tmp;
                        connection = connectionBetweenRooms;
                    }
                }
            }

            if(bestDistance != -1)
            {
                if(connectionType == ConnectionType.straight)
                    CreateStraightCorridor(connection.roomA, connection.roomB, connection.connectionTileA, connection.connectionTileB, true);
                else
                    CreateDirectCorridor(connection.roomA, connection.roomB, connection.connectionTileA, connection.connectionTileB, true);
            }
        }
    }
    private CAConnection GetBestConnection(CARoom room1, CARoom room2, bool forceAccessibilityFromMainRoom, out int bestDistance)
    {
        List<CAConnection> connectionsBetweenRooms = new List<CAConnection>();
        bestDistance = -1;

        foreach (Tile c1 in room1.edgeTiles)
        {
            foreach (Tile c2 in room2.edgeTiles)
            {
                if (!forceAccessibilityFromMainRoom && (Math.Abs(c1.tileX - c2.tileX) > maxCorridorLengthX || Math.Abs(c1.tileY - c2.tileY) > maxCorridorLengthY))
                    continue;

                int distanceBetweenRooms = (int)(Mathf.Pow(c1.tileX - c2.tileX, 2) + Mathf.Pow(c1.tileY - c2.tileY, 2));

                if (bestDistance > distanceBetweenRooms || bestDistance == -1)
                {
                    bestDistance = distanceBetweenRooms;
                    connectionsBetweenRooms.Clear();
                    connectionsBetweenRooms.Add(new CAConnection(c1, c2, room1, room2, distanceBetweenRooms));
                }
                else if (bestDistance == distanceBetweenRooms)
                    connectionsBetweenRooms.Add(new CAConnection(c1, c2, room1, room2, distanceBetweenRooms));
            }
        }

        if (bestDistance != -1)
        {
            int index = pseudoRandom.Next(0, connectionsBetweenRooms.Count);
            CAConnection connection = connectionsBetweenRooms[index];
            return connection;
        }

        return null;
    }

    bool CreateDirectCorridor(CARoom roomA, CARoom roomB, Tile tileA, Tile tileB, bool dontCross = false)
    {
        List<Tile> line = TileTools.GetLine(tileA, tileB);
        if (dontCross)
        {
            for (int i = 2; i < line.Count - 2; i++)
            {
                if (map[line[i].tileX, line[i].tileY] == 0)
                {
                    return false;
                }
            }
        }



        foreach (Tile c in line)
        {
            for (int x = c.tileX - 1; x <= c.tileX + 1; x++)
            {
                for (int y = c.tileY - 1; y <= c.tileY + 1; y++)
                {
                    map[x, y] = 0;
                    InstantiateFromArray(floorTiles, x, y);
                    //InstantiateFromArray(rubbleTiles, x, y);
                }
            }
        }

        CARoom.ConnectRooms(roomA, roomB);
        return true;
    }
    bool CreateStraightCorridor(CARoom roomA, CARoom roomB, Tile tileA, Tile tileB, bool dontCross = false)
    {
        List<Tile> passage = new List<Tile>() ;
        int check = 0;
        int sx, ex;
        int sy, ey;

        if (tileA.tileX > tileB.tileX) {
            sx = tileB.tileX;
            ex = tileA.tileX; }
        else {
            sx = tileA.tileX;
            ex = tileB.tileX; }

        if (tileA.tileY > tileB.tileY)
        {
            sy = tileB.tileY;
            ey = tileA.tileY;
        }
        else
        {
            sy = tileA.tileY;
            ey = tileB.tileY;
        }

        for (int i = sx + 1; i < ex; i++)
        {
            if (map[i, tileA.tileY] == 0)
            {
                check = 1;
                passage.Clear();
                break;
            }
            passage.Add(new Tile(i, tileA.tileY));
        }
        if (check != 1)
        {
            for (int i = sy + 1; i < ey; i++)
            {
                if (map[tileB.tileX, i] == 0)
                {
                    return false;
                }
                passage.Add(new Tile(tileB.tileX, i));
            }
            passage.Add(new Tile(tileB.tileX, tileA.tileY));
        }
        else
        {
            for (int i = sx + 1; i < ex; i++)
            {
                if (map[i, tileB.tileY] == 0)
                {
                    return false;
                }
                passage.Add(new Tile(i, tileB.tileY));
            }
            for (int i = sy + 1; i < ey; i++)
            {
                if (map[tileA.tileX, i] == 0)
                {
                    return false;
                }
                passage.Add(new Tile(tileA.tileX, i));
            }
            passage.Add(new Tile(tileA.tileX, tileB.tileY));
        }

        foreach (Tile c in passage)
        {
            map[c.tileX, c.tileY] = 0;
            InstantiateFromArray(floorTiles, c.tileX, c.tileY);
            //InstantiateFromArray(rubbleTiles, c.tileX, c.tileY);
        }
        CARoom.ConnectRooms(roomA, roomB);
        return true;
    }



    int MooreNeighbourhood(int gridX, int gridY)
    {
        int wallCount = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
        {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
            {
                if (neighbourX >= 0 && neighbourX < columns && neighbourY >= 0 && neighbourY < rows)
                {
                    if (neighbourX != gridX || neighbourY != gridY)
                    {
                        wallCount += map[neighbourX, neighbourY];
                    }
                }
                else
                {
                    wallCount++;
                }
            }
        }

        return 8 - wallCount;
    }
    int NeumanNeighbourhood(int gridX, int gridY)
    {
        int wallCount = 0;

        if (gridX == 0)
            wallCount++;
        else
            wallCount += map[gridX - 1, gridY];

        if (gridX == columns - 1)
            wallCount++;
        else
            wallCount += map[gridX + 1, gridY];

        if (gridY == 0)
            wallCount++;
        else
            wallCount += map[gridX, gridY - 1];

        if (gridY == rows - 1)
            wallCount++;
        else
            wallCount += map[gridX, gridY + 1];

        return 4 - wallCount;
    }
    public enum NeighbourhoodType
    {
        Moore, Neuman
    }

    public enum ConnectionType
    {
        straight, direct
    }

    class Ruleset
    {
        public NeighbourhoodType neighbourhoodType;
        public int survMin, survMax, newMin, newMax;
        public Ruleset(NeighbourhoodType nt, int survMin, int survMax, int newMin, int newMax)
        {
            this.neighbourhoodType = nt;
            this.survMin = survMin;
            this.survMax = survMax;
            this.newMin = newMin;
            this.newMax = newMax;
        }

    }

    void printMap(int[,] mapToPrint, String name)
    {
        String tmp = name + "\n";
        for (int i = 0; i < mapToPrint.GetLength(0); i++)
        {
            for (int j = 0; j < mapToPrint.GetLength(1); j++)
            {
                tmp += mapToPrint[i, j] + " ";
            }
            tmp += "\n";
        }
        Debug.Log(tmp);
    }

    /* OLD ROOM CONNECTING METHOD
    void ConnectClosestRooms(List<CARoom> allRooms, bool forceAccessibilityFromMainRoom = false)
    {
        List<CARoom> roomListA = new List<CARoom>();
        List<CARoom> roomListB = new List<CARoom>();

        if (forceAccessibilityFromMainRoom)
        {
            foreach (CARoom room in allRooms)
            {
                if (room.isAccessibleFromMainRoom)
                {
                    roomListB.Add(room);
                }
                else
                {
                    roomListA.Add(room);
                }
            }
        }
        else
        {
            roomListA = allRooms;
            roomListB = allRooms;
        }

        Tile bestTileA = new Tile();
        Tile bestTileB = new Tile();

        List<CAConnection> connections = new List<CAConnection>();

        foreach (CARoom roomA in roomListA)
        {
            if (!forceAccessibilityFromMainRoom)
            {
                if (!roomA.checkMaxPassages())
                    break;
                connections.Clear();
            }

            foreach (CARoom roomB in roomListB)
            {
                if (roomA == roomB || roomA.IsConnected(roomB))
                {
                    continue;
                }

                Tile tileA = new Tile();
                Tile tileB = new Tile();
                int bestDistance = -1;

                for (int tileIndexA = 0; tileIndexA < roomA.edgeTiles.Count; tileIndexA++)
                {
                    for (int tileIndexB = 0; tileIndexB < roomB.edgeTiles.Count; tileIndexB++)
                    {
                        tileA = roomA.edgeTiles[tileIndexA];
                        tileB = roomB.edgeTiles[tileIndexB];
                        int distanceBetweenRooms = (int)(Mathf.Pow(tileA.tileX - tileB.tileX, 2) + Mathf.Pow(tileA.tileY - tileB.tileY, 2));

                        if (bestDistance > distanceBetweenRooms || bestDistance == -1)
                        {
                            bestDistance = distanceBetweenRooms;
                            bestTileA = tileA;
                            bestTileB = tileB;
                        }
                    }
                }
                connections.Add(new CAConnection(bestTileA, bestTileB, roomA, roomB, bestDistance));
            }

            if (connections.Count > 0 && !forceAccessibilityFromMainRoom)
            {
                foreach (CAConnection c in connections)
                {
                    if (!c.roomB.checkMaxPassages())
                        continue;

                    if (connectionType == ConnectionType.straight)
                        if (CreateStraightCorridor(c.roomA, c.roomB, c.connectionTileA, c.connectionTileB, true))
                            if (!c.roomA.checkMaxPassages())
                                break;
                            else
                        if (CreateDirectCorridor(c.roomA, c.roomB, c.connectionTileA, c.connectionTileB))
                                if (!c.roomA.checkMaxPassages())
                                    break;
                }
            }
        }

        if (forceAccessibilityFromMainRoom && connections.Count > 0)
        {
            connections.Sort();
            CAConnection c = connections[0];
            if (connectionType == ConnectionType.straight)
                CreateStraightCorridor(c.roomA, c.roomB, c.connectionTileA, c.connectionTileB);
            else
                CreateDirectCorridor(c.roomA, c.roomB, c.connectionTileA, c.connectionTileB);
            ConnectClosestRooms(allRooms, true);
        }

        if (!forceAccessibilityFromMainRoom)
        {
            ConnectClosestRooms(allRooms, true);
        }
    }
    */
}