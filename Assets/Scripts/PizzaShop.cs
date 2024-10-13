using Mirror;
using UnityEngine;

public class PizzaShop : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnMoneyUpdated))] // Sync money across all clients
    public float money = 0; // Pizzeria's money
    public Transform spawnPoint; // Spawn point for the player

    [SerializeField]
    private Renderer triggerRenderer;

    public delegate void MoneyUpdateHandler(float newMoney);
    public event MoneyUpdateHandler OnMoneyUpdate;

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") 
            && other.gameObject.GetComponent<Player>().currentShop == this)
        {
            other.gameObject.GetComponent<Player>().RpcAddPizza();
        }
    }

    [Server]
    public void AddPoints(float pointsToAdd)
    {
        money += pointsToAdd;
        Debug.Log(gameObject.name + " Money: " + money);
    }


    public void SetTriggerVisualState(bool state)
    {
        if (triggerRenderer != null)
        {
            triggerRenderer.enabled = state;
        }
    }

    // Hook method that gets called when money is updated, sync it across clients
    private void OnMoneyUpdated(float oldMoney, float newMoney)
    {
        OnMoneyUpdate?.Invoke(newMoney); // Notify any listeners about the money update
        UpdatePizzaShopUI();
    }

    private void UpdatePizzaShopUI()
    {
        FindObjectOfType<GameUI>().UpdatePizzaShopInfo(this);
    }
}