using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class DeleteOptions : MonoBehaviour
{
    private DataPersistenceManagerOptions dataManager;

    private void Awake()
    {
        dataManager = GameObject.FindGameObjectWithTag("OptionsManager").GetComponent<DataPersistenceManagerOptions>();
    }

    public void DeleteOptionsFile()
    {
        File.Delete(Application.persistentDataPath + "/Options.txt");
        dataManager.LoadOptions();
    }
}
