using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class SantaController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float verticalSpeed = 5f;
    public SimpleJoystick joystick;

    [Header("Gift Settings")]
    public GameObject giftPrefab;
    public Transform dropPoint;

    [Header("Audio Settings")]
    public AudioSource loopSource; // Dedicated for sleigh loop
    public AudioSource sfxSource;  // Dedicated for one-shot SFX
    public AudioClip startClip;
    public AudioClip loopClip;
    public AudioClip dropClip;
    public AudioClip crashClip; // New sound for hitting obstacles
    public AudioClip shockClip; // New sound for hitting dark clouds

    [Header("Animation Settings")]
    public Animator santaAnimator;
    public string shockTriggerName = "Shock";
    public float shockLimitDuration = 1f; // How long to wait before Game Over screen
    private bool isHandlingGameOver = false;

    private bool wasPausedLastFrame = false;
    private int skipFrames = 0;

    void Start()
    {
        // Load Settings
        float sensitivity = PlayerPrefs.GetFloat("JoystickSensitivity", 1f);
        verticalSpeed *= sensitivity;

        int difficulty = PlayerPrefs.GetInt("GameDifficulty", 1); // 0: Easy, 1: Normal, 2: Hard
        if (difficulty == 0) moveSpeed *= 0.8f;
        else if (difficulty == 2) moveSpeed *= 1.3f;

        if (loopSource != null)
        {
            // Play start sound once on SFX source if possible
            if (startClip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(startClip);
            }
            else if (startClip != null)
            {
                loopSource.PlayOneShot(startClip);
            }

            // Play loop sound
            if (loopClip != null)
            {
                loopSource.clip = loopClip;
                loopSource.loop = true;
                loopSource.PlayDelayed(startClip != null ? startClip.length : 0);
            }
        }
    }

    void Update()
    {
        // Check if game is paused or over
        bool isPaused = GameManager.Instance != null && (GameManager.Instance.IsPaused || GameManager.Instance.IsGameOver);
        
        if (isPaused)
        {
            wasPausedLastFrame = true;
            return;
        }

        // If we just unpaused, start the frame suppression
        if (wasPausedLastFrame)
        {
            skipFrames = 3; // Ignore input and movement for 3 frames to clear the "Resume" click
            wasPausedLastFrame = false;
        }

        // Countdown suppression frames
        if (skipFrames > 0)
        {
            skipFrames--;
            return;
        }

        // Continuous Movement to the Right
        transform.Translate(Vector2.right * moveSpeed * Time.deltaTime);

        // Vertical Movement (Joystick) - Only if not crashed
        float vInput = 0f;
        if (!isHandlingGameOver)
        {
            if (joystick != null)
            {
                vInput = joystick.Vertical;
            }
            else
            {
                // Fallback to keyboard for testing
                vInput = Input.GetAxis("Vertical");
            }
        }

        if (Mathf.Abs(vInput) > 0.1f)
        {
            transform.Translate(Vector2.up * vInput * verticalSpeed * Time.deltaTime, Space.World);
        }

        // Plane-like Rotation Logic
        float targetAngle = vInput * 30f; // Max tilt 30 degrees
        Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 5f);

        // Check Bounds (Game Over if off-screen top/bottom)
        CheckBounds();

        // Handle Gift Dropping
        if (!isHandlingGameOver && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)))
        {
            // Only drop if not clicking on an actual UI button
            // And ensure we aren't dragging the joystick (simple check)
            if (!IsPointerOverButton() && Mathf.Abs(vInput) < 0.2f)
            {
                DropGift();
            }
        }
    }

    private float groundTimer = 0f;
    public float maxGroundTime = 2f;

    private void CheckBounds()
    {
        if (Camera.main != null)
        {
            Vector3 viewportPos = Camera.main.WorldToViewportPoint(transform.position);
            
            // 1. Check Ceiling (Too High) -> Instant Game Over
            if (viewportPos.y > 1.1f)
            {
                Debug.Log("Santa went out of bounds (Too High)!");
                if (loopSource != null) loopSource.Stop(); // Stop loop audio
                GameOver();
            }
            // 2. Check Ground (Too Low) -> Timer Based
            // "if the santa touch on ground for more then 2s then gome over"
            // We assume "Ground" is near the bottom of the screen (e.g., < 0.1)
            else if (viewportPos.y < 0.1f)
            {
                groundTimer += Time.deltaTime;
                if (groundTimer > maxGroundTime)
                {
                    Debug.Log("Santa stayed on ground too long!");
                    if (loopSource != null) loopSource.Stop(); // Stop loop audio
                    GameOver();
                }
            }
            else
            {
                // Reset timer if back in safe zone
                groundTimer = 0f;
            }
        }
    }
    
    // Detect collision with Obstacles (Trees/Houses)
    private void OnTriggerEnter2D(Collider2D other)
    {
       HandleCollision(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision.gameObject);
    }

    private void HandleCollision(GameObject hitObject)
    {
        if (isHandlingGameOver) return; // Prevent double triggering

        // DEBUG: Print everything we hit
        Debug.Log($"[COLLISION DEBUG] Santa hit: '{hitObject.name}' with Tag: '{hitObject.tag}'");
        
        // Ensure we aren't colliding with our own Gift (if it spawns closely)
        if (hitObject.CompareTag("Gift")) return;
        
        Debug.Log("Santa crashed into: " + hitObject.name);
        
        bool isDarkCloud = hitObject.GetComponent<PeriodicAnimator>() != null || hitObject.name.Contains("Dark");

        if (isDarkCloud)
        {
            if (loopSource != null) loopSource.Stop(); // Stop loop audio immediately
            
            // Play shock sound locally for best control
            if (sfxSource != null)
            {
                if (shockClip != null)
                {
                    sfxSource.PlayOneShot(shockClip);
                    Debug.Log("Santa: Playing shock clip locally.");
                }
                else
                {
                    Debug.LogWarning("Santa: shockClip is missing in the Inspector!");
                }
            }
            else if (AudioManager.Instance != null && shockClip != null)
            {
                // Fallback to global if local source missing
                AudioManager.Instance.PlaySFX(shockClip);
            }
            
            StartCoroutine(PlayShockAndGameOver());
        }
        else
        {
            // Optional: Play crash sound even if not deadly? Or only if deadly?
            // If it's NOT a dark cloud and NOT deadly, we might not want to stop audio.
            // But usually collision implies a stop. 
            // The user said: "if the santa touch other collition then dont do game over"
            // So we don't stop audio here if it's not a dark cloud.
            
            Debug.Log("Santa hit non-deadly obstacle: " + hitObject.name + " - Ignoring.");
        }
    }

    private System.Collections.IEnumerator PlayShockAndGameOver()
    {
        isHandlingGameOver = true;
        
        // 1. Play Animation
        if (santaAnimator != null)
        {
            santaAnimator.SetTrigger(shockTriggerName);
        }

        // 2. Disable Input (handled in Update) but KEEP MOVING forward
        // this.enabled = false; <--- REMOVED to allow motion

        // 3. Wait for animation to finish
        yield return new WaitForSeconds(shockLimitDuration);

        // 4. Trigger actual Game Over
        GameOver();
    }

    private void GameOver()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
    }

    private bool IsPointerOverButton()
    {
        if (EventSystem.current == null) return false;

        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        if (Input.touchCount > 0) eventData.position = Input.GetTouch(0).position;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            // Check if the object we hit (or its parents) has a Button component
            // Also ignore the Joystick area if needed, but usually Joystick handles its own events
            if (result.gameObject.GetComponentInParent<Button>() != null || result.gameObject.GetComponentInParent<SimpleJoystick>() != null)
            {
                return true;
            }
        }

        return false;
    }

    void DropGift()
    {
        if (giftPrefab != null && dropPoint != null)
        {
            GameObject gift = Instantiate(giftPrefab, dropPoint.position, Quaternion.identity);
            
            // Randomize size between 0.2 and 0.3 (based on previous update)
            float randomScale = Random.Range(0.2f, 0.3f);
            gift.transform.localScale = new Vector3(randomScale, randomScale, 1f);

            // Play drop sound
            if (sfxSource != null && dropClip != null)
            {
                sfxSource.PlayOneShot(dropClip);
            }
            else if (AudioManager.Instance != null && dropClip != null)
            {
                AudioManager.Instance.PlaySFX(dropClip);
            }
        }
    }

    public void ResetState()
    {
        isHandlingGameOver = false;
        this.enabled = true; // Ensure Update loop runs
        
        // Reset rotation if stuck in shock
        transform.rotation = Quaternion.identity;
        
        // Optionally reset Animator to Idle loop if needed
        // Reset audio if it was stopped
        if (loopSource != null && loopClip != null)
        {
            loopSource.clip = loopClip;
            loopSource.loop = true;
            loopSource.Play();
        }
        
        Debug.Log("Santa State Reset!");
    }

    public void PlayCollectSound(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
}
