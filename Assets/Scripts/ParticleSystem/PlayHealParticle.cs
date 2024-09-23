using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayHealParticle : MonoBehaviour
{
    private ParticleSystem particle;

    private void Start()
    {
        particle= GetComponent<ParticleSystem>();

        var player = GetComponentInParent<PlayerController>();
        player.OnHealthChange += OnHealthChange;
    }

    private void OnHealthChange(float normalizedValue)
    {
        if(normalizedValue == 1)
        {
            particle.Play();
        }
    }
}
