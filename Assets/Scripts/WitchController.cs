using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class WitchController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float verticalSpeed = 5f;
    public SimpleJoystick joystick;

    private float originalMoveSpeed;

    [Header("Gift Settings")]
    public GameObject giftPrefab;
    public Transform dropPoint;
    [Range(0.01f, 2.0f)] public float minGiftScale = 0.2f;
    [Range(0.01f, 2.0f)] public float maxGiftScale = 0.3f;

    [Header("Audio Settings")]
    public AudioSource loopSource; // Dedicated for loop
    public AudioSource sfxSource;  // Dedicated for one-shot SFX
    public AudioClip startClip;
    public AudioClip loopClip;
    public AudioClip dropClip;
    public AudioClip crashClip; // New sound for hitting obstacles
    public AudioClip shockClip; // New sound for hitting dark clouds

    [Header("Animation Settings")]
    public Animator witchAnimator;
    public string shockTriggerName = "Shock";
    public float shockLimitDuration = 1f; // How long to wait before Game Over screen
    private bool isHandlingGameOver = false;
    private float baseVerticalSpeed;

    private bool wasPausedLastFrame = false;
    private int skipFrames = 0;

    void Start()
    {
        baseVerticalSpeed = verticalSpeed;
        originalMoveSpeed = moveSpeed;

        // Load sensitivity from PlayerPrefs
        float sensitivity = PlayerPrefs.GetFloat("Sensitivity", 1.0f);
        verticalSpeed = baseVerticalSpeed * sensitivity;

        // Sync audio volumes with AudioManager instead of loading directly from PlayerPrefs
        SyncAudioVolumes();

        Debug.Log($"[WitchController] Loaded - Sensitivity: {sensitivity}");

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

    // Sync audio source volumes with AudioManager
    void SyncAudioVolumes()
    {
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.7f);
        
        if (loopSource != null)
        {
            loopSource.volume = sfxVolume;
        }
        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume;
        }
        
        Debug.Log($"[WitchController] Synced audio volumes - SFX Volume: {sfxVolume}");
    }

    private float audioSyncTimer = 0f;
    private const float AUDIO_SYNC_INTERVAL = 0.5f; // Check every 0.5 seconds

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

        // Periodically sync audio volumes with current settings
        audioSyncTimer += Time.deltaTime;
        if (audioSyncTimer >= AUDIO_SYNC_INTERVAL)
        {
            SyncAudioVolumes();
            audioSyncTimer = 0f;
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

        // Tilt Rotation Logic
        float targetAngle = vInput * 30f; // Max tilt 30 degrees
        Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 5f);

        // Check Bounds (Game Over if off-screen top/bottom)
        CheckBounds();

        // Check for Mouse Click (Left Click)
        if (!isHandlingGameOver)
        {
            if (Input.GetMouseButtonDown(0))
            {
                // For mouse, ensure click is on the right half of the screen
                if (Input.mousePosition.x > Screen.width / 2 && !IsPointerOverButton() && Mathf.Abs(vInput) < 0.2f)
                {
                    DropItem();
                }
            }
            // Check for Space Key (Testing)
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                if (Mathf.Abs(vInput) < 0.2f)
                {
                    DropItem();
                }
            }
            // Check for Touch Input
            else if (Input.touchCount > 0)
            {
                foreach (Touch touch in Input.touches)
                {
                    if (touch.phase == TouchPhase.Began)
                    {
                        if (touch.position.x > Screen.width / 2 && !IsPointerOverButton() && Mathf.Abs(vInput) < 0.2f)
                        {
                            DropItem();
                            break;
                        }
                    }
                }
            }
        }
    }

    private float outOfBoundsTimer = 0f;
    public float maxOutOfBoundsTime = 2f;

    private void CheckBounds()
    {
        if (Camera.main != null)
        {
            Vector3 viewportPos = Camera.main.WorldToViewportPoint(transform.position);
            
            // Check if Witch is outside the vertical viewport (too high or too low)
            bool isOffScreen = viewportPos.y > 1.05f || viewportPos.y < -0.05f;

            if (isOffScreen)
            {
                outOfBoundsTimer += Time.deltaTime;
                if (outOfBoundsTimer > maxOutOfBoundsTime)
                {
                    Debug.Log($"[DIAGNOSTICS] Witch stayed out of viewport (Y: {viewportPos.y}) for > {maxOutOfBoundsTime}s! Game Over.");
                    if (loopSource != null) loopSource.Stop();
                    GameOver();
                }
            }
            else
            {
                // Reset timer if back in safe zone
                outOfBoundsTimer = 0f;
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

        string tag = hitObject.tag;
        string name = hitObject.name;

        // Determine behavior based on object type
        bool isDarkCloud = hitObject.GetComponent<PeriodicAnimator>() != null || name.Contains("Dark");

        if (isDarkCloud)
        {
            Debug.Log("[DIAGNOSTICS] Witch hit Dark Cloud - Starting Shock Animation.");
            if (loopSource != null) loopSource.Stop();
            
            if (sfxSource != null && shockClip != null)
            {
                sfxSource.PlayOneShot(shockClip);
            }
            
            StartCoroutine(PlayShockAndGameOver());
            return; // Exit after triggering
        }

        // Filter out non-deadly/collectible items
        if (tag == "Gift" || tag == "Coin" || name.Contains("Coin") || tag == "Ground" || tag == "Tree" || tag == "House" || tag == "Chimney" || tag == "Obstacle" || name.Contains("Cloud") || tag == "Cloud")
        {
            // Note: Regular clouds are also ignored here for Witch, same as Santa.
            return; 
        }

        Debug.Log($"[COLLISION] Witch hit: '{name}' (Tag: {tag})");
    }

    private System.Collections.IEnumerator PlayShockAndGameOver()
    {
        isHandlingGameOver = true;
        
        if (witchAnimator != null)
        {
            witchAnimator.SetTrigger(shockTriggerName);
        }

        yield return new WaitForSeconds(shockLimitDuration);

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
            if (result.gameObject.GetComponentInParent<Button>() != null || result.gameObject.GetComponentInParent<SimpleJoystick>() != null)
            {
                return true;
            }
        }

        return false;
    }

    void DropItem()
    {
        if (giftPrefab != null && dropPoint != null)
        {
            GameObject item = Instantiate(giftPrefab, dropPoint.position, Quaternion.identity);
            
            // Randomize size based on settings
            float randomScale = Random.Range(minGiftScale, maxGiftScale);
            item.transform.localScale = new Vector3(randomScale, randomScale, 1f);

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
        
        transform.rotation = Quaternion.identity;
        
        SyncAudioVolumes();
        
        if (loopSource != null && loopClip != null)
        {
            loopSource.clip = loopClip;
            loopSource.loop = true;
            loopSource.Play();
        }
        
        Debug.Log("Witch State Reset!");
    }

    public void PlayCollectSound(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        moveSpeed = originalMoveSpeed * multiplier;
        Debug.Log($"[WitchController] Speed updated to {moveSpeed} (Multiplier: {multiplier})");
    }
}
