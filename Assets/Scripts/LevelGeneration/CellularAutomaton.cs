using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class CellularAutomaton : BoardCreator
{
    public string seed;
    public bool useRandomSeed;

    public int columns = 100;                                 // The number of columns on the board (how wide it will be).
    public int rows = 100;

    [Range(0, 100)]
    public int wallsFillPercent;
    [Range(0, 100)]
    public int rubbleFillPercent;

    public int smoothingIterations;
    public NeighbourhoodType neighbourhoodType;
    public int surviveMin, surviveMax, newMin, newMax;

    public bool hybridMode;
    public int smoothingIterationsHybrid;
    public NeighbourhoodType neighbourhoodTypeHybrid;
    public int surviveMinHyb, surviveMaxHyb, newMinHyb, newMaxHyb;

    public bool removdeDeadEnds;

    public int borderSize;
    public int roomThreshold;
    public int wallThreshold;
    public int roomUpperThreshold;
    public int wallUpperThreshold;
    public bool connectRooms;
    public int connectionThreshold;
    public ConnectionType connectionType;
    public int sizeFactor;



    public GameObject[] floorTiles;                           // An array of floor tile prefabs.
    public GameObject[] wallTiles;                            // An array of wall tile prefabs.
    public GameObject[] rubbleTiles;
    public GameObject exit;

    int[,] map;

    public override void SetupScene(int level)
    {
        this.level = level;
        BoardSetup();
    }
 
    private void BoardSetup()
    {
        float start = Time.realtimeSinceStartup;
        // Create the board holder.
        boardHolder = new GameObject("BoardHolder");

        GenerateMap();
        if (removdeDeadEnds)
            RemoveDeadEnds();



        List<Room> survivingRooms = OptimizeRooms();



        if (connectRooms && survivingRooms.Count > 0)
        {
            survivingRooms.Sort();
            survivingRooms.Reverse();
            survivingRooms[0].isMainRoom = true;
            survivingRooms[0].isAccessibleFromMainRoom = true;
            ConnectClosestRooms(survivingRooms);
        }

        float midpoint = Time.realtimeSinceStartup;
        StartCoroutine(Example());

        Vector3 playerPos = new Vector3(columns/2, rows/2, 0);
        GameManager.instance.GetPlayer().transform.position = playerPos;
        Vector3 exitPos = new Vector3(columns / 2 + 1, rows / 2, 0);
        GameObject tileInstance = Instantiate(exit, exitPos, Quaternion.identity) as GameObject;
        tileInstance.transform.parent = boardHolder.transform;

        float end = Time.realtimeSinceStartup;
        Debug.Log("Finish time: " + (midpoint - start) + "\nWith initialization: " + (end - start));
    }

    IEnumerator Example()
    {
        print(Time.time);
        yield return new WaitForSeconds(2);
        print(Time.time);
        InstantiateTiles();
    }

    private void RemoveDeadEnds()
    {
        int[,] tmpMap = new int[map.GetLength(0), map.GetLength(1)];
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                tmpMap[x, y] = map[x, y];
                int neighbourWallTiles = CheckNeighbourhood(x, y, NeighbourhoodType.Neuman);

            }
        }
        map = tmpMap;
    }

    private void InstantiateTiles()
    {
        if (useRandomSeed)
        {
            seed = Time.time.ToString();
        }

        System.Random pseudoRandom = new System.Random(seed.GetHashCode());


        int tmp = 0;
        // Go through all the tiles in the jagged array...
        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                // ... and instantiate a floor tile for it.
                if (map[i, j] == 0)
                {
                    tmp++;
                    InstantiateFromArray(floorTiles, i, j);
                    if (pseudoRandom.Next(0, 100) < rubbleFillPercent)
                    {
                        //InstantiateFromArray(rubbleTiles, i, j);
                    }
                }
                // If the tile type is Wall...
                else
                {
                    // ... instantiate a wall over the top.
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
            Ruleset ruleset2 = new Ruleset(neighbourhoodTypeHybrid, surviveMinHyb, surviveMaxHyb, newMinHyb, newMaxHyb);
            for (int i = 0; i < smoothingIterationsHybrid; i++)
            {
                SmoothMap(ruleset2);
            }
        }

        AddBorderToMap(borderSize);
    }

    void RegenerateRegion(List<Coord> area)
    {
        RefillRegion(area);

        Ruleset ruleset = new Ruleset(neighbourhoodType, surviveMin, surviveMax, newMin, newMax);
        for (int i = 0; i < smoothingIterations; i++)
        {
            ResmoothRegion(ruleset, area);
        }
        if (hybridMode)
        {
            Ruleset ruleset2 = new Ruleset(neighbourhoodTypeHybrid, surviveMinHyb, surviveMaxHyb, newMinHyb, newMaxHyb);
            for (int i = 0; i < smoothingIterationsHybrid; i++)
            {
                ResmoothRegion(ruleset2, area);
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

    void RandomFillMap()
    {
        System.Random pseudoRandom = new System.Random(seed.GetHashCode());

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

    void RefillRegion(List<Coord> area)
    {
        System.Random pseudoRandom = new System.Random(seed.GetHashCode());

        foreach (Coord c in area)
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

    void ResmoothRegion(Ruleset ruleset, List<Coord> area)
    {
        int[,] tmpMap = new int[map.GetLength(0), map.GetLength(1)];
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                tmpMap[x, y] = map[x, y];
            }
        }

        foreach (Coord c in area)
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

    void printMap(int[,] mapToPrint, String name)
    {
        String tmp = name + "\n";
        for(int i = 0; i < mapToPrint.GetLength(0); i++)
        {
            for (int j = 0; j < mapToPrint.GetLength(1); j++)
            {
                tmp += mapToPrint[i, j] + " ";
            }
            tmp += "\n";
        }
        Debug.Log(tmp);
    }

    List<Room> OptimizeRooms(List<Room> area = null)
    {
        List<List<Coord>> wallRegions = GetRegions(1);

        foreach (List<Coord> wallRegion in wallRegions)
        {
            if (wallRegion.Count < wallThreshold)
            {
                foreach (Coord tile in wallRegion)
                {
                    map[tile.tileX, tile.tileY] = 0;
                }
            }
        }

        List<List<Coord>> roomRegions = GetRegions(0);
        List<Room> survivingRooms = new List<Room>();

        foreach (List<Coord> roomRegion in roomRegions)
        {
            if (roomRegion.Count < roomThreshold)
            {
                foreach (Coord tile in roomRegion)
                {
                    map[tile.tileX, tile.tileY] = 1;
                }
            } else if(roomRegion.Count > roomUpperThreshold)
            {
                RegenerateRegion(roomRegion);
                List<Room> newSurvivingRooms = ReoptimizeRooms(roomRegion);
                survivingRooms.AddRange(newSurvivingRooms);
            }
            else
            {
                survivingRooms.Add(new Room(roomRegion, map, connectionThreshold, sizeFactor));
            }
        }

        return survivingRooms;
    }

    List<Room> ReoptimizeRooms(List<Coord> area)
    {
        List<List<Coord>> wallRegions = GetRegions(1, area);

        foreach (List<Coord> wallRegion in wallRegions)
        {
            if (wallRegion.Count < wallThreshold)
            {
                foreach (Coord tile in wallRegion)
                {
                    map[tile.tileX, tile.tileY] = 0;
                }
            }
        }

        List<List<Coord>> roomRegions = GetRegions(0, area);
        List<Room> survivingRooms = new List<Room>();

        foreach (List<Coord> roomRegion in roomRegions)
        {
            if (roomRegion.Count < roomThreshold)
            {
                foreach (Coord tile in roomRegion)
                {
                    map[tile.tileX, tile.tileY] = 1;
                }
            }
            else
            {
                survivingRooms.Add(new Room(roomRegion, map, connectionThreshold, sizeFactor));
            }
        }

        return survivingRooms;
    }

    List<List<Coord>> GetRegions(int tileType, List<Coord> area)
    {
        List<List<Coord>> regions = new List<List<Coord>>();
        int[,] mapFlags = new int[columns, rows];

        foreach(Coord c in area) {
            if (mapFlags[c.tileX, c.tileY] == 0 && map[c.tileX, c.tileY] == tileType)
            {
                List<Coord> newRegion = GetRegionTiles(c.tileX, c.tileY);
                regions.Add(newRegion);

                foreach (Coord tile in newRegion)
                {
                    mapFlags[tile.tileX, tile.tileY] = 1;
                }
            }
        }

        return regions;
    }

    List<List<Coord>> GetRegions(int tileType)
    {
        List<List<Coord>> regions = new List<List<Coord>>();
        int[,] mapFlags = new int[columns, rows];

        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                if (mapFlags[x, y] == 0 && map[x, y] == tileType)
                {
                    List<Coord> newRegion = GetRegionTiles(x, y);
                    regions.Add(newRegion);

                    foreach (Coord tile in newRegion)
                    {
                        mapFlags[tile.tileX, tile.tileY] = 1;
                    }
                }
            }
        }

        return regions;
    }

    List<Coord> GetRegionTiles(int startX, int startY)
    {
        List<Coord> tiles = new List<Coord>();
        int[,] mapFlags = new int[columns, rows];
        int tileType = map[startX, startY];

        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(new Coord(startX, startY));
        mapFlags[startX, startY] = 1;

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();
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
                            queue.Enqueue(new Coord(x, y));
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

    void ConnectClosestRooms(List<Room> allRooms, bool forceAccessibilityFromMainRoom = false)
    {
        List<Room> roomListA = new List<Room>();
        List<Room> roomListB = new List<Room>();

        if (forceAccessibilityFromMainRoom)
        {
            foreach (Room room in allRooms)
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

        Coord bestTileA = new Coord();
        Coord bestTileB = new Coord();

        List<Connection> connections = new List<Connection>();

        foreach (Room roomA in roomListA)
        {
            if (!forceAccessibilityFromMainRoom)
            {
                if (!roomA.checkMaxPassages())
                    break;
                connections.Clear();
            }

            foreach (Room roomB in roomListB)
            {
                if (roomA == roomB || roomA.IsConnected(roomB))
                {
                    continue;
                }

                Coord tileA = new Coord();
                Coord tileB = new Coord();
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
                connections.Add(new Connection(bestTileA, bestTileB, roomA, roomB, bestDistance));
            }

            if (connections.Count > 0 && !forceAccessibilityFromMainRoom)
            {
                connections.Sort();

                foreach (Connection c in connections)
                {
                    if (!c.roomB.checkMaxPassages())
                        continue;

                    if(connectionType == ConnectionType.straight)
                        if(CreateStraightPassage(c.roomA, c.roomB, c.connectionTileA, c.connectionTileB, true))
                            if (!c.roomA.checkMaxPassages())
                                break;
                    else
                        if (CreatePassage(c.roomA, c.roomB, c.connectionTileA, c.connectionTileB, true))
                            if (!c.roomA.checkMaxPassages())
                                break;
                }
            }
        }

        if (forceAccessibilityFromMainRoom && connections.Count > 0 )
        {
            connections.Sort();
            Connection c = connections[0];
            if (connectionType == ConnectionType.straight)
                CreateStraightPassage(c.roomA, c.roomB, c.connectionTileA, c.connectionTileB);
            else
                CreatePassage(c.roomA, c.roomB, c.connectionTileA, c.connectionTileB);
            ConnectClosestRooms(allRooms, true);
        }

        if (!forceAccessibilityFromMainRoom)
        {
            ConnectClosestRooms(allRooms, true);
        }
    }

    bool CreatePassage(Room roomA, Room roomB, Coord tileA, Coord tileB, bool dontCross = false)
    {
        List<Coord> line = GetLine(tileA, tileB);
        if (dontCross) {
            for(int i = 2; i < line.Count - 2; i++)
            {
                if (map[line[i].tileX, line[i].tileY] == 0)
                {
                    return false;
                }
            }
        }
    


        foreach (Coord c in line)
        {
            for (int x = c.tileX - 1; x <= c.tileX + 1; x++)
            {
                for (int y = c.tileY - 1; y <= c.tileY + 1; y++)
                {
                    InstantiateFromArray(floorTiles, x, y);
                }
            }
            //InstantiateFromArray(rubbleTiles, c.tileX, c.tileY);
        }

        Room.ConnectRooms(roomA, roomB);
        return true;
    }

    bool CreateStraightPassage(Room roomA, Room roomB, Coord tileA, Coord tileB, bool dontCross = false)
    {
        List<Coord> passage = new List<Coord>() ;
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
            passage.Add(new Coord(i, tileA.tileY));
        }
        if (check != 1)
        {
            for (int i = sy + 1; i < ey; i++)
            {
                if (map[tileB.tileX, i] == 0)
                {
                    return false;
                }
                passage.Add(new Coord(tileB.tileX, i));
            }
            passage.Add(new Coord(tileB.tileX, tileA.tileY));
        }
        else
        {
            for (int i = sx + 1; i < ex; i++)
            {
                if (map[i, tileB.tileY] == 0)
                {
                    return false;
                }
                passage.Add(new Coord(i, tileB.tileY));
            }
            for (int i = sy + 1; i < ey; i++)
            {
                if (map[tileA.tileX, i] == 0)
                {
                    return false;
                }
                passage.Add(new Coord(tileA.tileX, i));
            }
            passage.Add(new Coord(tileA.tileX, tileB.tileY));
        }

        foreach (Coord c in passage)
        {
            InstantiateFromArray(floorTiles, c.tileX, c.tileY);
            //InstantiateFromArray(rubbleTiles, c.tileX, c.tileY);
        }
        Room.ConnectRooms(roomA, roomB);
        return true;
    }
    List<Coord> GetLine(Coord from, Coord to)
    {
        List<Coord> line = new List<Coord>();

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
            line.Add(new Coord(x, y));

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

    Vector3 CoordToWorldPoint(Coord tile)
    {
        return new Vector3(-columns / 2 + .5f + tile.tileX, 2, -rows / 2 + .5f + tile.tileY);
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

    class Room : IComparable<Room>
    {
        public List<Coord> tiles;
        public List<Coord> edgeTiles;
        public List<Room> connectedRooms;
        public int roomSize;
        public bool isAccessibleFromMainRoom;
        public bool isMainRoom;
        public int passages;
        public int maxPassages;

        public Room()
        {
        }

        public Room(List<Coord> roomTiles, int[,] map, int connectionThreshold, int sizeFactor)
        {
            tiles = roomTiles;
            roomSize = tiles.Count;
            connectedRooms = new List<Room>();

            int sizeToPassages = 1 + roomSize / sizeFactor;
            if (sizeToPassages > connectionThreshold)
                sizeToPassages = connectionThreshold;
            maxPassages = sizeToPassages;
            passages = 0;

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

        public void SetAccessibleFromMainRoom()
        {
            if (!isAccessibleFromMainRoom)
            {
                isAccessibleFromMainRoom = true;
                foreach (Room connectedRoom in connectedRooms)
                {
                    connectedRoom.SetAccessibleFromMainRoom();
                }
            }
        }

        public static void ConnectRooms(Room roomA, Room roomB)
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

        public bool IsConnected(Room otherRoom)
        {
            return connectedRooms.Contains(otherRoom);
        }

        public int CompareTo(Room otherRoom)
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

    class Connection : IComparable<Connection>
    {
        public Room roomA;
        public Room roomB;
        public Coord connectionTileA;
        public Coord connectionTileB;
        public Int32 distance;

        public Connection(Coord a, Coord b, Room ra, Room rb, Int32 d)
        {
            connectionTileA = a;
            connectionTileB = b;
            roomA = ra;
            roomB = rb;
            distance = d;
        }

        public int CompareTo(Connection otherConnection)
        {
            //return otherRoom.roomSize.CompareTo(roomSize);
            return this.distance.CompareTo(otherConnection.distance);
        }
    }

    public enum NeighbourhoodType
    {
        Moore, Neuman
    }

    public enum ConnectionType
    {
        straight, conical
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
}