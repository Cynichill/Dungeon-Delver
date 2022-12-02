using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeChanger : MonoBehaviour, IDataPersistenceOptions
{
    [SerializeField] private AudioMixer audioMixer;
    private float storeSFXVol;
    private float storeMusicVol;
    private float storeMasterVol;
    [SerializeField] private int scriptIdentifier; //0 == music, 1 == sfx, 2 == master
    [SerializeField] private Slider slider;

    public void SetVolumeMusic(float vol)
    {
        audioMixer.SetFloat("MusicVolume", vol);
        slider.value = vol;
        storeMusicVol = vol;
    }

    public void SetVolumeSFX(float vol)
    {
        audioMixer.SetFloat("SFXVolume", vol);
        slider.value = vol;
        storeSFXVol = vol;
    }

    public void SetVolumeMaster(float vol)
    {
        audioMixer.SetFloat("MasterVolume", vol);
        slider.value = vol;
        storeMasterVol = vol;
    }

    public void LoadOptions(OptionsData data)
    {
        if (scriptIdentifier == 0)
        {
            SetVolumeMusic(data.MusicVolume);
        }
        else if (scriptIdentifier == 1)
        {
            SetVolumeSFX(data.SFXVolume);
        }
        else if (scriptIdentifier == 2)
        {
            SetVolumeMaster(data.MasterVolume);
        }

        storeMusicVol = data.MusicVolume;
        storeSFXVol = data.SFXVolume;
        storeMasterVol = data.MasterVolume;

        Debug.Log("Loaded!");
    }

    public void SaveOptions(OptionsData data)
    {
        if (scriptIdentifier == 0)
        {
            data.MusicVolume = storeMusicVol;
        }
        else if (scriptIdentifier == 1)
        {
            data.SFXVolume = storeSFXVol;
        }
        else if (scriptIdentifier == 2)
        {
            data.MasterVolume = storeMasterVol;
        }
    }
}
