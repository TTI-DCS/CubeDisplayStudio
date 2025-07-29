using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;
using System;

public class BuildOptimizer : EditorWindow
{
    private string version = "";
    private bool createPortable = true;
    private bool optimizeSettings = true;
    private string outputPath = "Distribution";

    [MenuItem("Tools/Build Optimizer")]
    public static void ShowWindow()
    {
        GetWindow<BuildOptimizer>("Build Optimizer");
    }

    private void OnGUI()
    {
        GUILayout.Label("CubeDisplayStudio Build Optimizer", EditorStyles.boldLabel);
        
        GUILayout.Space(10);
        
        // PlayerSettingsからバージョン情報を取得
        if (string.IsNullOrEmpty(version))
        {
            version = GetVersionFromPlayerSettings();
        }
        
        // バージョン設定
        GUILayout.Label("Build Settings", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        version = EditorGUILayout.TextField("Version:", version);
        if (GUILayout.Button("Refresh", GUILayout.Width(60)))
        {
            version = GetVersionFromPlayerSettings();
        }
        EditorGUILayout.EndHorizontal();
        outputPath = EditorGUILayout.TextField("Output Path:", outputPath);
        
        GUILayout.Space(10);
        
        // オプション設定
        GUILayout.Label("Options", EditorStyles.boldLabel);
        optimizeSettings = EditorGUILayout.Toggle("Optimize Build Settings", optimizeSettings);
        createPortable = EditorGUILayout.Toggle("Create Portable Version", createPortable);
        
        GUILayout.Space(20);
        
        // メインボタン
        if (GUILayout.Button("Create Distribution Package", GUILayout.Height(40)))
        {
            CreateDistributionPackage();
        }
        
        GUILayout.Space(10);
        
        // 個別ボタン
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Optimize Settings Only"))
        {
            OptimizeBuildSettings();
        }
        if (GUILayout.Button("Create Build Only"))
        {
            CreateBuildOnly();
        }
        EditorGUILayout.EndHorizontal();
        
        GUILayout.Space(10);
        
        // バージョン情報表示
        EditorGUILayout.HelpBox(
            $"Current Player Settings:\n" +
            $"• Bundle Version: {PlayerSettings.bundleVersion}\n" +
            $"• Product Name: {PlayerSettings.productName}\n" +
            $"• Company Name: {PlayerSettings.companyName}",
            MessageType.Info
        );
        
        GUILayout.Space(10);
        
        // 情報表示
        EditorGUILayout.HelpBox(
            "This tool will create a distribution package with all necessary files.\n" +
            "The package will include:\n" +
            "• Executable and runtime files\n" +
            "• Game data and assets\n" +
            "• README and installation instructions\n" +
            "• Optional portable version",
            MessageType.Info
        );
    }
    
    private void CreateDistributionPackage()
    {
        try
        {
            EditorUtility.DisplayProgressBar("Build Optimizer", "Starting distribution package creation...", 0f);
            
            // 1. ビルド設定の最適化
            if (optimizeSettings)
            {
                EditorUtility.DisplayProgressBar("Build Optimizer", "Optimizing build settings...", 0.1f);
                OptimizeBuildSettings();
            }
            
            // 2. ビルドの作成
            EditorUtility.DisplayProgressBar("Build Optimizer", "Creating build...", 0.3f);
            string buildPath = CreateBuild();
            
            if (string.IsNullOrEmpty(buildPath))
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Error", "Build failed! Please check the console for details.", "OK");
                return;
            }
            
            // 3. 配布フォルダの作成
            EditorUtility.DisplayProgressBar("Build Optimizer", "Creating distribution folder...", 0.6f);
            CreateDistributionFolder(buildPath);
            
            // 4. ポータブル版の作成
            if (createPortable)
            {
                EditorUtility.DisplayProgressBar("Build Optimizer", "Creating portable version...", 0.8f);
                CreatePortableVersion(buildPath);
            }
            
            EditorUtility.DisplayProgressBar("Build Optimizer", "Finalizing...", 1f);
            
            EditorUtility.ClearProgressBar();
            
            // 完了メッセージ
            string message = "Distribution package created successfully!\n\n";
            message += $"Location: {outputPath}\n";
            message += $"Version: {version}\n\n";
            message += "Files created:\n";
            message += "• Distribution folder with all files\n";
            if (createPortable) message += "• Portable version\n";
            
            EditorUtility.DisplayDialog("Success", message, "OK");
            
            // エクスプローラーでフォルダを開く
            if (Directory.Exists(outputPath))
            {
                EditorUtility.RevealInFinder(outputPath);
            }
        }
        catch (Exception e)
        {
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Error", $"An error occurred: {e.Message}", "OK");
            Debug.LogError($"Build Optimizer Error: {e}");
        }
    }
    
