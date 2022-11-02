using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseMapGenerator : MonoBehaviour
{

    [SerializeField] private float density = 65f;
    private int mapSizeX;
    private int mapSizeY;
    public int[,] grid;
    public PlayerControl controls;


    private GenerateMap genMap;
    [SerializeField] private int iterations = 6;

    [SerializeField] private int minMapBoundaries = 50;
    [SerializeField] private int maxMapBoundaries = 50;

    private void Awake()
    {
        controls = new PlayerControl();
        controls.Debug.DLANM.performed += ctx => GenerateNoiseGrid(true);
    }

    private void Start()
    {
        genMap = GetComponent<GenerateMap>();
        GenerateNoiseGrid(false);
    }

    private void GenerateNoiseGrid(bool DLA)
    {
        //Randomly select grid size
        mapSizeX = Random.Range(minMapBoundaries, maxMapBoundaries);
        mapSizeY = Random.Range(minMapBoundaries, maxMapBoundaries);

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
                    if (randomSeed.Next(0, 100) > density && !DLA)
                    {
                        //Tile is a floor
                        grid[i, j] = 0;
                    }
                    else
                    {
                        //Tile is a wall
                        grid[i, j] = 1;
                    }

                    if (DLA)
                    {
                        //Tile is a wall
                        grid[i, j] = 1;
                    }
                }
            }
        }

        genMap.GenerateDungeon(grid, iterations, mapSizeX, mapSizeY);
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
