using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveOptions : MonoBehaviour
{
    private DataPersistenceManagerOptions dataManager;
    // Start is called before the first frame update
    void Awake()
    {
        dataManager = GameObject.FindGameObjectWithTag("OptionsManager").GetComponent<DataPersistenceManagerOptions>();
    }

    public void SaveOptionsToFile()
    {
        dataManager.SaveOptions();
    }

}
