using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PathFind : MonoBehaviour
{
    private int mapX;
    private int mapY;

    private const int moveCardinalCost = 10;

    public void GetGridSize(int mapSizeX, int mapSizeY)
    {
        mapX = mapSizeX;
        mapY = mapSizeY;
    }

    public List<TileCoordinate> TilePath(TileCoordinate[,] receivedGrid, TileCoordinate startTile, TileCoordinate endTile)
    {
        HeapOptimization<TileCoordinate> openList = new HeapOptimization<TileCoordinate>(mapX * mapY);
        openList.Add(startTile);
        List<TileCoordinate> closedList = new List<TileCoordinate>();

        for (int i = 0; i < mapX; i++)
        {
            for (int j = 0; j < mapY; j++)
            {
                receivedGrid[i, j].ResetCosts(); //Reset costs and previous tile so multiple iterations don't get messed up by stored data
            }
        }

        startTile.gCost = 0;
        startTile.hCost = CalcDistanceCost(startTile, endTile);
        startTile.CalcFCost();

        while (openList.Count > 0)
        {
            TileCoordinate currentTile = openList.RemoveFirst();
            if (currentTile == endTile)
            {
                endTile = currentTile;
                //Reached the final tile
                openList.HeapClear();
                return CalcPath(endTile);
            }

            // openList.Remove(currentTile);
            closedList.Add(currentTile);

            foreach (TileCoordinate neighbour in GetListOfNeighbours(receivedGrid, currentTile))
            {
                //If tile has already been in openList previously, do not check
                if (closedList.Contains(neighbour))
                {
                    continue;
                }
                else
                {
                    //If potential cost is lower than current cost, add neighbour to open list
                    int potentialGCost = currentTile.gCost + CalcDistanceCost(currentTile, neighbour);

                    if (potentialGCost < neighbour.gCost)
                    {
                        neighbour.prevTile = currentTile;
                        neighbour.gCost = potentialGCost;
                        neighbour.hCost = CalcDistanceCost(neighbour, endTile);
                        neighbour.CalcFCost();

                        openList.Add(neighbour);
                    }
                }
            }
        }

        //No more open tiles
        return null;
    }

    private List<TileCoordinate> GetListOfNeighbours(TileCoordinate[,] receivedGrid, TileCoordinate currentTile)
    {
        List<TileCoordinate> neighbours = new List<TileCoordinate>();

        int x = currentTile.xCoord;
        int y = currentTile.yCoord;

        //Find cardinal neighbours only

        for (int i = x - 1; i <= x + 1; i++)
        {
            for (int j = y - 1; j <= y + 1; j++)
            {
                if (checkMapBoundary(i, j) && receivedGrid[i, j].floor == true)
                {
                    //Only treat cardinal directions as neighbours
                    if (i == x || j == y)
                    {
                        if (!(i == x && j == y))
                        {
                            neighbours.Add(receivedGrid[i, j]); //Add neighbour to list
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

        path.Reverse(); //Flip path so it's in the correct order

        return path;
    }

    private int CalcDistanceCost(TileCoordinate a, TileCoordinate b)
    {
        int xDistance = Mathf.Abs(a.xCoord - b.xCoord);
        int yDistance = Mathf.Abs(a.yCoord - b.yCoord);
        int remaining = Mathf.Abs(xDistance - yDistance);
        return Mathf.Min(xDistance, yDistance) + moveCardinalCost * remaining;
    }

}
