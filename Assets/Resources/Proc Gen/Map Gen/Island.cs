using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Island
{
    public List<TileCoordinate> tiles; //List of all tiles in the Island
    public List<TileCoordinate> edgeTiles; //List of all edge tiles in the Island
    public int IslandSize;

    public Island()
    {
    }

    public Island(List<TileCoordinate> IslandTiles, TileCoordinate[,] grid) //Take in a list of tiles and the map
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
                        if (grid[x, y].floor == false) //If wall is found
                        {
                            edgeTiles.Add(tile); //Add to edge tile list
                        }
                    }
                }
            }
        }
    }
}
