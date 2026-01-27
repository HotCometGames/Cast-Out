using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LasoScript : MonoBehaviour
{
    public float maxDistance = 25f;
    public float speed = 50f;
    Rigidbody rb;
    [SerializeField] AttackScript attackScript;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = transform.forward * speed;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject != attackScript.owner && other.gameObject.GetComponent<EntityStatHandler>() != null)
        {
            StuckEffect stuckEffect = other.gameObject.AddComponent<StuckEffect>();
            stuckEffect.ApplyEffect();
            Destroy(gameObject);
        }
    }
}
