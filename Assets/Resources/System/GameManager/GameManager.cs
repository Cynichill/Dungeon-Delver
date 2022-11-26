using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour, IDataPersistence
{
    public int floorCount = 0;
    public int floorHighScore = 0;
    private static GameObject instance;
    public float saveTime = 30f;
    private DataPersistenceManager dataManager;
    public bool debugOn = false;
    public int hp = 3;

    //Don't destroy this object on load, makes sure duplicates do not appear by deleting them if the object already exists elsewhere
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        if (instance == null)
            instance = gameObject;
        else
            Destroy(gameObject);

            dataManager = GameObject.FindGameObjectWithTag("DataManager").GetComponent<DataPersistenceManager>();
    }
    public void RestartScene(bool enableDebug)
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        debugOn = enableDebug;
    }

    public void EndGame()
    {
        Debug.Log("Game over YEAH!");

        //Save game, return to title
        dataManager.SaveGame();

        //SceneManager.LoadScene(0);
    }

    public void ChangeHealth(int changeBy)
    {
        hp -= changeBy;
        if(hp <= 0)
        { 
            EndGame();
        }
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
