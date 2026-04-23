using System;
using System.Collections.Generic;
using Core.Audio;
using DG.Tweening;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class TutorialView : MonoBehaviour
{
    public TextMeshProUGUI objectiveText;
    public TutorialManager tutorialManager;
    public CanvasGroup canvasGroup;
    public Image inventoryPointingImage;

    private readonly Queue<Action> _animationQueue = new();
    private bool _isPlayingAnimation = false;
    
    private IAudioService _audioService;

    [Inject]
    private void Initialize(IAudioService audioService)
    {
        _audioService = audioService;
    }
    
    private void Start()
    {
        if (tutorialManager.CurrentObjective == null)
        {
            Destroy(gameObject);
            return;
        }

        tutorialManager.ObjectiveComplete += OnObjectiveComplete;
        tutorialManager.ObjectiveUpdate += UpdateObjectiveText;
        tutorialManager.TutorialCompleted += OnTutorialCompleted;

        UpdateObjectiveText();
    }

    private void OnDestroy()
    {
        if (tutorialManager != null)
        {
            tutorialManager.ObjectiveComplete -= OnObjectiveComplete;
            tutorialManager.ObjectiveUpdate -= UpdateObjectiveText;
            tutorialManager.TutorialCompleted -= OnTutorialCompleted;
        }
    }

    private void OnObjectiveComplete()
    {
        if (tutorialManager.CurrentObjective is InventoryUpgradeObjective)
            inventoryPointingImage.DOFade(1f, 0.5f);
        else
            inventoryPointingImage.DOFade(0f, 0.5f);
        
        var nextObjectiveText = tutorialManager.CurrentObjective.GetObjectiveText();
        EnqueueAnimation(() => PlayObjectiveComplete(nextObjectiveText));
    }


    private void OnTutorialCompleted()
    {
        EnqueueAnimation(PlayTutorialCompleted);
    }

    private void EnqueueAnimation(Action animationAction)
    {
        _animationQueue.Enqueue(animationAction);
        TryPlayNext();
    }

    private void TryPlayNext()
    {
        if (_isPlayingAnimation || _animationQueue.Count == 0)
            return;

        _isPlayingAnimation = true;
        var nextAnimation = _animationQueue.Dequeue();
        nextAnimation.Invoke();
    }

    private void AnimationFinished()
    {
        _isPlayingAnimation = false;
        TryPlayNext();
    }

    private void PlayObjectiveComplete(string nextObjectiveText)
    {
        var originalPos = objectiveText.transform.localPosition;

        var seq = DOTween.Sequence();
        seq.Append(objectiveText.DOColor(Color.green, 0.3f).SetEase(Ease.Linear));
        seq.AppendCallback(() => _audioService.PlaySfx(SoundId.TutorialObjectiveComplete));
        seq.Append(objectiveText.transform.DOLocalMoveX(originalPos.x + 200f, 1f));
        seq.Join(objectiveText.DOFade(0f, 1f));

        seq.AppendCallback(() =>
        {
            objectiveText.text = nextObjectiveText;
            objectiveText.color = Color.white;

            var color = objectiveText.color;
            color.a = 0f;
            objectiveText.color = color;

            var pos = originalPos;
            pos.x -= 100f;
            objectiveText.transform.localPosition = pos;
        });

        seq.AppendInterval(0.2f);
        seq.Append(objectiveText.transform.DOLocalMoveX(originalPos.x, 1f));
        seq.Join(objectiveText.DOFade(1f, 1f));

        seq.OnComplete(AnimationFinished);
    }

    private void PlayTutorialCompleted()
    {
        var originalPos = objectiveText.transform.localPosition;

        var seq = DOTween.Sequence();
        seq.Append(objectiveText.DOColor(Color.green, 0.3f).SetEase(Ease.Linear));
        seq.AppendCallback(() => _audioService.PlaySfx(SoundId.TutorialObjectiveComplete));
        seq.Append(objectiveText.transform.DOLocalMoveX(originalPos.x + 200f, 1f));
        seq.Join(objectiveText.DOFade(0f, 1f));

        seq.AppendCallback(() =>
        {
            objectiveText.text = LocalizationManager.GetTranslation("Tutorial/TutorialComplete");
            objectiveText.color = Color.white;

            var color = objectiveText.color;
            color.a = 0f;
            objectiveText.color = color;

            var pos = originalPos;
            pos.x -= 100f;
            objectiveText.transform.localPosition = pos;
        });

        seq.AppendInterval(0.2f);
        seq.Append(objectiveText.transform.DOLocalMoveX(originalPos.x, 1f));
        seq.Join(objectiveText.DOFade(1f, 1f));
        seq.AppendInterval(2f);
        seq.Append(canvasGroup.DOFade(0f, 1f));
        seq.AppendCallback(() => Destroy(gameObject));
    }

    private void UpdateObjectiveText()
    {
        objectiveText.text = tutorialManager.CurrentObjective.GetObjectiveText();
    }
}