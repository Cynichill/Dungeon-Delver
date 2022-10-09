using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

public class DataPersistenceManagerTempGlobal : MonoBehaviour
{
    [Header("File Storage Config")]
    [SerializeField] private string fileName;
    [SerializeField] private bool useEncryption;

    private TempGlobalData tempGlobalData;
    private List<IDataPersistenceTempGlobal> dataPersistenceObjects;
    public static DataPersistenceManagerTempGlobal instance { get; private set;}
    private FileDataHandlerTempGlobal dataHandler;
    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("More than one Data Persistence Manager found in scene!");
        }

        instance = this;
    }

    private void Start()
    {
        this.dataHandler = new FileDataHandlerTempGlobal(Application.persistentDataPath, fileName, useEncryption);
        this.dataPersistenceObjects = FindAllDataPersistenceObjects();
        LoadTempGlobal();
    }

    public void NewTempGlobal()
    {
        this.tempGlobalData = new TempGlobalData();
    }

    public void LoadTempGlobal()
    {
        //Load any saved data from a file using data handler
        this.tempGlobalData = dataHandler.Load();

        //If no data can be loaded, initialize a new game
        if (this.tempGlobalData == null)
        {
            Debug.Log("No data was found. Initializing to default values.");
            NewTempGlobal();
        }

        foreach (IDataPersistenceTempGlobal dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.LoadTempGlobal(tempGlobalData);
        }
    }

    public void SaveTempGlobal()
    {
        //pass the data to other scripts so they can update it
        foreach (IDataPersistenceTempGlobal dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.SaveTempGlobal(tempGlobalData);
        }

        Debug.Log("Saved!");

        //save data to file using data handler
        dataHandler.Save(tempGlobalData);
    }

    private List<IDataPersistenceTempGlobal> FindAllDataPersistenceObjects()
    {
        IEnumerable<IDataPersistenceTempGlobal> dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>().OfType<IDataPersistenceTempGlobal>();

        return new List<IDataPersistenceTempGlobal>(dataPersistenceObjects);
    }

    private void OnApplicationQuit()
    {
       File.Delete(Application.persistentDataPath + "/tempData.cyn");
    }


}
