using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class SantaController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    [Header("Gift Settings")]
    public GameObject giftPrefab;
    public Transform dropPoint;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip startClip;
    public AudioClip loopClip;
    public AudioClip dropClip;

    private bool wasPausedLastFrame = false;
    private int skipFrames = 0;

    void Start()
    {
        if (audioSource != null)
        {
            // Play start sound once
            if (startClip != null)
            {
                audioSource.PlayOneShot(startClip);
            }

            // Play loop sound
            if (loopClip != null)
            {
                audioSource.clip = loopClip;
                audioSource.loop = true;
                audioSource.PlayDelayed(startClip != null ? startClip.length : 0);
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

        // Handle Gift Dropping
        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            // Only drop if not clicking on an actual UI button
            if (!IsPointerOverButton())
            {
                DropGift();
            }
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
            if (result.gameObject.GetComponentInParent<Button>() != null)
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
            if (AudioManager.Instance != null && dropClip != null)
            {
                AudioManager.Instance.PlaySFX(dropClip);
            }
        }
    }
}
