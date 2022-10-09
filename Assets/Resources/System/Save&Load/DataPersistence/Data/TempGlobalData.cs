using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

[System.Serializable]
public class TempGlobalData
{
    //Initial values for options, and if no data is found
    public int saveID;

    public TempGlobalData()
    {
        //Default values
        saveID = 0;
    }
}
