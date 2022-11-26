using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    private HealthUpdate updateUI;
    private GameManager gm;

    void Start()
    {
        gm = GameObject.FindGameObjectWithTag("gm").GetComponent<GameManager>();
        updateUI = GameObject.FindGameObjectWithTag("hpui").GetComponent<HealthUpdate>();
        updateUI.UpdateHealthUI(gm.hp);
    }

    public void DamagePlayer(int damage)
    {
        gm.ChangeHealth(damage);
        updateUI.UpdateHealthUI(gm.hp);
    }

}