using Mirror;
using System.Collections;
using UnityEngine;

public class CrateManager : NetworkBehaviour
{
    public static CrateManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [Server]
    public void RespawnCrate(Crate crate)
    {
        StartCoroutine(RespawnCrateCoroutine(crate));
    }

    private IEnumerator RespawnCrateCoroutine(Crate crate)
    {
        yield return new WaitForSeconds(5);
        crate.AssignRandomAbility();
        crate.Respawn(); // Ensure the crate gets reactivated on the server and clients
    }
}
