using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseMapGenerator : MonoBehaviour
{

    [SerializeField] private float density = 65f;
    private int mapSizeX;
    private int mapSizeY;
    public int[,] grid;

    private CellularAutomata applyCA;
    [SerializeField] private int iterations = 6;


    private void Start()
    {
        applyCA = GetComponent<CellularAutomata>();
        GenerateNoiseGrid();
        applyCA.ApplyCellularAutomata(grid, iterations, mapSizeX, mapSizeY);
    }

    private void GenerateNoiseGrid()
    {
        //Randomly select grid size
        mapSizeX = Random.Range(80, 140);
        mapSizeY = Random.Range(80, 140);

        grid = new int[mapSizeX, mapSizeY];

        System.Random randomSeed = new System.Random(Time.time.ToString().GetHashCode());

        //For each tile..
        for (int i = 0; i < mapSizeX; i++)
        {
            for (int j = 0; j < mapSizeY; j++)
            {
                //Force edges to be walls
                if (i < 5 || i > mapSizeX - 5 || j < 5 || j > mapSizeY - 5)
                {
                    grid[i, j] = 1;
                }
                else
                {
                    //If random number is bigger than noise map density..
                    if (randomSeed.Next(0, 100) > density)
                    {
                        //Tile is a floor
                        grid[i, j] = 0;
                    }
                    else
                    {
                        //Tile is a wall
                        grid[i, j] = 1;
                    }
                }
            }
        }
    }
}
