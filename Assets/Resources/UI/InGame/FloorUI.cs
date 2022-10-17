using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FloorUI : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text floorText;
    private GameManager gm;

    private void Start()
    {
        gm = GameObject.FindGameObjectWithTag("gm").GetComponent<GameManager>();

        floorText.text = "Floor: " + gm.floorCount.ToString();
    }

}
