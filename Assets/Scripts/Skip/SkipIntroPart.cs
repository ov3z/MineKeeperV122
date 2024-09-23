using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkipIntroPart : MonoBehaviour
{
    public static SkipIntroPart Instance;
    public bool skipIntro = false;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this);
    }

    private void Update()
    {
        if (skipIntro)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            skipIntro = true;
        }
    }
}