using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

[System.Serializable]
public class ItemData
{
    public string name;
    public Sprite sprite;
    public ChunkObjectDefinition prefabDefinition;
    public int manaCost;
}
public class PlayerLogicScript : MonoBehaviour
{
    [Header("Inventory Settings")]
    public ItemData[] inventory = new ItemData[7];
    public ItemData[] wands = new ItemData[2];
    public int selectedItem = 0;
    public GameObject[] slots;
    public GameObject[] inventorySlots;
    public GraphicRaycaster raycaster;
    public EventSystem eventSystem;
    public int slotTradeSelected = -1;
    public bool inMenu = false;
    [Header("Player Stats")]
    public int health = 100;
    public int maxHealth = 100;
    public int mana = 50;
    public int maxMana = 50;
    public float maxSpellDistance = 10f;
    public int gold = 0;
    public float damageCooldown = .5f;
    private float lastDamageTimer = 0f;

    [Header("References")]
    public Transform player;
    public PlayerMovement playerMovement;
    public Camera playerCamera;
    public Transform cameraTransform;
    public TextMeshProUGUI captions;
    public GameObject inventoryUI;
    public GameObject tradeMenuUI;
    public GameObject YukiObject;
    public TradeMenuScript tradeMenuScript;
    public Slider manaBar;
    public Slider healthBar;

    [Header("Spells")]
    public GameObject punchPrefab;
    public GameObject plantPrefab;
    public GameObject fireballPrefab;

    [Header("Creatures")]
    public float creatureSpawnMaxDistance = 100f;
    public float creatureSpawnMinDistance = 50f;
    public float creatureSpawnInterval = 10f;
    private float creatureSpawnTimer = 0f;
    public int maxCreaturesToSpawnAtOnce = 3;
    public int maxCreatures = 25;

    //Crafting
    public bool inCraftingRune = false;

    string currentLookAt;
    string currentLookAtTag;
    string currentMenu = "";
    GameObject currentLookAtObject;

    // Start is called before the first frame update
    void Start()
    {
        UpdateHotbar();
        manaBar.maxValue = maxMana;
        manaBar.value = mana;
        healthBar.maxValue = maxHealth;
        healthBar.value = health;
    }

