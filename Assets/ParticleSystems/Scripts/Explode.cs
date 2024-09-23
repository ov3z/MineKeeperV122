using UnityEngine;

public class Explode : MonoBehaviour
{
    public float minForce;
    public float maxForce;
    public float radius;
    // Start is called before the first frame update
    void Start()
    {
        ExplosionForce();
    }

    void ExplosionForce()
    {
        foreach(Transform t in transform)
        {
            var rb = t.GetComponent<Rigidbody>();

            if (rb != null)
                rb.AddExplosionForce(Random.Range(minForce, maxForce), transform.position, radius);
        }
    }
}
