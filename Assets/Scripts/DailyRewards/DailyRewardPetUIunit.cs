using System.Collections;
using UnityEngine;

public class DailyRewardPetUIunit : PetUIUnit
{
    protected override IEnumerator Start()
    {
        petPanel.OnOpen += Initialize;
        yield return null;
        Initialize();
    }

    protected override void Initialize()
    {
        LoadUIState();
        InitializeTexts();
    }

    protected override void InitializeTexts()
    {
        capacityBonusText.text = $"{capacityBonus}";
    }

    public void OpenDailyRewardPanel()
    {

    }
}
