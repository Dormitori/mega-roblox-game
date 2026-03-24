using UnityEngine;

public class ShowReturnButton : MonoBehaviour
{
    public ReturnButton returnButton;
    
    public void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        returnButton.ShowButton();
    }
}