using UnityEngine;

public class PopupWindowsCloseTrigger : MonoBehaviour
{
    public PopUpWindow popUpWindow;

    public void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        popUpWindow.HideWindowAnimated();
    }
}