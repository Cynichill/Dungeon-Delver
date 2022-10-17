using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HiScoreUI : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text scoreText;
    private GameManager gm;

    private void Start()
    {
        gm = GameObject.FindGameObjectWithTag("gm").GetComponent<GameManager>();
        StartCoroutine(DelayDisplay());
    }
    
    private IEnumerator DelayDisplay()
    {
        yield return new WaitForEndOfFrame();
        scoreText.text = "Hi: " + gm.floorHighScore.ToString();
    }

}
