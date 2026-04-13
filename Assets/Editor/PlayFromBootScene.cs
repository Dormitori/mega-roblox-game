#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class PlayFromBootScene
{
    private const string MenuPath = "Tools/Прогресс/Play всегда с Boot сцены";
    private const string PrefKey = "PlayFromBootScene.Enabled";

    private static bool Enabled
    {
        get => EditorPrefs.GetBool(PrefKey, true);
        set => EditorPrefs.SetBool(PrefKey, value);
    }

    static PlayFromBootScene()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    [MenuItem(MenuPath, false, 50)]
    private static void Toggle()
    {
        Enabled = !Enabled;
        Debug.Log($"[PlayFromBootScene] Enabled = {Enabled}");
    }

    [MenuItem(MenuPath, true)]
    private static bool ToggleValidate()
    {
        Menu.SetChecked(MenuPath, Enabled);
        return true;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (!Enabled)
            return;

        if (state != PlayModeStateChange.ExitingEditMode)
            return;

        var scenes = EditorBuildSettings.scenes;
        if (scenes == null || scenes.Length == 0 || string.IsNullOrEmpty(scenes[0].path))
        {
            Debug.LogWarning("[PlayFromBootScene] В Build Settings нет сцен (или у 0 сцены пустой path).");
            return;
        }

        var bootPath = scenes[0].path;
        var activePath = SceneManager.GetActiveScene().path;

        if (activePath == bootPath)
            return;

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorApplication.isPlaying = false;
            return;
        }

        EditorSceneManager.OpenScene(bootPath, OpenSceneMode.Single);
    }
}
#endif

