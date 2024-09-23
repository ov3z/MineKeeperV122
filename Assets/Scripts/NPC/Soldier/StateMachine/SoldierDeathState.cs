using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoldierDeathState : SoldierState
{
    private Animator animator;
    private ParticleSystem deathParticle;

    private const string DIE_ANIM_KEY = "Idle";

    public SoldierDeathState(SoldierController soldierController) : base(soldierController)
    {
        animator = ownerController.Animator;
        deathParticle = ownerController.DeathParticle;

        canChaseEnemies = false;
        canGoToMine = false;
    }

    public override void OnStateStart()
    {
        animator.SetTrigger(DIE_ANIM_KEY);
        CaveGameManager.Instance.RemoveSoldier(ownerController);
        deathParticle.gameObject.SetActive(true);
        deathParticle.transform.parent = null;
        ownerController.gameObject.SetActive(false);
    }

    public override void OnStateEnd()
    {

    }
}
