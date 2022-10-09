using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Staircase : MonoBehaviour
{
    [SerializeField] private float addTime = 10f;

    private CountdownTimer timer;
    private GameManager gm;

    private bool interacted = false;

    private void Awake()
    {
        timer = GameObject.FindGameObjectWithTag("Timer").GetComponent<CountdownTimer>();
        gm = GameObject.FindGameObjectWithTag("gm").GetComponent<GameManager>();
    }
    public void OnInteract()
    {
        if (!interacted)
        {
            interacted = true; //Prevent double interactions with stairs to skip multiple floors

            //Increment floor count
            gm.floorCount += 1;

            //Increase time by timeIncreasePerFloor
            timer.currentTime += addTime;

            //Save current time to gm
            gm.saveTime = timer.currentTime;

            //Save player info

            //Reload scene and generate new floor
            gm.RestartScene();
        }

    }
}
