using UnityEngine;

public class SellShopTrigger : MonoBehaviour
{
    public SellShop sellShop;
    
    public void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        sellShop.OpenShop();
    }

    public void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        sellShop.CloseShop();
    }
}