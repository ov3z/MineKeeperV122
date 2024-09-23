using UnityEngine;

public class ResourceRegisterHelper : MonoBehaviour
{
    [SerializeField] ResourceTypes resourceType;

    private ICollectable holder;

    private void Start()
    {
        IntroductionQuestTargetSystem.Instance.AddResource(resourceType, transform);
        holder = transform.GetComponent<ICollectable>();
        holder.OnDevastation += Discard;
    }

    private void Discard(ICollectable sender)
    {
        IntroductionQuestTargetSystem.Instance.DiscardResource(resourceType, transform);
    }
}
