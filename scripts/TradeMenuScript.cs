using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class Trade
{
    public ItemData[] itemCosts;
    public ItemData[] itemGains;
}

[System.Serializable]
public class MultiTrade
{
    public List<Trade> trades;
}

public class TradeMenuScript : MonoBehaviour
{
    public string traderName;
    public List<Trade> trades;
    public GameObject tradeButtonPrefab;
    public List<GameObject> tradeButtons = new List<GameObject>();
    public TMP_Text traderNameText;
    // Start is called before the first frame update
    void Start()
    {
        CreateTradeButtons();
    }

    public void CreateTradeButtons()
    {
        for (int i = 0; i < trades.Count; i++)
        {
            Trade trade = trades[i];
            Vector3 localPos = new Vector3(0, 80 - (i * 50), 0); // Set the desired position for the button
            GameObject button = Instantiate(tradeButtonPrefab, transform);
            RectTransform rt = button.GetComponent<RectTransform>();
            rt.localPosition = localPos;
            TradeButtonScript tbs = button.GetComponent<TradeButtonScript>();
            tbs.trade = trade;
            tradeButtons.Add(button);
        }
        traderNameText.text = traderName;
    }

    public void ClearTradeButtons()
    {
        foreach (GameObject button in tradeButtons)
        {
            Destroy(button);
        }
        tradeButtons.Clear();
        traderNameText.text = "";
    }
}