    // Update is called once per frame
    void Update()
    {
        WhatAmILookingAt();
        UpdateCreatureSpawning();
        if (currentLookAtTag == "Yuki")
        {
            float a = currentLookAtObject.transform.position.x - player.position.x;
            float b = currentLookAtObject.transform.position.z - player.position.z;
            float c = Mathf.Sqrt(a * a + b * b);
            float d = currentLookAtObject.transform.position.y - player.position.y;
            float distance = Mathf.Sqrt(c * c + d * d);
            if(distance > 15f) { return; }
            Transform yukiTransform = currentLookAtObject.transform;
            GameObject newYuki = Instantiate(YukiObject, yukiTransform.position, yukiTransform.rotation);
            currentLookAtObject = newYuki;
            Destroy(currentLookAtObject);
        }
        //CheckForLavaAbove();
        if (!inMenu)
        {
            // Handle player input for inventory selection
            if (Input.GetKeyDown(KeyCode.Alpha1)) { selectedItem = 0; UpdateHotbar(); }
            if (Input.GetKeyDown(KeyCode.Alpha2)) { selectedItem = 1; UpdateHotbar(); }
            if (Input.GetKeyDown(KeyCode.Alpha3)) { selectedItem = 2; UpdateHotbar(); }
            if (Input.GetKeyDown(KeyCode.Alpha4)) { selectedItem = 3; UpdateHotbar(); }
            if (Input.GetKeyDown(KeyCode.Alpha5)) { selectedItem = 4; UpdateHotbar(); }
            if (Input.GetKeyDown(KeyCode.Alpha6)) { selectedItem = 5; UpdateHotbar(); }
            if (Input.GetKeyDown(KeyCode.Alpha7)) { selectedItem = 6; UpdateHotbar(); }

            // Handle item usage
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                UseItem(selectedItem, KeyCode.Mouse0);
            }
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                UseItem(selectedItem, KeyCode.Mouse1);
            }
            if (Input.GetKeyDown(KeyCode.Q))
            {
                DropItem(selectedItem);
            }
        }
        if (Input.GetKeyDown(KeyCode.E))
            {
                if (currentMenu == "Inventory" || currentMenu == "")
                {
                    inventoryUI.SetActive(!inventoryUI.activeSelf);
                    Cursor.lockState = inventoryUI.activeSelf ? CursorLockMode.None : CursorLockMode.Locked;
                    playerMovement.inputEnabled = !inventoryUI.activeSelf;
                    inMenu = inventoryUI.activeSelf;
                    slotTradeSelected = -1;
                    currentMenu = inventoryUI.activeSelf ? "Inventory" : "";
                    UpdateHotbar();
                }
                else if (currentMenu == "Trade")
                {
                    tradeMenuUI.SetActive(false);
                    Cursor.lockState = CursorLockMode.Locked;
                    playerMovement.inputEnabled = true;
                    inMenu = false;
                    slotTradeSelected = -1;
                    currentMenu = "";
                    UpdateHotbar();
                }
            }


        if (Input.GetMouseButtonDown(0) && inventoryUI.activeSelf) // left click
        {
            PointerEventData pointerData = new PointerEventData(eventSystem)
            {
                position = Input.mousePosition
            };

            // Raycast UI
            List<RaycastResult> results = new List<RaycastResult>();
            raycaster.Raycast(pointerData, results);

            if (results.Count > 0)
            {
                GameObject clickedObject = results[0].gameObject;
                Debug.Log("Clicked on: " + clickedObject.name);

                string slotName = clickedObject.name; // Assuming the slot GameObjects are named "Item0", "Item1", etc.
                if (slotName.StartsWith("Item"))
                {
                    int slotIndex;
                    if (int.TryParse(slotName.Substring(4), out slotIndex))
                    {
                        if (slotTradeSelected == -1)
                        {
                            slotTradeSelected = slotIndex;
                        }
                        else
                        {
                            SwitchItems(slotTradeSelected, slotIndex);
                            slotTradeSelected = -1;
                            UpdateHotbar();
                        }
                    }
                }
            }
        }
    }

    void SwitchItems(int slot1, int slot2)
    {
        if (slot1 < 0 || slot1 >= inventory.Length || slot2 < 0 || slot2 >= inventory.Length)
        {
            Debug.LogWarning("Invalid inventory indices for switching items.");
            return;
        }

        ItemData temp = inventory[slot1];
        inventory[slot1] = inventory[slot2];
        inventory[slot2] = temp;
        UpdateHotbar();
    }

    void UseItem(int item, KeyCode key)
    {
        if (key == KeyCode.Mouse1)
        {
            switch (currentLookAtTag)
            {
                case "Orc":
                    // Handled above
                    // Open trade menu
                    Debug.Log("Opening trade menu...");
                    currentMenu = "Trade";
                    Cursor.lockState = CursorLockMode.None;
                    playerMovement.inputEnabled = false;
                    tradeMenuScript.ClearTradeButtons();
                    List<Trade> trades = currentLookAtObject.GetComponent<OrcScript>().trades;
                    tradeMenuScript.trades = trades;
                    tradeMenuScript.CreateTradeButtons();
                    tradeMenuUI.SetActive(true);
                    ItemHoldingUIScript.PlayUseAnimation();
                    return;
                case "Item":
                    // Pick up item
                    ItemData itemPickup = currentLookAtObject.GetComponent<ItemPickup>()?.itemData;
                    if (itemPickup != null)
                    {
                        // Find the first empty slot in the inventory
                        for (int i = 0; i < inventory.Length; i++)
                        {
                            if (inventory[i] == null || inventory[i].name == null || inventory[i].name == "")
                            {
                                inventory[i] = itemPickup;
                                Debug.Log($"Picked up item: {itemPickup.name}");
                                UpdateHotbar();
                                Destroy(currentLookAtObject);
                                break;
                            }
                        }
                    }
                    ItemHoldingUIScript.PlayUseAnimation();
                    return;
                default:
                    break;
            }
            switch(inventory[item].name)
            {
                case "Mana Berry":
                Debug.Log("Consuming Mana Berry...");
                mana += 20;
                if (mana > maxMana) { mana = maxMana; }
                manaBar.value = mana;
                inventory[item] = null;
                UpdateHotbar();
                break;
                default:
                    Debug.Log("Right click has no effect with this item.");
                    return;
            }
            return;
            
        }
        if (inventory[item].manaCost > mana)
            {
                Debug.Log("Not enough mana to use this item.");
                return;
            }
        

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;
        Vector3 hitPoint = new Vector3();

        if (Physics.Raycast(ray, out hit, maxSpellDistance))
        {
            // This is the position in world space where your sight hits
            hitPoint = hit.point;

            Debug.Log("Looking at point: " + hitPoint);
        }
        else
        {
            Debug.Log("Looking at nothing");
            Ray missRay = new Ray(playerCamera.transform.position + (playerCamera.transform.forward * maxSpellDistance), -playerCamera.transform.up);
            RaycastHit missHit;
            if (Physics.Raycast(missRay, out missHit, maxSpellDistance))
            {
                hitPoint = missHit.point;
                Debug.Log("Looking at point: " + hitPoint);
            }
        }


        Debug.Log($"Using item: {item}");
        // Implement item usage logic here
        switch(inventory[item].name)
        {
            case "Fire Wand":
                // Cast a fire spell
                Debug.Log("Casting fire spell...");
                GameObject fireball = Instantiate(fireballPrefab, playerCamera.transform.position + playerCamera.transform.forward, cameraTransform.rotation);
                fireball.GetComponent<FireBallScript>().speed = 50f;
                fireball.GetComponent<AttackScript>().owner = this.gameObject;
                break;
            
            case "Nature Wand":
                // Cast a nature spell
                Debug.Log("Casting nature spell...");
                if (hitPoint == new Vector3()) { return; }
                Instantiate(plantPrefab, hitPoint, Quaternion.identity);
                break;
            case "Stick":
                if(currentLookAtTag == "Well")
                {
                    string orbToLookFor;
                    ItemData wandToLookFor = null;
                    switch (currentLookAt)
                    {
                        case "NatureWell(Clone)":
                            orbToLookFor = "Nature Crystal Orb";
                            wandToLookFor = wands[0];
                            break;
                        case "DarkWell(Clone)":
                            orbToLookFor = "Dark CrystalOrb";
                            wandToLookFor = wands[1];
                            break;
                        case "FireWell(Clone)":
                            orbToLookFor = "Fire Crystal Orb";
                            wandToLookFor = wands[2];
                            break;
                        default:
                            orbToLookFor = "Light Crystal Orb";
                            break;
                    }
                    for (int i = 0; i < inventory.Length; i++)
                    {
                        if (inventory[i] != null && inventory[i].name != "")
                        {
                            if (inventory[i].name == orbToLookFor)
                            {
                                inventory[i] = null;
                                Debug.Log("Enchanting Stick...");
                                inventory[item] = wandToLookFor;
                                UpdateHotbar();
                                break;
                            }
                        }
                    }
                    
                } else
                {
                    GameObject punch1 = Instantiate(punchPrefab, player.position + cameraTransform.forward * 2f, cameraTransform.rotation);
                    punch1.GetComponent<AttackScript>().owner = this.gameObject;
                    Debug.Log("Punching...");
                }
                break;
            case "Rock":
                if(currentLookAtTag == "Crystal")
                {
                    Debug.Log("Mining Crystal...");
                    ItemData itemPickup = currentLookAtObject.GetComponent<ItemPickup>()?.itemData;
                    // Find the first empty slot in the inventory
                    for (int j = 0; j < inventory.Length; j++)
                    {
                        if (inventory[j] == null || inventory[j].name == null || inventory[j].name == "")
                        {
                            inventory[j] = itemPickup;
                            Debug.Log($"Mined up item: {itemPickup.name}");
                            Destroy(currentLookAtObject);
                            UpdateHotbar();
                            break;
                        }
                    }
                    UpdateHotbar();
                } else
                {
                    GameObject punch1 = Instantiate(punchPrefab, player.position + cameraTransform.forward * 2f, cameraTransform.rotation);
                    punch1.GetComponent<AttackScript>().owner = this.gameObject;
                    Debug.Log("Punching...");
                }
                break;
            case "Mana Berry":
                Debug.Log("Consuming Mana Berry...");
                mana += 20;
                if (mana > maxMana) { mana = maxMana; }
                manaBar.value = mana;
                inventory[item] = null;
                UpdateHotbar();
                break;
            default:
                GameObject punch = Instantiate(punchPrefab, player.position + cameraTransform.forward * 2f, cameraTransform.rotation);
                punch.GetComponent<AttackScript>().owner = this.gameObject;
                Debug.Log("Punching...");
                break;
        }
        mana -= inventory[item].manaCost;
        manaBar.value = mana;
        ItemHoldingUIScript.PlayUseAnimation();
    }

    public void UpdateHotbar()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            Image slotImage = slots[i].GetComponent<UnityEngine.UI.Image>();

            if (inventory[i] == null || string.IsNullOrEmpty(inventory[i].name))
            {
                // Make transparent
                Color c = slotImage.color;
                c.a = 0f;
                slotImage.color = c;
                if(i == selectedItem)
                {
                    ItemHoldingUIScript.ClearSprite();
                }
                continue;
            }

            Debug.Log($"Updating slot {i} with item {inventory[i]}");

            if (inventory[i].sprite != null)
            {
                slotImage.sprite = inventory[i].sprite;
            }
            else
            {
                Debug.LogWarning($"No sprite found for item {inventory[i]}");
            }

            // Ensure visible
            Color visible = slotImage.color;
            visible.a = 1f;
            slotImage.color = visible;

            // Highlight selected item
            if (i == selectedItem)
            {
                slots[i].GetComponent<RectTransform>().localScale = Vector3.one * 1.5f;
                ItemHoldingUIScript.SetSprite(inventory[i].sprite);
            }
            else
            {
                slots[i].GetComponent<RectTransform>().localScale = Vector3.one;
            }
        }

        for (int i = 0; i < inventorySlots.Length; i++)
        {
            int j = i + slots.Length;
            Image slotImage = inventorySlots[i].GetComponent<UnityEngine.UI.Image>();

            if (inventory[j] == null || string.IsNullOrEmpty(inventory[j].name))
            {
                // Make transparent
                Color c = slotImage.color;
                c.a = 0f;
                slotImage.color = c;
                continue;
            }

            Debug.Log($"Updating inventory slot {i} with item {inventory[j]}");

            if (inventory[j].sprite != null)
            {
                slotImage.sprite = inventory[j].sprite;
            }
            else
            {
                Debug.LogWarning($"No sprite found for item {inventory[j]}");
            }

            // Ensure visible
            Color visible = slotImage.color;
            visible.a = 1f;
            slotImage.color = visible;
        }
    }


    void DropItem(int item)
    {
        if (inventory[item] == null || inventory[item].name == null || inventory[item].name == "")
        {
            Debug.Log("No item in this slot to drop.");
            return;
        }

        // Instantiate the item's prefab in front of the player
        Vector3 dropPosition = player.position + player.forward * 2f;
        if (inventory[item].prefabDefinition.prefab != null)
        {
            Instantiate(inventory[item].prefabDefinition.prefab, dropPosition, Quaternion.identity);
            Debug.Log($"Dropped item: {inventory[item].name}");
            // Remove the item from the inventory
            inventory[item] = new ItemData();
            UpdateHotbar();
        }
        else
        {
            Debug.LogWarning($"No prefab found for item {inventory[item].name}, cannot drop.");
        }
    }

    void WhatAmILookingAt()
    {
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, maxSpellDistance))
        {
            switch (hit.collider.tag)
            {
                case "Crystal":
                    AttemptToSetText("Left Click with a Rock Item to Mine it");
                    currentLookAt = hit.collider.gameObject.GetComponent<ItemPickup>().itemData.name;
                    currentLookAtTag = "Crystal";
                    currentLookAtObject = hit.collider.gameObject;
                    break;
                case "Well":
                    AttemptToSetText("Left Click with a Stick Item to Enchant it");
                    currentLookAt = hit.collider.gameObject.name;
                    currentLookAtTag = "Well";
                    currentLookAtObject = hit.collider.gameObject;
                    break;
                case "Item":
                    AttemptToSetText("Right Click to pick up " + hit.collider.gameObject.GetComponent<ItemPickup>().itemData.name);
                    currentLookAt = hit.collider.gameObject.GetComponent<ItemPickup>().itemData.name;
                    currentLookAtTag = "Item";
                    currentLookAtObject = hit.collider.gameObject;
                    break;
                case "Orc":
                    AttemptToSetText("An Orc. Right Click to Trade.");
                    currentLookAt = "Orc";
                    currentLookAtTag = "Orc";
                    currentLookAtObject = hit.collider.gameObject;
                    break;
                case "Yuki":
                    currentLookAt = "Yuki";
                    currentLookAtTag = "Yuki";
                    currentLookAtObject = hit.collider.gameObject;
                    break;
                default:
                    break;
            }
        }
        else
        {
            AttemptToSetText("");
            currentLookAt = "";
            currentLookAtTag = "";
            currentLookAtObject = null;
        }
    }
    void AttemptToSetText(string text)
    {
        if(inCraftingRune)
        {
            captions.text = "Left Click to Craft Item";
        } else
        {
            captions.text = text;
        }
    }

    void SpawnCreature(float maxDistance, float minDistance)
    {
        if(NearCross())
        {
            return;
        }
        float theta = UnityEngine.Random.Range(0f, 2f * Mathf.PI);
        float radius = UnityEngine.Random.Range(minDistance, maxDistance);
        Vector3 spawnPosition = new Vector3(player.position.x + radius * Mathf.Cos(theta), WorldGeneration2.GetHeight(player.position.x + radius * Mathf.Cos(theta), player.position.z + radius * Mathf.Sin(theta)), player.position.z + radius * Mathf.Sin(theta));
        if(!WorldGeneration2.PosHasLava(spawnPosition.x, spawnPosition.z))
        {
            MobData mobToSpawn = WorldGeneration2.GetMobDataAtPosition(spawnPosition.x, spawnPosition.z);
            if(mobToSpawn == null)
            {
                return;
            }
            spawnPosition += new Vector3(0f, mobToSpawn.mobPrefab.GetComponent<EnemyScript>().height/2, 0f);
            GameObject enemyObject = Instantiate(mobToSpawn.mobPrefab, spawnPosition, Quaternion.identity);
        }
    }
    void UpdateCreatureSpawning()
    {
        creatureSpawnTimer += Time.deltaTime;
        if(creatureSpawnTimer >= creatureSpawnInterval)
        {
            creatureSpawnTimer = 0f;
            int creaturesToSpawn = UnityEngine.Random.Range(1, maxCreaturesToSpawnAtOnce + 1);
            for(int i = 0; i < creaturesToSpawn; i++)
            {
                if(EnemyScript.enemyCount >= maxCreatures)
                {
                    return;
                }
                SpawnCreature(creatureSpawnMaxDistance, creatureSpawnMinDistance);
            }
        }
    }

    bool NearCross()
    {
        return false;
    }

    void OnCollisionStay(Collision other)
    {
        if (other.gameObject.CompareTag("Lava"))
        {
            Debug.Log("Taking lava damage");
            takeDamage(1);
        }
    }
    private void OnTriggerStay(Collider other)
    {
        String tag = other.tag;
        switch (tag)
        {
            case "Crafting":
                inCraftingRune = true;
                AttemptToSetText("Left Click to Craft Item");
                break;
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
    private void OnTriggerExit(Collider other)
    {
        String tag = other.tag;
        switch (tag)
        {
            case "Crafting":
                inCraftingRune = false;
                AttemptToSetText("");
                break;
            default:
                break;
        }  
    }

    void CheckForLavaAbove()
    {
        RaycastHit hit;
        Vector3 origin = transform.position;
        Vector3 direction = Vector3.up;

        if (Physics.Raycast(origin, direction, out hit, 10))
        {
            // Check tag
            if (hit.collider.CompareTag("Lava"))
            {
                takeDamage(1);
                Debug.Log("Taking lava damage");
            }
        }
    }

    public void takeDamage(int damage)
    {
        if (Time.time - lastDamageTimer < damageCooldown)
        {
            return;
        }
        lastDamageTimer = Time.time;
        health -= damage;
        if (health < 0) { health = 0; }
        healthBar.value = health;
        if (health == 0)
        {
            // Handle player death
            Debug.Log("Player has died.");
        }
    }
}
