using UnityEngine;

public class SellShopTrigger : MonoBehaviour
{
    public SellShop sellShop;
    
    public void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Player") return;
        sellShop.gameObject.SetActive(true);
        sellShop.ShowEntries();
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.tag != "Player") return;
        sellShop.CloseShop();
        sellShop.gameObject.SetActive(false);
    }
}