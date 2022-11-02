using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChooseCam : MonoBehaviour
{
    public GenerateMap genMap;

    [SerializeField] private GameObject cam1;
    [SerializeField] private GameObject cam2;
    // Start is called before the first frame update
    void Start()
    {
        genMap = GameObject.FindGameObjectWithTag("Generation").GetComponent<GenerateMap>();
    
        if(genMap.debugControl)
        {
            cam2.SetActive(true);
            cam1.SetActive(false);
        }
        else
        {
            cam1.SetActive(true);
            cam2.SetActive(false);
        }
    }

}
