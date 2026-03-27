using UnityEngine;
using TMPro;

public class PlayerInteract : MonoBehaviour
{
    public float interactDistance = 3f;
    public TextMeshProUGUI interactText;

    public Camera cam;

    void Update()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            if (hit.collider.CompareTag("Phone"))
            {
                interactText.text = "กด E เพื่อรับสาย";

                if (Input.GetKeyDown(KeyCode.E))
                {
                    hit.collider.GetComponent<PhoneSystem>().AnswerCall();
                }
            }
            else
            {
                interactText.text = "";
            }
        }
        else
        {
            interactText.text = "";
        }
    }
}