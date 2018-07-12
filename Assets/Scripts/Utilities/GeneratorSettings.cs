public struct DTSettings
{
    public DTSettings(string presetSettings = "defalut")
    {
        seed = "default seed";
        useRandomSeed = true;
        initialRooms = 60;
        initialAreaWidth = 20;
        initialAreaHeight = 20;
        initialRoomMinWidth = 5;
        initialRoomMinHeight = 5;
        initialRoomMaxWidth = 15;
        initialRoomMaxHeight = 15;
        minRoomWidth = 7;
        minRoomHeight = 7;
        useOrCheck = true;
        additionalConnections = 20;
    }

    //Seed settings
    public string seed;
    public bool useRandomSeed;

    //Initial rooms settings
    public int initialRooms;
    public int initialAreaWidth, initialAreaHeight;
    public int initialRoomMinWidth, initialRoomMinHeight;
    public int initialRoomMaxWidth, initialRoomMaxHeight;

    //Room removal settings
    public int minRoomWidth, minRoomHeight;
    public bool useOrCheck; //false = AND check, true = OR check

    //Connection settings
    public int additionalConnections;
}

public struct BTSettings
{
    public BTSettings(string presetSettings = "defalut")
    {
        seed = "default seed";
        useRandomSeed = true;
        levelWidth = 80;
        levelHeight = 80;
        minAreaWidth = 15;
        minAreaHeight = 15;
        separationOffset = 7;
        minRoomWidth = 6;
        minRoomHeight = 6;
        maxRoomWidth = 15;
        maxRoomHeight = 15;
        useOrCheck = true;
        maxConnectionLengthX = 12;
        maxConnectionLengthY = 12;
    }
    //Seed settings
    public string seed;
    public bool useRandomSeed;

    //Bisection settings
    public int levelWidth, levelHeight;
    public int minAreaWidth, minAreaHeight;
    public int separationOffset;

    //Room settings
    public int minRoomWidth, minRoomHeight;
    public int maxRoomWidth, maxRoomHeight;
    public bool useOrCheck; //false = AND check, true = OR check

    //Connection settings
    public int maxConnectionLengthX, maxConnectionLengthY;
}

public struct CASettings
{
    public CASettings(string presetSettings = "default")
    {
        seed = "default seed";
        useRandomSeed = true;
        levelWidth = 80;
        levelHeight = 80;
        maxRoomSize = 10000;
        maxWallSize = 10000;
        enableConnections = true;

        if (presetSettings == "maze") {
            wallFill = 60;
            iterations1 = 6;
            iterations2 = 4;
            enableSecondRuleset = true;
            ruleset1 = new CARuleset(NeighbourhoodType.Moore, 3, 8, 5, 8);
            ruleset2 = new CARuleset(NeighbourhoodType.Neuman, 1, 4, 3, 3);
            minRoomSize = 8;
            minWallSize = 4;
            useDirectConnections = false;
            maxLeavingConnections = 5;
            connectionSizeFactor = 8;
            maxConnectionLengthX = 8;
            maxConnectionLengthY = 8;
        }
        else if (presetSettings == "severs") {
            wallFill = 60;
            iterations1 = 6;
            iterations2 = 4;
            enableSecondRuleset = true;
            ruleset1 = new CARuleset(NeighbourhoodType.Moore, 3, 8, 5, 8);
            ruleset2 = new CARuleset(NeighbourhoodType.Neuman, 1, 4, 3, 3);
            minRoomSize = 8;
            minWallSize = 4;
            useDirectConnections = false;
            maxLeavingConnections = 5;
            connectionSizeFactor = 8;
            maxConnectionLengthX = 8;
            maxConnectionLengthY = 8;
        }
        else if (presetSettings == "cave") {
            wallFill = 60;
            iterations1 = 6;
            iterations2 = 4;
            enableSecondRuleset = true;
            ruleset1 = new CARuleset(NeighbourhoodType.Moore, 3, 8, 5, 8);
            ruleset2 = new CARuleset(NeighbourhoodType.Neuman, 1, 4, 3, 3);
            minRoomSize = 8;
            minWallSize = 4;
            useDirectConnections = false;
            maxLeavingConnections = 5;
            connectionSizeFactor = 8;
            maxConnectionLengthX = 8;
            maxConnectionLengthY = 8;
        }
        else
        {
            wallFill = 60;
            iterations1 = 6;
            iterations2 = 4;
            enableSecondRuleset = true;
            ruleset1 = new CARuleset(NeighbourhoodType.Moore, 3, 8, 5, 8);
            ruleset2 = new CARuleset(NeighbourhoodType.Neuman, 1, 4, 3, 3);
            minRoomSize = 8;
            minWallSize = 4;
            useDirectConnections = false;
            maxLeavingConnections = 5;
            connectionSizeFactor = 8;
            maxConnectionLengthX = 8;
            maxConnectionLengthY = 8;
        }
    }

    //Seed settings
    public string seed;
    public bool useRandomSeed;

    //Level settings
    public int levelWidth, levelHeight;
    public int wallFill;

    //Rulesets
    public int iterations1, iterations2;
    public bool enableSecondRuleset;
    public CARuleset ruleset1, ruleset2;

    //Room settings
    public int minRoomSize, maxRoomSize;
    public int minWallSize, maxWallSize;

    //Connection settings
    public bool enableConnections;
    public bool useDirectConnections;
    public int maxLeavingConnections;
    public int connectionSizeFactor;
    public int maxConnectionLengthX, maxConnectionLengthY;
   
}

public enum NeighbourhoodType
{
    Moore, Neuman
}

public struct CARuleset
{
    public NeighbourhoodType neighbourhoodType;
    public int survMin, survMax, newMin, newMax;
    public CARuleset(NeighbourhoodType nt, int survMin, int survMax, int newMin, int newMax)
    {
        this.neighbourhoodType = nt;
        this.survMin = survMin;
        this.survMax = survMax;
        this.newMin = newMin;
        this.newMax = newMax;
    }
}

public struct SGSettings
{
    public SGSettings(string presetSettings = "default")
    {
        levelWidth = 40;
        levelHeight = 40;
        roomNumberMin = 10;
        roomNumberMax = 20;
        roomWidthMin = 5;
        roomWidthMax = 10;
        roomHeightMin = 5;
        roomHeightMax = 10;
        connectionLengthMin = 3;
        connectionLengthMax = 10;
    }
    //Level settings
    public int levelWidth, levelHeight;
    public int roomNumberMin, roomNumberMax;
    public int roomWidthMin, roomWidthMax;
    public int roomHeightMin, roomHeightMax;
    public int connectionLengthMin, connectionLengthMax;
}