    private void OptimizeBuildSettings()
    {
        // Player Settings の最適化
        PlayerSettings.companyName = "WonderBox";
        PlayerSettings.productName = "CubeDisplayStudio";
        PlayerSettings.defaultScreenWidth = 1920;
        PlayerSettings.defaultScreenHeight = 1080;
        PlayerSettings.runInBackground = true;
        PlayerSettings.muteOtherAudioSources = true;
        
        // グラフィックス設定の最適化
        PlayerSettings.gpuSkinning = true;
        
        // デバッグ設定の無効化
        PlayerSettings.usePlayerLog = false;
        // PlayerSettings.submitAnalytics = false; // Unity 2022以降で非推奨
        
        // スプラッシュスクリーンの無効化
        PlayerSettings.showUnitySplashScreen = false;
        // PlayerSettings.showUnitySplashLogo = false; // Unity 2022以降で非推奨
        
        // その他の最適化
        PlayerSettings.forceSingleInstance = false;
        PlayerSettings.resizableWindow = false;
        
        // ビルド設定の最適化
        EditorUserBuildSettings.development = false;
        EditorUserBuildSettings.allowDebugging = false;
        
        // PlayerSettings.asset = false; // Unity 2022以降で非推奨
        AssetDatabase.SaveAssets();
        
        Debug.Log("Build settings optimized for distribution!");
    }
    
    private string CreateBuild()
    {
        string buildPath = $"Build/CubeDisplayStudio_v{version}";
        
        // ビルドオプションの設定
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = GetEnabledScenes();
        buildPlayerOptions.locationPathName = buildPath + ".exe";
        buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
        buildPlayerOptions.options = BuildOptions.None;
        
        // ビルド実行
        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildResult result = report.summary.result;
        
        if (result == BuildResult.Succeeded)
        {
            Debug.Log($"Build created successfully: {buildPath}");
            return buildPath;
        }
        else
        {
            Debug.LogError($"Build failed: {result}");
            return null;
        }
    }
    
    private void CreateBuildOnly()
    {
        string buildPath = CreateBuild();
        if (!string.IsNullOrEmpty(buildPath))
        {
            EditorUtility.DisplayDialog("Success", $"Build created successfully!\nLocation: {buildPath}", "OK");
        }
    }
    
    private string[] GetEnabledScenes()
    {
        var scenes = new System.Collections.Generic.List<string>();
        for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
        {
            if (EditorBuildSettings.scenes[i].enabled)
            {
                scenes.Add(EditorBuildSettings.scenes[i].path);
            }
        }
        return scenes.ToArray();
    }
    
    private void CreateDistributionFolder(string buildPath)
    {
        string distPath = $"{outputPath}/CubeDisplayStudio_v{version}";
        if (Directory.Exists(distPath))
        {
            Directory.Delete(distPath, true);
        }
        
        Directory.CreateDirectory(distPath);
        
        // 必要なファイルをコピー
        if (File.Exists(buildPath + ".exe"))
        {
            File.Copy(buildPath + ".exe", Path.Combine(distPath, "CubeDisplayStudio.exe"));
        }
        
        // UnityPlayer.dllをコピー
        if (File.Exists("Build/UnityPlayer.dll"))
        {
            File.Copy("Build/UnityPlayer.dll", Path.Combine(distPath, "UnityPlayer.dll"));
        }
        
        // UnityCrashHandler64.exeをコピー
        if (File.Exists("Build/UnityCrashHandler64.exe"))
        {
            File.Copy("Build/UnityCrashHandler64.exe", Path.Combine(distPath, "UnityCrashHandler64.exe"));
        }
        
        if (Directory.Exists(buildPath + "_Data"))
        {
            CopyDirectory(buildPath + "_Data", Path.Combine(distPath, "CubeDisplayStudio_Data"));
        }
        
        // MonoBleedingEdgeフォルダをコピー
        if (Directory.Exists("Build/MonoBleedingEdge"))
        {
            CopyDirectory("Build/MonoBleedingEdge", Path.Combine(distPath, "MonoBleedingEdge"));
        }
        
        // D3D12フォルダをコピー
        if (Directory.Exists("Build/D3D12"))
        {
            CopyDirectory("Build/D3D12", Path.Combine(distPath, "D3D12"));
        }
        
        // READMEファイルを作成
        CreateReadmeFile(distPath);
        
        // 起動用バッチファイルを作成
        CreateStartupBatch(distPath);
        
        Debug.Log($"Distribution folder created: {distPath}");
    }
    
