using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Construye AssetBundles compatibles con WebGL a partir de las escenas exportadas.
/// Los bundles originales del juego son de StandaloneWindows64 y no se pueden cargar en WebGL.
/// Version parche 10b (GREED): incluye act-2 con los niveles 4-*.
/// </summary>
public static class WebGLBundleBuilder
{
    private const string ScenesDir = "Assets/Scenes";
    private const string OutputDir = "Builds/WebGLBundles";

    [MenuItem("ULTRAKILL Port/Bundles/Build Act 0 (Tutorial + Prelude)")]
    public static void BuildAct0()
    {
        BuildBundles(new Dictionary<string, string[]>
        {
            { "acts/act-0", FindScenes(n =>
                n == "Tutorial" ||
                n.StartsWith("Level 0-")) }
        });
    }

    [MenuItem("ULTRAKILL Port/Bundles/Build All Acts")]
    public static void BuildAll()
    {
        BuildBundles(new Dictionary<string, string[]>
        {
            // Reparto IDENTICO al de los bundles originales del parche 10b:
            //   act-0 = Tutorial + Level 0-*        (6 niveles + tutorial)
            //   act-1 = Level 1-* + Level 2-* + Level 3-* + Intermission1 (13)
            //   act-2 = Level 4-* + EarlyAccessEnd  (GREED)
            //   prime = Level P-*
            { "acts/act-0", FindScenes(n =>
                n == "Tutorial" ||
                n.StartsWith("Level 0-")) },
            { "acts/act-1", FindScenes(n =>
                n.StartsWith("Level 1-") ||
                n.StartsWith("Level 2-") ||
                n.StartsWith("Level 3-") ||
                n == "Intermission1") },
            { "acts/act-2", FindScenes(n =>
                n.StartsWith("Level 4-") ||
                n == "EarlyAccessEnd") },
            { "acts/prime", FindScenes(n => n.StartsWith("Level P-")) }
        });
    }

    private static string[] FindScenes(System.Func<string, bool> predicate)
    {
        string[] guids = AssetDatabase.FindAssets("t:Scene", new[] { ScenesDir });
        return guids
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(p => predicate(Path.GetFileNameWithoutExtension(p)))
            .OrderBy(p => p)
            .ToArray();
    }

    private static void BuildBundles(Dictionary<string, string[]> bundles)
    {
        Directory.CreateDirectory(OutputDir);

        List<AssetBundleBuild> builds = new List<AssetBundleBuild>();
        foreach (KeyValuePair<string, string[]> pair in bundles)
        {
            if (pair.Value.Length == 0)
            {
                Debug.LogWarning("[WebGLBundles] Sin escenas para " + pair.Key);
                continue;
            }
            Debug.Log("[WebGLBundles] " + pair.Key + " <- " + string.Join(", ", pair.Value));
            builds.Add(new AssetBundleBuild
            {
                assetBundleName = pair.Key,
                assetNames = pair.Value
            });
        }

        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(
            OutputDir,
            builds.ToArray(),
            BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.ForceRebuildAssetBundle,
            BuildTarget.WebGL);

        if (manifest == null)
        {
            Debug.LogError("[WebGLBundles] BuildPipeline devolvio null");
            return;
        }

        foreach (string name in manifest.GetAllAssetBundles())
        {
            string path = Path.Combine(OutputDir, name);
            Debug.Log("[WebGLBundles] OK " + name + " " +
                (new FileInfo(path).Length / (1024f * 1024f)).ToString("0.0") + " MB");
        }
    }
}
