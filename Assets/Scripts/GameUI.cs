using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public TMP_Text infoText; // Reference to the UI Text element

    public GameObject pizzaShopInfoPrefab; // Prefab for displaying pizza shop info
    public Transform contentPanel; // Parent panel for the list items

    private List<GameObject> pizzaShopInfos = new List<GameObject>();

    public Image abilityImage; // Reference to the UI Image element

    public Sprite accelerationSprite;
    public Sprite phantomSprite;
    public Sprite moneySprite;

    private void Start()
    {
        GameManager.Instance.OnRoundUpdate += UpdateRoundInfo;
        GameManager.Instance.OnPizzaShopUpdate += UpdatePizzaShopInfo;

        for (int i = 0; i < GameManager.Instance.pizzaShops.Length; i++)
        {
            GameObject newInfo = Instantiate(pizzaShopInfoPrefab, contentPanel);
            pizzaShopInfos.Add(newInfo);
        }
    }

    private void OnDisable()
    {
        GameManager.Instance.OnRoundUpdate -= UpdateRoundInfo;
        GameManager.Instance.OnPizzaShopUpdate -= UpdatePizzaShopInfo;
    }

    private void UpdateRoundInfo(int currentRound, int maxRounds)
    {
        infoText.text = $"Раунд: {currentRound}/{maxRounds}\n";
    }

    private void UpdatePizzaShopInfo(PizzaShop[] pizzaShops)
    {
        // Add new items
        for (int i = 0; i < pizzaShops.Length; i++)
        {
            var shop = pizzaShops[i];
            var infoText = pizzaShopInfos[i].GetComponentInChildren<TMP_Text>();
            infoText.text = $"{shop.gameObject.name}: {shop.money:n2} $";
        }
        foreach (PizzaShop shop in pizzaShops)
        {
            
        }
    }

    public void UpdateAbilityImage(Crate.Ability ability)
    {
        abilityImage.gameObject.SetActive(true);
        switch (ability)
        {
            case Crate.Ability.Acceleration:
                abilityImage.sprite = accelerationSprite;
                break;
            case Crate.Ability.Phantom:
                abilityImage.sprite = phantomSprite;
                break;
            case Crate.Ability.Money:
                abilityImage.sprite = moneySprite;
                break;
        }
    }

    public void ClearAbilityImage()
    {
        abilityImage.gameObject.SetActive(false);
    }
}
