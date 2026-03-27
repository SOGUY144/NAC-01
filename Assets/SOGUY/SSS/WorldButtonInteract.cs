using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
[RequireComponent(typeof(BoxCollider))]
public class WorldButtonInteract : MonoBehaviour
{
    [Header("Hover Settings")]
    [Tooltip("The color the button changes to when looked at.")]
    public Color hoverColor = Color.gray;

    private Color normalColor;
    private Image buttonImage;
    private Button uiButton;

    void Awake()
    {
        buttonImage = GetComponent<Image>();
        uiButton = GetComponent<Button>();

        if (buttonImage != null)
        {
            normalColor = buttonImage.color;
        }
    }

    /// <summary>
    /// Called when the player's raycast hits this button.
    /// </summary>
    public void OnHoverEnter()
    {
        if (buttonImage != null)
        {
            buttonImage.color = hoverColor;
        }
    }

    /// <summary>
    /// Called when the player's raycast stops hitting this button.
    /// </summary>
    public void OnHoverExit()
    {
        if (buttonImage != null)
        {
            buttonImage.color = normalColor;
        }
    }

    /// <summary>
    /// Called when the player clicks the interact button while looking at this button.
    /// </summary>
    public void OnInteract()
    {
        if (uiButton != null)
        {
            uiButton.onClick.Invoke(); // Programmatically trigger the standard UI button event
        }
    }
}
