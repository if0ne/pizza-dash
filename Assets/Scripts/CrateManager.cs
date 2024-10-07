using System.Collections;
using UnityEngine;

public class CrateManager : MonoBehaviour
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

    public void RespawnCrate(Crate crate)
    {
        StartCoroutine(RespawnCrateCoroutine(crate));
    }

    private IEnumerator RespawnCrateCoroutine(Crate crate)
    {
        yield return new WaitForSeconds(5);
        crate.AssignRandomAbility();
        crate.transform.position = crate.spawnPosition;
        crate.gameObject.SetActive(true);
    }
}
