using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Stats", menuName = "Stats/EnemyStats")]
public class EnemyStats : ScriptableObject
{
    public float speed;
    public float damage;
    public float health;
    public float chaseRange;
    public float attackRange;
}
