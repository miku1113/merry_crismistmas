using UnityEngine;

[CreateAssetMenu(fileName = "Level_", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Level Info")]
    public string levelName = "Level 1";
    public int levelNumber = 1;
    public Sprite levelPreviewImage;
    
    [Header("Scene Configuration")]
    public string sceneName = "SampleScene";
    public bool isLocked = false;
    public int price = 0;
    public bool isFree = false;
    public Sprite playSprite;
    public Sprite buySprite;
    
    [Header("Gameplay Modifiers")]
    [Tooltip("Multiplier for overall difficulty (affects spawn rates, speed, etc.)")]
    [Range(0.5f, 3.0f)]
    public float difficultyMultiplier = 1.0f;
    
    [Tooltip("Multiplier for Santa's move speed")]
    [Range(0.8f, 2.0f)]
    public float speedMultiplier = 1.0f;
    
    [Tooltip("Multiplier for obstacle spawn rate")]
    [Range(0.5f, 2.5f)]
    public float spawnRateMultiplier = 1.0f;
}
