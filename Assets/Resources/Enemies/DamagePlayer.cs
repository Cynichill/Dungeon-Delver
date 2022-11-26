using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamagePlayer : MonoBehaviour
{
    private int damage = 1;
    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        PlayerHealth player = hitInfo.GetComponent<PlayerHealth>();
        //If hit player then damange them
        if (player != null)
        {
            player.DamagePlayer(damage);
        }
    }
}
