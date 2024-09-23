using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScissorsAnimator : MonoBehaviour
{
    [SerializeField] Transform rightBlade;
    [SerializeField] Transform leftBlade;
    [SerializeField] Transform rightHand;
    [SerializeField] Transform leftHand;

    private void Update()
    {
        rightBlade.LookAt(rightHand, Vector3.up);
        leftBlade.LookAt(leftHand, Vector3.up);
    }
}