    private void CreatePortableVersion(string buildPath)
    {
        string portablePath = $"{outputPath}/CubeDisplayStudio_v{version}_Portable";
        if (Directory.Exists(portablePath))
        {
            Directory.Delete(portablePath, true);
        }
        
        Directory.CreateDirectory(portablePath);
        
        // 必要なファイルをコピー
        if (File.Exists(buildPath + ".exe"))
        {
            File.Copy(buildPath + ".exe", Path.Combine(portablePath, "CubeDisplayStudio.exe"));
        }
        
        // UnityPlayer.dllをコピー
        if (File.Exists("Build/UnityPlayer.dll"))
        {
            File.Copy("Build/UnityPlayer.dll", Path.Combine(portablePath, "UnityPlayer.dll"));
        }
        
        // UnityCrashHandler64.exeをコピー
        if (File.Exists("Build/UnityCrashHandler64.exe"))
        {
            File.Copy("Build/UnityCrashHandler64.exe", Path.Combine(portablePath, "UnityCrashHandler64.exe"));
        }
        
        if (Directory.Exists(buildPath + "_Data"))
        {
            CopyDirectory(buildPath + "_Data", Path.Combine(portablePath, "CubeDisplayStudio_Data"));
        }
        
        // MonoBleedingEdgeフォルダをコピー
        if (Directory.Exists("Build/MonoBleedingEdge"))
        {
            CopyDirectory("Build/MonoBleedingEdge", Path.Combine(portablePath, "MonoBleedingEdge"));
        }
        
        // D3D12フォルダをコピー
        if (Directory.Exists("Build/D3D12"))
        {
            CopyDirectory("Build/D3D12", Path.Combine(portablePath, "D3D12"));
        }
        
        // ポータブル用README
        CreatePortableReadme(portablePath);
        
        // 起動用バッチファイル
        CreateStartupBatch(portablePath);
        
        Debug.Log($"Portable version created: {portablePath}");
    }
    

    
    private void CreateReadmeFile(string folderPath)
    {
        string readmeContent = $@"# CubeDisplayStudio v{version} - Standard Version

## Installation Instructions

### Standard Installation
1. Extract all files to a folder of your choice
2. Run `CubeDisplayStudio.exe` to start the application
3. For best performance, run as administrator

### What's Included
- CubeDisplayStudio.exe (Main executable)
- UnityPlayer.dll (Unity runtime)
- UnityCrashHandler64.exe (Crash handler)
- CubeDisplayStudio_Data/ (Game data and assets)
- MonoBleedingEdge/ (Mono runtime)
- D3D12/ (DirectX 12 runtime)
- README.md (This file)
- Start_CubeDisplayStudio.bat (Launcher script)

### System Requirements
- Windows 10 or later
- DirectX 12 compatible graphics card
- 4GB RAM minimum
- 2GB free disk space

### Troubleshooting
- If the application doesn't start, ensure DirectX 12 is installed
- Run as administrator if you encounter permission issues
- Check Windows Event Viewer for detailed error messages

### Support
For technical support, please contact the development team.

Build Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
Version: {version}
";
        
        File.WriteAllText(Path.Combine(folderPath, "README.md"), readmeContent);
    }
    
    private void CreatePortableReadme(string folderPath)
    {
        string readmeContent = $@"# CubeDisplayStudio v{version} - Portable Version

## Portable Installation

This is a portable version that can run from any location without installation.
Perfect for USB drives, network shares, or temporary installations.

### Usage
1. Extract all files to any folder (USB drive, network share, etc.)
2. Run `Start_CubeDisplayStudio.bat` or `CubeDisplayStudio.exe`
3. The application will run from the current location

### What's Included
- CubeDisplayStudio.exe (Main executable)
- UnityPlayer.dll (Unity runtime)
- UnityCrashHandler64.exe (Crash handler)
- CubeDisplayStudio_Data/ (Game data and assets)
- MonoBleedingEdge/ (Mono runtime)
- D3D12/ (DirectX 12 runtime)
- README.md (This file)
- Start_CubeDisplayStudio.bat (Launcher script)

### System Requirements
- Windows 10 or later
- DirectX 12 compatible graphics card
- 4GB RAM minimum

### Key Differences from Standard Version
- Can run from any location (USB, network, etc.)
- No installation required
- All settings stored locally
- Perfect for temporary or mobile use

### Note
This portable version is designed to run without installation.
All settings and data are stored locally in the application folder.

Build Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
Version: {version}
";
        
        File.WriteAllText(Path.Combine(folderPath, "README.md"), readmeContent);
    }
    
    private void CreateStartupBatch(string folderPath)
    {
        string batchContent = @"@echo off
echo Starting CubeDisplayStudio...
echo.
if exist ""CubeDisplayStudio.exe"" (
    start """" ""CubeDisplayStudio.exe""
) else (
    echo Error: CubeDisplayStudio.exe not found!
    echo Please ensure all files are extracted correctly.
    pause
)
";
        
        File.WriteAllText(Path.Combine(folderPath, "Start_CubeDisplayStudio.bat"), batchContent);
    }
    

    
    private string GetVersionFromPlayerSettings()
    {
        // PlayerSettingsからバージョン情報を取得
        string bundleVersion = PlayerSettings.bundleVersion;
        
        // バージョンが設定されていない場合はデフォルト値を返す
        if (string.IsNullOrEmpty(bundleVersion))
        {
            return "1.0.0";
        }
        
        return bundleVersion;
    }
    
    private void CopyDirectory(string sourceDir, string destDir)
    {
        Directory.CreateDirectory(destDir);
        
        foreach (string file in Directory.GetFiles(sourceDir))
        {
            string fileName = Path.GetFileName(file);
            string destFile = Path.Combine(destDir, fileName);
            File.Copy(file, destFile);
        }
        
        foreach (string subDir in Directory.GetDirectories(sourceDir))
        {
            string subDirName = Path.GetFileName(subDir);
            string destSubDir = Path.Combine(destDir, subDirName);
            CopyDirectory(subDir, destSubDir);
        }
    }
} 