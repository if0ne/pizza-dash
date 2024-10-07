using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int playerId;
    public bool hasPizza = false;

    public PizzaShop currentShop; // The shop the player got the pizza from
    public House currentHouse; // The house to deliver the pizza to

    [SerializeField]
    private GameObject pizzaSocket;

    public ArrowHint arrow;

    // Fields for abilities and coroutines
    public Crate.Ability? currentAbility;
    private GameUI gameUI;

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

    private void Start()
    {
        gameUI = FindObjectOfType<GameUI>();
    }

    private void Update()
    {
        DetectDoubleClick();
    }

    private void DetectDoubleClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Time.time - lastClickTime < doubleClickTime)
            {
                UseCurrentAbility();
            }
            lastClickTime = Time.time;
        }
    }

    private void UseCurrentAbility()
    {
        if (currentAbility.HasValue)
        {
            switch (currentAbility.Value)
            {
                case Crate.Ability.Phantom:
                    StartPhantomAbility(3.0f); // Example duration
                    break;
                case Crate.Ability.Acceleration:
                    StartAccelerationAbility(3.0f); // Example duration
                    break;
                case Crate.Ability.Money:
                    StartMoneyAbility();
                    break;
            }
            currentAbility = null;
            gameUI.ClearAbilityImage();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("House") && hasPizza && currentHouse == other.GetComponentInParent<House>())
        {
            RemovePizza();

            float deliveryTime = Time.time - currentHouse.deliveryStartTime;
            Debug.Log(deliveryTime);
            float points = deliveryTime <= 15f ? 15.0f - deliveryTime : 0;
            AddPoints(points);

            GameManager.Instance.PlayerDeliveredPizza(this);
            return;
        }
    }

    // Method to check if the player is inside an object
    private bool IsInsideObject()
    {
        Collider[] hitColliders = Physics.OverlapSphere(carController.transform.position, checkRadius, obstacleLayer);
        return hitColliders.Length > 0;
    }

    public void AddPizza() {
        hasPizza = true;
        pizzaSocket.SetActive(true);
        currentShop.SetTriggerVisualState(false); // Hide the trigger visual
    }

    private void RemovePizza()
    {
        hasPizza = false;
        pizzaSocket.SetActive(false);
        currentShop.SetTriggerVisualState(true); // Show the trigger visual
    }

    public void AddPoints(float pointsToAdd)
    {
        currentShop.AddPoints(pointsToAdd);
    }

    public void SetTarget(House target)
    {
        currentHouse = target;
        arrow.SetTarget(target?.transform);
        arrow.gameObject.SetActive(target != null); // Activate or deactivate based on target
    }

    public void StartMoneyAbility()
    {
        currentShop.AddPoints(3.0f);
    }

    // Method to start the phantom ability
    public void StartPhantomAbility(float duration)
    {
        if (phantomCoroutine != null)
        {
            StopCoroutine(phantomCoroutine);
        }
        phantomCoroutine = StartCoroutine(PhantomAbilityCoroutine(duration));
    }

    // Coroutine for the phantom ability
    private IEnumerator PhantomAbilityCoroutine(float duration)
    {
        carController.gameObject.layer = LayerMask.NameToLayer("Phantom");
        yield return new WaitForSeconds(duration);

        // Wait until the player is not inside an object
        while (IsInsideObject())
        {
            yield return null;
        }

        carController.gameObject.layer = 0;
    }

    // Method to start the acceleration ability
    public void StartAccelerationAbility(float duration)
    {
        if (accelerationCoroutine != null)
        {
            StopCoroutine(accelerationCoroutine);
            carController.maxSpeed = carController.originalMaxSpeed; // Restore original max speed
            carController.acceleration = carController.originalAcceleration; // Restore original acceleration
        }
        accelerationCoroutine = StartCoroutine(AccelerationAbilityCoroutine(duration));
    }

    // Coroutine for the acceleration ability
    private IEnumerator AccelerationAbilityCoroutine(float duration)
    {
        carController.maxSpeed *= 1.35f; // Double the max speed
        carController.acceleration *= 1.35f; // Double the acceleration

        yield return new WaitForSeconds(duration);

        carController.maxSpeed = carController.originalMaxSpeed; // Restore original max speed
        carController.acceleration = carController.originalAcceleration; // Restore original acceleration

        accelerationCoroutine = null;
    }

    public void PickUpAbility(Crate.Ability ability)
    {
        currentAbility = ability;
        gameUI.UpdateAbilityImage(ability);
    }
}
