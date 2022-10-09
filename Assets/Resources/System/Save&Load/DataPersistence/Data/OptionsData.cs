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
    public bool fullScreen; //If player selected fullscreen
    public int screenWidth;
    public int screenHeight;

    public OptionsData()
    {
        //Default values
        SFXVolume = 0;
        MusicVolume = 0;
        fullScreen = false;
        screenHeight = 800;
        screenWidth = 1280;
    }
}
