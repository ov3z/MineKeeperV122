using System.Collections.Generic;
using UnityEngine;
using static PlayerController;

public class PlayerSkinSetup : MonoBehaviour
{

    [SerializeField] List<PlayerToolUnit> toolUnits;
    [SerializeField] AnimationEventHandler animationEventHandler;
    [SerializeField] Renderer playerRenderer;
    [SerializeField] Animator animator;
    [SerializeField] Transform fireSword;
    [SerializeField] Transform ordinaryTrail;

    public List<PlayerToolUnit> ToolUnits => toolUnits;
    public AnimationEventHandler AnimationEventHandler => animationEventHandler;
    public Renderer PlayerRenderer => playerRenderer;
    public Transform VisualTransform => transform;
    public Animator Animator => animator;
    public Transform FireSword => fireSword;
    public Transform OrdinaryTrail => ordinaryTrail;
}
