using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialText : MonoBehaviour
{
    [SerializeField] private bool shouldSelfTerminate;
    private bool isFirstLoad;

    private void Awake()
    {
        isFirstLoad = PlayerPrefs.GetInt("IsFromInitialLoadScene", 1) == 1 ? true : false;
    }

    private void Start()
    {
        if (!isFirstLoad && shouldSelfTerminate)
        {
            gameObject.SetActive(false);
        }
        else
        {
            MouseEventListener.OnGetMouseButtonUp += DisableOnPlayerMove;
            PlayerController.Instance.OnStateChange += DisableOnPlayerMove;
        }
    }

    private void DisableOnPlayerMove(States newState)
    {
        if (newState == States.Move)
        {
            PlayerController.Instance.OnStateChange -= DisableOnPlayerMove;
            MouseEventListener.OnGetMouseButtonUp -= DisableOnPlayerMove;
            gameObject.SetActive(false);
        }
    }

    private void DisableOnPlayerMove()
    {
        PlayerController.Instance.OnStateChange -= DisableOnPlayerMove;
        MouseEventListener.OnGetMouseButtonUp -= DisableOnPlayerMove;
        gameObject.SetActive(false);
    }
}
