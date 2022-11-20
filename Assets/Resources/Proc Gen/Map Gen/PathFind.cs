using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFind : MonoBehaviour
{
    private int[,] grid;
    private int mapX;
    private int mapY;
    private List<TileCoordinate> openList;
    private List<TileCoordinate> closedList;

    private const int moveCardinalCost = 10;

    public void GetGrid(int[,] receivedGrid, int mapSizeX, int mapSizeY)
    {
        grid = receivedGrid;
        mapX = mapSizeX;
        mapY = mapSizeY;
    }

    public List<TileCoordinate> TilePath(TileCoordinate startTile, TileCoordinate endTile)
    {
        openList = new List<TileCoordinate> { startTile };
        closedList = new List<TileCoordinate>();

        /*
                for (int x = 0; x < mapX; x++)
                {
                    for (int y = 0; y < mapY; y++)
                    {
                        if (grid[x, y] == 0)
                        {
                            TileCoordinate pathNode = new TileCoordinate(x, y);
                            pathNode.gCost = int.MaxValue;
                            pathNode.CalcFCost();
                            pathNode.prevTile = null;
                        }
                    }
                }
                */

        startTile.gCost = 0;
        startTile.hCost = CalcDistanceCost(startTile, endTile);
        startTile.CalcFCost();

        while (openList.Count > 0)
        {
            TileCoordinate currentTile = GetLowestFCostTile(openList);
            if (currentTile.xCoord == endTile.xCoord && currentTile.yCoord == endTile.yCoord)
            {
                endTile = currentTile;
                //Reached the final tile
                return CalcPath(endTile);
            }

            openList.Remove(currentTile);
            closedList.Add(currentTile);

            foreach (TileCoordinate neighbour in GetListOfNeighbours(currentTile))
            {
                if (closedList.Contains(neighbour))
                {
                    continue;
                }
                else
                {
                    int potentialGCost = currentTile.gCost + CalcDistanceCost(currentTile, neighbour);

                    if (potentialGCost < neighbour.gCost)
                    {
                        neighbour.prevTile = currentTile;
                        neighbour.gCost = potentialGCost;
                        neighbour.hCost = CalcDistanceCost(neighbour, endTile);
                        neighbour.CalcFCost();

                        bool unused = true;

                        foreach(TileCoordinate tile in openList)
                        {
                            if(tile.xCoord == neighbour.xCoord && tile.yCoord == neighbour.yCoord)
                            {
                                unused = false;
                            }
                        }

                        if(unused)
                        {
                            openList.Add(neighbour);
                        }
                        
                    }
                }
            }
        }

        Debug.Log("Didn't work!");
        //No more open tiles
        return null;
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
                if (checkMapBoundary(i, j) && grid[i, j] == 0)
                {
                    //Only treat cardinal directions as neighbours
                    if (i == x || j == y)
                    {
                        if (!(i == x && j == y))
                        {
                            TileCoordinate tile = new TileCoordinate(i, j);
                            tile.gCost = int.MaxValue;
                            neighbours.Add(tile);
                        }
                    }
                }
            }
        }

        return neighbours;
    }

    private bool checkMapBoundary(int x, int y)
    {
        //If cell exists over or under the boundaries of the map
        if (x >= mapX || y >= mapY || x < 0 || y < 0)
        {
            return false;
        }

        //If all checks pass, return true
        return true;

    }

    private List<TileCoordinate> CalcPath(TileCoordinate endTile)
    {
        List<TileCoordinate> path = new List<TileCoordinate>();
        path.Add(endTile);
        TileCoordinate currentTile = endTile;

        while (currentTile.prevTile != null)
        {
            path.Add(currentTile.prevTile);
            currentTile = currentTile.prevTile;
        }

        path.Reverse();

        return path;
    }

    private int CalcDistanceCost(TileCoordinate a, TileCoordinate b)
    {
        int xDistance = Mathf.Abs(a.xCoord - b.xCoord);
        int yDistance = Mathf.Abs(a.yCoord - b.yCoord);
        int remaining = Mathf.Abs(xDistance - yDistance);
        return Mathf.Min(xDistance, yDistance) + moveCardinalCost * remaining;
    }

    private TileCoordinate GetLowestFCostTile(List<TileCoordinate> pathTileList)
    {
        TileCoordinate lowestFCostNode = pathTileList[0];

        for (int i = 1; i < pathTileList.Count; i++)
        {
            if (pathTileList[i].fCost < lowestFCostNode.fCost)
            {
                lowestFCostNode = pathTileList[i];
            }
        }

        return lowestFCostNode;

    }
}
