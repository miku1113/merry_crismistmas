using UnityEngine;

public class WaterBall : MonoBehaviour
{
    private bool hasCollided = false;

    void Update()
    {
        // Cleanup if water ball falls too far
        if (transform.position.y < -15f)
        {
            Destroy(gameObject);
        }
    }

    // Handles solid collisions (like the Ground)
    void OnCollisionEnter2D(Collision2D collision)
    {
        HandleImpact(collision.gameObject);
    }

    // Handles trigger collisions (like the Yagna)
    void OnTriggerEnter2D(Collider2D other)
    {
        HandleImpact(other.gameObject);
    }

    private void HandleImpact(GameObject hitObject)
    {
        if (hasCollided) return;

        if (hitObject.CompareTag("Chimney") || hitObject.name.Contains("Yagna")) // Support both tag and name for flexibility
        {
            hasCollided = true;
            Debug.Log("Water ball hit Yagna! Fire extinguished! +1 Point");
            
            // Notify the yagna it was hit
            Yagna yagna = hitObject.GetComponentInParent<Yagna>();
            if (yagna != null) 
            {
                yagna.ExtinguishFire();
            }
            else 
            {
                Debug.LogWarning("No Yagna script found on " + hitObject.name + " or its parents!");
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(1);
            }
            Destroy(gameObject, 0.1f);
        }
        else if (hitObject.CompareTag("Ground") || hitObject.CompareTag("Tree") || hitObject.tag == "Obstacle")
        {
            hasCollided = true;
            Debug.Log("Water Ball hit " + hitObject.tag + "! Game Over.");
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameOver();
            }
            Destroy(gameObject, 2f);
        }
    }
}
