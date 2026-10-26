using System.IO;
using UnityEditor;
using UnityEngine;

public static class WebGLBuilder
{
    public static void Build()
    {
        string[] scenes =
        {
            "Assets/Scenes/Main Menu.unity",
            "Assets/Scenes/Tutorial.unity",
            // El Cyber Grind (escena "Endless") es una escena del player, no de
            // los bundles. Sin esto, OptionsManager.ChangeLevel("Endless") cae en
            // CanStreamedLevelBeLoaded == false y vuelve al menu principal.
            "Assets/Scenes/Endless.unity"
        };

        PlayerSettings.WebGL.template = "APPLICATION:Default";
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.memorySize = 512;
        // El dialogo de error de Unity mostrara la excepcion y la pila en vez
        // de "exception handling has been disabled in this build".
        PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.FullWithStacktrace;
        PlayerSettings.runInBackground = true;
        PlayerSettings.defaultWebScreenWidth = 1280;
        PlayerSettings.defaultWebScreenHeight = 720;

        Directory.CreateDirectory("Builds/WebGL");

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = "Builds/WebGL",
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };

        var report = BuildPipeline.BuildPlayer(options);
        var s = report.summary;
        Debug.Log("BUILD RESULT: " + s.result + " | errors=" + s.totalErrors +
                  " | warnings=" + s.totalWarnings + " | sizeMB=" + (s.totalSize / 1048576.0).ToString("F1"));

        if (s.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            try
            {
                string customPage = Path.Combine("Assets/Editor", "WebGLTemplate.html");
                if (File.Exists(customPage))
                {
                    File.Copy(customPage, Path.Combine("Builds/WebGL", "index.html"), true);
                    Debug.Log("Pagina WebGL personalizada aplicada (pantalla completa + Esc).");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("No se pudo aplicar WebGLTemplate.html: " + e.Message);
            }
        }
        else
        {
            EditorApplication.Exit(1);
        }
    }
}
