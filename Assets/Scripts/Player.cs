using Mirror;
using System.Collections;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public int playerId;

    [SyncVar]
    public bool hasPizza = false;

    [SyncVar(hook = nameof(OnCurrentShopChanged))]
    public PizzaShop currentShop; // The shop the player got the pizza from

    [SyncVar]
    public House currentHouse; // The house to deliver the pizza to

    [SerializeField]
    private GameObject pizzaSocket;

    public ArrowHint arrow;

    // Fields for abilities and coroutines
    [SyncVar(hook = nameof(OnAbilityChanged))]
    public Crate.Ability currentAbility;

    public GameUI gameUI;

    private Coroutine phantomCoroutine;
    private Coroutine accelerationCoroutine;

    private CarController carController;

    private float lastClickTime;
    private const float doubleClickTime = 0.3f; // Time interval to detect double-click

    public float checkRadius = 1.0f; // Adjust this value based on your needs
    public LayerMask obstacleLayer; // Layer mask for obstacles

    private void Awake()
    {
        carController = GetComponent<CarController>();
    }

    [ClientRpc]
    public void RpcInitialize(PizzaShop pizzaShop)
    {
        currentShop = pizzaShop;
        if (isLocalPlayer)
        {
            gameUI = FindObjectOfType<GameUI>();
            gameUI.localPlayer = this;
            pizzaShop.SetTriggerVisualState(true);
        }
    }

    private void Update()
    {
        if (isLocalPlayer)
        {
            DetectDoubleClick();
        }
    }

    private void DetectDoubleClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Time.time - lastClickTime < doubleClickTime)
            {
                CmdUseCurrentAbility();
            }
            lastClickTime = Time.time;
        }
    }

    [Command] // Client tells the server to use an ability
    private void CmdUseCurrentAbility()
    {
        if (currentAbility != Crate.Ability.None)
        {
            switch (currentAbility)
            {
                case Crate.Ability.Phantom:
                    if (phantomCoroutine != null)
                    {
                        StopCoroutine(phantomCoroutine);
                    }
                    phantomCoroutine = StartCoroutine(PhantomAbilityCoroutine(3.0f)); // Server logic
                    TargetStartPhantomAbility(connectionToClient, 3.0f); // Notify the client
                    break;
                case Crate.Ability.Acceleration:
                    if (accelerationCoroutine != null)
                    {
                        StopCoroutine(accelerationCoroutine);
                    }
                    accelerationCoroutine = StartCoroutine(AccelerationAbilityCoroutine(3.0f)); // Server logic
                    TargetStartAccelerationAbility(connectionToClient, 3.0f); // Notify the client
                    break;
                case Crate.Ability.Money:
                    currentShop.AddPoints(3.0f); // Give money to the player’s shop
                    break;
            }
            currentAbility = Crate.Ability.None;
            TargetClearAbilityUI(); // Update the UI for the local player
        }
    }

    [TargetRpc]
    private void TargetClearAbilityUI()
    {
        gameUI.ClearAbilityImage();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isLocalPlayer) return;

        if (other.CompareTag("House") && hasPizza)
        {
            House house = other.GetComponentInParent<House>();

            if (house != null && house == currentHouse)
            {
                // Send a command to the server to handle pizza delivery
                CmdHandlePizzaDelivery();
            }
        }
    }

    [Command] // This command is sent from the client to the server
    private void CmdHandlePizzaDelivery()
    {
        if (currentHouse != null && hasPizza)
        {
            // Server-side pizza delivery logic
            RpcRemovePizza();  // Notify clients to update their UI
            ServerHandlePizzaDelivery();  // Process the delivery on the server
        }
    }

    [ClientRpc] // Server handles pizza delivery
    private void RpcRemovePizza()
    {
        if (isLocalPlayer)
        {
            currentShop.SetTriggerVisualState(true);
        }

        hasPizza = false;
        UpdatePizzaSocket(false); // Update pizza socket for all clients
    }

    // New server-side method to handle pizza delivery
    [Server]
    private void ServerHandlePizzaDelivery()
    {
        float deliveryTime = Time.time - currentHouse.deliveryStartTime;
        float points = deliveryTime <= 15f ? 15.0f - deliveryTime : 0;
        currentShop.AddPoints(points);

        GameManager.Instance.PlayerDeliveredPizza(this); // Server directly calls the GameManager's command
    }

    private void UpdatePizzaSocket(bool state)
    {
        pizzaSocket.SetActive(state);
    }

    [ClientRpc]
    public void RpcAddPizza()
    {
        hasPizza = true;
        UpdatePizzaSocket(true);

        if (isLocalPlayer)
            currentShop.SetTriggerVisualState(false);
    }

    [ClientRpc]
    public void RpcSetTarget(House target)
    {
        currentHouse = target;
        if (isLocalPlayer)
        {
            arrow.SetTarget(target?.transform);
            arrow.gameObject.SetActive(target != null); // Activate or deactivate based on target
        }
    }

    // Method to check if the player is inside an object
    private bool IsInsideObject()
    {
        Collider[] hitColliders = Physics.OverlapSphere(carController.transform.position, checkRadius, obstacleLayer);
        return hitColliders.Length > 0;
    }

    private IEnumerator PhantomAbilityCoroutine(float duration)
    {
        carController.gameObject.layer = LayerMask.NameToLayer("Phantom");
        yield return new WaitForSeconds(duration);

        // Wait until the player is not inside an object
        while (IsInsideObject())
        {
            yield return null;
        }

        carController.gameObject.layer = 0; // Set back to default
    }

    [TargetRpc]
    private void TargetStartPhantomAbility(NetworkConnection target, float duration)
    {
        if (phantomCoroutine != null)
        {
            StopCoroutine(phantomCoroutine);
        }
        phantomCoroutine = StartCoroutine(PhantomAbilityCoroutine(duration)); // Run on the client
    }

    private IEnumerator AccelerationAbilityCoroutine(float duration)
    {
        carController.maxSpeed *= 1.35f; // Increase speed
        carController.acceleration *= 1.35f; // Increase acceleration

        yield return new WaitForSeconds(duration);

        carController.maxSpeed = carController.originalMaxSpeed; // Restore speed
        carController.acceleration = carController.originalAcceleration; // Restore acceleration
    }

    [TargetRpc]
    private void TargetStartAccelerationAbility(NetworkConnection target, float duration)
    {
        if (accelerationCoroutine != null)
        {
            StopCoroutine(accelerationCoroutine);
        }
        accelerationCoroutine = StartCoroutine(AccelerationAbilityCoroutine(duration)); // Run on the client
    }

    [ClientRpc]
    public void RpcPickUpAbility(Crate.Ability ability)
    {
        Debug.Log("Picked up ability: " + ability);
        currentAbility = ability;
    }

    void OnAbilityChanged(Crate.Ability oldAbility, Crate.Ability newAbility)
    {
        Debug.Log($"Ability changed from {oldAbility} to {newAbility}");

        // Update any client-side UI or effects based on the new ability
        RpcUpdateAbilityUI(newAbility);
    }

    [ClientRpc]
    private void RpcUpdateAbilityUI(Crate.Ability ability)
    {
        if (isLocalPlayer)
        {
            gameUI.UpdateAbilityImage(ability);
        }
    }

    private void OnCurrentShopChanged(PizzaShop oldShop, PizzaShop newShop)
    {
        if (isLocalPlayer && newShop != null)
        {
            Debug.Log("Set new shop");
            newShop.SetTriggerVisualState(true);
        }
    }

    public void ShowFinalResult(bool didWin)
    {
        if (isLocalPlayer)
        {
            gameUI.ShowFinalResult(didWin);
        }
    }
}
