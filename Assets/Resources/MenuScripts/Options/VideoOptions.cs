using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class VideoOptions : MonoBehaviour, IDataPersistenceOptions
{

    //Resolutions
    List<int> widths = new List<int>() { 568, 960, 1280, 1920 };
    List<int> heights = new List<int>() { 320, 540, 800, 1080 };
    [SerializeField] TMP_Dropdown dropDown;
    [SerializeField] VideoOptions otherOptions;
    [SerializeField] private int scriptIdentifier;
    private int currentScreenMode = 0;
    private int currentScreenSize = 0;
    private int width = 1280;
    private int height = 800;

    public void SetScreenSize(int index)
    {
        //Changes window height and length
        width = widths[index];
        height = heights[index];
        Screen.SetResolution(width, height, false);

        //Updates other script with values
        otherOptions.UpdateSize(width, height);

        currentScreenSize = index;
    }

    public void SetScreenMode(int index)
    {

        currentScreenMode = index;
        //0 = Windowed, 1 = Borderless Window, 2 = Fullscreen
        switch (index)
        {
            case 0:
                Screen.SetResolution(width, height, false);
                dropDown.interactable = true;
                break;

            case 1:
                Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);
                dropDown.interactable = false;
                break;

            case 2:
                Screen.SetResolution(1920, 1080, FullScreenMode.ExclusiveFullScreen);
                dropDown.interactable = false;
                break;
        }
    }

    public void UpdateSize(int w, int h)
    {
        width = w;
        height = h;
    }

    public void LoadOptions(OptionsData data)
    {
        if (scriptIdentifier == 0)
        {
            if (data.screenMode == 0)
            {
                SetScreenSize(data.screenSize);
            }
        }
        else if (scriptIdentifier == 1)
        {
            SetScreenMode(data.screenMode);
        }

        Debug.Log("Loaded!");
    }

    public void SaveOptions(OptionsData data)
    {
        if (scriptIdentifier == 0)
        {
            data.screenSize = currentScreenSize;
        }
        else if (scriptIdentifier == 1)
        {
            data.screenMode = currentScreenMode;
        }
    }

}
