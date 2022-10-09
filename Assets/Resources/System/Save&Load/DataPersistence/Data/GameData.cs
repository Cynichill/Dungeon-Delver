using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    //Initial values for new games, and if no data is found

    public int playerFloorHighScore;

    public GameData()
    {
        playerFloorHighScore = 0;
    }

}
