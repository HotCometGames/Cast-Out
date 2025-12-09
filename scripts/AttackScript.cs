using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackScript : MonoBehaviour
{
    public GameObject owner;

    public int damage = 0;
    public float attackDuration = 0.5f;
    private float attackTimer = 0f;

    public float spawnOffset = 0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        attackTimer += Time.deltaTime;
        if (attackTimer >= attackDuration)
        {
            Destroy(gameObject);
        }
    }
}
