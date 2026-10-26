using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class DoorDiag
{
    private static double startTime;
    private static bool running;

    public static void Run()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Level 4-1.unity");
        startTime = EditorApplication.timeSinceStartup;
        running = true;
        EditorApplication.update += Tick;
        EditorApplication.isPlaying = true;
    }

    private static void Tick()
    {
        if (!running)
        {
            return;
        }
        if (EditorApplication.timeSinceStartup - startTime < 12.0)
        {
            return;
        }
        running = false;
        EditorApplication.update -= Tick;
        EditorApplication.isPlaying = false;
        EditorApplication.Exit(0);
    }
}
