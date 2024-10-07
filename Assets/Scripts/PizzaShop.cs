using UnityEngine;

public class PizzaShop : MonoBehaviour
{
    public float money = 0; // Pizzeria's money
    public Transform spawnPoint; // Spawn point for the player

    [SerializeField]
    private Renderer triggerRenderer;

    public delegate void MoneyUpdateHandler(float newMoney);
    public event MoneyUpdateHandler OnMoneyUpdate;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") 
            && other.gameObject.GetComponent<Player>().currentShop == this)
        {
            other.gameObject.GetComponent<Player>().AddPizza();
        }
    }

    public void AddPoints(float pointsToAdd)
    {
        money += pointsToAdd;
        Debug.Log(gameObject.name + " Money: " + money);
        OnMoneyUpdate?.Invoke(money);
    }

    public void SetTriggerVisualState(bool state)
    {
        if (triggerRenderer != null)
        {
            triggerRenderer.enabled = state;
        }
    }
}