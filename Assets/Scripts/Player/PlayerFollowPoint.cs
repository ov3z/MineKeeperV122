using UnityEngine;

public class PlayerFollowPoint : MonoBehaviour
{
    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public void Release()
    {
        PlayerController.Instance.ReturnFollowPoint(this);
    }
}
