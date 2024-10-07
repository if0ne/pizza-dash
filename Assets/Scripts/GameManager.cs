using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameObject[] houses;
    public PizzaShop[] pizzaShops;
    private int currentRound = 0;
    public int maxRounds = 10; // Maximum number of rounds
    public float roundStartDelay = 3f; // Customizable delay for starting a new round
    public float gameStart = 5.0f;

    public List<Player> players = new List<Player>(); // List of players
    public GameObject playerPrefab;
    public Joystick joystick; // Reference to the joystick

    public delegate void RoundUpdateHandler(int currentRound, int maxRounds);
    public event RoundUpdateHandler OnRoundUpdate;

    public delegate void PizzaShopUpdateHandler(PizzaShop[] pizzaShops);
    public event PizzaShopUpdateHandler OnPizzaShopUpdate;

    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        SpawnPlayers();
        StartCoroutine(StartGame());
        SubscribeToPizzaShopEvents();
        UpdateUI();
    }

    private void SpawnPlayers()
    {
        for (int i = 0; i < pizzaShops.Length; i++)
        {
            // Use the spawn point from the corresponding pizzeria
            Transform spawnPoint = pizzaShops[i].spawnPoint;
            GameObject playerObject = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);
            Player player = playerObject.GetComponent<Player>();
            player.currentShop = pizzaShops[i];
            players.Add(player);

            // Assign the joystick to the CarController script
            CarController carController = playerObject.GetComponent<CarController>();
            carController.joystick = joystick;
            Camera.main.GetComponent<CameraFollow>().player = player.transform;

            player.playerId = i;

            Debug.Log("Player spawned with ID: " + player.playerId);
        }
    }

    private IEnumerator StartGame()
    {
        yield return new WaitForSeconds(gameStart);
        StartNewRound();
    }

    private void StartNewRound()
    {
        if (currentRound < maxRounds)
        {
            currentRound++;
            foreach (Player player in players)
            {
                AssignHouseToPlayer(player);
            }
            UpdateUI();
        }
        else
        {
            DetermineWinningPizzeria();
        }
    }

    void AssignHouseToPlayer(Player player)
    {
        House house = houses[Random.Range(0, houses.Length)].GetComponent<House>();
        house.isOrderActive = true;
        player.SetTarget(house);
        house.ActivateOrder(player.currentShop);
    }

    public void PlayerDeliveredPizza(Player player)
    {
        player.currentHouse.DeactivateOrder();
        player.SetTarget(null);

        // Check if all players have delivered their pizzas
        bool allDelivered = true;
        foreach (Player p in players)
        {
            if (p.currentHouse != null)
            {
                allDelivered = false;
                break;
            }
        }

        if (allDelivered)
        {
            StartCoroutine(DelayedStartNewRound());
        }
    }

    private IEnumerator DelayedStartNewRound()
    {
        yield return new WaitForSeconds(roundStartDelay);
        StartNewRound();
    }

    void DetermineWinningPizzeria()
    {
        PizzaShop winningShop = null;
        float highestMoney = 0;

        foreach (PizzaShop shop in pizzaShops)
        {
            if (shop.money > highestMoney)
            {
                highestMoney = shop.money;
                winningShop = shop;
            }
        }

        if (winningShop != null)
        {
            Debug.Log("The winning pizzeria is " + winningShop.gameObject.name + " with " + highestMoney + " money!");
        }
        else
        {
            Debug.Log("No winning pizzeria!");
        }
    }

    private void SubscribeToPizzaShopEvents()
    {
        foreach (PizzaShop shop in pizzaShops)
        {
            shop.OnMoneyUpdate += HandleMoneyUpdate;
        }
    }

    private void HandleMoneyUpdate(float newMoney)
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        OnRoundUpdate?.Invoke(currentRound, maxRounds);
        OnPizzaShopUpdate?.Invoke(pizzaShops);
    }
}
