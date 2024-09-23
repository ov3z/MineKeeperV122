using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Plant Config", menuName = "Configs/PlatConfig")]
public class PlantConfig : ScriptableObject
{
    public ResourceTypes type;
    public string GATHER_ANIM_KEY;
    public float timeForResetting;
    public float visibilityAngle;
    public float powerForPiece;
    public int piecesCountMax;
}
