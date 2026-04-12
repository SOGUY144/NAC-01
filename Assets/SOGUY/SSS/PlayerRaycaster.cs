using UnityEngine;

public class PlayerRaycaster : MonoBehaviour
{
    [Header("Raycast Settings")]
    [Tooltip("How far the player can reach out to interact with UI.")]
    public float interactionDistance = 5f;
    [Tooltip("The layers that UI buttons are on.")]
    public LayerMask uiLayerMask;

    [Header("Input Settings")]
    [Tooltip("The input key to trigger the interaction (e.g., Left Mouse Button).")]
    public KeyCode interactKey = KeyCode.Mouse0;

    private Camera mainCamera;
    private WorldButtonInteract currentTarget;
    private RadioInteract currentRadio;

    void Start()
    {
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
        {
            Debug.LogError("PlayerRaycaster needs to be attached to a Camera.");
        }
    }

    void Update()
    {
        PerformRaycast();
        HandleInput();
    }

    private void PerformRaycast()
    {
        if (mainCamera == null) return;

        // Create a ray from the center of the screen
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        // Perform the raycast
        if (Physics.Raycast(ray, out hit, interactionDistance, uiLayerMask))
        {
            // Try to get the WorldButtonInteract script from the hit object
            WorldButtonInteract hitButton = hit.collider.GetComponent<WorldButtonInteract>();
            RadioInteract hitRadio = hit.collider.GetComponent<RadioInteract>();

            if (hitButton != null)
            {
                // If we're looking at a NEW button
                if (currentTarget != hitButton)
                {
                    // Reset the previous target if it exists
                    ClearTarget();

                    currentTarget = hitButton;
                    currentTarget.OnHoverEnter(); // Hover on the new target
                }
            }
            else if (hitRadio != null)
            {
                // ถ้าหันไปโฟกัสโดนวิทยุ!
                if (currentRadio != hitRadio)
                {
                    ClearTarget();
                    
                    currentRadio = hitRadio;
                    currentRadio.ShowPrompt(); // โชว์ป้ายตัว E
                }
            }
            else
            {
                // We hit something on the UI layer, but it's not interactive
                ClearTarget();
            }
        }
        else
        {
            // The raycast hit nothing
            ClearTarget();
        }
    }

    private void HandleInput()
    {
        // Check if the player pressed the interact key and we have a valid target
        if (Input.GetKeyDown(interactKey))
        {
            if (currentTarget != null) currentTarget.OnInteract();
            if (currentRadio != null) currentRadio.TriggerInteraction();
        }
    }

    private void ClearTarget()
    {
        if (currentTarget != null)
        {
            currentTarget.OnHoverExit();
            currentTarget = null;
        }

        if (currentRadio != null)
        {
            currentRadio.HidePrompt();
            currentRadio = null;
        }
    }
}