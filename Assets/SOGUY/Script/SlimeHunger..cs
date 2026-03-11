using UnityEngine;

public class SlimeHunger : MonoBehaviour
{
    [Header("Hunger Settings")]
    public float maxHunger = 100f;
    public float currentHunger = 100f;

    [Header("Hunger Decay")]
    public float hungerDecreaseRate = 3f;

    void Update()
    {
        DecreaseHunger();
    }

    void DecreaseHunger()
    {
        currentHunger -= hungerDecreaseRate * Time.deltaTime;

        currentHunger = Mathf.Clamp(currentHunger, 0, maxHunger);
    }

    public void Eat(int foodValue)
    {
        currentHunger += foodValue;

        currentHunger = Mathf.Clamp(currentHunger, 0, maxHunger);

        Debug.Log("Slime ate food. Hunger: " + currentHunger);
    }
}