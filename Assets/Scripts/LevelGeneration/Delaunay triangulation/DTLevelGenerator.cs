using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


class DTLevelGenerator : LevelGenerator
{
    public string seed;
    public bool useRandomSeed;

    public int numberOfStartingRooms = 60;
    public int startingRoomGenerationAreaWidth = 20;                                 // The number of columns on the board (how wide it will be).
    public int startingRoomGenerationAreaHeight = 20;

    public int widthLow = 5, widthHigh = 15, heightLow = 5, heightHigh = 15;
    public int minWidth = 9, minHeight = 9;
    public Boolean alternativeMinCheck = true;
    [Range(0, 100)]
    public int additionalConnectionsPercent = 20;

    int[,] map;

    public GameObject[] floorTiles;                           // An array of floor tile prefabs.
    public GameObject[] wallTiles;                            // An array of wall tile prefabs.
    public GameObject[] rubbleTiles;
    public GameObject exit;

    private System.Random pseudoRandom;
    private ArrayList startingRoomsList = new ArrayList();
    private bool initializationStarted = false;
    private bool initilizationFinished = false;
    private bool DTFinished = false;
    private GameObject cellsHolder;
    private List<DTNode> roomList = new List<DTNode>();
    private DelaunayTriangulation traingulationScript = new DelaunayTriangulation();
    private DTMinSpanningTree treeScript = new DTMinSpanningTree();

    private float st, it1, it2, mt, et;

    public override void SetupScene(int level)
    {
        this.level = level;
        SetupParameters();

        initilizationFinished = false;
        initializationStarted = false;
        startingRoomsList.Clear();
        roomList.Clear();
        traingulationScript = new DelaunayTriangulation();
        treeScript = new DTMinSpanningTree();
        CreateLevel();
    }

    private void SetupParameters()
    {
        DTSettings settings = UIDTSettings.instance.GetSettings();

        seed = settings.seed;
        useRandomSeed = settings.useRandomSeed;

        numberOfStartingRooms = settings.initialRooms;
        startingRoomGenerationAreaWidth = settings.initialAreaWidth;
        startingRoomGenerationAreaHeight = settings.initialAreaHeight;
        widthLow = settings.initialRoomMinWidth;
        heightLow = settings.initialRoomMinHeight;
        widthHigh = settings.initialRoomMaxWidth;
        heightHigh = settings.initialRoomMaxHeight;

        minWidth = settings.minRoomWidth;
        minHeight = settings.minRoomHeight;
        alternativeMinCheck = settings.useOrCheck;

        additionalConnectionsPercent = settings.additionalConnections;
    }

    private void CreateLevel()
    {
        st = Time.realtimeSinceStartup;

        boardHolder = new GameObject("BoardHolder");
        GenerateMap();
    }

