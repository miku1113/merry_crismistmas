using UnityEngine;

public class Coin : MonoBehaviour
{
    public int scoreValue = 5;
    public AudioClip collectSound;

    void Update()
    {
        // Cleanup if coin goes off-screen (similar to other obstacles)
        if (transform.position.x < Camera.main.transform.position.x - 15f)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Try to find any player controller
        SantaController santa = other.GetComponent<SantaController>();
        WitchController witch = other.GetComponent<WitchController>();
        PlaneController plane = other.GetComponent<PlaneController>();

        if (santa == null && witch == null && plane == null && other.CompareTag("Player"))
        {
            santa = other.GetComponentInParent<SantaController>();
            witch = other.GetComponentInParent<WitchController>();
            plane = other.GetComponentInParent<PlaneController>();
        }

        if (santa != null) Collect(santa, null, null);
        else if (witch != null) Collect(null, witch, null);
        else if (plane != null) Collect(null, null, plane);
    }

    void Collect(SantaController santa, WitchController witch, PlaneController plane)
    {
        // Add to coin count only
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddCoin(1); 
        }

        // Play sound via the respective controller
        if (collectSound != null)
        {
            if (santa != null) santa.PlayCollectSound(collectSound);
            else if (witch != null) witch.PlayCollectSound(collectSound);
            else if (plane != null) plane.PlayCollectSound(collectSound);
        }
        else
        {
            Debug.LogWarning("Coin: collectSound is missing in the Inspector!");
        }
        
        // Destroy the coin
        Destroy(gameObject);
    }
}
