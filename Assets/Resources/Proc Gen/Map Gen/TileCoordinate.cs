using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileCoordinate : IHeapItem<TileCoordinate>
{
    public int xCoord;
    public int yCoord;
    public int gCost = int.MaxValue;
    public int hCost;
    public int fCost;
    public bool floor = false;
    int heapIndex;

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

    public void ResetCosts()
    {
        gCost = int.MaxValue;
        hCost = 0;
        fCost = 0;
        prevTile = null;
    }

    public int HeapIndex
    {
        get
        {
            return heapIndex;
        }
        set
        {
            heapIndex = value;
        }
    }

    public int CompareTo(TileCoordinate compareNode)
    {
        int compare = fCost.CompareTo(compareNode.fCost);
        if(compare == 0)
        {
            compare = hCost.CompareTo(compareNode.hCost);
        }

        return -compare;
    }
}
