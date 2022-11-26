using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthUpdate : MonoBehaviour
{
    public Image[] health;
    public Sprite hpFull;
    public Sprite hpEmpty;

    public void UpdateHealthUI(int hp)
    {
        //Change UI to reflect health
        switch (hp)
        {
            case 3:
                health[0].sprite = hpFull;
                health[1].sprite = hpFull;
                health[2].sprite = hpFull;
                break;
            case 2:
                health[0].sprite = hpFull;
                health[1].sprite = hpFull;
                health[2].sprite = hpEmpty;
                break;
            case 1:
                health[0].sprite = hpFull;
                health[1].sprite = hpEmpty;
                health[2].sprite = hpEmpty;
                break;
            case 0:
                health[0].sprite = hpEmpty;
                health[1].sprite = hpEmpty;
                health[2].sprite = hpEmpty;
                break;
        }
    }
}