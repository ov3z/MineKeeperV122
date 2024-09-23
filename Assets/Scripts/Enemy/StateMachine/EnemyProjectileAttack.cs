using UnityEngine;

public class EnemyProjectileAttack : EnemyAttackingState
{
    private ProjectileTypes usedProjectile;

    private Transform mouth;

    public EnemyProjectileAttack(ShootingEnemyController enemyController) : base(enemyController)
    {
        mouth = enemyController.Mouth;
        usedProjectile = enemyController.ProjectileType;
    }

    protected override void DealDamage()
    {
        Projectile projectile = PoolingSystem.Instance.GetProjectilePool(usedProjectile).GetObject() as Projectile;
        SetUpProjectile(projectile);
    }

    private void SetUpProjectile(Projectile projectile)
    {
        projectile.SetDamage(damage);

        Vector3 dirToTarget = (target.transform.position - ownerController.transform.position).normalized;
        projectile.SetDirection(dirToTarget);

        projectile.transform.position = mouth.TransformPoint(Vector3.zero);
        projectile.gameObject.SetActive(true);
    }
}
