using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

[System.Serializable]
public class OptionsData
{
    //Initial values for options, and if no data is found
    public float SFXVolume; //Volume player set
    public float MusicVolume; //Volume player set
    public float MasterVolume; //Volume player set
    public int screenMode; //Screenmode last used
    public int screenSize; //Screensize last used

    public OptionsData()
    {
        //Default values
        SFXVolume = 0;
        MusicVolume = 0;
        MasterVolume = 0;
        screenMode = 0;
        screenSize = 3;
    }
}
