using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileCoordinate
{
    public int xCoord;
    public int yCoord;
    public int gCost;
    public int hCost;
    public int fCost;

    public TileCoordinate prevTile;

    public TileCoordinate(int x, int y)
    {
        xCoord = x;
        yCoord = y;
    }

    //Empty initializer
    public TileCoordinate()
    {

    }

    public void CalcFCost()
    {
        fCost = gCost + hCost;
    }
}
