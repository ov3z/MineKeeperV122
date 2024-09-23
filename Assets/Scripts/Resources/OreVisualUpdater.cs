using System.Collections.Generic;
using UnityEngine;

public class OreVisualUpdater : MonoBehaviour
{
    private ResourceTypes updaterType;

    public OreVisualUpdater(ResourceTypes updaterType)
    {
        this.updaterType = updaterType;
    }

    public void UpdateVisauls(ref int collectedBerriesCount, ref int berriesCountMax, ref List<Transform> pieces, Vector3 collectionPosition)
    {
        float closestDistance = float.MaxValue;
        Transform closestPiece = pieces[0];

        foreach(var piece in pieces)
        {
            if(piece.gameObject.activeSelf)
            {
                float distance = Vector3.Distance(piece.transform.position, collectionPosition);

                if(distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPiece = piece;
                }
            }
        }

        closestPiece.gameObject.SetActive(false);

        Transform piecesRoot = pieces[0].parent.parent;
        TryPlayEffect(piecesRoot);

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
}