    private void GenerateMap()
    {
        if (useRandomSeed)
        {
            seed = Time.time.ToString();
        }
        pseudoRandom = new System.Random(seed.GetHashCode());
        cellsHolder = new GameObject("CellsHolder");
        for (int i = 0; i < numberOfStartingRooms; i++)
        {
            GameObject roomRectangle = (GameObject)Instantiate(Resources.Load("RoomRectangle"));
            bool xEven = false, yEven = false;
            int xScale = pseudoRandom.Next(widthLow, widthHigh);
            int yScale = pseudoRandom.Next(heightLow, heightHigh);

            if (xScale % 2 == 0) { xEven = true; }
            if (yScale % 2 == 0) { yEven = true; }

            roomRectangle.transform.localScale = new Vector3(xScale, yScale, roomRectangle.transform.localScale.z);

            int xPos = pseudoRandom.Next(0, startingRoomGenerationAreaWidth);
            int yPos = pseudoRandom.Next(0, startingRoomGenerationAreaHeight);

            roomRectangle.transform.position = new Vector3(-startingRoomGenerationAreaWidth/2 + xPos, -startingRoomGenerationAreaHeight/2 + yPos, 0);

            roomRectangle.GetComponent<Renderer>().material.shader = Shader.Find("Sprites/Default");
            roomRectangle.GetComponent<Renderer>().material.color = new Color(UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f));
            
            roomRectangle.GetComponent<DTRoomRectangle>().Setup(xEven, yEven);
            
            startingRoomsList.Add(roomRectangle);
            roomRectangle.transform.SetParent(cellsHolder.transform);
        }
        initializationStarted = true;
        it1 = Time.realtimeSinceStartup;
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameManager.instance.gameInProgress)
        {
            return;
        }

        if (initializationStarted)
        {
            if (CellsStill())
            {
                initializationStarted = false;
                initilizationFinished = true;
            }
        }
        else if (initilizationFinished)
        {
            initilizationFinished = false;
            ContinueGeneration();
        }
    }

    private void ContinueGeneration()
    {
        it2 = Time.realtimeSinceStartup;
        //turn large cells into rooms;
        ChooseLargeRooms();

        //initalize the triangulation
        traingulationScript.Triangulate(roomList, pseudoRandom);

        //choose edges that get turned into corridors
        treeScript.ChooseConnectionsFromEdges(roomList, traingulationScript.GetTriangulation(), pseudoRandom, additionalConnectionsPercent);
        mt = Time.realtimeSinceStartup;

        InstantiateTiles(roomList, treeScript.getConnections());
        et = Time.realtimeSinceStartup;

        //Debug.Log("Test number: " + level + " - rooms: " + roomList.Count + "//" + numberOfStartingRooms + " - separation: " + (it2 - it1) + " - generation: " + (it1 - st + mt - it2) + " - visualzation: " + (et - mt));
        //TestLogger.AddLine(level + "\t" + roomList.Count + "\t" + numberOfStartingRooms + "\t" + (it2 - it1) + "\t" + (it1 - st + mt - it2) + "\t" + (et - mt));
    }

    private void InstantiateTiles(List<DTNode> rooms, List<DTEdge> connections)
    {
        Debug.Log(rooms.Count + " " + connections.Count);
        foreach (DTNode r in rooms)
        {
            DrawRoom(r);
        }

        foreach(DTEdge c in connections)
        {
            DrawConnection(c);
        }


        Vector3 playerPos = new Vector3(0, 0, 0);
        GameManager.instance.GetPlayer().transform.position = playerPos;
    }

    private void DrawConnection(DTEdge c)
    {
        DTNode room1 = c.getNodeA();
        DTNode room2 = c.getNodeB();

        Vector2 pos = new Vector2((float)Math.Round(room1.getNodePosition().x), (float)Math.Round(room1.getNodePosition().y));

        float xDir = 0f, yDir = 0f;

        if (room1.getNodePosition().x <= room2.getNodePosition().x)
            xDir = 1f;
        else
            xDir = -1f;

        while (pos.x != (float)Math.Round(room2.getNodePosition().x))
        {
            ConnectionFloor(pos.x, pos.y);
            ConnectionFloor(pos.x, pos.y + 1f);


            pos = new Vector2(pos.x + xDir, pos.y);
        }

        if (room1.getNodePosition().y <= room2.getNodePosition().y)
            yDir = 1f;
        else
            yDir = -1f;

        while (pos.y != (float)Math.Round(room2.getNodePosition().y))
        {
            ConnectionFloor(pos.x, pos.y);
            ConnectionFloor(pos.x + 1f, pos.y);

            pos = new Vector2(pos.x, pos.y + yDir);
        }
    }

    private void ConnectionFloor(float x, float y)
    {
        DestroyByTag(new Vector2(x, y), 0.1f, "Wall");
        if (!CheckForTag(new Vector2(x, y), 0.1f, "Floor"))
            InstantiateFromArray(floorTiles, x, y);

        for(int i = (int)x - 1; i <= (int)x + 1; i++)
        {
            for (int j = (int)y - 1; j <= (int)y + 1; j++)
            {
                if (!CheckForTag(new Vector2(i, j), 0.1f, "Floor"))
                    InstantiateFromArray(wallTiles, i, j);

            }
        }
    }

    private void DestroyByTag(Vector2 center, float radius, String tag)
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(center, radius);
        foreach (Collider2D col in hitColliders)
        {
            if (col.tag == tag)
                Destroy(col.gameObject);
        }
    }

    private void DrawRoom(DTNode r)
    {
        int iMin = (int)(r.getNodePosition().x - r.getParentRoom().transform.localScale.x / 2 + 0.5f);
        int iMax = (int)(r.getNodePosition().x + r.getParentRoom().transform.localScale.x / 2 + 0.5f);
        int jMin = (int)(r.getNodePosition().y - r.getParentRoom().transform.localScale.y / 2 + 0.5f);
        int jMax = (int)(r.getNodePosition().y + r.getParentRoom().transform.localScale.y / 2 + 0.5f);

        for (int i = iMin; i < iMax; i++)
        {
            for (int j = jMin; j < jMax; j++)
            {
                if(i == iMin || i == iMax - 1 || j == jMin || j == jMax - 1)
                {
                    if (CheckForTag(new Vector2(i, j), 0.1f, "Floor"))
                        continue;
                    else
                        InstantiateFromArray(wallTiles, i, j);
                }
                else
                    InstantiateFromArray(floorTiles, i, j);
            }
        }
    }

    private bool CheckForTag(Vector2 center, float radius, String tag)
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(center, radius);
        foreach(Collider2D col in hitColliders)
        {
            if (col.tag == tag)
                return true;
        }
        return false;
    }

    private bool CheckForWall(Vector2 center, float radius)
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(center, radius);
        foreach (Collider2D col in hitColliders)
        {
            if (col.tag == "Floor")
                return true;
        }
        return false;
    }

    //returns if all the cells have stopped moving or not
    private bool CellsStill()
    {

        bool placed = true;
        foreach (GameObject aCell in startingRoomsList)
        {
            if (!aCell.GetComponent<DTRoomRectangle>().GetHasStopped())
            {
                placed = false;
            }
        }
        return placed;

    }

    //handles choosing which cells to turn to rooms
    private void ChooseLargeRooms()
    {
        foreach (GameObject sr in startingRoomsList)
        {
            sr.SetActive(false);

            if (alternativeMinCheck)
            {
                if (sr.transform.localScale.x > minHeight || sr.transform.localScale.y > minHeight)
                {
                    sr.SetActive(true);
                    DTNode tmpRoom = new DTNode(sr.transform.position.x, sr.transform.position.y, sr.gameObject);
                    roomList.Add(tmpRoom);
                }
            }
            else
            {
                if (sr.transform.localScale.x > minHeight && sr.transform.localScale.y > minHeight)
                {
                    sr.SetActive(true);
                    DTNode tmpRoom = new DTNode(sr.transform.position.x, sr.transform.position.y, sr.gameObject);
                    roomList.Add(tmpRoom);
                }
            }

            Destroy(sr.GetComponent<DTRoomRectangle>());
        }
    }

    private void InstantiateTiles()
    {
        foreach (GameObject c in startingRoomsList)
        {
            if (c.GetComponent<DTRoomRectangle>() != null)
            {
                Debug.Log(c.transform.position.x + " " + c.transform.localScale.x);

                Debug.Log(c.transform.localScale.x + " " + (int)(c.transform.position.x - c.transform.localScale.x / 2) + " " + (int)(c.transform.position.x + c.transform.localScale.x / 2));

                //Vector2 mySize = new Vector2(c.GetComponent<Renderer>().bounds.size.x, c.GetComponent<Renderer>().bounds.size.y);
                for (int i = (int)(c.transform.position.x - c.transform.localScale.x / 2 + 0.5f); i < (int)(c.transform.position.x + c.transform.localScale.x / 2 + 0.5f); i++)
                {
                    for (int j = (int)(c.transform.position.y - c.transform.localScale.y / 2 + 0.5f); j < (int)(c.transform.position.y + c.transform.localScale.y / 2 + 0.5f); j++)
                    {
                        InstantiateFromArray(floorTiles, i, j);
                    }
                }
            }
        }
    }
}