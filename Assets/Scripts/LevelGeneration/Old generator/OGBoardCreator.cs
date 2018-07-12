using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class OGBoardCreator : LevelGenerator
{
    // The type of tile that will be laid in a specific position.
    public enum TileType
    {
        Wall, Room, Corridor, Empty, Occupied
    }

    public int columns = 100;                                 // The number of columns on the board (how wide it will be).
    public int rows = 100;

    public IntRange numRooms = new IntRange(5, 10);         // The range of the number of rooms there can be.
    public IntRange roomWidth = new IntRange(2, 8);         // The range of widths rooms can have.
    public IntRange roomHeight = new IntRange(2, 8);        // The range of heights rooms can have.
    public IntRange corridorLength = new IntRange(3, 6);    // The range of lengths corridors between rooms can have.

    public IntRange startingRoomRange = new IntRange(1, 2);
    public IntRange exitRoomRange = new IntRange(6, 10);

    public IntRange enemiesPerRoom = new IntRange(0, 2);
    public IntRange rubblePerRoom = new IntRange(2, 8);
    public IntRange foodPerRoom = new IntRange(1, 3);
    public IntRange enemiesScaling = new IntRange(20, 60);

    public GameObject[] floorTiles;                           // An array of floor tile prefabs.
    public GameObject[] wallTiles;                            // An array of wall tile prefabs.
    public GameObject[] outerWallTiles;                       // An array of outer wall tile prefabs.
    public GameObject[] enemyTiles;
    public GameObject[] foodTiles;
    public GameObject exit;

    private TileType[][] tiles;                               // A jagged array of tile types representing the board, like a grid.
    private OGRoom[] rooms;                                     // All the rooms that are created for this board.
    private OGCorridor[] corridors;                             // All the corridors that connect the rooms.
    private int startingRoom, exitRoom;

    private List<Vector3> gridPositions = new List<Vector3>(); //Positions grid for randomizing objects inside each room


    public override void SetupScene(int level)
    {
        this.level = level;
        SetupParameters();
        BoardSetup();
    }

    private void SetupParameters()
    {
        SGSettings settings = UISGSettings.instance.GetSettings();

        columns = settings.levelWidth;
        rows = settings.levelHeight;
        numRooms = new IntRange(settings.roomNumberMin, settings.roomNumberMax);
        roomWidth = new IntRange(settings.roomWidthMin, settings.roomWidthMax);
        roomHeight = new IntRange(settings.roomHeightMin, settings.roomHeightMax);
        corridorLength = new IntRange(settings.connectionLengthMin, settings.connectionLengthMax);
    }

    private void BoardSetup()
    {
        float start = Time.realtimeSinceStartup;
        // Create the board holder.
        boardHolder = new GameObject("BoardHolder");

        gridPositions.Clear();

        startingRoom = startingRoomRange.Random;
        exitRoom = exitRoomRange.Random;

        SetupTilesArray();

        CreateRoomsAndCorridors();

        SetTilesValuesForRooms();
        if (corridors != null)
            SetTilesValuesForCorridors();
        float midpoint = Time.realtimeSinceStartup;

        StartCoroutine(Example());
        float end = Time.realtimeSinceStartup;
        //Debug.Log("Finish time: " + (midpoint - start) + "\nWith initialization: " + (end - start));
        //TestLogger.AddLine(level + "\t" + (midpoint - start) + "\t" + (end - start));
    }
    IEnumerator Example()
    {
        print(Time.time);
        yield return new WaitForSeconds(0);
        print(Time.time);
        InstantiateTiles();
        Vector3 playerPos = new Vector3(columns / 2, rows / 2, 0);
        GameManager.instance.GetPlayer().transform.position = playerPos;
    }

    private void SetupTilesArray()
    {
        // Set the tiles jagged array to the correct width.
        tiles = new TileType[columns][];

        // Go through all the tile arrays...
        for (int i = 0; i < tiles.Length; i++)
        {
            // ... and set each tile array is the correct height.
            tiles[i] = new TileType[rows];
            for (int j = 0; j < tiles[i].Length; j++)
            {
                tiles[i][j] = TileType.Empty;
            }
        }
    }


    private void CreateRoomsAndCorridors()
    {
        // Create the rooms array with a random size.
        int randomNumRooms = numRooms.Random;
        rooms = new OGRoom[randomNumRooms];

        if (exitRoom > randomNumRooms)
            exitRoom = randomNumRooms;

        // Create the first room and corridor.
        rooms[0] = new OGRoom();

        // Setup the first room, there is no previous corridor so we do not use one.
        rooms[0].SetupRoom(roomWidth, roomHeight, columns, rows);
        if (startingRoom == 1)
        {
            Vector3 playerPos = new Vector3(rooms[0].xPos, rooms[0].yPos, 0);
            ClearSpace(playerPos, 0.1f);
            GameManager.instance.GetPlayer().transform.position = playerPos;
        }
        // Setup the first corridor using the first room.
        if (randomNumRooms >= 2)
        {
            corridors = new OGCorridor[randomNumRooms - 1];
            corridors[0] = new OGCorridor();
            corridors[0].SetupCorridor(rooms[0], corridorLength, roomWidth, roomHeight, columns, rows, true);
        }
        /*
        if (startingRoom == 1)
        {
            Vector3 playerPos = new Vector3(rooms[0].xPos, rooms[0].yPos, 0);
            if (ClearSpace(playerPos, 0.1f))
                playerPos = new Vector3(rooms[0].xPos, rooms[0].yPos, 0);
            GameManager.instance.GetPlayer().transform.position = playerPos;
        }

        if (exitRoom == 1)
        {
            Vector3 exitPos = new Vector3(rooms[0].xPos + rooms[0].roomWidth - 1, rooms[0].yPos + rooms[0].roomHeight - 1, 0);
            if (ClearSpace(exitPos, 0.1f))
                exitPos = new Vector3(rooms[0].xPos + rooms[0].roomWidth - 1, rooms[0].yPos + rooms[0].roomHeight - 1, 0);
            GameObject tileInstance = Instantiate(exit, exitPos, Quaternion.identity) as GameObject;
            tileInstance.transform.parent = boardHolder.transform;
        }
        */

        for (int i = 1; i < rooms.Length; i++)
        {
            // Create a room.
            rooms[i] = new OGRoom();

            // Setup the room based on the previous corridor.
            rooms[i].SetupRoom(roomWidth, roomHeight, columns, rows, corridors[i - 1]);

            // If we haven't reached the end of the corridors array...
            if (i < corridors.Length)
            {
                // ... create a corridor.
                corridors[i] = new OGCorridor();

                // Setup the corridor based on the room that was just created.
                corridors[i].SetupCorridor(rooms[i], corridorLength, roomWidth, roomHeight, columns, rows, false);
            }
            /*
            if (i == startingRoom - 1)
            {
                Vector3 playerPos = new Vector3(rooms[i].xPos, rooms[i].yPos, 0);
                if (ClearSpace(playerPos, 0.1f))
                    playerPos = new Vector3(rooms[i].xPos, rooms[i].yPos + 1, 0);
                GameManager.instance.GetPlayer().transform.position = playerPos;
            }
            
            if (i == exitRoom - 1)
            {
                Vector3 exitPos = new Vector3(rooms[i].xPos, rooms[i].yPos, 0);
                if (ClearSpace(exitPos, 0.1f))
                    exitPos = new Vector3(rooms[i].xPos + 1, rooms[i].yPos, 0);
                GameObject tileInstance = Instantiate(exit, exitPos, Quaternion.identity) as GameObject;
                tileInstance.transform.parent = boardHolder.transform;
            }*/
        }
    }


    private void SetTilesValuesForRooms()
    {
        // Go through all the rooms
        for (int i = 0; i < rooms.Length; i++)
        {
            int xCoord, yCoord;
            OGRoom currentRoom = rooms[i];

            gridPositions.Clear();

            //Bottom wall
            yCoord = currentRoom.yPos - 1;
            for (int j = -1; j < currentRoom.roomWidth + 1; j++)
            {
                xCoord = currentRoom.xPos + j;
                if (tiles[xCoord][yCoord] != TileType.Room)
                    tiles[xCoord][yCoord] = TileType.Wall;
            }
            //Top wall
            yCoord = currentRoom.yPos + currentRoom.roomHeight;
            for (int j = -1; j < currentRoom.roomWidth + 1; j++)
            {
                xCoord = currentRoom.xPos + j;
                if (tiles[xCoord][yCoord] != TileType.Room)
                    tiles[xCoord][yCoord] = TileType.Wall;
            }
            //Left wall
            xCoord = currentRoom.xPos - 1;
            for (int j = 0; j < currentRoom.roomHeight + 1; j++)
            {
                yCoord = currentRoom.yPos + j;
                if (tiles[xCoord][yCoord] != TileType.Room)
                    tiles[xCoord][yCoord] = TileType.Wall;
            }
            //Right wall
            xCoord = currentRoom.xPos + currentRoom.roomWidth;
            for (int j = 0; j < currentRoom.roomHeight + 1; j++)
            {
                yCoord = currentRoom.yPos + j;
                if (tiles[xCoord][yCoord] != TileType.Room)
                    tiles[xCoord][yCoord] = TileType.Wall;
            }

            //Inside
            for (int j = 0; j < currentRoom.roomWidth; j++)
            {
                xCoord = currentRoom.xPos + j;
                for (int k = 0; k < currentRoom.roomHeight; k++)
                {
                    yCoord = currentRoom.yPos + k;
                    tiles[xCoord][yCoord] = TileType.Room;

                    gridPositions.Add(new Vector3(xCoord, yCoord, 0f));
                }
            }

            //RandomizeRoomContents(currentRoom);
        }
    }


    private void SetTilesValuesForCorridors()
    {
        // Go through every corridor...
        for (int i = 0; i < corridors.Length; i++)
        {
            OGCorridor currentCorridor = corridors[i];

            // and go through it's length.
            for (int j = 0; j < currentCorridor.corridorLength; j++)
            {
                // Start the coordinates at the start of the corridor.
                int xCoord = currentCorridor.startXPos;
                int yCoord = currentCorridor.startYPos;

                // Depending on the direction, add or subtract from the appropriate
                // coordinate based on how far through the length the loop is.
                switch (currentCorridor.direction)
                {
                    case Direction.North:
                        yCoord += j;
                        if (tiles[xCoord - 1][yCoord] != TileType.Room && tiles[xCoord - 1][yCoord] != TileType.Corridor)
                            tiles[xCoord - 1][yCoord] = TileType.Wall;
                        if (tiles[xCoord + 1][yCoord] != TileType.Room && tiles[xCoord + 1][yCoord] != TileType.Corridor)
                            tiles[xCoord + 1][yCoord] = TileType.Wall;
                        break;
                    case Direction.East:
                        xCoord += j;
                        if (tiles[xCoord][yCoord - 1] != TileType.Room && tiles[xCoord][yCoord - 1] != TileType.Corridor)
                            tiles[xCoord][yCoord - 1] = TileType.Wall;
                        if (tiles[xCoord][yCoord + 1] != TileType.Room && tiles[xCoord][yCoord + 1] != TileType.Corridor)
                            tiles[xCoord][yCoord + 1] = TileType.Wall;
                        break;
                    case Direction.South:
                        yCoord -= j;
                        if (tiles[xCoord - 1][yCoord] != TileType.Room && tiles[xCoord - 1][yCoord] != TileType.Corridor)
                            tiles[xCoord - 1][yCoord] = TileType.Wall;
                        if (tiles[xCoord + 1][yCoord] != TileType.Room && tiles[xCoord + 1][yCoord] != TileType.Corridor)
                            tiles[xCoord + 1][yCoord] = TileType.Wall;
                        break;
                    case Direction.West:
                        xCoord -= j;
                        if (tiles[xCoord][yCoord - 1] != TileType.Room && tiles[xCoord][yCoord - 1] != TileType.Corridor)
                            tiles[xCoord][yCoord - 1] = TileType.Wall;
                        if (tiles[xCoord][yCoord + 1] != TileType.Room && tiles[xCoord][yCoord + 1] != TileType.Corridor)
                            tiles[xCoord][yCoord + 1] = TileType.Wall;
                        break;
                }

                // Set the tile at these coordinates to Floor.
                if (tiles[xCoord][yCoord] != TileType.Room)
                    tiles[xCoord][yCoord] = TileType.Corridor;
            }
        }
    }


    private void InstantiateTiles()
    {
        // Go through all the tiles in the jagged array...
        for (int i = 0; i < tiles.Length; i++)
        {
            for (int j = 0; j < tiles[i].Length; j++)
            {
                // ... and instantiate a floor tile for it.
                if (tiles[i][j] != TileType.Empty && tiles[i][j] != TileType.Wall)
                    InstantiateFromArray(floorTiles, i, j);

                // If the tile type is Wall...
                if (tiles[i][j] == TileType.Wall)
                {
                    // ... instantiate a wall over the top.
                    InstantiateFromArray(outerWallTiles, i, j);
                }
            }
        }
    }


    #region OuterWalls
    void InstantiateOuterWalls()
    {
        // The outer walls are one unit left, right, up and down from the board.
        float leftEdgeX = -1f;
        float rightEdgeX = columns + 0f;
        float bottomEdgeY = -1f;
        float topEdgeY = rows + 0f;

        // Instantiate both vertical walls (one on each side).
        InstantiateVerticalOuterWall(leftEdgeX, bottomEdgeY, topEdgeY);
        InstantiateVerticalOuterWall(rightEdgeX, bottomEdgeY, topEdgeY);

        // Instantiate both horizontal walls, these are one in left and right from the outer walls.
        InstantiateHorizontalOuterWall(leftEdgeX + 1f, rightEdgeX - 1f, bottomEdgeY);
        InstantiateHorizontalOuterWall(leftEdgeX + 1f, rightEdgeX - 1f, topEdgeY);
    }


    void InstantiateVerticalOuterWall(float xCoord, float startingY, float endingY)
    {
        // Start the loop at the starting value for Y.
        float currentY = startingY;

        // While the value for Y is less than the end value...
        while (currentY <= endingY)
        {
            // ... instantiate an outer wall tile at the x coordinate and the current y coordinate.
            InstantiateFromArray(outerWallTiles, xCoord, currentY);

            currentY++;
        }
    }


    void InstantiateHorizontalOuterWall(float startingX, float endingX, float yCoord)
    {
        // Start the loop at the starting value for X.
        float currentX = startingX;

        // While the value for X is less than the end value...
        while (currentX <= endingX)
        {
            // ... instantiate an outer wall tile at the y coordinate and the current x coordinate.
            InstantiateFromArray(outerWallTiles, currentX, yCoord);

            currentX++;
        }
    }
    #endregion

    // Losowe rozmieszczenie przeciwników/jedzenia
    Vector3 RandomPosition()
    {
        int randomIndex = Random.Range(0, gridPositions.Count);
        Vector3 randomPosition = gridPositions[randomIndex];
        gridPositions.RemoveAt(randomIndex);
        return randomPosition;
    }

    private void LayoutObjectAtRandom(OGRoom room, GameObject[] tileArray, IntRange range, int flatBonus = 0)
    {
        int objectCount = range.Random + flatBonus;

        for (int i = 0; i < objectCount; i++)
        {
            if (gridPositions.Count == 0)
                break;
            Vector3 randomPosition = RandomPosition();
            if (CheckForColliders(randomPosition, 0.1f))
            {
                i--;
                continue;
            }
            InstantiateFromArray(tileArray, randomPosition);
        }
    }

    private void RandomizeRoomContents(OGRoom room)
    {
        LayoutObjectAtRandom(room, foodTiles, foodPerRoom);
        LayoutObjectAtRandom(room, wallTiles, rubblePerRoom);

        int enemiesCountScaling = EnemiesCountScaling();
        LayoutObjectAtRandom(room, enemyTiles, enemiesPerRoom, enemiesCountScaling);
    }

    private bool CheckForColliders(Vector2 center, float radius)
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(center, radius);
        if (hitColliders.Length > 0)
            return true;
        return false;
    }

    private bool ClearSpace(Vector2 center, float radius)
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(center, radius);
        for (int i = 0; i < hitColliders.Length; i++)
        {
            if (hitColliders[i].name == "Player" || hitColliders[i].name == "Exit")
                return false;
            Destroy(hitColliders[i].gameObject);
        }
        return true;
    }

    private int EnemiesCountScaling()
    {
        int baseScaling = enemiesScaling.minimum * (level - 1);
        int result = enemiesScaling.Random + baseScaling;

        return result / 100;
    }
}