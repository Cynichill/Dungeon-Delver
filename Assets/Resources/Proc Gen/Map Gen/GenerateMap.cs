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
    public bool testingDLA;

    //Map Size
    private int mapSizeX = 0;
    private int mapSizeY = 0;

    //Prefabs
    [SerializeField] private GameObject floorPrefab;
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject timerPrefab;
    [SerializeField] private GameObject chaserPrefab;
    [SerializeField] private GameObject collectorPrefab;
    private Transform objectParent;

    //Generation Rules
    [SerializeField] private int wallThresholdSize = 50;
    [SerializeField] private int floorThresholdSize = 50;
    [SerializeField] private int passagewayRadius = 1;
    public TileCoordinate[,] tempGrid;
    private List<int> usedSpots = new List<int>();
    private List<int> usedWorstSpots = new List<int>();
    private Transform player;
    private Transform exit;
    private List<Island> leftoverIslands = new List<Island>();
    private List<TileCoordinate> worstTiles = new List<TileCoordinate>();
    private GameManager gm;
    [SerializeField] private PathFind pf;
    private int threshold = 20;
    private List<TileCoordinate> farAwayTiles = new List<TileCoordinate>();

    private void Awake()
    {
        gm = GameObject.FindGameObjectWithTag("gm").GetComponent<GameManager>();
        debugControl = gm.debugOn;
        controls = new PlayerControl();
        controls.Debug.EnableDebug.performed += ctx => gm.RestartScene(true);

        player = GameObject.FindGameObjectWithTag("Player").transform;
        exit = GameObject.FindGameObjectWithTag("Exit").transform;
        objectParent = GameObject.FindGameObjectWithTag("ObjParent").transform;

        if (debugControl)
        {
            controls.Debug.CAIteration.performed += ctx => CADebug();
            controls.Debug.Floodfill.performed += ctx => FloodFillDebug();
            controls.Debug.ConnectIslands.performed += ctx => ConnectDebug();
            controls.Debug.PlaceObjects.performed += ctx => PlaceObjectsDebug();
            controls.Debug.SpawnGrid.performed += ctx => SpawnGridNow();
            controls.Debug.DLA.performed += ctx => DLADebug();
        }
    }

    public void GenerateDungeon(TileCoordinate[,] grid, int count, int mapSizeXParam, int mapSizeYParam)
    {
        mapSizeX = mapSizeXParam;
        mapSizeY = mapSizeYParam;
        tempGrid = grid; //On first iteration, tempgrid is noise map, on further it's the next iteration

        pf.GetGridSize(mapSizeX, mapSizeY);

        if (!debugControl)
        {
            if (testingDLA)
            {
                ApplyDiffusionLimitedAggregation(); //Apply DLA
            }
            ApplyCellularAutomata(tempGrid, count); //Apply CA to noise map or DLA
            FloodFill(); //Use floodfill to fill and remove areas of the map
            ConnectAllIslands(); //Connect all islands 
            PlaceObjects(leftoverIslands.First());//This will be the final map, place player and exit somewhere
            SpawnGrid(tempGrid); //Spawn dungeon
        }
    }

    private void SpawnGridNow()
    {
        DestroyChildren();
        SpawnGrid(tempGrid);
    }

    private void CADebug()
    {
        ApplyCellularAutomata(tempGrid, 1);
        SpawnGridNow();
    }

    private void DLADebug()
    {
        ApplyDiffusionLimitedAggregation();
        SpawnGridNow();
    }

    private void FloodFillDebug()
    {
        FloodFill();
        SpawnGridNow();
    }

    private void ConnectDebug()
    {
        ConnectAllIslands();
        SpawnGridNow();
    }

    private void PlaceObjectsDebug()
    {
        PlaceObjects(leftoverIslands.First());
        SpawnGridNow();
    }

    private void ApplyCellularAutomata(TileCoordinate[,] grid, int count)
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
                                    if (grid[x, y].floor == false)
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
                        tempGrid[j, k].floor = false; //Wall
                    }
                    else if (wallCount < 4)
                    {
                        tempGrid[j, k].floor = true; //Floor
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

    private void ApplyDiffusionLimitedAggregation()
    {
        List<TileCoordinate> floorTiles = new List<TileCoordinate>();

        //Carve out small section of squares in approximate middle of map
        for (int x = mapSizeX / 2 - 1; x <= mapSizeX / 2 + 1; x++)
        {
            for (int y = mapSizeY / 2 - 1; y <= mapSizeY / 2 + 1; y++)
            {
                if (checkMapBoundary(x, y))
                {
                    tempGrid[x, y].floor = true;
                }
            }
        }

        floorTiles = GetSurroundingTiles(mapSizeX / 2, mapSizeY / 2);

        int maxSteps = 500; //Random.Range(150, 200);
        int maxShots = 8; //Random.Range(15, 40);

        List<TileCoordinate> dugTiles = new List<TileCoordinate>();
        List<TileCoordinate> tempDugTiles = new List<TileCoordinate>();

        //Get random seed
        System.Random randomSeed = new System.Random(Time.time.ToString().GetHashCode());

        for (int q = 0; q < maxShots; q++)
        {
            int i = 0;

            //Choose random floor tile from starting 'seed'
            int startTile = randomSeed.Next(floorTiles.Count);
            int x = floorTiles[startTile].xCoord;
            int y = floorTiles[startTile].yCoord;

            int moveDirection = 0;
            int bannedDirection = 5;

            while (i < maxSteps)
            {
                //Cardinal direction
                moveDirection = randomSeed.Next(0, 4);

                while (moveDirection == bannedDirection)
                {
                    moveDirection = randomSeed.Next(0, 4);
                }

                switch (moveDirection)
                {
                    //Up
                    case 0:
                        y += 1;
                        bannedDirection = 1;
                        break;

                    //Down
                    case 1:
                        y -= 1;
                        bannedDirection = 0;
                        break;

                    //Left
                    case 2:
                        x -= 1;
                        bannedDirection = 3;
                        break;

                    //Right
                    case 3:
                        x += 1;
                        bannedDirection = 2;
                        break;
                }

                i++;

                //If new tile is in range..
                if (checkMapBoundary(x, y))
                {
                    foreach (TileCoordinate tile in dugTiles)
                    {
                        if (tile.xCoord == x && tile.yCoord == y)
                        {
                            //Ran into dug tile, stop!
                            break;
                        }
                    }

                    //If new tile is a wall
                    if (tempGrid[x, y].floor == false)
                    {
                        //Turn it into a floor
                        tempGrid[x, y].floor = true;
                        //Add tile to available floor tiles
                        //floorTiles.Add(new TileCoordinate(x, y));
                        tempDugTiles.Add(tempGrid[x, y]);
                        //break;
                    }
                }
                else
                {
                    break;
                }
            }

            foreach (TileCoordinate tile in tempDugTiles)
            {
                dugTiles.Add(tile);
            }
            tempDugTiles.Clear();

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

    private List<List<TileCoordinate>> GetTileRegions(bool tileType)
    {
        List<List<TileCoordinate>> regions = new List<List<TileCoordinate>>(); //Create a list of tile regions, or connected tile clusters
        int[,] tileFlags = new int[mapSizeX, mapSizeY]; //Create an array equal to the size of the map to mark spaces we've checked

        for (int i = 0; i < mapSizeX; i++)
        {
            for (int j = 0; j < mapSizeY; j++)
            {
                if (tileFlags[i, j] == 0 && tempGrid[i, j].floor == tileType) //If area has not been checked and is of the type we are checking..
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
        bool tileType = tempGrid[initX, initY].floor; //Match tile type of initial tile

        Queue<TileCoordinate> queue = new Queue<TileCoordinate>(); //Queue up initial tile
        queue.Enqueue(new TileCoordinate(initX, initY));
        tileFlags[initX, initY] = 1; //Mark initial tile as checked

        while (queue.Count > 0)
        {
            tiles.Add(queue.Dequeue()); //Add tile to list
            TileCoordinate tile = tiles.Last(); //Dequeue current tile

            //Check all adjacent tiles to see if they match the type of current tile
            for (int x = tile.xCoord - 1; x <= tile.xCoord + 1; x++)
            {
                for (int y = tile.yCoord - 1; y <= tile.yCoord + 1; y++)
                {
                    if (checkMapBoundary(x, y) && (x == tile.xCoord || y == tile.yCoord)) //If tile is adjacent and in bounds..
                    {
                        if (tileFlags[x, y] == 0 && tempGrid[x, y].floor == tileType)
                        {
                            tileFlags[x, y] = 1; //Mark as flagged
                            queue.Enqueue(tempGrid[x, y]); //Queue tile
                        }
                    }
                }
            }
        }

        return tiles;
    }

    private void FloodFill()
    {
        List<List<TileCoordinate>> wallRegions = GetTileRegions(false); //Find all tiles that are walls
        foreach (List<TileCoordinate> wallRegion in wallRegions)
        {
            if (wallRegion.Count < wallThresholdSize) //Destroy small clusters of walls that are smaller than the threshold
            {
                foreach (TileCoordinate tile in wallRegion)
                {
                    tempGrid[tile.xCoord, tile.yCoord].floor = true;
                }
            }
        }

        List<List<TileCoordinate>> floorRegions = GetTileRegions(true); //Find all tiles that are floors
        foreach (List<TileCoordinate> floorRegion in floorRegions)
        {
            if (floorRegion.Count < floorThresholdSize) //Destroy small clusters of floors that are smaller than the threshold
            {
                foreach (TileCoordinate tile in floorRegion)
                {
                    tempGrid[tile.xCoord, tile.yCoord].floor = false;
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

            List<List<TileCoordinate>> newFloorRegions = GetTileRegions(true); //Find all seperated Islands

            foreach (List<TileCoordinate> floorRegion in newFloorRegions)
            {
                leftoverIslands.Add(new Island(floorRegion, tempGrid)); //Get new list of leftover Islands
            }
        }
    }

    private void ConnectIslands(List<Island> leftoverIslands)
    {

        int closestDistance = 0;
        int longestDistance = 999;
        TileCoordinate bestTile1 = new TileCoordinate();
        TileCoordinate bestTile2 = new TileCoordinate();
        TileCoordinate worstTile1 = new TileCoordinate();
        TileCoordinate worstTile2 = new TileCoordinate();
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

                        if (distanceBetweenTiles > longestDistance || !connectionFound)
                        {
                            worstTile1 = Island1.edgeTiles[tiles1];
                            worstTile2 = Island2.edgeTiles[tiles2];
                        }

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
                if (!worstTiles.Contains(worstTile1))
                {
                    worstTiles.Add(worstTile1);
                }
                if (!worstTiles.Contains(worstTile2))
                {
                    worstTiles.Add(worstTile2);
                }
                CreatePassage(bestIsland1, bestIsland2, bestTile1, bestTile2);
            }
        }
    }

    private void CreatePassage(Island Island1, Island Island2, TileCoordinate tile1, TileCoordinate tile2)
    {
        //Debug.DrawLine(CoordToWorldPoint(tile1), CoordToWorldPoint(tile2), Color.green, 100);
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
            line.Add(tempGrid[x, y]);

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
                        tempGrid[coord.xCoord + i, coord.yCoord + j].floor = true; //Create line
                    }
                }
            }
        }
    }

    private void PlaceObjects(Island viableTiles)
    {

        List<TileCoordinate> sources = new List<TileCoordinate>();

        int playerSpawn = AllocateSpot(viableTiles.tiles);

        //Place player on random viable tile
        player.transform.position = new Vector3(viableTiles.tiles[playerSpawn].xCoord, viableTiles.tiles[playerSpawn].yCoord, 0);

        sources.Add(viableTiles.tiles[playerSpawn]);

        CreateDijkstraMap(sources);

        if (farAwayTiles.Count == 0)
        {
            //Bad generation, can't place exit
            gm.RestartScene(false);
        }

        int exitSpawn = AllocateSpot(farAwayTiles);

        //Place exit on random viable tile
        exit.transform.position = new Vector3(farAwayTiles[exitSpawn].xCoord, farAwayTiles[exitSpawn].yCoord, 0);

        sources.Add(farAwayTiles[exitSpawn]);

        CreateDijkstraMap(sources);

        if (farAwayTiles.Count > 0)
        {

            int enemySpawn = AllocateSpot(farAwayTiles);

            var chaser = Instantiate(chaserPrefab, new Vector3(farAwayTiles[enemySpawn].xCoord, farAwayTiles[enemySpawn].yCoord, 0), Quaternion.identity);
            chaser.transform.parent = objectParent.transform;
            chaser.GetComponent<ChaserAI>().grid = tempGrid;
            chaser.GetComponent<ChaserAI>().FindObjects();
            chaser.GetComponent<ChaserAI>().SetTarget(player.transform.position);

            sources.Add(farAwayTiles[enemySpawn]);
            CreateDijkstraMap(sources);

        }

        if (farAwayTiles.Count > 0)
        {
            int collectorSpawn = AllocateSpot(farAwayTiles);

            var collector = Instantiate(collectorPrefab, new Vector3(farAwayTiles[collectorSpawn].xCoord, farAwayTiles[collectorSpawn].yCoord, 0), Quaternion.identity);
            collector.transform.parent = objectParent.transform;
            collector.GetComponent<CollectorAI>().grid = tempGrid;

            sources.Add(farAwayTiles[collectorSpawn]);
            CreateDijkstraMap(sources);


            List<GameObject> collectList = new List<GameObject>();

            while (farAwayTiles.Count > 0)
            {
                int collectable = AllocateSpot(farAwayTiles);
                var collect = Instantiate(timerPrefab, new Vector3(farAwayTiles[collectable].xCoord, farAwayTiles[collectable].yCoord, 0), Quaternion.identity);
                collect.transform.parent = objectParent.transform;

                collectList.Add(collect);

                sources.Add(farAwayTiles[collectable]);
                CreateDijkstraMap(sources);
            }

            collector.GetComponent<CollectorAI>().FindObjects(collectList);
            collector.GetComponent<CollectorAI>().SetTarget();
        }
    }

    private int AllocateSpot(List<TileCoordinate> viableTiles)
    {
        //Get random seed
        System.Random randomSeed = new System.Random(Time.time.ToString().GetHashCode());

        int randomSpot;
        int limit;
        int limitBreak = 0;

        //Get random element from viable tiles
        randomSpot = randomSeed.Next(viableTiles.Count);

        limit = viableTiles.Count;

        while (CheckSpot(randomSpot))
        {
            //Generate new number if spot already taken
            randomSpot = randomSeed.Next(viableTiles.Count);
            limitBreak++;

            if (limitBreak == limit)
            {
                Debug.Log("All spots used! (Uh oh!)");
                return 99999;
            }
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

    private void SpawnGrid(TileCoordinate[,] grid)
    {
        //Create gameobjects using prefabs based on grid
        for (int i = 0; i < mapSizeX; i++)
        {
            for (int j = 0; j < mapSizeY; j++)
            {
                if (grid[i, j].floor == true)
                {
                    var tile = Instantiate(floorPrefab, new Vector3(i, j, 0), Quaternion.identity);
                    tile.transform.parent = gameObject.transform;

                    int store = grid[i, j].dijkstraDistance;

                    if (store == 0)
                    {
                        tile.GetComponent<SpriteRenderer>().color = Color.white;
                    }
                    else if (store >= 1 && store <= 5)
                    {
                        tile.GetComponent<SpriteRenderer>().color = Color.red;
                    }
                    else if (store >= 6 && store <= 10)
                    {
                        tile.GetComponent<SpriteRenderer>().color = new Color(1.0f, 0.64f, 0.0f); //Orange
                    }
                    else if (store >= 11 && store <= 15)
                    {
                        tile.GetComponent<SpriteRenderer>().color = Color.yellow;
                    }
                    else if (store >= 16 && store <= 20)
                    {
                        tile.GetComponent<SpriteRenderer>().color = Color.green;
                    }
                    else if (store >= 21 && store <= 25)
                    {
                        tile.GetComponent<SpriteRenderer>().color = Color.blue;
                    }
                    else if (store >= 26 && store <= 30)
                    {
                        tile.GetComponent<SpriteRenderer>().color = Color.magenta;
                    }
                    else if (store >= 31)
                    {
                        tile.GetComponent<SpriteRenderer>().color = Color.grey;
                    }
                }
                else if (grid[i, j].floor == false)
                {
                    var tile = Instantiate(wallPrefab, new Vector3(i, j, 0), Quaternion.identity);
                    tile.transform.parent = gameObject.transform;
                }
            }
        }
    }

    private void CreateDijkstraMap(List<TileCoordinate> sourceTiles)
    {

        foreach (TileCoordinate tile in tempGrid)
        {
            //Reset all distances and visited markers
            tile.ResetDijkstra();
        }

        foreach (TileCoordinate source in sourceTiles)
        {
            farAwayTiles.Clear();
            foreach (TileCoordinate tile in tempGrid)
            {
                //Reset all distances and visited markers
                tile.ResetMarker();
            }
            tempGrid[source.xCoord, source.yCoord].dijkstraDistance = 0;
            tempGrid[source.xCoord, source.yCoord].dijkstraVisited = true;
            FillMapFromSource(tempGrid[source.xCoord, source.yCoord]);
        }
    }

    private void FillMapFromSource(TileCoordinate source)
    {
        List<TileCoordinate> openList = new List<TileCoordinate>();
        openList.Add(source);

        while (openList.Count > 0)
        {
            TileCoordinate currentTile = openList[0];
            openList.Remove(openList[0]);

            foreach (TileCoordinate neighbour in GetListOfNeighbours(currentTile))
            {
                //If tile has already been in openList previously, do not check
                if (neighbour.dijkstraVisited)
                {
                    continue;
                }
                else
                {
                    if (neighbour.dijkstraDistance > currentTile.dijkstraDistance + 1)
                    {
                        neighbour.dijkstraDistance = currentTile.dijkstraDistance + 1;
                    }

                    neighbour.dijkstraVisited = true;

                    if (neighbour.dijkstraDistance >= threshold)
                    {
                        farAwayTiles.Add(neighbour);
                    }

                    openList.Add(neighbour);
                }
            }
        }
    }

    private List<TileCoordinate> GetListOfNeighbours(TileCoordinate currentTile)
    {
        List<TileCoordinate> neighbours = new List<TileCoordinate>();

        int x = currentTile.xCoord;
        int y = currentTile.yCoord;

        //Find cardinal neighbours only

        for (int i = x - 1; i <= x + 1; i++)
        {
            for (int j = y - 1; j <= y + 1; j++)
            {
                if (checkMapBoundary(i, j) && tempGrid[i, j].floor == true)
                {
                    //Only treat cardinal directions as neighbours
                    if (i == x || j == y)
                    {
                        if (!(i == x && j == y))
                        {
                            neighbours.Add(tempGrid[i, j]); //Add neighbour to list
                        }
                    }
                }
            }
        }
        return neighbours;
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
