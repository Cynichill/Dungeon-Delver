using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellularAutomata : MonoBehaviour
{

    int mapSizeX = 0;
    int mapSizeY = 0;
    [SerializeField] private GameObject floorPrefab;
    [SerializeField] private GameObject wallPrefab;
    int[,] tempGrid;

    public void ApplyCellularAutomata(int[,] grid, int count, int mapSizeXParam, int mapSizeYParam)
    {
        mapSizeX = mapSizeXParam;
        mapSizeY = mapSizeYParam;

        //On first iteration, tempgrid is noise map, on further it's the next iteration
        tempGrid = grid;

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
                    else if(wallCount < 4)
                    {
                        tempGrid[j, k] = 0; //Floor
                    }
                    else if(wallCount == 4)
                    {
                        tempGrid[j,k] = grid[j, k]; //If = 4, remain same
                    }
                }
            }

            grid = tempGrid;
        }

        SpawnGrid(tempGrid);
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

    private void SpawnGrid(int[,] grid)
    {
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
}
