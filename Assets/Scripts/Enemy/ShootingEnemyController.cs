using UnityEngine;

public class ShootingEnemyController : EnemyController
{
    [SerializeField] private Transform mouth;
    [SerializeField] private ProjectileTypes projectileType;

    public Transform Mouth => mouth;
    public ProjectileTypes ProjectileType => projectileType;

    protected override void InitializeStatesMap()
    {
        enemyStatesMap.Add(EnemyStates.Idle, new EnemyIdleState(this));
        enemyStatesMap.Add(EnemyStates.ReturnHome, new EnemyReturnHomeState(this));
        enemyStatesMap.Add(EnemyStates.Chase, new EnemyChasingState(this));
        enemyStatesMap.Add(EnemyStates.Attack, new EnemyProjectileAttack(this));
        enemyStatesMap.Add(EnemyStates.Dead, new EnemyDeadState(this));
        enemyStatesMap.Add(EnemyStates.Tackled, new EnemyTackledState(this));
    }
}
