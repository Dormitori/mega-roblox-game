using UnityEngine;
using UnityEngine.UI;

public class ReturnButton : MonoBehaviour
{
    public Transform playerTransform;
    public Transform respawnTransform;
    public CharacterController characterController;
    
    
    private Button _button;
    
    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnClick);
        HideButton();
    }

    public void ShowButton()
    {
        gameObject.SetActive(true);
    }

    public void HideButton()
    {
        gameObject.SetActive(false);
    }

    private void OnClick()
    {
        characterController.enabled = false;
        playerTransform.transform.position = respawnTransform.position;
        playerTransform.transform.rotation = respawnTransform.rotation;
        characterController.enabled = true;
        HideButton();
    }
}