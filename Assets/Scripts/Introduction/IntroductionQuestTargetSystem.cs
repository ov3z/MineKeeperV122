using System.Collections.Generic;
using UnityEngine;

public class IntroductionQuestTargetSystem : QuestTargetSystem
{
    private Transform rabbitTransform;
    private List<Transform> enemiesList = new();

    public void AddFollowTarget(Transform followTarget)
    {
        rabbitTransform = followTarget;
    }

    public Transform GetFollowRabbitTarget()
    {
        return rabbitTransform;
    }

    public void AddEnemy(Transform target)
    {
        enemiesList.Add(target);
    }

    public Transform GetClosestEnemy(Vector3 requestPositon)
    {
        float minDistance = float.MaxValue;
        Transform closestEnemy = null;
        foreach (var enemy in enemiesList)
        {
            float distance = Vector3.Distance(requestPositon, enemy.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestEnemy = enemy;
            }
        }
        return closestEnemy;
    }
}
