using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;
using UnityEngine.AI;

public class DeliveryGuyGoToStackState : DeliveryGuyState
{
    private Transform stackTransform;
    private Transform delivererTransform;

    private float speed;
    private NavMeshAgent agent;
    private Animator animator;
    private string SPEED_KEY = "Speed";

    public DeliveryGuyGoToStackState(DeliveryGuy controller) : base(controller)
    {
        stackTransform = controller.StackTransform;
        delivererTransform = controller.transform;
        agent = controller.Agent;
        animator = controller.Animator;
        speed = controller.Speed;
    }


    public override void OnStateStart()
    {
        controller.OnDelivererTriggerEnter += OnTriggerEnter;

        Debug.Log("start");

        agent.enabled = true;
        agent.SetDestination(stackTransform.position);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<ResourceStack>(out var interactable))
        {
            Debug.Log("statte satrte");

            controller.SetInteractionTarget(interactable);
            controller.SwitchState(DeliveryGuyStates.Collect);
        }
    }

    public override void OnStateUpdate()
    {
        /*    var direction = (stackTransform.position - delivererTransform.position).normalized;

            delivererTransform.position += direction * speed * Time.deltaTime;

            var angleY = Mathf.Atan2(direction.x, direction.z);
            var delivererEuler = delivererTransform.eulerAngles;
            delivererEuler.y = angleY;

            delivererTransform.eulerAngles = delivererEuler;
        */

        var instantSpeed = agent.velocity.magnitude;
        var velocityDirection = agent.velocity.normalized;

        animator.SetFloat(SPEED_KEY, instantSpeed / speed);

        delivererTransform.forward = Vector3.Lerp(delivererTransform.forward, velocityDirection, 15 * Time.deltaTime);
    }

    public override void OnStateEnd()
    {
        controller.OnDelivererTriggerEnter -= OnTriggerEnter;

        animator.SetFloat(SPEED_KEY, 0);
        agent.SetDestination(delivererTransform.position);
        agent.enabled = false;
    }
}
