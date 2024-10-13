using Mirror;
using UnityEngine;

public class House : NetworkBehaviour
{
    [SyncVar] // Sync the active order status across all clients
    public bool isOrderActive = false;

    [SyncVar] // Sync the linked shop across clients
    public PizzaShop linkedShop;

    [SyncVar] // Sync the delivery start time across clients
    public float deliveryStartTime;

    public GameObject triggerChild; // Reference to the trigger child

    // Server activates an order
    [Server]
    public void ActivateOrder(PizzaShop shop)
    {
        isOrderActive = true;
        linkedShop = shop;
        deliveryStartTime = Time.time;
        RpcNotifyClientsOfOrder(shop.netId); // Notify all clients about the new order, passing the shop ID
    }

    // Deactivate the order on the server
    [Server]
    public void DeactivateOrder()
    {
        isOrderActive = false;
        linkedShop = null;
        RpcNotifyClientsOfOrder(0); // Notify all clients that the order is deactivated
    }

    // Sync method to notify clients to check if they should activate the trigger
    [ClientRpc]
    private void RpcNotifyClientsOfOrder(uint shopNetId)
    {
        if (shopNetId == 0)
        {
            // No active order, deactivate the trigger
            triggerChild.SetActive(false);
        }
        else
        {
            // Only activate the trigger for the client that owns the linked shop
            Player localPlayer = NetworkClient.connection.identity.GetComponent<Player>();

            if (localPlayer != null && localPlayer.currentShop != null && localPlayer.currentShop.netId == shopNetId)
            {
                triggerChild.SetActive(true); // Activate trigger for the correct client
            }
            else
            {
                triggerChild.SetActive(false); // Deactivate for other clients
            }
        }
    }
}
