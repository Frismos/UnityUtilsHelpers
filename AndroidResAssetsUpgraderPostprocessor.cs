#if UNITY_2021_2_OR_NEWER
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Android;
using System.Text;

public class AndroidResAssetsUpgraderPostprocessor : IPostGenerateGradleAndroidProject
{
    private const string ASK_ABOUT_UPGRADE_RES_FOLDERS = nameof(ASK_ABOUT_UPGRADE_RES_FOLDERS);
    private const string ASK_ABOUT_UPGRADE_ASSETS_FOLDERS = nameof(ASK_ABOUT_UPGRADE_ASSETS_FOLDERS);

    private const string ANDROID_RES_PATH = "Assets/Plugins/Android/res";
    private const string ANDROID_RES_LEGACY_DIREACTORY = "res-legacy";
    private static readonly string ANDROID_RES_LEGACY_PATH = $"Assets/Plugins/Android/{ANDROID_RES_LEGACY_DIREACTORY}";

    private const string ANDROID_ASSETS_PATH = "Assets/Plugins/Android/assets";
    private const string ANDROID_ASSETS_LEGACY_DIREACTORY = "assets-legacy";
    private static readonly string ANDROID_ASSETS_LEGACY_PATH = $"Assets/Plugins/Android/{ANDROID_ASSETS_LEGACY_DIREACTORY}";

    private const string RES_DIRECTORY_IN_MAIN = "src/main/res";
    private const string ASSETS_DIRECTORY_IN_MAIN = "src/main/assets";

    private static void ValidateFolder(string path, string legacyPath, string legacyDirectory, string sessionStateKey)
    {
        if (!SessionState.GetBool(sessionStateKey, true))
        {
            return;
        }

        if (!Directory.Exists(path))
        {
            return;
        }

        var result = EditorUtility.DisplayDialog($"Upgrade {path} folder ? ",
            $@"Starting Unity 2021.2 {path} folder can no longer be used for copying res files to gradle project, this has to be done either via android plugins or manually.
Proceed with upgrade? 
('{path}' will be moved into '{legacyPath}')",
            "Yes",
            "No and don't ask again in this Editor session");

        if (!result)
        {
            SessionState.SetBool(ASK_ABOUT_UPGRADE_RES_FOLDERS, false);
            return;
        }

        if (Directory.Exists(legacyPath))
        {
            EditorUtility.DisplayDialog(
                "Upgrade failed",
                @$"Cannot upgrade since '{legacyPath}' already exists, delete it.
or manually merge '{path}' into '{legacyPath}' 
and delete '{path}'.
Restart Editor afterwards.",
                "Ok");
            return;
        }

        AssetDatabase.RenameAsset(path, legacyDirectory);
    }


    [InitializeOnLoadMethod]
    public static void ValidateResFolder()
    {
        ValidateFolder(ANDROID_RES_PATH, ANDROID_RES_LEGACY_PATH, ANDROID_RES_LEGACY_DIREACTORY, ASK_ABOUT_UPGRADE_RES_FOLDERS);
        ValidateFolder(ANDROID_ASSETS_PATH, ANDROID_ASSETS_LEGACY_PATH, ANDROID_ASSETS_LEGACY_DIREACTORY, ASK_ABOUT_UPGRADE_ASSETS_FOLDERS);
    }

    public static void Log(string message)
    {
        UnityEngine.Debug.LogFormat(UnityEngine.LogType.Log, UnityEngine.LogOption.NoStacktrace, null, message);
    }

    public int callbackOrder { get { return 0; } }

    private void CopyFolder(string legacyPath, string direactoryInMain, string path)
    {
        if (!Directory.Exists(legacyPath))
            return;

        var destination = Path.Combine(path, direactoryInMain).Replace("\\", "/");
        var log = new StringBuilder();
        log.AppendLine("Legacy Android res files copying");
        log.AppendLine($"Copying '{legacyPath}' -> '{destination}':");
        RecursiveCopy(new DirectoryInfo(legacyPath),
            new DirectoryInfo(destination),
            new[] { ".meta" },
            log);

        Log(log.ToString());
    }

    public void OnPostGenerateGradleAndroidProject(string path)
    {
        CopyFolder(ANDROID_RES_LEGACY_PATH, RES_DIRECTORY_IN_MAIN, path);
        CopyFolder(ANDROID_ASSETS_LEGACY_PATH, ASSETS_DIRECTORY_IN_MAIN, path);
    }

    private static void RecursiveCopy(DirectoryInfo source, DirectoryInfo target, string[] ignoredExtensions, StringBuilder log)
    {
        if (!Directory.Exists(target.FullName))
        {
            Directory.CreateDirectory(target.FullName);
        }
           
        foreach (FileInfo fi in source.GetFiles())
        {
            if (ignoredExtensions.Contains(fi.Extension))
            {
                continue;
            }

            var destination = Path.Combine(target.ToString(), fi.Name);
            log.AppendLine($" {fi.FullName} -> {destination}");
            fi.CopyTo(destination, true);
        }

        foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
        {
            DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
            RecursiveCopy(diSourceSubDir, nextTargetSubDir, ignoredExtensions, log);
        }
    }
}
#endif