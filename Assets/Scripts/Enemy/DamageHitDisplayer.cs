using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageHitDisplayer : MonoBehaviour
{

    private MeshRenderer meshRenderer;
    private SkinnedMeshRenderer skinnedMeshRenderer;
    private Material[] initialMaterials;
    private Material[] outlinedMaterials;
    private Coroutine outlinedPartCoroutine;

    private void Start()
    {
        SetUpMaterials();

        var damageable = transform.GetComponentInParent<IDamageable>();
        damageable.OnHealthChange += ShowPartOutlined;

    }

    private void SetUpMaterials()
    {
        if (TryGetComponent(out meshRenderer))
        {
            initialMaterials = meshRenderer.materials;
            outlinedMaterials = new Material[initialMaterials.Length];
        }
        else if (TryGetComponent(out skinnedMeshRenderer))
        {
            initialMaterials = skinnedMeshRenderer.materials;
            outlinedMaterials = new Material[initialMaterials.Length];
        }

        for (int i = 0; i < initialMaterials.Length; i++)
        {
            outlinedMaterials[i] = new Material(initialMaterials[i]);
            ColorUtility.TryParseHtmlString("#fe5757", out var newColor);
            outlinedMaterials[i].SetColor("_BaseColor", newColor);
        }
    }

    private void ShowPartOutlined(float _)
    {
        if (outlinedPartCoroutine != null)
        {
            StopCoroutine(outlinedPartCoroutine);
        }
        outlinedPartCoroutine = StartCoroutine(ShowPartOutlinedRoutine());
    }

    private IEnumerator ShowPartOutlinedRoutine()
    {
        if (meshRenderer)
        {
            meshRenderer.materials = outlinedMaterials;
        }
        else
        {
            skinnedMeshRenderer.materials = outlinedMaterials;
        }


        yield return new WaitForSeconds(0.1f);

        if (meshRenderer)
            meshRenderer.materials = initialMaterials;
        else
            skinnedMeshRenderer.materials = initialMaterials;
    }
}
