using UnityEngine;

public class SlimeCapture : MonoBehaviour
{
    bool captured = false;

    public string slimeType = "WaterSlime"; // ตั้งใน Inspector ของแต่ละ prefab

    public void Capture(PlayerInventory inventory)
    {
        if (captured) return;
        captured = true;
        inventory.AddSlime(gameObject, slimeType);
    }
}