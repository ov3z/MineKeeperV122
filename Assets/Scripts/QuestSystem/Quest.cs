using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Quest : ScriptableObject, IQuest
{
    public string description;
    [SerializeField] protected Reward _reward;
    [SerializeField] protected float _objectiveAmount;
    [SerializeField] protected bool OpenRewardPanel;
    [SerializeField] protected Sprite icon;
    protected float _currentAmount;

    public event Action<Reward> OnQuestComplete;
    public event Action<float> OnProgressUpdate;
    public event Action OnQuestPanelOpen;
    public event Action OnQuestPanelClose;

    public bool IsCompleted => _currentAmount >= _objectiveAmount;
    public Sprite Icon => icon;
    public string Description => description;
    public string ProgressText => $"{Mathf.Clamp(_currentAmount, 0, _objectiveAmount)}/{_objectiveAmount}";
    public float ProgressNormalized => Mathf.Clamp01(_currentAmount / _objectiveAmount);
    public Reward Reward => _reward;

    public virtual bool Initialize()
    {
        _currentAmount = PlayerPrefs.GetFloat(description, 0);

        if (IsCompleted)
        {
            FireOnQuestComplete();
        }

        return !IsCompleted;
    }

    public void FireOnQuestComplete()
    {
        OnQuestComplete?.Invoke(_reward);
        //WideData();
        /*YsoCorp.GameUtils.YCManager.instance.OnGameFinished(true); // yso corp finish level
        Debug.Log($"Quest have finished { QuestController.Instance.CurrentQuestIndex}");*/
    }

    public void FireOnProgressUpdate(float progress)
    {
        OnProgressUpdate?.Invoke(progress);
    }

    protected void FireQuestPanelOpen()
    {
        OnQuestPanelOpen?.Invoke();
       /*int questIndex =  QuestController.Instance.CurrentQuestIndex;
       Debug.Log($"Quest have started: {questIndex}");
       YsoCorp.GameUtils.YCManager.instance.OnGameStarted(questIndex); // for yso sdk*/
    }

    protected void FireQuestPanelClose()
    {
        OnQuestPanelClose?.Invoke();
    }

    public virtual Transform GetQuestTarget() => null;
    public virtual Transform GetQuestCanvasTarget() => null;

    protected void Save()
    {
        PlayerPrefs.SetFloat(description, _currentAmount);
    }

    public void WipeData()
    {
        PlayerPrefs.SetFloat(description, 0);
    }

    public virtual void Dispose() { }
}