using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CountdownTimer : MonoBehaviour
{
    public float currentTime = 0f;
    private GameManager gm;

    private bool gameEnded = false;

    private void Awake()
    {
        gm = GameObject.FindGameObjectWithTag("gm").GetComponent<GameManager>();
        currentTime = gm.saveTime;
    }

    private void Update()
    {
        currentTime -= 1 * Time.deltaTime;

        if (currentTime <= 0)
        {
            currentTime = 0;
            if (!gameEnded)
            {
                gameEnded = true;
                gm.EndGame();
            }
        }
    }

}
