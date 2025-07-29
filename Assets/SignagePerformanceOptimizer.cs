using UnityEngine;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;

public class SignagePerformanceOptimizer : MonoBehaviour
{
    [Header("Performance Settings")]
    [SerializeField] private bool enablePerformanceMode = true;
    [SerializeField] private int targetFrameRate = 30;
    [SerializeField] private bool disableVSync = true;
    [SerializeField] private bool optimizeForSignage = true;
    
    [Header("Memory Settings")]
    [SerializeField] private bool enableGarbageCollection = true;
    [SerializeField] private int garbageCollectionInterval = 600; // 10分間隔
    [SerializeField] private bool enableMemoryMonitoring = true;
    [SerializeField] private float memoryThreshold = 150f; // MB (2GB環境推奨)
    
    [Header("Rendering Settings")]
    [SerializeField] private bool disableShadows = true;
    [SerializeField] private bool disablePostProcessing = false;
    [SerializeField] private bool optimizeLights = true;
    
    private float lastGCTime;
    private Light[] sceneLights;
    private Camera mainCamera;
    
    void Awake()
    {
        if (!enablePerformanceMode) return;
        
        // フレームレート設定
        Application.targetFrameRate = targetFrameRate;
        
        // VSync設定
        if (disableVSync)
        {
            QualitySettings.vSyncCount = 0;
        }
        
        // 起動時のメモリ最適化
        System.GC.Collect();
        System.GC.WaitForPendingFinalizers();
        System.GC.Collect();
        
        // コンポーネント取得
        mainCamera = Camera.main;
        sceneLights = FindObjectsOfType<Light>();
        
        // レンダリング最適化
        if (optimizeForSignage)
        {
            OptimizeRendering();
        }
        
        Debug.Log($"Signage Performance Optimizer initialized - Target FPS: {targetFrameRate}");
    }
    
    void Start()
    {
        if (!enablePerformanceMode) return;
        
        // 起動時の最適化
        OptimizeSceneObjects();
    }
    
    void Update()
    {
        if (!enablePerformanceMode) return;
        
        // メモリ監視とガベージコレクション
        if (enableMemoryMonitoring)
        {
            MonitorMemoryUsage();
        }
        
        // 定期的なガベージコレクション
        if (enableGarbageCollection && Time.time - lastGCTime > garbageCollectionInterval)
        {
            PerformGarbageCollection();
        }
        
        // パフォーマンス監視
        MonitorPerformance();
    }
    
    void OptimizeRendering()
    {
        // シャドウ設定
        if (disableShadows)
        {
            QualitySettings.shadows = ShadowQuality.Disable;
            QualitySettings.shadowDistance = 0;
            Debug.Log("Shadows disabled for performance optimization");
        }
        
        // レンダリングパス最適化
        if (mainCamera != null)
        {
            mainCamera.renderingPath = RenderingPath.Forward;
            mainCamera.allowHDR = false;
            mainCamera.allowMSAA = false;
            Debug.Log("Camera rendering optimized: Forward path, HDR/MSAA disabled");
        }
        else
        {
            Debug.LogWarning("Main camera not found for rendering optimization");
        }
        
        // ライト最適化
        if (optimizeLights)
        {
            OptimizeLights();
        }
    }
    
    void OptimizeLights()
    {
        // nullチェックを追加
        if (sceneLights == null || sceneLights.Length == 0)
        {
            Debug.Log("No lights found in scene for optimization");
            return;
        }
        
        foreach (Light light in sceneLights)
        {
            if (light != null)
            {
                // シャドウを無効化
                light.shadows = LightShadows.None;
                
                // ライトの範囲を制限
                if (light.type == LightType.Point || light.type == LightType.Spot)
                {
                    light.range = Mathf.Min(light.range, 10f);
                }
                
                // ライトの強度を調整
                light.intensity = Mathf.Min(light.intensity, 2f);
            }
        }
        
        Debug.Log($"Optimized {sceneLights.Length} lights in scene");
    }
    
    void OptimizeSceneObjects()
    {
        // 不要なコンポーネントを無効化
        var audioSources = FindObjectsOfType<AudioSource>();
        foreach (var audio in audioSources)
        {
            if (audio != null && !audio.isPlaying)
            {
                audio.enabled = false;
            }
        }
        
        // パーティクルシステムの最適化
        var particleSystems = FindObjectsOfType<ParticleSystem>();
        foreach (var ps in particleSystems)
        {
            if (ps != null)
            {
                var main = ps.main;
                main.maxParticles = Mathf.Min(main.maxParticles, 100);
            }
        }
    }
    
    void MonitorPerformance()
    {
        // フレームレート監視
        if (Time.frameCount % 300 == 0) // 5秒ごと
        {
            float currentFPS = 1.0f / Time.unscaledDeltaTime;
            if (currentFPS < targetFrameRate * 0.8f)
            {
                Debug.LogWarning($"Performance warning: Current FPS ({currentFPS:F1}) is below target ({targetFrameRate})");
            }
        }
    }
    
    void MonitorMemoryUsage()
    {
        // メモリ使用量を監視（10秒ごと）
        if (Time.frameCount % 600 == 0)
        {
            long totalMemory = System.GC.GetTotalMemory(false);
            float memoryMB = totalMemory / (1024f * 1024f);
            
            // Unityメモリ情報を取得
            long totalReservedMemory = UnityEngine.Profiling.Profiler.GetTotalReservedMemoryLong();
            long totalAllocatedMemory = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong();
            float totalMemoryMB = totalReservedMemory / (1024f * 1024f);
            float allocatedMemoryMB = totalAllocatedMemory / (1024f * 1024f);
            
            if (memoryMB > memoryThreshold)
            {
                Debug.LogWarning($"Managed memory usage high: {memoryMB:F1}MB (Threshold: {memoryThreshold}MB)");
                
                // メモリ使用量が閾値を超えた場合、強制GC実行
                if (enableGarbageCollection)
                {
                    PerformGarbageCollection();
                }
            }
            
            // Unityメモリ使用量も警告
            if (totalMemoryMB > memoryThreshold * 2f) // Unityメモリが閾値の2倍以上
            {
                Debug.LogWarning($"Unity memory usage high: {totalMemoryMB:F1}MB (Allocated: {allocatedMemoryMB:F1}MB)");
            }
            
            Debug.Log($"Managed: {memoryMB:F1}MB, Unity Total: {totalMemoryMB:F1}MB, Allocated: {allocatedMemoryMB:F1}MB");
        }
    }
    

    
    void PerformGarbageCollection()
    {
        Debug.Log("Performing garbage collection...");
        
        // GC実行前のメモリ使用量
        long beforeMemory = System.GC.GetTotalMemory(false);
        
        // ガベージコレクション実行
        System.GC.Collect();
        System.GC.WaitForPendingFinalizers();
        System.GC.Collect();
        
        // GC実行後のメモリ使用量
        long afterMemory = System.GC.GetTotalMemory(false);
        float freedMemory = (beforeMemory - afterMemory) / (1024f * 1024f);
        
        lastGCTime = Time.time;
        
        Debug.Log($"Garbage collection completed. Freed: {freedMemory:F1}MB");
    }
    
    // エディタでのプレビュー用
    void OnValidate()
    {
        if (Application.isPlaying && enablePerformanceMode)
        {
            Application.targetFrameRate = targetFrameRate;
        }
    }
    
    // アプリケーション終了時のクリーンアップ
    void OnApplicationQuit()
    {
        if (enableGarbageCollection)
        {
            System.GC.Collect();
        }
    }
} 