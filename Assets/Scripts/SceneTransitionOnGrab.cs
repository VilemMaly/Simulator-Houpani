using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class SceneTransitionOnGrab : MonoBehaviour
{
    [SerializeField] private string sceneName = "";

    public void LoadTargetScene()
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            Debug.Log("Loading" + sceneName);
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("Scene name is not set on SceneTransitionOnGrab script.");
        }
    }

    // For XRI events
    public void OnSelectEntered(BaseInteractionEventArgs args)
    {
        LoadTargetScene();
    }
}
