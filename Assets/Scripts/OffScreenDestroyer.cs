using UnityEngine;

public class OffScreenDestroyer : MonoBehaviour
{
    [SerializeField] float delaySeconds = 2f;
    private bool isOffScreen = false;
    private float offScreenTimer = 0f;

    void Update()
    {
        if (Camera.main == null) return;

        Vector3 screenPoint = Camera.main.WorldToViewportPoint(transform.position);

        // If object is off-screen to the left (x < 0)
        if (screenPoint.x < -0.1f)
        {
            if (!isOffScreen)
            {
                isOffScreen = true;
                offScreenTimer = 0f;
            }
            
            offScreenTimer += Time.deltaTime;

            if (offScreenTimer >= delaySeconds)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            // Reset if it comes back into view (unlikely for trees but safe)
            isOffScreen = false;
        }
    }
}
