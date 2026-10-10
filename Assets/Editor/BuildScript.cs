using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BuildScript
{
    // Имя игры (будет использоваться в имени exe-файла)
    private const string GAME_NAME = "AnyRPG Engine";

    // Куда складывать сборки (относительно корня проекта)
    private const string BUILD_FOLDER = "Builds";

    // Список сцен, которые должны попасть в билд.
    // ВАЖНО: первая сцена в списке — загрузочная (Initialization/MainMenu).
    private static readonly string[] Scenes =
    {
 /*       
        "Assets/AnyRPG/Addons/anymmo-fishnet/Games/AnyMMODemo/Scenes/AnyMMODemoMainMenu/AnyMMODemoMainMenu.unity",
        "Assets/AnyRPG/Addons/anymmo-fishnet/Games/AnyMMODemo/Scenes/AnyMMODemo/AnyMMODemo.unity",
        "Assets/AnyRPG/Addons/anymmo-fishnet/Games/AnyMMODemo/Scenes/AnyMMODemoCutsceneZone/AnyMMODemoCutsceneZone.unity",
        "Assets/AnyRPG/Addons/anymmo-fishnet/Games/AnyMMODemo/Scenes/AnyMMODemoDungeon/AnyMMODemoDungeon.unity",
        "Assets/AnyRPG/Addons/anymmo-fishnet/Games/AnyMMODemo/Scenes/AnyMMODemoZone/AnyMMODemoZone.unity",
*/
 
         "Assets/AnyRPG/Addons/anymmo-fishnet/Games/AnyMMODemo/Scenes/AnyMMODemo/AnyMMODemo.unity",

         "Assets/AnyRPG/Addons/anymmo-fishnet/Games/AnyMMODemo/Scenes/AnyMMODemoCutsceneZone/AnyMMODemoCutsceneZone.unity",
         "Assets/AnyRPG/Addons/anymmo-fishnet/Games/AnyMMODemo/Scenes/AnyMMODemoDungeon/AnyMMODemoDungeon.unity",
         "Assets/AnyRPG/Addons/anymmo-fishnet/Games/AnyMMODemo/Scenes/AnyMMODemoMainMenu/AnyMMODemoMainMenu.unity",
         "Assets/AnyRPG/Addons/anymmo-fishnet/Games/AnyMMODemo/Scenes/AnyMMODemoZone/AnyMMODemoZone.unity",

    };

    // =========================================================
    //   МЕНЮ TOOLS → BUILD
    // =========================================================

    [MenuItem("Tools/Build/Build Windows Client", false, 10)]
    public static void BuildWindowsClient()
    {
        Build(BuildTarget.StandaloneWindows64, "Windows", GAME_NAME + ".exe", BuildOptions.None);
    }

    [MenuItem("Tools/Build/Build Windows Client (Development)", false, 11)]
    public static void BuildWindowsClientDev()
    {
        Build(BuildTarget.StandaloneWindows64, "Windows_Dev", GAME_NAME + ".exe", BuildOptions.Development);
    }

    [MenuItem("Tools/Build/Build Windows Server", false, 20)]
    public static void BuildWindowsServer()
    {
        // Для серверного билда используем субтаргет Server
        Build(BuildTarget.StandaloneWindows64, "WindowsServer", GAME_NAME + ".exe", 
              BuildOptions.None, StandaloneBuildSubtarget.Server);
    }

    [MenuItem("Tools/Build/Build Windows Server (Development)", false, 21)]
    public static void BuildWindowsServerDev()
    {
        Build(BuildTarget.StandaloneWindows64, "WindowsServer_Dev", GAME_NAME + ".exe", 
              BuildOptions.Development, StandaloneBuildSubtarget.Server);
    }

    [MenuItem("Tools/Build/Build Linux Client", false, 30)]
    public static void BuildLinuxClient()
    {
        Build(BuildTarget.StandaloneLinux64, "Linux", GAME_NAME + ".x86_64", BuildOptions.None);
    }

    [MenuItem("Tools/Build/Build Linux Client (Development)", false, 31)]
    public static void BuildLinuxClientDev()
    {
        Build(BuildTarget.StandaloneLinux64, "Linux_Dev", GAME_NAME + ".x86_64", BuildOptions.Development);
    }

    [MenuItem("Tools/Build/Build Linux Server", false, 40)]
    public static void BuildLinuxServer()
    {
        Build(BuildTarget.StandaloneLinux64, "LinuxServer", GAME_NAME + ".x86_64", 
              BuildOptions.None, StandaloneBuildSubtarget.Server);
    }

    [MenuItem("Tools/Build/Build Linux Server (Development)", false, 41)]
    public static void BuildLinuxServerDev()
    {
        Build(BuildTarget.StandaloneLinux64, "LinuxServer_Dev", GAME_NAME + ".x86_64", 
              BuildOptions.Development, StandaloneBuildSubtarget.Server);
    }

    // =========================================================
    //   ОСНОВНАЯ ЛОГИКА СБОРКИ
    // =========================================================

    private static void Build(
        BuildTarget target,
        string subFolder,
        string exeName,
        BuildOptions options,
        StandaloneBuildSubtarget subtarget = StandaloneBuildSubtarget.Player)
    {
        // Проверяем, что все сцены существуют
        foreach (var scene in Scenes)
        {
            if (!File.Exists(scene))
            {
                Debug.LogError($"[BuildScript] Сцена не найдена: {scene}. Сборка отменена.");
                return;
            }
        }

        // Формируем путь вывода
        string outputDir = Path.Combine(BUILD_FOLDER, subFolder);
        string outputPath = Path.Combine(outputDir, exeName);

        // Создаём папку, если её нет
        Directory.CreateDirectory(outputDir);

        // Настраиваем параметры сборки
        var buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = Scenes,
            locationPathName = outputPath,
            target = target,
            options = options,
            subtarget = (int)subtarget, // 0 = Player, 1 = Server
        };

        Debug.Log($"[BuildScript] Начинаю сборку: {target} / {subtarget} → {outputPath}");

        // Запускаем сборку
        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[BuildScript] ✅ Сборка успешна: {outputPath} ({summary.totalSize / (1024 * 1024)} MB)");
        }
        else if (summary.result == BuildResult.Failed)
        {
            Debug.LogError($"[BuildScript] ❌ Сборка провалена. Ошибок: {summary.totalErrors}");
        }
        else if (summary.result == BuildResult.Cancelled)
        {
            Debug.LogWarning("[BuildScript] ⚠️ Сборка отменена.");
        }
    }

    // =========================================================
    //   МЕТОДЫ ДЛЯ КОМАНДНОЙ СТРОКИ
    //   Вызываются через -executeMethod BuildScript.CmdBuildXxx
    // =========================================================

    public static void CmdBuildWindowsClient()   => BuildWindowsClient();
    public static void CmdBuildWindowsServer()   => BuildWindowsServer();
    public static void CmdBuildLinuxClient()     => BuildLinuxClient();
    public static void CmdBuildLinuxServer()     => BuildLinuxServer();
}