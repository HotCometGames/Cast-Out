using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DarkBallScript : MonoBehaviour
{
    [SerializeField]
    GameObject darkAttackPrefab;
    public float speed = 50f;
    // Start is called before the first frame update
    void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.velocity = transform.forward * speed;
        SoundManager.PlaySound("Swoosh", transform.position, 1f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            GameObject attack = Instantiate(darkAttackPrefab, transform.position, Quaternion.identity);
            attack.GetComponent<AttackScript>().owner = GetComponent<AttackScript>().owner;
            Destroy(gameObject);
        }
    }
}
