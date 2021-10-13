using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayedExplosiveScript : MonoBehaviour
{
    [SerializeField] private LayerMask attackMask;
    [SerializeField] private float forceMult = 2f;

    private Vector3 offset;
    private GameObject follow;

    void Start()
    {
        StartCoroutine("Explode");
    }

    private void Update()
    {
        if (follow)
        {
            transform.position = follow.transform.position + offset;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetFollow(GameObject target, Vector3 offsetIn)
    {
        follow = target;
        offset = offsetIn;
    }

    private IEnumerator Explode()
    {
        yield return new WaitForSeconds(3f);

        Collider[] hitObjects = Physics.OverlapSphere(transform.position, 1f, attackMask);
        print("THINGS HIT: " + hitObjects.Length);
        foreach (Collider hitCol in hitObjects)
        {
            if (hitCol.GetComponent<EnemyBehaviour>())
            {
                print("KNOCKBACK");
                Vector3 aim = (transform.position - hitCol.transform.position).normalized;
                Vector3 direction = new Vector3(aim.x, 0.75f, aim.y);
                hitCol.GetComponent<EnemyBehaviour>().Knockback(direction * (forceMult + (AbilityManager.SoleManager.GetSpecialAbilityLevel(0) * 0.2f)));
            }
        }

        Destroy(gameObject);
    }
}
