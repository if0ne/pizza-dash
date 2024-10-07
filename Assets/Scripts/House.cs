using UnityEngine;

public class House : MonoBehaviour
{
    public bool isOrderActive = false;
    public PizzaShop linkedShop; // The shop that placed the order
    public float deliveryStartTime;
    public GameObject triggerChild; // Reference to the trigger child

    public void ActivateOrder(PizzaShop shop)
    {
        isOrderActive = true;
        linkedShop = shop;
        deliveryStartTime = Time.time;
        triggerChild.SetActive(true); // Activate the trigger
    }

    public void DeactivateOrder()
    {
        isOrderActive = false;
        linkedShop = null;
        triggerChild.SetActive(false); // Deactivate the trigger
    }
}
