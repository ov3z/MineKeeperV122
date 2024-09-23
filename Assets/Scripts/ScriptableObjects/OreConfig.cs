using UnityEngine;

[CreateAssetMenu(fileName = "New Ore Config", menuName = "Configs/OreConfig")]
public class OreConfig : ScriptableObject
{
    public ResourceTypes type;
    public string GATHER_ANIM_KEY;
    public float visibilityAngle;
    public float powerForPiece;
    public int piecesCountMax;
}