using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceBallScript : MonoBehaviour
{
    public float maxDistance = 25f;
    public float speed = 50f;
    private Vector3 startPosition;
    // Start is called before the first frame update
    void Start()
    {
        startPosition = transform.position;
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.velocity = transform.forward * speed;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<EntityStatHandler>() != null)
        {
            FreezeEffect freezeEffect = other.gameObject.AddComponent<FreezeEffect>();
            freezeEffect.ApplyEffect();
            Destroy(gameObject);
        }
    }
}
