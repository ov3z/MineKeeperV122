using System.Collections.Generic;
using UnityEngine;

public class PlantVisualUpdater
{
    private ResourceTypes updaterType;

    public PlantVisualUpdater(ResourceTypes updaterType)
    {
        this.updaterType = updaterType;
    }

    public void UpdateVisauls(ref int collectedBerriesCount, ref int berriesCountMax, ref List<Transform> pieces)
    {
        switch (updaterType)
        {
            case ResourceTypes.Blueberry:
                UpdateVisualsForFruitHolder(ref collectedBerriesCount, ref berriesCountMax, ref pieces);
                break;
            case ResourceTypes.Wood:
                UpdateVisualsForTree(ref collectedBerriesCount, ref berriesCountMax, ref pieces);
                break;
            case ResourceTypes.Wheat:
                UpdateVisualsForWheat(ref collectedBerriesCount, ref berriesCountMax, ref pieces);
                break;
        }
    }


    private void UpdateVisualsForFruitHolder(ref int collectedBerriesCount, ref int berriesCountMax, ref List<Transform> pieces)
    {
        int collectedFruitsCount = Mathf.Clamp(Mathf.CeilToInt(pieces.Count * collectedBerriesCount / berriesCountMax), 0, pieces.Count);
        for (int i = 0; i < collectedFruitsCount; i++)
        {
            if (pieces[i].gameObject.activeSelf)
                pieces[i].gameObject.SetActive(false);
        }
        for (int i = collectedFruitsCount; i < pieces.Count; i++)
        {
            if (!pieces[i].gameObject.activeSelf)
                pieces[i].gameObject.SetActive(true);
        }

        PlayLeafParticle(pieces);
    }

    private void UpdateVisualsForTree(ref int collectedBerriesCount, ref int berriesCountMax, ref List<Transform> pieces)
    {
        int collectedFruitsCount = Mathf.Clamp(Mathf.CeilToInt((pieces.Count - 1) * collectedBerriesCount / berriesCountMax), 0, (pieces.Count - 1));

        foreach (var piece in pieces)
            piece.gameObject.SetActive(false);

        pieces[collectedFruitsCount].gameObject.SetActive(true);

        PlayLeafParticle(pieces);
    }

    private void UpdateVisualsForWheat(ref int collectedBerriesCount, ref int berriesCountMax, ref List<Transform> pieces)
    {
        int collectedFruitsCount = Mathf.Clamp(Mathf.CeilToInt((pieces.Count - 1) * collectedBerriesCount / berriesCountMax), 0, (pieces.Count - 1));
        for (int i = 0; i < collectedFruitsCount; i++)
        {
            if (pieces[i].gameObject.activeSelf)
                pieces[i].gameObject.SetActive(false);
        }
        for (int i = collectedFruitsCount; i < pieces.Count - 1; i++)
        {
            if (!pieces[i].gameObject.activeSelf)
                pieces[i].gameObject.SetActive(true);
        }
    }

    private void PlayLeafParticle(List<Transform> pieces)
    {
        for (int i = 0; i < pieces.Count; i++)
        {
            if (pieces[i].ActiveSelf() && pieces[i].HasChildren())
            {
                TryPlayEffect(pieces[i]);
                return;
            }
        }

        TryPlayEffect(pieces[0].parent.parent);
    }

    private bool TryPlayEffect(Transform targetParent)
    {
        bool isThereeffect = targetParent.GetLastChild().TryGetComponent<ParticleSystem>(out var particleSystem);
        if (isThereeffect)
        {
            particleSystem.Play();
        }
        return isThereeffect;
    }

    public void ResetVisuals(ref List<Transform> pieces)
    {
        switch (updaterType)
        {
            case ResourceTypes.Blueberry:
            case ResourceTypes.Wheat:
                ResetFruitHolderVisuals(ref pieces);
                break;
            case ResourceTypes.Wood:
                ResetTreeVisauls(ref pieces);
                break;
        }
    }

    private void ResetFruitHolderVisuals(ref List<Transform> pieces)
    {
        foreach (var piece in pieces)
        {
            piece.gameObject.SetActive(true);
        }
    }

    private void ResetTreeVisauls(ref List<Transform> pieces)
    {
        foreach (var piece in pieces)
        {
            piece.gameObject.SetActive(false);
        }

        pieces[0].gameObject.SetActive(true);
    }
}
