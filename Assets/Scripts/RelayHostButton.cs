using UnityEngine;

public class RelayHostButton : MonoBehaviour
{
    public RelayVRMenu menu;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("HOST BUTTON TRIGGER: " + other.name);

        if (!other.CompareTag(menu.handTag))
            return;

        menu.PressHost();
    }
}