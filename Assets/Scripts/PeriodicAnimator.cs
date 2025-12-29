using UnityEngine;

public class PeriodicAnimator : MonoBehaviour
{
    public Animator animator;
    public AudioSource audioSource; // Optional: Assign this in Inspector
    public string triggerName = "Pulse";
    public float interval = 2f;

    private float timer;

    void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        // Set timer to interval so it triggers immediately upon spawning
        timer = interval;
    }

    void Update()
    {
        if (animator == null) return;

        timer += Time.deltaTime;
        if (timer >= interval)
        {
            animator.SetTrigger(triggerName);
            
            // Sync Audio with Animation
            if (audioSource != null && audioSource.clip != null)
            {
                audioSource.Play();
            }

            timer = 0f;
        }
    }
}
