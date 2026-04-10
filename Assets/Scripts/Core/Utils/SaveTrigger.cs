using UnityEngine;
using System;
using System.Collections;
using UnityEditor;

public class SaveTrigger : MonoBehaviour
{
    public int autoSaveInterval;
    public static event Action OnSave;

    private void Awake()
    {
        RegisterEditorExitHook();
        StartCoroutine(AutoSave());
    }

    private IEnumerator AutoSave()
    {
        var wait = new WaitForSeconds(autoSaveInterval);
        while (true)
        {
            yield return wait;
            Debug.Log("AutoSaving...");
            OnSave?.Invoke();
        }
    }


    private void OnApplicationQuit()
    {
        TriggerSave();
    }

    private void RegisterEditorExitHook()
    {
#if UNITY_EDITOR
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
    }

#if UNITY_EDITOR
    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            TriggerSave();
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }
    }
#endif

    private void TriggerSave()
    {
        Debug.Log("Save on exit!!");
        OnSave?.Invoke();
    }
}