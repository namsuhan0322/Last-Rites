using UnityEngine;

public class ShowShopUI : MonoBehaviour
{
    public GameObject shop;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            shop.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            shop.SetActive(false);
        }
    }
}
