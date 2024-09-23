using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoarAnimationController : MonoBehaviour
{
    private Animator boarAnimator;

    private string MOVE_KEY = "Move";
    private string IDLE_KEY = "Idle";

    private void Awake()
    {
        boarAnimator = GetComponent<Animator>();
    }

    private void Start()
    {
        PlayerController.Instance.OnStateChange += OnPlayerStateChange;
    }

    private void OnPlayerStateChange(States newState)
    {
        if(newState == States.Move)
        {
            boarAnimator.SetBool(MOVE_KEY,true);
            boarAnimator.SetBool(IDLE_KEY, false);
        }
        else
        {
            boarAnimator.SetBool(IDLE_KEY,true);
            boarAnimator.SetBool(MOVE_KEY, false);
        }
    }

    private void OnDestroy()
    {
        if(PlayerController.Instance)
        {
            PlayerController.Instance.OnStateChange -= OnPlayerStateChange;
        }
    }
}
