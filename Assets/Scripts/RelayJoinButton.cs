using UnityEngine;

public class RelayJoinButton : MonoBehaviour
{
    public RelayVRMenu menu;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("JOIN BUTTON TRIGGER: " + other.name);

        if (!other.CompareTag(menu.handTag))
            return;

        menu.PressJoin();
    }
}