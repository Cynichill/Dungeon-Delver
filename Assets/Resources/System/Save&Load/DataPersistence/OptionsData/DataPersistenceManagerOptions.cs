using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DataPersistenceManagerOptions : MonoBehaviour
{
    [Header("File Storage Config")]
    [SerializeField] private string fileName;
    [SerializeField] private bool useEncryption;

    private OptionsData optionsData;
    private List<IDataPersistenceOptions> dataPersistenceObjects;
    public static DataPersistenceManagerOptions instance { get; private set;}
    private FileDataHandlerOptions dataHandler;
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
        this.dataHandler = new FileDataHandlerOptions(Application.persistentDataPath, fileName, useEncryption);
        this.dataPersistenceObjects = FindAllDataPersistenceObjects();
        LoadOptions();
    }

    public void NewOptions()
    {
        this.optionsData = new OptionsData();
    }

    public void LoadOptions()
    {
        //Load any saved data from a file using data handler
        this.optionsData = dataHandler.Load();

        //If no data can be loaded, initialize a new game
        if (this.optionsData == null)
        {
            Debug.Log("No data was found. Initializing to default values.");
            NewOptions();
        }

        foreach (IDataPersistenceOptions dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.LoadOptions(optionsData);
        }
    }

    public void SaveOptions()
    {
        //pass the data to other scripts so they can update it
        foreach (IDataPersistenceOptions dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.SaveOptions(optionsData);
        }

        Debug.Log("Saved!");

        //save data to file using data handler
        dataHandler.Save(optionsData);
    }

    private List<IDataPersistenceOptions> FindAllDataPersistenceObjects()
    {
        IEnumerable<IDataPersistenceOptions> dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>().OfType<IDataPersistenceOptions>();

        return new List<IDataPersistenceOptions>(dataPersistenceObjects);
    }

    private void OnApplicationQuit()
    {
       // SaveGame();
    }


}
