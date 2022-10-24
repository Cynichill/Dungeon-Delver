using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class timerCollectable : MonoBehaviour
{

    [SerializeField] private int timeGain = 20;
    private bool interacted = false; //Prevent double pick up
    private CountdownTimer timer;
    private void Awake()
    {
        timer = GameObject.FindGameObjectWithTag("Timer").GetComponent<CountdownTimer>();
    }

    public void OnInteract()
    {
        if (!interacted)
        {
            interacted = true;

            //Add time
            timer.currentTime += timeGain;

            //Delete self
            Object.Destroy(this.gameObject);
        }

    }
}
