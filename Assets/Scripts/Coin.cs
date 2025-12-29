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
        SantaController santa = other.GetComponent<SantaController>();
        if (santa == null && other.CompareTag("Player"))
        {
            santa = other.GetComponentInParent<SantaController>();
        }

        if (santa != null)
        {
            Collect(santa);
        }
    }

    void Collect(SantaController santa)
    {
        // Add to coin count only (Score logic removed)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddCoin(1); 
        }

        // Play sound via Santa (since coin is about to be destroyed)
        if (santa != null && collectSound != null)
        {
            santa.PlayCollectSound(collectSound);
        }
        else if (collectSound == null)
        {
            Debug.LogWarning("Coin: collectSound is missing in the Inspector!");
        }
        
        // Destroy the coin
        Destroy(gameObject);
    }
}
