using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManagerPizza : NetworkManager
{
    public ushort serverPort = 7777;  // Port for the server
    public ushort clientPort = 7778;  // Port for the client

    public static NetworkManagerPizza Instance;

    public int requiredPlayers = 2;   // Number of players required to start the game
    public List<NetworkConnectionToClient> connections = new List<NetworkConnectionToClient>();
    private bool isGameStarted = false;

    public override void Awake()
    {
        base.Awake();
        Instance = this;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        // Set the server port before starting the server
        TelepathyTransport telepathyTransport = Transport.active as TelepathyTransport;
        if (telepathyTransport != null)
        {
            telepathyTransport.port = serverPort;
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        // Set the client port before starting the client
        TelepathyTransport telepathyTransport = Transport.active as TelepathyTransport;
        if (telepathyTransport != null)
        {
            telepathyTransport.port = clientPort;
        }
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        // Store the connection instead of adding the player prefab directly
        connections.Add(conn);

        Debug.Log("Player connected with connection ID: " + conn.connectionId);

        // Start the game if 2 players are connected
        if (connections.Count == requiredPlayers && !isGameStarted)
        {
            StartGame();
        }
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        connections.Remove(conn);

        Debug.Log("Player disconnected: " + conn.connectionId);
        base.OnServerDisconnect(conn);
    }

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        base.OnServerConnect(conn);
        // Call this when the second player connects
        if (NetworkServer.connections.Count == requiredPlayers)
        {
            FindObjectOfType<GameUI>().OnSecondPlayerConnected(); // Ensure you have a way to access the GameUI instance
        }
    }

    [Server]
    private void StartGame()
    {
        isGameStarted = true;
        GameManager.Instance.InitializeGame(connections);
    }
}
