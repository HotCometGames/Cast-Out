using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CraftingRuneScript : MonoBehaviour
{
    [SerializeField] List<ItemData> craftingItems = new List<ItemData>();
    private List<GameObject> itemsInRune = new List<GameObject>();
    [SerializeField]  List<List<ItemData>> craftingRecipes = new List<List<ItemData>>();
    [SerializeField] List<List<ItemData>> craftingResults = new List<List<ItemData>>();
    private bool playerInRange = false;

    // Start is called before the first frame update
    void Start()
    {
        craftingRecipes.Add(new List<ItemData> { craftingItems[3], craftingItems[3], craftingItems[3], craftingItems[3] }); // Example recipe
        craftingResults.Add(new List<ItemData> { craftingItems[9] }); // Example result 
    }

    // Update is called once per frame
    void Update()
    {
        itemsInRune.RemoveAll(item => item == null);
        if (playerInRange && Input.GetMouseButtonDown(0))
            CraftItem();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Item") == false)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                playerInRange = true;
            }
            return;
        }
        itemsInRune.Add(other.gameObject);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Item") == false)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                playerInRange = false;
        }
            return;
        }
        itemsInRune.Remove(other.gameObject);
    }


    void CraftItem()
    {
        foreach (List<ItemData> recipe in craftingRecipes)
        {
            if (IsRecipeMatch(recipe))
            {
                Debug.Log("Crafting successful!");
                // Implement crafting logic here
                for (int i = 0; i < recipe.Count; i++)
                {
                    Destroy(itemsInRune[i]);
                }
                for (int i = 0; i < craftingResults[craftingRecipes.IndexOf(recipe)].Count; i++)
                {
                    Instantiate(craftingResults[craftingRecipes.IndexOf(recipe)][i].prefabDefinition.prefab, transform.position + Vector3.up * 2.0f, Quaternion.identity);
                }
                return;
            }
        }
        Debug.Log("No matching recipe found.");
    }

    bool IsRecipeMatch(List<ItemData> recipe)
    {
        if (recipe.Count != itemsInRune.Count)
            return false;

        List<ItemData> tempItems = new List<ItemData>();
        foreach (GameObject item in itemsInRune)
        {
            ItemData itemData = item.GetComponent<ItemPickup>().itemData;
            if (itemData != null)
            {
                tempItems.Add(itemData);
            }
        }

        foreach (ItemData requiredItem in recipe)
        {
            bool found = false;
            for (int i = 0; i < tempItems.Count; i++)
            {
                if (tempItems[i].name == requiredItem.name)
                {
                    found = true;
                    tempItems.RemoveAt(i);
                    break;
                }
            }
            if (!found)
                return false;
        }
        return true;
    }
}
