using UnityEngine;
using UnityEngine.UI;

public class SignageScaler : MonoBehaviour
{
    [Header("Target Resolution")]
    [SerializeField] private Vector2Int targetResolution = new Vector2Int(1920, 1080);
    
    [Header("Scaling Options")]
    [SerializeField] private ScalingMode scalingMode = ScalingMode.FitToScreen;
    [SerializeField] private AnchorPreset anchorPreset = AnchorPreset.TopLeft;
    [SerializeField] private bool maintainAspectRatio = true;
    
    [Header("UI Elements")]
    [SerializeField] private RectTransform[] uiElements;
    
    private Vector2Int currentResolution;
    private Vector2 scaleFactor;
    private Vector2 offset;
    
    public enum ScalingMode
    {
        FitToScreen,        // 画面に合わせてスケール
        StretchToScreen,    // 画面全体に引き伸ばし
        FixedScale,         // 固定スケール
        Custom              // カスタム設定
    }
    
    public enum AnchorPreset
    {
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        Center,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }
    
    void Start()
    {
        InitializeScaling();
        ApplyScaling();
    }
    
    void Update()
    {
        // 解像度変更を検出
        Vector2Int newResolution = new Vector2Int(Screen.width, Screen.height);
        if (newResolution != currentResolution)
        {
            currentResolution = newResolution;
            ApplyScaling();
        }
    }
    
    void InitializeScaling()
    {
        currentResolution = new Vector2Int(Screen.width, Screen.height);
        CalculateScaleFactor();
        CalculateOffset();
    }
    
    void CalculateScaleFactor()
    {
        switch (scalingMode)
        {
            case ScalingMode.FitToScreen:
                // ドットバイドット表示のため、スケールは1.0（スケールなし）
                scaleFactor = Vector2.one;
                break;
                
            case ScalingMode.StretchToScreen:
                // ドットバイドット表示のため、スケールは1.0（スケールなし）
                scaleFactor = Vector2.one;
                break;
                
            case ScalingMode.FixedScale:
                scaleFactor = Vector2.one;
                break;
                
            case ScalingMode.Custom:
                // カスタム設定はインスペクターで調整
                break;
        }
    }
    
    void CalculateOffset()
    {
        // ドットバイドット表示のため、左上を原点として固定
        offset = Vector2.zero;
    }
    
    void ApplyScaling()
    {
        // メインカメラの設定（ドットバイドット）
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            // カメラのビューポートを左上に固定（ドットバイドット）
            float viewportWidth = (float)targetResolution.x / currentResolution.x;
            float viewportHeight = (float)targetResolution.y / currentResolution.y;
            
            // 左上を原点として設定
            float viewportX = 0.0f;
            float viewportY = 1.0f - viewportHeight; // 左上から開始
            
            mainCamera.rect = new Rect(viewportX, viewportY, viewportWidth, viewportHeight);
        }
        
        // UI要素のスケーリング（ドットバイドット）
        foreach (RectTransform uiElement in uiElements)
        {
            if (uiElement != null)
            {
                ApplyUIScaling(uiElement);
            }
        }
        
        // Canvas Scalerの設定（ドットバイドット用）
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize; // ピクセル単位で固定
                scaler.scaleFactor = 1.0f; // スケールファクターを1.0に固定
            }
        }
        
        Debug.Log($"Dot-by-Dot Display Applied - Target: {targetResolution}, Current: {currentResolution}, No Scaling");
    }
    
    void ApplyUIScaling(RectTransform uiElement)
    {
        // ドットバイドット表示のため、スケールは1.0（スケールなし）
        uiElement.localScale = Vector3.one;
        
        // アンカーポイントを左上に設定
        uiElement.anchorMin = new Vector2(0, 1); // 左上
        uiElement.anchorMax = new Vector2(0, 1); // 左上
        uiElement.pivot = new Vector2(0, 1); // 左上
        
        // 位置は変更しない（元の位置を保持）
        // サイズも変更しない（元のサイズを保持）
    }
    
    // エディタでのプレビュー用
    void OnValidate()
    {
        if (Application.isPlaying)
        {
            InitializeScaling();
            ApplyScaling();
        }
    }
    
    // デバッグ情報の表示
    void OnGUI()
    {
        if (Application.isPlaying)
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label($"Target Resolution: {targetResolution}");
            GUILayout.Label($"Current Resolution: {currentResolution}");
            GUILayout.Label($"Scale Factor: {scaleFactor}");
            GUILayout.Label($"Offset: {offset}");
            GUILayout.Label($"Scaling Mode: {scalingMode}");
            GUILayout.EndArea();
        }
    }
} 