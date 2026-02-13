using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToxicScript : MonoBehaviour
{
    public float speed = 50f;
    private Vector3 startPosition;
    GameObject owner;
    // Start is called before the first frame update
    void Start()
    {
        startPosition = transform.position;
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.velocity = transform.forward * speed;
        owner = GetComponent<AttackScript>().owner;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<EntityStatHandler>() != null && other.gameObject != owner)
        {
            PoisonEffect poisonEffect = other.gameObject.AddComponent<PoisonEffect>();
            poisonEffect.ApplyEffect();
            Destroy(gameObject);
        }
    }
}
