using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class BoardCreator : MonoBehaviour
{
	// The type of tile that will be laid in a specific position.
	public enum TileType
	{
		Wall, Room, Corridor, Empty
	}

	public int columns = 100;                                 // The number of columns on the board (how wide it will be).
	public int rows = 100;                                    // The number of rows on the board (how tall it will be).

	public IntRange numRooms = new IntRange (15, 20);         // The range of the number of rooms there can be.
	public IntRange roomWidth = new IntRange (3, 10);         // The range of widths rooms can have.
	public IntRange roomHeight = new IntRange (3, 10);        // The range of heights rooms can have.
	public IntRange corridorLength = new IntRange (6, 10);    // The range of lengths corridors between rooms can have.

	public IntRange enemiesPerRoom = new IntRange (0, 2);
	public IntRange rubblePerRoom = new IntRange (2, 8);
	public IntRange foodPerRoom = new IntRange (1, 3);

	public GameObject[] floorTiles;                           // An array of floor tile prefabs.
	public GameObject[] wallTiles;                            // An array of wall tile prefabs.
	public GameObject[] outerWallTiles;                       // An array of outer wall tile prefabs.
	public GameObject[] enemyTiles;
	public GameObject[] foodTiles;
	public GameObject player;

	private TileType[][] tiles;                               // A jagged array of tile types representing the board, like a grid.
	private Room[] rooms;                                     // All the rooms that are created for this board.
	private Corridor[] corridors;                             // All the corridors that connect the rooms.
	private GameObject boardHolder;                           // GameObject that acts as a container for all other tiles.

	private List<Vector3> gridPositions = new List<Vector3>(); //Positions grid for randomizing objects inside each room
	private int level;


	public void SetupScene(int level){
		this.level = level;
		BoardSetup();
	}

	private void BoardSetup ()
	{
		// Create the board holder.
		boardHolder = new GameObject("BoardHolder");

		gridPositions.Clear ();

		SetupTilesArray ();

		CreateRoomsAndCorridors ();

		SetTilesValuesForRooms ();
		SetTilesValuesForCorridors ();

		InstantiateTiles ();
		//InstantiateOuterWalls ();
	}


	void SetupTilesArray ()
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


	void CreateRoomsAndCorridors ()
	{
		// Create the rooms array with a random size.
		rooms = new Room[numRooms.Random];

		// There should be one less corridor than there is rooms.
		corridors = new Corridor[rooms.Length - 1];

		// Create the first room and corridor.
		rooms[0] = new Room ();
		corridors[0] = new Corridor ();

		// Setup the first room, there is no previous corridor so we do not use one.
		rooms[0].SetupRoom(roomWidth, roomHeight, columns, rows);

		// Setup the first corridor using the first room.
		corridors[0].SetupCorridor(rooms[0], corridorLength, roomWidth, roomHeight, columns, rows, true);

		for (int i = 1; i < rooms.Length; i++)
		{
			// Create a room.
			rooms[i] = new Room ();

			// Setup the room based on the previous corridor.
			rooms[i].SetupRoom (roomWidth, roomHeight, columns, rows, corridors[i - 1]);

			// If we haven't reached the end of the corridors array...
			if (i < corridors.Length)
			{
				// ... create a corridor.
				corridors[i] = new Corridor ();

				// Setup the corridor based on the room that was just created.
				corridors[i].SetupCorridor(rooms[i], corridorLength, roomWidth, roomHeight, columns, rows, false);
			}

			if (i == 2)
			{
				Debug.Log ("Hello", gameObject);
				Vector3 playerPos = new Vector3 (rooms[i].xPos, rooms[i].yPos, 0);
				player.transform.position = playerPos;
			}
		}

	}


	void SetTilesValuesForRooms ()
	{
		// Go through all the rooms
		for (int i = 0; i < rooms.Length; i++)
		{
			int xCoord, yCoord;
			Room currentRoom = rooms[i];

			gridPositions.Clear ();

			//Bottom wall
			yCoord = currentRoom.yPos - 1;
			for (int j = -1; j < currentRoom.roomWidth + 1; j++) {
				xCoord = currentRoom.xPos + j;
				Debug.Log (xCoord + " " + yCoord, gameObject);
				if(tiles[xCoord][yCoord] != TileType.Room)
					tiles[xCoord][yCoord] = TileType.Wall;
			}
			//Top wall
			yCoord = currentRoom.yPos + currentRoom.roomHeight;
			for (int j = -1; j < currentRoom.roomWidth + 1; j++) {
				xCoord = currentRoom.xPos + j;
				if(tiles[xCoord][yCoord] != TileType.Room)
					tiles[xCoord][yCoord] = TileType.Wall;
			}
			//Left wall
			xCoord = currentRoom.xPos - 1 ;
			for (int j = 0; j < currentRoom.roomHeight + 1; j++) {
				yCoord = currentRoom.yPos + j;
				if(tiles[xCoord][yCoord] != TileType.Room)
					tiles[xCoord][yCoord] = TileType.Wall;
			}
			//Right wall
			xCoord = currentRoom.xPos + currentRoom.roomWidth;
			for (int j = 0; j < currentRoom.roomHeight + 1; j++) {
				yCoord = currentRoom.yPos + j;
				if(tiles[xCoord][yCoord] != TileType.Room)
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

			RandomizeRoomContents(currentRoom);
		}
	}


	void SetTilesValuesForCorridors ()
	{
		// Go through every corridor...
		for (int i = 0; i < corridors.Length; i++)
		{
			Corridor currentCorridor = corridors[i];

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
					if(tiles[xCoord - 1][yCoord] != TileType.Room && tiles[xCoord][yCoord] != TileType.Corridor)
						tiles[xCoord - 1][yCoord] = TileType.Wall;
					if(tiles[xCoord + 1][yCoord] != TileType.Room && tiles[xCoord][yCoord] != TileType.Corridor)
						tiles[xCoord + 1][yCoord] = TileType.Wall;
					break;
				case Direction.East:
					xCoord += j;
					if(tiles[xCoord][yCoord - 1] != TileType.Room && tiles[xCoord][yCoord] != TileType.Corridor)
						tiles[xCoord][yCoord - 1] = TileType.Wall;
					if(tiles[xCoord][yCoord + 1] != TileType.Room && tiles[xCoord][yCoord] != TileType.Corridor)
						tiles[xCoord][yCoord + 1] = TileType.Wall;
					break;
				case Direction.South:
					yCoord -= j;
					if(tiles[xCoord - 1][yCoord] != TileType.Room && tiles[xCoord][yCoord] != TileType.Corridor)
						tiles[xCoord - 1][yCoord] = TileType.Wall;
					if(tiles[xCoord + 1][yCoord] != TileType.Room && tiles[xCoord][yCoord] != TileType.Corridor)
						tiles[xCoord + 1][yCoord] = TileType.Wall;
					break;
				case Direction.West:
					xCoord -= j;
					if(tiles[xCoord][yCoord - 1] != TileType.Room && tiles[xCoord][yCoord] != TileType.Corridor)
						tiles[xCoord][yCoord - 1] = TileType.Wall;
					if(tiles[xCoord][yCoord + 1] != TileType.Room && tiles[xCoord][yCoord] != TileType.Corridor)
						tiles[xCoord][yCoord + 1] = TileType.Wall;
					break;
				}

				// Set the tile at these coordinates to Floor.
				if(tiles[xCoord][yCoord] != TileType.Room)
					tiles[xCoord][yCoord] = TileType.Corridor;
			}
		}
	}


	void InstantiateTiles ()
	{
		// Go through all the tiles in the jagged array...
		for (int i = 0; i < tiles.Length; i++)
		{
			for (int j = 0; j < tiles[i].Length; j++)
			{
				// ... and instantiate a floor tile for it.
				if(tiles[i][j] != TileType.Empty && tiles[i][j] != TileType.Wall)
					InstantiateFromArray (floorTiles, i, j);

				// If the tile type is Wall...
				if (tiles[i][j] == TileType.Wall)
				{
					Debug.Log ("Hello", gameObject);
					// ... instantiate a wall over the top.
					InstantiateFromArray (outerWallTiles, i, j);
				}
			}
		}
	}


	#region OuterWalls
	void InstantiateOuterWalls ()
	{
		// The outer walls are one unit left, right, up and down from the board.
		float leftEdgeX = -1f;
		float rightEdgeX = columns + 0f;
		float bottomEdgeY = -1f;
		float topEdgeY = rows + 0f;

		// Instantiate both vertical walls (one on each side).
		InstantiateVerticalOuterWall (leftEdgeX, bottomEdgeY, topEdgeY);
		InstantiateVerticalOuterWall(rightEdgeX, bottomEdgeY, topEdgeY);

		// Instantiate both horizontal walls, these are one in left and right from the outer walls.
		InstantiateHorizontalOuterWall(leftEdgeX + 1f, rightEdgeX - 1f, bottomEdgeY);
		InstantiateHorizontalOuterWall(leftEdgeX + 1f, rightEdgeX - 1f, topEdgeY);
	}


	void InstantiateVerticalOuterWall (float xCoord, float startingY, float endingY)
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


	void InstantiateHorizontalOuterWall (float startingX, float endingX, float yCoord)
	{
		// Start the loop at the starting value for X.
		float currentX = startingX;

		// While the value for X is less than the end value...
		while (currentX <= endingX)
		{
			// ... instantiate an outer wall tile at the y coordinate and the current x coordinate.
			InstantiateFromArray (outerWallTiles, currentX, yCoord);

			currentX++;
		}
	}
	#endregion

	void InstantiateFromArray (GameObject[] prefabs, float xCoord, float yCoord)
	{
		// Create a random index for the array.
		int randomIndex = Random.Range(0, prefabs.Length);

		// The position to be instantiated at is based on the coordinates.
		Vector3 position = new Vector3(xCoord, yCoord, 0f);

		// Create an instance of the prefab from the random index of the array.
		GameObject tileInstance = Instantiate(prefabs[randomIndex], position, Quaternion.identity) as GameObject;

		// Set the tile's parent to the board holder.
		tileInstance.transform.parent = boardHolder.transform;
	}


	// Losowe rozmieszczenie przeciwników/jedzenia
	Vector3 RandomPosition(){
		int randomIndex = Random.Range (0, gridPositions.Count);
		Vector3 randomPosition = gridPositions [randomIndex];
		gridPositions.RemoveAt (randomIndex);
		return randomPosition;
	}

	void LayoutObjectAtRandom(Room room, GameObject[] tileArray, IntRange range, int flatBonus = 0){
		int objectCount = range.Random;

		for(int i = 0; i < objectCount; i++){
			if (gridPositions.Count == 0)
				break;
			Vector3 randomPosition = RandomPosition ();
			GameObject tileChoice = tileArray [Random.Range (0, tileArray.Length)];
			Instantiate (tileChoice, randomPosition, Quaternion.identity);
		}
	}

	void RandomizeRoomContents(Room room){
		LayoutObjectAtRandom (room, foodTiles, foodPerRoom);
		LayoutObjectAtRandom (room, wallTiles, rubblePerRoom);

		int enemiesCountScaling = (int)Math.Log (level, 2f);
		LayoutObjectAtRandom (room, enemyTiles, enemiesPerRoom, enemiesCountScaling);

		//Instantiate (exit, new Vector3 (columns - 1, rows - 1, 0f), Quaternion.identity);
	}
}