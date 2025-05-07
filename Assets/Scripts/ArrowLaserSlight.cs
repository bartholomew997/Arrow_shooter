using UnityEngine;

public class ArrowLaserSight : MonoBehaviour
{
    public Transform arrowSpawnPoint; // Assign the same spawn point as your arrow
    public LineRenderer lineRenderer; // Create and assign a LineRenderer in the inspector
    public float laserDistance = 50f;
    public Color defaultColor = Color.red;
    public Color hitColor = Color.red;

    private void Update()
    {
        if (arrowSpawnPoint == null || lineRenderer == null) return;

        RaycastHit hit;
        Vector3 direction = arrowSpawnPoint.forward;

        // Raycast from arrow point forward
        if (Physics.Raycast(arrowSpawnPoint.position, direction, out hit, laserDistance))
        {
            lineRenderer.SetPosition(0, arrowSpawnPoint.position);
            lineRenderer.SetPosition(1, hit.point);
            lineRenderer.startColor = hitColor;
            lineRenderer.endColor = hitColor;
        }
        else
        {
            lineRenderer.SetPosition(0, arrowSpawnPoint.position);
            lineRenderer.SetPosition(1, arrowSpawnPoint.position + direction * laserDistance);
            lineRenderer.startColor = defaultColor;
            lineRenderer.endColor = defaultColor;
        }
    }
}