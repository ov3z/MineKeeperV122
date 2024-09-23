using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IQuest
{
    public event Action<Reward> OnQuestComplete;

    public void FireOnQuestComplete();
}
