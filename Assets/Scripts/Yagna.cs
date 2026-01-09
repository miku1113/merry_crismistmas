using UnityEngine;

public class Yagna : MonoBehaviour
{
    private Collider2D yagnaCollider;
    private bool isExtinguished = false;
    private bool isOffScreen = false;

    [Header("Visuals")]
    public GameObject fireVisual; // Assign the fire sprite/object here

    void Start()
    {
        // Get collider even if it's on a child object
        yagnaCollider = GetComponentInChildren<Collider2D>();
        
        // Register with the centralized tracker (treating Yagna as a Chimney for tracking)
        // Note: GameManager treats everything in activeChimneys as a target that must be hit.
        // We'll use a trick where we cast it to a dummy chimney or update GameManager later if needed.
        // For now, let's assume we might need a generic target script or base class.
        // But to keep it simple and similar to existing code:
    }

    void Update()
    {
        // DESTROY if too far to the left of the player (Cleanup)
        if (GameManager.Instance != null && GameManager.Instance.santaTransform != null)
        {
            if (transform.position.x < GameManager.Instance.santaTransform.position.x - 25f)
            {
                Destroy(gameObject);
                return;
            }
        }

        if (isExtinguished || isOffScreen) return;

        // MISS DETECTION
        if (Camera.main != null)
        {
            Vector3 screenPoint = Camera.main.WorldToViewportPoint(transform.position);
            
            // If Yagna is off-screen to the left (x < 0) and hasn't been extinguished
            if (screenPoint.x < -0.1f)
            {
                isOffScreen = true;
                Debug.Log("Yagna script detected Missed Fire at ViewportX: " + screenPoint.x);
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.GameOver();
                }
            }
        }
    }

    public bool IsActive()
    {
        return !isExtinguished;
    }

    public void ExtinguishFire()
    {
        if (isExtinguished) return;

        Debug.Log("Yagna extinguished: Hiding fire and disabling collider.");
        isExtinguished = true;
        
        if (fireVisual != null)
        {
            fireVisual.SetActive(false);
        }

        // Disable the collider to prevent multiple hits
        if (yagnaCollider != null)
        {
            yagnaCollider.enabled = false;
        }
    }
}
