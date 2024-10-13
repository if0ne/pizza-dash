using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public TMP_Text infoText; // Reference to the UI Text element
    public string ipAddress = "localhost"; // Default IP (can be updated via UI)

    public GameObject pizzaShopInfoPrefab; // Prefab for displaying pizza shop info
    public Transform contentPanel; // Parent panel for the list items

    private List<GameObject> pizzaShopInfos = new List<GameObject>();

    public Image abilityImage; // Reference to the UI Image element

    public Sprite accelerationSprite;
    public Sprite phantomSprite;
    public Sprite moneySprite;

    public Player localPlayer; // Reference to the local player instance

    public GameObject initialPanel; // Panel for initial host/connect buttons
    public GameObject waitingPanel; // Panel for waiting for a second player
    public GameObject gamePanel; // Main game panel
    public GameObject finalResultPanel; // Panel for displaying the final result
    public TMP_Text finalResultText; // Text element to show win/lose message

    private bool isSubscribed = false;

    private void Start()
    {
        // Setup UI
        initialPanel.SetActive(true);
        waitingPanel.SetActive(false);
        gamePanel.SetActive(false);
    }

    // Call this from GameManager.InitializeGame to subscribe to events
    public void SubscribeToGameEvents()
    {
        if (isSubscribed) return;  // Prevent double subscriptions

        GameManager.Instance.OnRoundUpdate += UpdateRoundInfo;
        GameManager.Instance.OnPizzaShopUpdate += UpdatePizzaShopInfo;

        // Create UI elements for each pizza shop
        for (int i = 0; i < GameManager.Instance.pizzaShops.Length; i++)
        {
            GameObject newInfo = Instantiate(pizzaShopInfoPrefab, contentPanel);
            pizzaShopInfos.Add(newInfo);
        }

        isSubscribed = true;
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
    }

    public void UpdateAbilityImage(Crate.Ability ability)
    {
        switch (ability)
        {
            case Crate.Ability.Acceleration:
                abilityImage.gameObject.SetActive(true);
                abilityImage.sprite = accelerationSprite;
                break;
            case Crate.Ability.Phantom:
                abilityImage.gameObject.SetActive(true);
                abilityImage.sprite = phantomSprite;
                break;
            case Crate.Ability.Money:
                abilityImage.gameObject.SetActive(true);
                abilityImage.sprite = moneySprite;
                break;
            case Crate.Ability.None:
                abilityImage.gameObject.SetActive(false);
                break;
        }
    }

    public void ClearAbilityImage()
    {
        if (localPlayer.isLocalPlayer) // Ensure only the local player's UI is updated
        {
            abilityImage.gameObject.SetActive(false);
        }
    }

    public void StartHost()
    {
        // Show waiting panel
        initialPanel.SetActive(false);
        waitingPanel.SetActive(true);

        // Start hosting the game
        NetworkManagerPizza.Instance.StartHost();
        Debug.Log("Hosting a game...");
    }

    public void SetIPAddress(string newIP)
    {
        ipAddress = newIP;
    }

    public void JoinServer()
    {
        // Show waiting panel
        initialPanel.SetActive(false);
        gamePanel.SetActive(true);

        // Set the network address to the given IP
        NetworkManagerPizza.Instance.networkAddress = ipAddress;

        // Start client to join the game
        NetworkManagerPizza.Instance.StartClient();
        Debug.Log("Joining game at IP: " + ipAddress);
    }

    public void OnSecondPlayerConnected()
    {
        // Hide waiting panel and show game panel
        waitingPanel.SetActive(false);
        gamePanel.SetActive(true);
    }

    // Call this to show the final result
    public void ShowFinalResult(bool didWin)
    {
        finalResultPanel.SetActive(true);
        finalResultText.text = didWin ? "You Win!" : "You Lose!";
    }
}
