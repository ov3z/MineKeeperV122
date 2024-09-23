using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class DeliveryGuyGoToMarketState : DeliveryGuyState
{
    private Transform marketTransform;
    private Transform delivererTransform;
    private NavMeshAgent agent;
    private Animator animator;

    private string SPEED_KEY = "Speed";
    private float speed;

    public DeliveryGuyGoToMarketState(DeliveryGuy controller) : base(controller)
    {
        marketTransform = controller.MarketTransform;
        delivererTransform = controller.transform;
        agent = controller.Agent;
        speed = controller.Speed;
        animator = controller.Animator;
    }

    public override void OnStateStart()
    {
        controller.OnDelivererTriggerEnter += OnTriggerEnter;

        agent.enabled = true;
        agent.SetDestination(marketTransform.position);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<TradeStationInteractor>(out var interactable))
        {
            controller.SetInteractionTarget(interactable);
            controller.SwitchState(DeliveryGuyStates.Unload);
        }
    }

    public override void OnStateUpdate()
    {
        /*var direction = (marketTransform.position - delivererTransform.position).normalized;

        delivererTransform.position += direction * speed * Time.deltaTime;

        var angleY = Mathf.Atan2(direction.x, direction.z);
        var delivererEuler = delivererTransform.eulerAngles;
        delivererEuler.y = angleY;
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
