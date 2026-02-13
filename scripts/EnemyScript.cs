using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;

public class EnemyScript : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] public EntityStatHandler entityStats;
    [SerializeField] public float height = 1;
    [SerializeField] public int health = 25;
    [SerializeField] float damage = 10;
    [SerializeField] float attackDuration = 0.5f;
    [SerializeField] bool passive = false;
    [SerializeField] string personality = "Neutral";
    [Header("Drop Data")]
    [SerializeField] ItemData[] drops;
    [SerializeField] float[] dropChances; //should be same length as drops array
    [Header("References")]
    //public static Transform playerTransform;
    [SerializeField] Rigidbody rb;
    [SerializeField] GameObject attackPrefab;

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

    //Refferences
    [SerializeField] Material defaultMaterial;
    [SerializeField] Material hitMaterial;

    //static
    static public int enemyCount = 0;
    // Start is called before the first frame update
    void Start()
    {
        enemyCount += 1;
        lastPosition = transform.position;
        targetPosition = GetRandomPositionInTerritory();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateHealth();
        if(lastDamageTimer > 0)
        {
            lastDamageTimer -= Time.deltaTime;
        }
        if(!SeePlayer())
        {
            MovementDecision();
        } else
        {
            if(!passive)
            {
                ChasePlayer();
            } else if(personality == "Friendly")
            {
                GreetPlayer();
            }
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

    void takeDamage(float damageAmount)
    {
        if(lastDamageTimer > 0)
        {
            return;
        }
        lastDamageTimer = damageCooldown;
        entityStats.currentHealth -= damageAmount;
        if(entityStats.currentHealth <= 0)
        {
            Die();
            return;
        }
        else
        {
            StartCoroutine(FlashHitMaterial());
        }
    }
    void UpdateHealth()
    {
        if (entityStats.currentHealth <= 0)
        {
            Die();
        }
    }
    IEnumerator FlashHitMaterial()
    {
        GetComponent<Renderer>().material = hitMaterial;
        yield return new WaitForSeconds(0.2f);
        GetComponent<Renderer>().material = defaultMaterial;
    }
    void Die()  
    {
        enemyCount -= 1;
        DropItems();
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
                lookRotation = Quaternion.Euler(0, lookRotation.eulerAngles.y, 0);
                transform.rotation = lookRotation;
            }

            //move towards player
            rb.velocity = lookDirection * entityStats.currentSpeed;
        } else if(GetDistanceToPlayer() > attackDistanceMin)
        {
            //look at player
            Vector3 toPlayer = PlayerMovement.instance.position - transform.position;
            Vector3 lookDirection = toPlayer.normalized;
            if (lookDirection != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
                lookRotation = Quaternion.Euler(0, lookRotation.eulerAngles.y, 0);
                transform.rotation = lookRotation;
            }

            //move towards player
            float thisSpeed = walkSpeed;
            if (entityStats.currentSpeed < walkSpeed)
            {
                thisSpeed = entityStats.currentSpeed;
            }
            rb.velocity = lookDirection * thisSpeed;

            TryToAttackPlayer();
        } else
        {
            Vector3 toPlayer = PlayerMovement.instance.position - transform.position;
            Vector3 lookDirection = toPlayer.normalized;
            if (lookDirection != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
                lookRotation = Quaternion.Euler(0, lookRotation.eulerAngles.y, 0);
                transform.rotation = lookRotation;
            }
            rb.velocity = Vector3.zero;
            TryToAttackPlayer();
        }
    }

    void GreetPlayer()
    {
        if(GetDistanceToPlayer() > attackDistanceMin)
        {
            //look at player
            Vector3 toPlayer = PlayerMovement.instance.position - transform.position;
            Vector3 lookDirection = toPlayer.normalized;
            if (lookDirection != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
                lookRotation = Quaternion.Euler(0, lookRotation.eulerAngles.y, 0);
                transform.rotation = lookRotation;
            }

            //move towards player
            float thisSpeed = walkSpeed;
            if (entityStats.currentSpeed < walkSpeed)
            {
                thisSpeed = entityStats.currentSpeed;
            }
            rb.velocity = lookDirection * thisSpeed;

        } else
        {
            Vector3 toPlayer = PlayerMovement.instance.position - transform.position;
            Vector3 lookDirection = toPlayer.normalized;
            if (lookDirection != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
                lookRotation = Quaternion.Euler(0, lookRotation.eulerAngles.y, 0);
                transform.rotation = lookRotation;
            }
            rb.velocity = Vector3.zero;
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
                    lookRotation = Quaternion.Euler(0, lookRotation.eulerAngles.y, 0);
                    transform.rotation = lookRotation;
                }

                //move towards target
                
                float thisSpeed = walkSpeed;
                if (entityStats.currentSpeed < walkSpeed)
                {
                    thisSpeed = entityStats.currentSpeed;
                }
                rb.velocity = lookDirection * thisSpeed;
                
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
        if(lastAttackTime <= 0 && !passive)
        {
            Debug.Log("Enemy attacked the player!");
            AttackPlayer();
            lastAttackTime = attackCooldown;
        } else
        {
            lastAttackTime -= Time.deltaTime;
        }
    }

    void AttackPlayer()
    {
        //instantiate attack prefab
        Vector3 toPlayer = PlayerMovement.instance.position - transform.position;
        Vector3 lookDirection = toPlayer.normalized;
        Vector3 attackPosition = transform.position + lookDirection * (height / 2);
        Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
        GameObject attackObject = Instantiate(attackPrefab, attackPosition, lookRotation);
        AttackScript attackScript = attackObject.GetComponent<AttackScript>();
        if(attackScript != null)
        {
            attackScript.owner = this.gameObject;
            attackScript.damage = entityStats.currentDamage;
            attackScript.attackDuration = attackDuration;
            attackObject.transform.position += transform.forward * attackScript.spawnOffset;
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

    void DropItems()
    {
        for(int i = 0; i < drops.Length; i++)
        {
            float chance = dropChances[i];
            float roll = UnityEngine.Random.Range(0f, 1f);
            if(roll <= chance)
            {
                //drop the item
                GameObject itemObject = Instantiate(drops[i].prefabDefinition.prefab, transform.position, Quaternion.identity);
            }
        }
    }

    public void OnChunkUnloaded()
    {
        enemyCount -= 1;
    }

    public void OnChunkLoaded()
    {
        enemyCount += 1;
    }
}
