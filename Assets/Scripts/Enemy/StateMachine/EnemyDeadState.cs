using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;

public class EnemyDeadState : EnemyState
{
    private Animator animator;
    private Collider trigger;
    private NavMeshAgent agent;
    private ParticleSystem deathParticle;

    private const string DEAD_ANIM_KEY = "Die";

    public EnemyDeadState(EnemyController enemyController) : base(enemyController)
    {
        animator = ownerController.Animator;
        trigger = ownerController.Trigger;
        agent = ownerController.Agent;
        deathParticle = ownerController.DeathParticle;
    }

    public override void OnStateStart()
    {
        animator.SetTrigger(DEAD_ANIM_KEY);
        trigger.enabled = false;

        if (GameManager.Instance.IsInCave)
            CaveGameManager.Instance.DiscardEnemy(ownerController);

        agent.enabled = false;
        ownerController.transform.DOMoveY(ownerController.transform.position.y - 3, 1).SetEase(Ease.Linear).SetDelay(3f);

        QuestEvents.FireOnEnemyKill(1);

        deathParticle.Play();
        SoundManager.Instance.Play(SoundTypes.EnemyDeath);
    }

    public override void OnStateEnd()
    {

    }
}
