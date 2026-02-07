using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TradeButtonScript : MonoBehaviour
{
    public Trade trade;
    public GameObject[] itemCostSlots;
    public GameObject[] itemGainSlots;
    // Start is called before the first frame update
    void Start()
    {
        UpdateTradeSlots();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void UpdateTradeSlots()
    {
        for (int i = 0; i < itemCostSlots.Length; i++)
        {
            if (i < trade.itemCosts.Length)
            {
                itemCostSlots[i].SetActive(true);
                itemCostSlots[i].GetComponent<UnityEngine.UI.Image>().sprite = trade.itemCosts[i].sprite;
            }
            else
            {
                itemCostSlots[i].SetActive(false);
            }
        }

        for (int i = 0; i < itemGainSlots.Length; i++)
        {
            if (i < trade.itemGains.Length)
            {
                itemGainSlots[i].SetActive(true);
                itemGainSlots[i].GetComponent<UnityEngine.UI.Image>().sprite = trade.itemGains[i].sprite;
            }
            else
            {
                itemGainSlots[i].SetActive(false);
            }
        }
    }
    public void OnMouseDown()
    {
        Debug.Log("Trade button clicked!");
        // Implement trade logic here
        PlayerLogicScript player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerLogicScript>();
        ItemData[] inventory = player.inventory.Clone() as ItemData[];
        for (int i = 0; i < trade.itemCosts.Length; i++)
        {
            bool found = false;
            for (int j = 0; j < inventory.Length; j++)
            {
                if (inventory[j] != null && inventory[j].name == trade.itemCosts[i].name)
                {
                    inventory[j] = null; // Remove the item from inventory
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                Debug.Log("Trade failed: missing item " + trade.itemCosts[i].name);
                return; // Exit if any item cost is not found
            }
        }
        for (int i = 0; i < trade.itemGains.Length; i++)
        {
            bool added = false;
            for (int j = 0; j < inventory.Length; j++)
            {
                if (inventory[j] == null)
                {
                    inventory[j] = trade.itemGains[i]; // Add the item to inventory
                    added = true;
                    break;
                }
            }
            if (!added)
            {
                Debug.Log("Trade failed: inventory full");
                return; // Exit if inventory is full
            }
        }
        player.inventory = inventory;
        player.UpdateHotbar();
        Debug.Log("Trade successful!");
    }
}
