using UnityEngine;

public class Key : MonoBehaviour
{
    public AudioClip collectSound;

    void Update()
    {
        // Cleanup if key goes off-screen
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

        if (santa != null || witch != null || plane != null)
        {
            Collect(santa, witch, plane);
        }
    }

    void Collect(SantaController santa, WitchController witch, PlaneController plane)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddKey(1); 
        }

        // Play sound via the respective controller
        if (collectSound != null)
        {
            if (santa != null) santa.PlayCollectSound(collectSound);
            else if (witch != null) witch.PlayCollectSound(collectSound);
            else if (plane != null) plane.PlayCollectSound(collectSound);
        }
        
        Destroy(gameObject);
    }
}
