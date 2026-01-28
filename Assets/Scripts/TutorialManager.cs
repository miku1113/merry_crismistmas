using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("UI Panels")]
    [Header("UI Panels")]
    public GameObject joystickTutorialPanel;
    public GameObject dropTutorialPanel;
    // Dark Cloud Panel removed as requested

    [Header("Settings")]
    public SimpleJoystick joystick;
    public float stepDelay = 2.0f; // Manual delay before Drop Tutorial

    private bool isTutorialActive = false;
    private int currentStep = 0; // 0: None, 1: Joystick, 2: Waiting Delay, 3: Drop Input

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Ensure panels are hidden at start
        if (joystickTutorialPanel != null) joystickTutorialPanel.SetActive(false);
        if (dropTutorialPanel != null) dropTutorialPanel.SetActive(false);
    }

    public void StartTutorial()
    {
        isTutorialActive = true;
        currentStep = 1;

        // Step 1: Joystick - Start Immediately
        StartCoroutine(ShowJoystickTutorial());
    }

    private IEnumerator ShowJoystickTutorial()
    {
        yield return null; 

        if (joystickTutorialPanel != null) joystickTutorialPanel.SetActive(true);
        Time.timeScale = 0; // Pause game
    }

    private void CompleteJoystickStep()
    {
        if (joystickTutorialPanel != null) joystickTutorialPanel.SetActive(false);
        Time.timeScale = 1; // Resume game
        currentStep = 2; // Waiting Delay
        
        // Start Timer for Drop Tutorial
        StartCoroutine(WaitAndTriggerDrop());
    }

    private IEnumerator WaitAndTriggerDrop()
    {
        // Wait for manual delay set in Inspector
        yield return new WaitForSeconds(stepDelay); 
        
        TriggerDropTutorial();
    }

    private void TriggerDropTutorial()
    {
         currentStep = 3; // In Drop Input Step
         if (dropTutorialPanel != null) dropTutorialPanel.SetActive(true);
         Time.timeScale = 0; // Pause game
    }

    private void CompleteDropStep()
    {
        if (dropTutorialPanel != null) dropTutorialPanel.SetActive(false);
        Time.timeScale = 1; // Resume game
        currentStep = 0;
        isTutorialActive = false;
        
        // Mark Tutorial as Complete
        PlayerPrefs.SetInt("TutorialComplete", 1);
        PlayerPrefs.Save();
        
        Debug.Log("Tutorial Completed!");
    }

    void Update()
    {
        if (!isTutorialActive) return;

        if (currentStep == 1) // Joystick Step
        {
            if (joystick != null && Mathf.Abs(joystick.Vertical) > 0.5f)
            {
                CompleteJoystickStep();
            }
            else if (Mathf.Abs(Input.GetAxis("Vertical")) > 0.5f)
            {
                CompleteJoystickStep();
            }
        }
        // Step 2 is purely waiting on Coroutine, no Update logic needed
        else if (currentStep == 3) // In Drop Tutorial (Paused)
        {
            // Check for Right Side touch or Click
            if (Input.GetMouseButtonDown(0))
            {
                if (Input.mousePosition.x > Screen.width / 2)
                {
                    CompleteDropStep();
                }
            }
            else if (Input.touchCount > 0)
            {
                 foreach (Touch touch in Input.touches)
                {
                    if (touch.phase == TouchPhase.Began && touch.position.x > Screen.width / 2)
                    {
                        CompleteDropStep();
                        break;
                    }
                }
            }
            // Keyboard fallback
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                CompleteDropStep();
            }
        }
    }
}
