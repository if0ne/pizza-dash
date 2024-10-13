using Mirror;
using UnityEngine;

public class Crate : NetworkBehaviour
{
    public enum Ability { None, Acceleration, Phantom, Money }

    [SyncVar]
    public Ability crateAbility;

    [HideInInspector]
    public Vector3 spawnPosition;

    void Start()
    {
        spawnPosition = transform.position;

        if (isServer) // Only the server assigns abilities
        {
            AssignRandomAbility();
        }
    }

    [ServerCallback]
    void OnTriggerEnter(Collider other)
    {
        Player player = other.GetComponent<Player>();
        if (player != null && player.currentAbility == Ability.None)
        {
            player.RpcPickUpAbility(crateAbility);
            CrateManager.Instance.RespawnCrate(this);
            RpcDeactivateCrate(); // Deactivate the crate for all players
        }
    }

    // Server assigns a random ability to the crate
    [Server]
    public void AssignRandomAbility()
    {
        crateAbility = (Ability)Random.Range(1, System.Enum.GetValues(typeof(Ability)).Length);
    }

    // Deactivate the crate across all clients
    [ClientRpc]
    public void RpcDeactivateCrate()
    {
        gameObject.SetActive(false); // Deactivate the crate on all clients
    }

    // This method is called on the server to respawn the crate
    [Server]
    public void Respawn()
    {
        transform.position = spawnPosition;
        gameObject.SetActive(true); // Reactivate the crate on the server
        RpcRespawnCrate(); // Notify clients to reactivate the crate
    }

    // This method is called to sync respawn across clients
    [ClientRpc]
    public void RpcRespawnCrate()
    {
        gameObject.SetActive(true); // Reactivate the crate on all clients
    }
}
