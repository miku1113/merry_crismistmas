using UnityEngine;

public class Gift : MonoBehaviour
{
    private bool hasCollided = false;

    void Update()
    {
        // Cleanup if gift falls too far
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

    // Handles trigger collisions (like the Chimney)
    void OnTriggerEnter2D(Collider2D other)
    {
        HandleImpact(other.gameObject);
    }

    private void HandleImpact(GameObject hitObject)
    {
        if (hasCollided) return;

        if (hitObject.CompareTag("Chimney"))
        {
            hasCollided = true;
            Debug.Log("Gift fell into chimney! +1 Point");
            
            // Notify the chimney it was hit
            // Use GetComponentInParent to be extra safe if collider is on a child
            Chimney chimney = hitObject.GetComponentInParent<Chimney>();
            if (chimney != null) 
            {
                chimney.RegisterGift();
            }
            else 
            {
                Debug.LogWarning("No Chimney script found on " + hitObject.name + " or its parents!");
            }

            GameManager.Instance.AddScore(1);
            Destroy(gameObject, 0.1f);
        }
        else if (hitObject.CompareTag("Ground") || hitObject.CompareTag("Tree"))
        {
            hasCollided = true;
            Debug.Log("Gift hit " + hitObject.tag + "! Game Over.");
            GameManager.Instance.GameOver();
            Destroy(gameObject, 2f);
        }
    }
}
