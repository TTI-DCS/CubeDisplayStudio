using UnityEngine;
using UnityEngine.UI;

public class SignageDisplayController : MonoBehaviour
{
    [Header("Display Settings")]
    [SerializeField] private int targetWidth = 1920;
    [SerializeField] private int targetHeight = 1080;
    [SerializeField] private bool forceFullscreen = true;
    [SerializeField] private bool lockCursor = true;
    
    [Header("Scaling Settings")]
    [SerializeField] private bool enableScaling = true;
    [SerializeField] private Vector2 anchorPoint = new Vector2(0, 1); // Top-left anchor
    
    private Camera mainCamera;
    private Canvas canvas;
    private RectTransform canvasRectTransform;
    

    
    void Awake()
    {
        // フルスクリーン設定
        if (forceFullscreen)
        {
            Screen.fullScreen = true;
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        }
        
        // カーソルロック
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        // 解像度設定
        Screen.SetResolution(targetWidth, targetHeight, forceFullscreen);
        
        // コンポーネント取得
        mainCamera = Camera.main;
        canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            canvasRectTransform = canvas.GetComponent<RectTransform>();
        }
    }
    
    void Start()
    {
        ApplyDisplaySettings();
    }
    
    void Update()
    {
        // フルスクリーン切り替えのショートカット（開発用）
        if (Input.GetKeyDown(KeyCode.F11))
        {
            ToggleFullscreen();
        }
        
        // ESCキーでアプリケーション終了（サイネージ用途）
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            QuitApplication();
        }
    }
    
    void ApplyDisplaySettings()
    {
        if (!enableScaling) return;
        
        // 現在の画面解像度を取得
        int currentWidth = Screen.currentResolution.width;
        int currentHeight = Screen.currentResolution.height;
        
        // ドットバイドット表示のため、スケールは1.0（スケールなし）
        float scale = 1.0f;
        
        // カメラの設定
        if (mainCamera != null)
        {
            // カメラのビューポートを左上に固定（ドットバイドット）
            float viewportWidth = (float)targetWidth / currentWidth;
            float viewportHeight = (float)targetHeight / currentHeight;
            
            // 左上を原点として設定
            float offsetX = 0.0f;
            float offsetY = 1.0f - viewportHeight; // 左上から開始
            
            mainCamera.rect = new Rect(offsetX, offsetY, viewportWidth, viewportHeight);
        }
        
        // Canvasの設定
        if (canvas != null && canvasRectTransform != null)
        {
            // Canvas Scalerの設定（ドットバイドット用）
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize; // ピクセル単位で固定
                scaler.scaleFactor = 1.0f; // スケールファクターを1.0に固定
            }
            
            // Canvasのアンカーポイントを左上に設定
            canvasRectTransform.anchorMin = new Vector2(0, 1); // 左上
            canvasRectTransform.anchorMax = new Vector2(0, 1); // 左上
            canvasRectTransform.pivot = new Vector2(0, 1); // 左上
            canvasRectTransform.anchoredPosition = Vector2.zero; // 左上に配置
        }
        
        Debug.Log($"Dot-by-Dot Display Applied - Target: {targetWidth}x{targetHeight}, Current: {currentWidth}x{currentHeight}, No Scaling");
    }
    
    void ToggleFullscreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
        Debug.Log($"Fullscreen toggled: {Screen.fullScreen}");
    }
    
    // アプリケーションを終了するメソッド
    void QuitApplication()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    // エディタでのプレビュー用
    void OnValidate()
    {
        if (Application.isPlaying)
        {
            ApplyDisplaySettings();
        }
    }
    
    // 画面解像度変更時の処理
    void OnRectTransformDimensionsChange()
    {
        if (Application.isPlaying)
        {
            ApplyDisplaySettings();
        }
    }
} 