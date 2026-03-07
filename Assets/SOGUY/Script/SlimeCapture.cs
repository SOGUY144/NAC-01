using UnityEngine;

public class SlimeCapture : MonoBehaviour
{
    bool captured = false;

    public void Capture(PlayerInventory inventory)
    {
        if (captured) return;

        captured = true;

        inventory.AddSlime(gameObject);

        Destroy(gameObject);
    }
}