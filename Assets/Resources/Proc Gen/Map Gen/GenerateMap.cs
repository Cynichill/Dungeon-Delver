using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GenerateMap : MonoBehaviour
{

    //Debug & Showcase variables

    //Control
    public PlayerControl controls;
    public bool debugControl;
    private int mapSizeX = 0;
    private int mapSizeY = 0;
    [SerializeField] private GameObject floorPrefab;
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private int wallThresholdSize = 50;
    [SerializeField] private int floorThresholdSize = 50;
    [SerializeField] private int passagewayRadius = 1;
    private int[,] tempGrid;

    private List<int> usedSpots = new List<int>();

    private Transform player;
    private Transform exit;
    private List<Island> leftoverIslands = new List<Island>();

    private void Awake()
    {
        controls = new PlayerControl();
        if (debugControl)
        {
            controls.Debug.CAIteration.performed += ctx => ApplyCellularAutomata(tempGrid, 1);
            controls.Debug.Floodfill.performed += ctx => FloodFill();
            controls.Debug.ConnectIslands.performed += ctx => ConnectAllIslands();
            controls.Debug.PlaceObjects.performed += ctx => PlaceObjects(leftoverIslands.First());
            controls.Debug.SpawnGrid.performed += ctx => SpawnGrid(tempGrid);
            controls.Debug.DestroyChildren.performed += ctx => DestroyChildren();
        }
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        exit = GameObject.FindGameObjectWithTag("Exit").transform;
    }

    struct TileCoordinate
    {
        public int xCoord;
        public int yCoord;

        public TileCoordinate(int x, int y)
        {
            xCoord = x;
            yCoord = y;
        }
    }

    class Island
    {
        public List<TileCoordinate> tiles; //List of all tiles in the Island
        public List<TileCoordinate> edgeTiles; //List of all edge tiles in the Island
        public int IslandSize;

        public Island()
        {
        }

        public Island(List<TileCoordinate> IslandTiles, int[,] grid) //Take in a list of tiles and the map
        {
            tiles = IslandTiles;
            IslandSize = tiles.Count;
            edgeTiles = new List<TileCoordinate>();

            foreach (TileCoordinate tile in tiles) //For each tile..
            {
                for (int x = tile.xCoord - 1; x <= tile.xCoord + 1; x++) //Check all cardinals
                {
                    for (int y = tile.yCoord - 1; y <= tile.yCoord + 1; y++)
                    {
                        if (x == tile.xCoord || y == tile.yCoord) //Check ONLY cardinals
                        {
                            if (grid[x, y] == 1) //If wall is found
                            {
                                edgeTiles.Add(tile); //Add to edge tile list
                            }
                        }
                    }
                }
            }
        }
    }

    public void GenerateDungeon(int[,] grid, int count, int mapSizeXParam, int mapSizeYParam)
    {
        mapSizeX = mapSizeXParam;
        mapSizeY = mapSizeYParam;
        tempGrid = grid; //On first iteration, tempgrid is noise map, on further it's the next iteration

        if (!debugControl)
        {
            ApplyCellularAutomata(grid, count); //Apply CA to noise map
            FloodFill(); //Use floodfill to fill and remove areas of the map
            ConnectAllIslands(); //Connect all islands 
            PlaceObjects(leftoverIslands.First());//This will be the final map, place player and exit somewhere
            SpawnGrid(tempGrid); //Spawn dungeon
        }
    }

    private void ApplyCellularAutomata(int[,] grid, int count)
    {
        //For x iterations
        for (int i = 0; i < count; i++)
        {
            //For each cell of the grid..
            for (int j = 0; j < mapSizeX; j++)
            {
                for (int k = 0; k < mapSizeY; k++)
                {
                    int wallCount = 0;

                    //Count the amount of neighbours that are 'wall tiles'
                    for (int x = j - 1; x <= j + 1; x++)
                    {
                        for (int y = k - 1; y <= k + 1; y++)
                        {
                            //If cell exists outside map, count it as a wall
                            if (checkMapBoundary(x, y))
                            {
                                //Do not add the current cell to the count
                                if (x != j || y != k)
                                {
                                    //If neighbouring cell is wall, add to count
                                    if (grid[x, y] == 1)
                                    {
                                        wallCount++;
                                    }
                                }
                            }
                            else
                            {
                                wallCount++;
                            }
                        }
                    }

                    //Add new cell to new grid based on old grid's neighbouring cells
                    if (wallCount > 4)
                    {
                        tempGrid[j, k] = 1; //Wall
                    }
                    else if (wallCount < 4)
                    {
                        tempGrid[j, k] = 0; //Floor
                    }
                    else if (wallCount == 4)
                    {
                        tempGrid[j, k] = grid[j, k]; //If = 4, remain same
                    }
                }
            }

            grid = tempGrid;
        }
    }

    private bool checkMapBoundary(int x, int y)
    {
        //If cell exists over or under the boundaries of the map
        if (x >= mapSizeX || y >= mapSizeY || x < 0 || y < 0)
        {
            return false;
        }

        //If all checks pass, return true
        return true;

    }

    private List<List<TileCoordinate>> GetTileRegions(int tileType)
    {
        List<List<TileCoordinate>> regions = new List<List<TileCoordinate>>(); //Create a list of tile regions, or connected tile clusters
        int[,] tileFlags = new int[mapSizeX, mapSizeY]; //Create an array equal to the size of the map to mark spaces we've checked

        for (int i = 0; i < mapSizeX; i++)
        {
            for (int j = 0; j < mapSizeY; j++)
            {
                if (tileFlags[i, j] == 0 && tempGrid[i, j] == tileType) //If area has not been checked and is of the type we are checking..
                {
                    List<TileCoordinate> newRegion = GetSurroundingTiles(i, j); //Create a list of surrounding connected tiles of the same type
                    regions.Add(newRegion);

                    foreach (TileCoordinate tile in newRegion)
                    {
                        tileFlags[tile.xCoord, tile.yCoord] = 1; //Mark each tile as checked
                    }
                }
            }
        }

        return regions;
    }

    private List<TileCoordinate> GetSurroundingTiles(int initX, int initY)
    {
        List<TileCoordinate> tiles = new List<TileCoordinate>(); //Create a list of tiles
        int[,] tileFlags = new int[mapSizeX, mapSizeY]; //Create a list of tile flags to mark which have been checked
        int tileType = tempGrid[initX, initY]; //Match tile type of initial tile

        Queue<TileCoordinate> queue = new Queue<TileCoordinate>(); //Queue up initial tile
        queue.Enqueue(new TileCoordinate(initX, initY));
        tileFlags[initX, initY] = 1; //Mark initial tile as checked

        while (queue.Count > 0)
        {
            TileCoordinate tile = queue.Dequeue(); //Dequeue current tile
            tiles.Add(tile); //Add tile to list

            //Check all adjacent tiles to see if they match the type of current tile
            for (int x = tile.xCoord - 1; x <= tile.xCoord + 1; x++)
            {
                for (int y = tile.yCoord - 1; y <= tile.yCoord + 1; y++)
                {
                    if (checkMapBoundary(x, y) && (x == tile.xCoord || y == tile.yCoord)) //If tile is adjacent and in bounds..
                    {
                        if (tileFlags[x, y] == 0 && tempGrid[x, y] == tileType)
                        {
                            tileFlags[x, y] = 1; //Mark as flagged
                            queue.Enqueue(new TileCoordinate(x, y)); //Queue tile
                        }
                    }
                }
            }
        }

        return tiles;
    }

    private void FloodFill()
    {
        List<List<TileCoordinate>> wallRegions = GetTileRegions(1); //Find all tiles that are walls
        foreach (List<TileCoordinate> wallRegion in wallRegions)
        {
            if (wallRegion.Count < wallThresholdSize) //Destroy small clusters of walls that are smaller than the threshold
            {
                foreach (TileCoordinate tile in wallRegion)
                {
                    tempGrid[tile.xCoord, tile.yCoord] = 0;
                }
            }
        }

        List<List<TileCoordinate>> floorRegions = GetTileRegions(0); //Find all tiles that are floors
        foreach (List<TileCoordinate> floorRegion in floorRegions)
        {
            if (floorRegion.Count < floorThresholdSize) //Destroy small clusters of floors that are smaller than the threshold
            {
                foreach (TileCoordinate tile in floorRegion)
                {
                    tempGrid[tile.xCoord, tile.yCoord] = 1;
                }
            }
            else
            {
                leftoverIslands.Add(new Island(floorRegion, tempGrid)); //If Island is large enough, add to list of Islands to combine
            }
        }

    }

    private void ConnectAllIslands()
    {
        while (leftoverIslands.Count > 1) //Connect seperated Islands until there is only one big connected Island
        {

            ConnectIslands(leftoverIslands);
            leftoverIslands.Clear(); //Clear list

            List<List<TileCoordinate>> newFloorRegions = GetTileRegions(0); //Find all seperated Islands

            foreach (List<TileCoordinate> floorRegion in newFloorRegions)
            {
                leftoverIslands.Add(new Island(floorRegion, tempGrid)); //Get new list of leftover Islands
            }
        }
    }

    private void ConnectIslands(List<Island> leftoverIslands)
    {

        int closestDistance = 0;
        TileCoordinate bestTile1 = new TileCoordinate();
        TileCoordinate bestTile2 = new TileCoordinate();
        Island bestIsland1 = new Island();
        Island bestIsland2 = new Island();
        bool connectionFound = false;


        //Go through each Island, connect closest Islands
        foreach (Island Island1 in leftoverIslands)
        {
            connectionFound = false;
            foreach (Island Island2 in leftoverIslands)
            {
                if (Island1 == Island2) //Do not connect same Island to itself
                {
                    continue;
                }

                for (int tiles1 = 0; tiles1 < Island1.edgeTiles.Count; tiles1++)
                {
                    for (int tiles2 = 0; tiles2 < Island2.edgeTiles.Count; tiles2++)
                    {
                        int distanceBetweenTiles = (int)(Mathf.Pow(Island1.edgeTiles[tiles1].xCoord - Island2.edgeTiles[tiles2].xCoord, 2) + Mathf.Pow(Island1.edgeTiles[tiles1].yCoord - Island2.edgeTiles[tiles2].yCoord, 2)); //Find distance between tiles

                        if (distanceBetweenTiles < closestDistance || !connectionFound) //Use for loop to find closest Island and closest tile for each Island
                        {
                            closestDistance = distanceBetweenTiles;
                            bestTile1 = Island1.edgeTiles[tiles1];
                            bestTile2 = Island2.edgeTiles[tiles2];
                            bestIsland1 = Island1;
                            bestIsland2 = Island2;
                            connectionFound = true;
                        }
                    }
                }
            }

            if (connectionFound)
            {
                CreatePassage(bestIsland1, bestIsland2, bestTile1, bestTile2);
            }
        }
    }

    private void CreatePassage(Island Island1, Island Island2, TileCoordinate tile1, TileCoordinate tile2)
    {
        Debug.DrawLine(CoordToWorldPoint(tile1), CoordToWorldPoint(tile2), Color.green, 100);
        List<TileCoordinate> line = PassageLine(tile1, tile2); //Get line from point 1 to point 1

        //Turn line into passageway
        foreach (TileCoordinate tile in line)
        {
            ExpandLine(tile, passagewayRadius);
        }
    }

    private Vector3 CoordToWorldPoint(TileCoordinate tile)
    {
        return new Vector3(tile.xCoord, tile.yCoord, 0);
    }

    private List<TileCoordinate> PassageLine(TileCoordinate point1, TileCoordinate point2)
    {

        //Get the line from tile 1 to tile 2 in tiles

        // Line is y = mx + c

        //Achieve line by 'plotting' line on grid

        //y increases if mx + 0.5 >= 1

        List<TileCoordinate> line = new List<TileCoordinate>();


        //Get starting point
        int x = point1.xCoord;
        int y = point1.yCoord;

        //Get gradient values
        int dx = point2.xCoord - x;
        int dy = point2.yCoord - y;

        //invert line if dy > dx
        bool inverted = false;

        //Incremental steps (always -1 or 1)
        int step = (int)Mathf.Sign(dx);
        int gradientStep = (int)Mathf.Sign(dy);

        //Get absolute values 
        int longest = Mathf.Abs(dx);
        int shortest = Mathf.Abs(dy);

        //if dy > dx
        if (longest < shortest)
        {
            inverted = true;

            //Swap values for formula
            longest = Mathf.Abs(dy);
            shortest = Mathf.Abs(dx);

            step = (int)Mathf.Sign(dy);
            gradientStep = (int)Mathf.Sign(dx);
        }

        int gradAccum = longest / 2; //Gradient Accumulation is = dx/2, 

        for (int i = 0; i < longest; i++)
        {
            //Add current point to line
            line.Add(new TileCoordinate(x, y));

            //If inverted, add step to Y, else add to X
            if (inverted)
            {
                y += step;
            }
            else
            {
                x += step;
            }

            //Increase gradient accum by shortest
            gradAccum += shortest;

            if (gradAccum >= longest) //If GA is equal to or greater than dx, increase y value
            {

                //Add to x or y based on inversion
                if (inverted)
                {
                    x += gradientStep;
                }
                else
                {
                    y += gradientStep;
                }

                //Reset gradAccumn to re-do process
                gradAccum -= longest;
            }

        }

        return line;
    }

    private void ExpandLine(TileCoordinate coord, int radius)
    {
        //For radius of line..
        for (int i = -radius; i <= radius; i++)
        {
            for (int j = -radius; j <= radius; j++)
            {
                //If X coord ^2 + Y coord ^2 <= Radius ^2..
                if ((int)Mathf.Pow(i, 2) + (int)Mathf.Pow(j, 2) <= (int)Mathf.Pow(radius, 2))
                {
                    if (checkMapBoundary(coord.xCoord + i, coord.yCoord + j)) //Check if proposed value is within map boundary
                    {
                        tempGrid[coord.xCoord + i, coord.yCoord + j] = 0; //Create line
                    }
                }
            }
        }
    }

    private void PlaceObjects(Island viableTiles)
    {
        int playerSpawn = AllocateSpot(viableTiles);
        //Place player on random viable tile
        player.transform.position = new Vector3(viableTiles.tiles[playerSpawn].xCoord, viableTiles.tiles[playerSpawn].yCoord, 0);

        int exitSpawn = AllocateSpot(viableTiles);
        //Place exit on random viable tile
        exit.transform.position = new Vector3(viableTiles.tiles[exitSpawn].xCoord, viableTiles.tiles[exitSpawn].yCoord, 0);

        //IF SYSTEM HAS MORE OBJECTS TO PLACE THAN VIABLE TILES, ALLOCATESPOT WILL CAUSE INFINITE LOOP
    }

    private int AllocateSpot(Island viableTiles)
    {
        //Get random seed
        System.Random randomSeed = new System.Random(Time.time.ToString().GetHashCode());

        //Get random element from viable tiles
        int randomSpot = randomSeed.Next(viableTiles.tiles.Count);

        while (CheckSpot(randomSpot))
        {
            //Generate new number if spot already taken
            randomSpot = randomSeed.Next(viableTiles.tiles.Count);
        }

        usedSpots.Add(randomSpot);

        return randomSpot;
    }

    private bool CheckSpot(int randomSpot)
    {
        foreach (int spot in usedSpots)
        {
            if (randomSpot == spot)
            {
                return true;
            }
        }

        return false;
    }

    private void SpawnGrid(int[,] grid)
    {
        //Create gameobjects using prefabs based on grid
        for (int i = 0; i < mapSizeX; i++)
        {
            for (int j = 0; j < mapSizeY; j++)
            {
                if (grid[i, j] == 0)
                {
                    var tile = Instantiate(floorPrefab, new Vector3(i, j, 0), Quaternion.identity);
                    tile.transform.parent = gameObject.transform;
                }
                else if (grid[i, j] == 1)
                {
                    var tile = Instantiate(wallPrefab, new Vector3(i, j, 0), Quaternion.identity);
                    tile.transform.parent = gameObject.transform;
                }
            }
        }
    }

    private void DestroyChildren()
    {
        foreach (Transform child in transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }
}
