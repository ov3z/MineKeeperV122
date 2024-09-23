using UnityEngine;

public class EnemyRegisterHelper : MonoBehaviour
{
    private void Start()
    {
        (IntroductionQuestTargetSystem.Instance as IntroductionQuestTargetSystem).AddEnemy(transform);
    }
}
