using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneHandler : MonoBehaviour
{
    public float ChangeTime = 1f;

    public void Start()
    {
        SceneChange(true, false, 0);
    }

    public void SceneChange(bool inOut, bool sceneChange, int scene)
    {
        //true == fade out, false == fade in

        SceneTransitionFade fade = GameObject.FindGameObjectWithTag("SceneTransition").GetComponent<SceneTransitionFade>();

        if (inOut)
        {
            fade.ChangeAnimationState("CrossFadeOut");
        }
        else
        {
            fade.ChangeAnimationState("CrossFadeIn");
        }

        if (sceneChange)
        {
            StartCoroutine(ChangeSceneTime(scene));
        }
    }

    private IEnumerator ChangeSceneTime(int scene)
    {
        yield return new WaitForSeconds(ChangeTime);
        SceneManager.LoadScene(scene);
    }
}
