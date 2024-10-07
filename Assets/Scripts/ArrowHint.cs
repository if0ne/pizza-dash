using UnityEngine;

public class ArrowHint : MonoBehaviour
{
    private Transform target; // Reference to the target point

    void Update()
    {
        if (target == null) { return; }

        Vector3 direction = target.position - transform.position;
        direction.y = 0; // Ignore the y-axis
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = lookRotation * Quaternion.Euler(0, 180, 0);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
