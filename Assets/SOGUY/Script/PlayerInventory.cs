using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Slime Counts")]
    public int waterSlime = 0;
    public int fireSlime = 0;
    public int greenSlime = 0;
    public int maxSlime = 10;

    [Header("Food")]
    public int banana = 0;
    public int maxFood = 10;

    [Header("Plort")]
    public int plort = 0;
    public int maxPlort = 20;

    public int TotalSlimes() => waterSlime + fireSlime + greenSlime;

    public void AddSlime(GameObject slime, string slimeType)
    {
        if (TotalSlimes() >= maxSlime)
        {
            Debug.Log("Slime Inventory เต็ม!");
            return;
        }

        if (slimeType == "WaterSlime") waterSlime++;
        else if (slimeType == "FireSlime") fireSlime++;
        else if (slimeType == "GreenSlime") greenSlime++;

        Debug.Log($"Captured {slimeType}! Total: {TotalSlimes()}");
        Destroy(slime);
    }

    public void AddBanana()
    {
        if (banana >= maxFood)
        {
            Debug.Log("Food Inventory เต็ม!");
            return;
        }
        banana++;
        Debug.Log($"Got Banana! Total: {banana}");
    }

    public void AddPlort()
    {
        if (plort >= maxPlort)
        {
            Debug.Log("Plort Inventory เต็ม!");
            return;
        }
        plort++;
        Debug.Log($"Got Plort! Total: {plort}");
    }

    public bool RemoveItem(string type)
    {
        if (type == "WaterSlime" && waterSlime > 0) { waterSlime--; return true; }
        if (type == "FireSlime" && fireSlime > 0) { fireSlime--; return true; }
        if (type == "GreenSlime" && greenSlime > 0) { greenSlime--; return true; }
        if (type == "Banana" && banana > 0) { banana--; return true; }
        if (type == "Plort" && plort > 0) { plort--; return true; }
        return false;
    }
}