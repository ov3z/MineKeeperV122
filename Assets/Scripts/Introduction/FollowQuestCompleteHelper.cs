using System.Collections;
using UnityEngine;

public class FollowQuestCompleteHelper : MonoBehaviour
{
    private IEnumerator Start()
    {
        yield return new WaitForSeconds(0.75f);
        QuestEvents.FireOnFollowCompletion();
    }
}
