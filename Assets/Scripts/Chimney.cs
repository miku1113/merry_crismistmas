using UnityEngine;

public class Chimney : MonoBehaviour
{
    private Collider2D chimneyCollider;
    private bool isFulfilled = false;
    private bool isOffScreen = false;

    void Start()
    {
        // Get collider even if it's on a child object
        chimneyCollider = GetComponentInChildren<Collider2D>();
        
        // Register with the centralized tracker
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterChimney(this);
        }
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UnregisterChimney(this);
        }
    }

    void Update()
    {
        // DESTROY if too far to the left of the Santa (Cleanup)
        if (GameManager.Instance != null && GameManager.Instance.santaTransform != null)
        {
            if (transform.position.x < GameManager.Instance.santaTransform.position.x - 25f)
            {
                Destroy(gameObject);
                return;
            }
        }

        if (isFulfilled || isOffScreen) return;

        // LOCAL MISS DETECTION (Primary)
        if (Camera.main != null)
        {
            Vector3 screenPoint = Camera.main.WorldToViewportPoint(transform.position);
            
            // If house is off-screen to the left (x < 0) and hasn't received a gift
            if (screenPoint.x < -0.1f)
            {
                isOffScreen = true;
                Debug.Log("Chimney script detected Missed House at ViewportX: " + screenPoint.x);
                GameManager.Instance.GameOver();
            }
        }
    }

    public bool IsActive()
    {
        // House is active until it receives a gift
        return !isFulfilled;
    }

    public void RegisterGift()
    {
        if (isFulfilled) return;

        Debug.Log("House fulfilled: Setting flag and disabling collider.");
        isFulfilled = true;
        
        // Disable the collider to prevent multiple hits
        if (chimneyCollider != null)
        {
            chimneyCollider.enabled = false;
        }
    }
}
