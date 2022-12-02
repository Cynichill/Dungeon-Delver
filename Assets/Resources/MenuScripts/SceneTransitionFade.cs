using UnityEngine;

public class SceneTransitionFade : MonoBehaviour
{

    private Animator animator; //Objects animator
    private string currentState; //Object state

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    
    public void ChangeAnimationState(string newState)
    {
        if (currentState == newState) return;

        //play the animation
        animator.Play(newState);

        //reassign the current state
        currentState = newState;
    }

}
