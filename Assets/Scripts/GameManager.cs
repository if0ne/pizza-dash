using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;
    public GameObject[] houses;
    public PizzaShop[] pizzaShops;

    [SyncVar]
    private int currentRound = 0;

    public int maxRounds = 10;
    public float roundStartDelay = 3f;
    public float gameStartDelay = 5.0f;

    public List<Player> players = new List<Player>();

    public delegate void RoundUpdateHandler(int currentRound, int maxRounds);
    public event RoundUpdateHandler OnRoundUpdate;

    public delegate void PizzaShopUpdateHandler(PizzaShop[] pizzaShops);
    public event PizzaShopUpdateHandler OnPizzaShopUpdate;

    void Awake()
    {
        Instance = this;
    }

    // This method will be called by NetworkManagerPizza to initialize the game
    [Server]
    public void InitializeGame(List<NetworkConnectionToClient> connections)
    {
        pizzaShops = FindObjectsOfType<PizzaShop>();
        SpawnPlayers(connections);
        RpcInitializeGameUI();
        RpcUpdateUI(currentRound, maxRounds); // Notify all clients about the new round
        StartCoroutine(StartGame());
    }

    [Server]
    private void SpawnPlayers(List<NetworkConnectionToClient> connections)
    {
        // Spawn players for all connected clients
        for (int i = 0; i < connections.Count; i++)
        {
            var conn = connections[i];

            // Manually instantiate the player prefab for each connection
            GameObject playerObj = Instantiate(NetworkManagerPizza.Instance.playerPrefab, pizzaShops[i].spawnPoint.position, Quaternion.identity);
            NetworkServer.Spawn(playerObj, conn);
            NetworkServer.AddPlayerForConnection(conn, playerObj);

            // Get the Player component and store it
            Player player = playerObj.GetComponent<Player>();
            player.RpcInitialize(pizzaShops[i]);
            player.playerId = i;
            players.Add(player);  // Add the player to the list of players

            Debug.Log("Player spawned with ID: " + player.playerId);
        }

    }

    private IEnumerator StartGame()
    {
        yield return new WaitForSeconds(gameStartDelay);
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
            RpcUpdateUI(currentRound, maxRounds); // Notify all clients about the new round
        }
        else
        {
            DetermineWinningPizzeria();
        }
    }

    [Server]
    void AssignHouseToPlayer(Player player)
    {
        House house = houses[Random.Range(0, houses.Length)].GetComponent<House>();
        house.isOrderActive = true;
        player.RpcSetTarget(house);
        house.ActivateOrder(player.currentShop);
    }

    [Server]
    public void PlayerDeliveredPizza(Player player)
    {
        player.currentHouse.DeactivateOrder();
        player.currentHouse = null;  // Clear the current house on the server
        player.RpcSetTarget(null);

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

    [Server]
    private IEnumerator DelayedStartNewRound()
    {
        yield return new WaitForSeconds(roundStartDelay);
        StartNewRound();
    }

    [Server]
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

    // ClientRpc to update UI for all players when a new round starts
    [ClientRpc]
    private void RpcUpdateUI(int currentRound, int maxRounds)
    {
        OnRoundUpdate?.Invoke(currentRound, maxRounds);
        OnPizzaShopUpdate?.Invoke(pizzaShops);
    }

    [ClientRpc]
    private void RpcInitializeGameUI()
    {
        // Initialize the game UI for all clients
        GameUI localGameUI = FindObjectOfType<GameUI>();
        if (localGameUI != null)
        {
            localGameUI.SubscribeToGameEvents();
        }
    }
}
