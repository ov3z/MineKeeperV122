using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*using AnimatorController = UnityEditor.Animations.AnimathorController;*/

public class EquipmentAnimationHandler : MonoBehaviour
{
    [SerializeField] Animator scissorsAnimator;

    [SerializeField] GameObject animatedScissors;
    [SerializeField] GameObject scriptedScissors;

    private const string GATHER_ANIM_KEY = "GatherBerry";

    #region Scissors
    public void IdleToGather()
    {
        StartCoroutine(IdleToGetherCoroutine());
    }

    private IEnumerator IdleToGetherCoroutine()
    {
        yield return null;
        scissorsAnimator.SetBool("Idle", true);
        scissorsAnimator.SetBool(GATHER_ANIM_KEY, true);
    }

    public void GatherToIdle()
    {
        scissorsAnimator.SetBool(GATHER_ANIM_KEY, false);
    }

    public void EnableScriptedAnimation()
    {
        animatedScissors.SetActive(false);
        scriptedScissors.SetActive(true);
    }

    public void DisabbleScriptedAnimation()
    {
        scriptedScissors.SetActive(false);
        animatedScissors.SetActive(true);
    }

    public void DisableScriptedSiccors()
    {
        scriptedScissors.SetActive(false);
    }
    #endregion



}
