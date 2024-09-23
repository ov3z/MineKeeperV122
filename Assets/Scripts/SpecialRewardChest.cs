using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialRewardChest : PayoutValidation
{
    [SerializeField] private List<SkinSelectionUnit> rewardSkins = new();
    [SerializeField] private UpgradePanel panel;

    [SerializeField] private ParticleSystem disappearParticle;

    public override void OnPayout()
    {
        foreach (var skin in rewardSkins)
        {
            skin.Unlock();
        }

        disappearParticle.Play();

        StartCoroutine(EnablePanel());
    }

    private IEnumerator EnablePanel()
    {
        yield return new WaitForSeconds(4.5f);
        panel.OpenSettingsPanel();
    }
}
