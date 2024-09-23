using System.Collections;
using UnityEngine;

public class DeathState : StateBase
{
    private ParticleSystem deathParticle;
    private Vector3 intialParticlePos;

    public DeathState(PlayerController playerController) : base(playerController)
    {
        deathParticle = ownerController.DeathParticle;
        intialParticlePos = deathParticle.transform.localPosition;
    }

    public override void OnStateStart()
    {
        CoroutineRunner.Instance.StartCoroutine(MakePlayerDisappearAsync());
    }

    private IEnumerator MakePlayerDisappearAsync()
    {
        deathParticle.gameObject.SetActive(true);
        deathParticle.Play();
        deathParticle.transform.SetParent(null);

        yield return null;

        ownerController.gameObject.SetActive(false);

        CaveLevelUI.Instance.EnableCountdown();

        /*        yield return new WaitForSeconds(0.7f);

                CaveGameManager.Instance.RespawnPlayer();

                ownerController.SetState(States.Idle);*/
    }

    public override void OnStateEnd()
    {
        ownerController.RestoreHealth();
        deathParticle.transform.SetParent(ownerController.transform);
        deathParticle.transform.localPosition = intialParticlePos;
        deathParticle.gameObject.SetActive(false);
    }
}
