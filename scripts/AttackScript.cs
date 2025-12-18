using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackScript : MonoBehaviour
{
    public GameObject owner;

    public int damage = 0;
    public float attackDuration = 0.5f;
    public float rotationOffset = 0f;
    private float attackTimer = 0f;
    

    public float spawnOffset = 0f;
    // Start is called before the first frame update
    void Start()
    {
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0, rotationOffset, 0));
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
