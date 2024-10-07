using UnityEngine;

public class Crate : MonoBehaviour
{
    public enum Ability { Acceleration, Phantom, Money }
    public Ability crateAbility;

    [HideInInspector]
    public Vector3 spawnPosition;

    void Start()
    {
        spawnPosition = transform.position;
        AssignRandomAbility();
    }

    void OnTriggerEnter(Collider other)
    {
        Player player = other.GetComponent<Player>();
        if (player != null && player.currentAbility == null)
        {
            player.PickUpAbility(crateAbility);
            CrateManager.Instance.RespawnCrate(this);
            gameObject.SetActive(false);
        }
    }

    public void AssignRandomAbility()
    {
        crateAbility = (Ability)Random.Range(0, System.Enum.GetValues(typeof(Ability)).Length);
    }
}
