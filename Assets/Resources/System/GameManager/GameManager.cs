using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour, IDataPersistence
{
    public int floorCount = 0;
    private int floorHighScore = 0;
    private static GameObject instance;
    public float saveTime = 30f;
    private DataPersistenceManager dataManager;

    //Don't destroy this object on load, makes sure duplicates do not appear by deleting them if the object already exists elsewhere
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        if (instance == null)
            instance = gameObject;
        else
            Destroy(gameObject);

            dataManager = GameObject.FindGameObjectWithTag("DataManager").GetComponent<DataPersistenceManager>();
    }
    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void EndGame()
    {
        Debug.Log("Game over YEAH!");

        //Save game, return to title
        dataManager.SaveGame();

        //SceneManager.LoadScene(0);
    }

    public void LoadData(GameData data)
    {
        floorHighScore = data.playerFloorHighScore;
    }

    public void SaveData(GameData data)
    {
        if(floorCount > data.playerFloorHighScore)
        {
            data.playerFloorHighScore = floorCount;
        }
    }

}
