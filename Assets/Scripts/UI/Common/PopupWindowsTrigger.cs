using UnityEngine;

public class PopupWindowsTrigger : MonoBehaviour
{
    public PopUpWindow popUpWindow;
    
    public void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        popUpWindow.ShowWindowAnimated();
    }

    public void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        popUpWindow.HideWindowAnimated();
    }
}