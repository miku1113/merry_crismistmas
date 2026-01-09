using UnityEngine;

/// <summary>
/// RavanController is a variant of SantaController.
/// It uses the same movement logic but drops WaterBalls.
/// </summary>
public class RavanController : SantaController
{
    // We can mostly rely on SantaController's logic.
    // However, we need to ensure the drop logic uses the WaterBall prefab.
    // Since SantaController.DropGift is not virtual, we might need to hide it or 
    // simply let the user assign the WaterBall prefab to the giftPrefab slot in the inspector.
    
    // To make it cleaner for the user, we can add a specific header.
    [Header("Ravan Specifics")]
    public string ravanName = "Ravan";

    // If we want to change any specific behavior (like speed), we can override Start or Update.
    // For now, the user said "script will remain same but instead of gift it will drop water ball".
    // This can be achieved by just using the SantaController script on the Ravan object 
    // and swapping the prefab.
    
    // HOWEVER, to keep them distinct in code and allow for future differences, 
    // having a separate class is better.
}
