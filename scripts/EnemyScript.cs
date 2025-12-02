using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] public float height = 1;
    [SerializeField] public int health = 25;
    [SerializeField] int attackType = 0;
    [Header("References")]
    //public static Transform playerTransform;
    [SerializeField] Rigidbody rb;
    [SerializeField] GameObject[] attackPrefabs;

    //path finding
    public Vector3 lastPosition;
    [SerializeField] public float territoryRadius = 20f;
    private Vector3 targetPosition = new Vector3(0, 1, 0);

    //decision making
    [SerializeField] float[] decisionDelayRange = new float[2] {0.5f, 5f};
    private float decisionDelay = 1f;
    private float decisionTimer = 0f;
    private bool isDeciding = false;

    //visibility and attack ranges
    [SerializeField] float viewDistance = 10f;
    [SerializeField] float targetViewDistance = 15f;
    [SerializeField] float attackDistanceMax = 2f;
    [SerializeField] float attackDistanceMin = .5f;
    private bool isChasingPlayer = false;
    [SerializeField] float attackCooldown = 2f;
    private float lastAttackTime = 0f;
    public float damageCooldown = .5f;
    private float lastDamageTimer = 0f;

    //movement speeds
    [SerializeField] float walkSpeed = 5f;
    [SerializeField] float runSpeed = 12f;
    [SerializeField] float rotationTime = .2f;
    // Start is called before the first frame update
    void Start()
    {
        lastPosition = transform.position;
        targetPosition = GetRandomPositionInTerritory();
    }

    // Update is called once per frame
    void Update()
    {
        if(lastDamageTimer > 0)
        {
            lastDamageTimer -= Time.deltaTime;
        }
        if(!SeePlayer())
        {
            MovementDecision();
        } else
        {
            ChasePlayer();
        }
        GetAboveGroundPosition();
    }

    private void OnTriggerStay(Collider other)
    {
        String tag = other.tag;
        switch (tag)
        {
            case "Lava":
                Debug.Log("Taking lava damage");
                takeDamage(10);
                break;
            case "AttackHitBox":
                AttackScript attackScript = other.GetComponent<AttackScript>();
                if (attackScript != null)
                {
                    if (attackScript.owner == this.gameObject)
                    {
                        break;
                    }
                    Debug.Log("Taking attack damage");
                    takeDamage(attackScript.damage);
                }
                break;
            default:
                break;
        }  
    }

    void takeDamage(int damageAmount)
    {
        if(lastDamageTimer > 0)
        {
            return;
        }
        lastDamageTimer = damageCooldown;
        health -= damageAmount;
        if(health <= 0)
        {
            Die();
        }
    }
    void Die()  
    {
        Destroy(gameObject);
    }

    bool SeePlayer()
    {
        if(Vector3.Distance(transform.position, PlayerMovement.instance.position) < viewDistance && !isChasingPlayer || Vector3.Distance(transform.position, PlayerMovement.instance.position) < targetViewDistance && isChasingPlayer)
        {
            isChasingPlayer = true;
            return true;
        }
        isChasingPlayer = false;
        return false;
    }
    void ChasePlayer()
    {
        if(GetDistanceToPlayer() > attackDistanceMax)
        {
            //look at player
            Vector3 toPlayer = PlayerMovement.instance.position - transform.position;
            Vector3 lookDirection = toPlayer.normalized;
            if (lookDirection != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = lookRotation;
            }

            //move towards player
            rb.velocity = lookDirection * runSpeed;
        } else if(GetDistanceToPlayer() > attackDistanceMin)
        {
            //look at player
            Vector3 toPlayer = PlayerMovement.instance.position - transform.position;
            Vector3 lookDirection = toPlayer.normalized;
            if (lookDirection != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = lookRotation;
            }

            //move towards player
            rb.velocity = lookDirection * walkSpeed;

            TryToAttackPlayer();
        } else
        {
            rb.velocity = Vector3.zero;
            TryToAttackPlayer();
        }
    }

    void MovementDecision()
    {
        if(!isDeciding)
        {
            if(GetDistanceToTarget() < 1f)
            {
                decisionDelay = UnityEngine.Random.Range(decisionDelayRange[0], decisionDelayRange[1]);
                decisionTimer = 0f;
                lastPosition = transform.position;
                isDeciding = true;
            } else
            {
                //look at target
                Vector3 toTarget = targetPosition - transform.position;
                Vector3 lookDirection = toTarget.normalized;
                if (lookDirection != Vector3.zero)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
                    transform.rotation = lookRotation;
                }

                //move towards target
                
                rb.velocity = lookDirection * walkSpeed;
                
            }
        } else
        {
            decisionTimer += Time.deltaTime;
            if(decisionTimer >= decisionDelay)
            {
                targetPosition = GetRandomPositionInTerritory();
                decisionTimer = 0f;
                isDeciding = false;
            }
        }
    }

    void TryToAttackPlayer()
    {
        //attack logic here
        if(lastAttackTime <= 0)
        {
            Debug.Log("Enemy attacked the player!");
            AttackPlayer(attackType);
            lastAttackTime = attackCooldown;
        } else
        {
            lastAttackTime -= Time.deltaTime;
        }
    }

    void AttackPlayer(int attackOption)
    {
        switch(attackOption)
        {
            case 0:
                Debug.Log("Enemy Bites");
                GameObject bit = Instantiate(attackPrefabs[0], transform.position + transform.forward * 1f, Quaternion.Euler(transform.rotation.eulerAngles.x ,transform.rotation.eulerAngles.y - 90,transform.rotation.eulerAngles.z));
                bit.GetComponent<AttackScript>().owner = this.gameObject;
                break;
            default:
                Debug.Log("Enemy Bites");
                break;
        }
    }

    void GetAboveGroundPosition()
    {
        if(transform.position.y < WorldGeneration2.GetHeight(transform.position.x, transform.position.z))
            transform.position = new Vector3(transform.position.x, WorldGeneration2.GetHeight(transform.position.x, transform.position.z) + height/2, transform.position.z);
    }

    float GetDistanceToTarget()
    {
        return Vector3.Distance(transform.position, targetPosition);
    }

    float GetDistanceToPlayer()
    {
        return Vector3.Distance(transform.position, PlayerMovement.instance.position);
    }
    Vector3 GetRandomPositionInTerritory()
    {
        Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * territoryRadius;
        Vector3 randomPosition = new Vector3(lastPosition.x + randomCircle.x, WorldGeneration2.GetHeight(lastPosition.x + randomCircle.x, lastPosition.z + randomCircle.y), lastPosition.z + randomCircle.y);
        return randomPosition;
    }
}
