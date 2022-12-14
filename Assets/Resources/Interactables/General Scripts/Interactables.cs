using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactables : MonoBehaviour
{
    [SerializeField][Range(0, 4)] private int objectType; // 0 = staircase, 1 = timer

    //When player interacts with this object, do something based on object type
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "Interaction")
        {
            switch (objectType)
            {
                case 0:
                    GetComponent<Staircase>().OnInteract();
                    break;
                case 1:
                    GetComponent<TimerCollectable>().OnInteract();
                    break;
            }
        }
        else if (col.gameObject.tag == "Collector")
        {
            switch (objectType)
            {
                case 1:
                    GetComponent<TimerCollectable>().EnemyCollect();
                    break;
            }
        }
    }
}
