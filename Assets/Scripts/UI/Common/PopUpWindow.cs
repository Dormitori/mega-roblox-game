using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class PopUpWindow : MonoBehaviour
{
    public Button closeButton;
    
    public float animationYOffset = 50;
    public float animationDuration = 0.2f;
    
    public InputControls characterControls;
    
    private CanvasGroup _canvasGroup;

    private Vector3 _initialPos;
    private Vector3 _offsetPos;
    
    public virtual void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _initialPos = _canvasGroup.transform.localPosition;
        
        _offsetPos = _canvasGroup.transform.localPosition;
        _offsetPos.y -= animationYOffset;
        
        closeButton.onClick.AddListener(HideWindowImmediately);
        
        HideWindowImmediately();
    }

    public virtual void OnWindowShow()
    {
        characterControls.CanMine = false;
        characterControls.CanCameraScroll = false;
    }

    public virtual void OnWindowHide()
    {
        characterControls.CanMine = true;
        characterControls.CanCameraScroll = true;
    }

    public void ShowWindowAnimated()
    {
        OnWindowShow();
        _canvasGroup.DOFade(1f, animationDuration);
        _canvasGroup.transform.localPosition = _offsetPos;
        _canvasGroup.transform.DOLocalMove(_initialPos, animationDuration).OnComplete(
            () => {_canvasGroup.blocksRaycasts = true;}
            );
    }
    
    public void HideWindowAnimated()
    {
        OnWindowHide();
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.DOFade(0f, animationDuration);
        _canvasGroup.transform.localPosition = _initialPos;
        _canvasGroup.transform.DOLocalMove(_offsetPos, animationDuration);
    }

    public void ShowWindowImmediately()
    {
        OnWindowShow();
        _canvasGroup.blocksRaycasts = true;
        _canvasGroup.alpha = 1;
    }
    
    public void HideWindowImmediately()
    {
        OnWindowHide();
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.alpha = 0;
    }
    
}