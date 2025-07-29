using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Runtime.InteropServices;

public class SignageDisplayController : MonoBehaviour
{
    [Header("Display Settings")]
    [SerializeField] private int targetWidth = 1920;
    [SerializeField] private int targetHeight = 1080;
    [SerializeField] private bool useBorderlessWindow = true;
    [SerializeField] private bool lockCursor = true;
    
    [Header("Scaling Settings")]
    [SerializeField] private bool enableScaling = true;
    [SerializeField] private Vector2 anchorPoint = new Vector2(0, 1); // Top-left anchor
    
    private Camera mainCamera;
    private Canvas canvas;
    private RectTransform canvasRectTransform;
    
    // Windows API用のDllImport
    #if UNITY_STANDALONE_WIN
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();
    
    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
    
    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
    
    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
    
    // Windows API定数
    private const int GWL_STYLE = -16;
    private const int GWL_EXSTYLE = -20;
    private const int WS_CAPTION = 0x00C00000;
    private const int WS_THICKFRAME = 0x00040000;
    private const int WS_MINIMIZEBOX = 0x00020000;
    private const int WS_MAXIMIZEBOX = 0x00010000;
    private const int WS_SYSMENU = 0x00080000;
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_FRAMECHANGED = 0x0020;
    #endif
    

    
    void Awake()
    {
        // ウィンドウモード設定
        Screen.fullScreen = false;
        
        // 解像度設定
        Screen.SetResolution(targetWidth, targetHeight, false);
        
        // コンポーネント取得
        mainCamera = Camera.main;
        canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            canvasRectTransform = canvas.GetComponent<RectTransform>();
        }
        
        // 起動時にボーダーなしウィンドウを左上に配置
        if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
            // 少し遅延を入れてウィンドウが作成された後に実行
            StartCoroutine(SetWindowPositionDelayed());
        }
    }
    
    void Start()
    {
        ApplyDisplaySettings();
    }
    
    void Update()
    {
        // ウィンドウモード切り替えのショートカット（開発用）
        if (Input.GetKeyDown(KeyCode.F11))
        {
            ToggleWindowMode();
        }
        
        // ESCキーでアプリケーション終了（サイネージ用途）
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            QuitApplication();
        }
        
        // 標準設定：ボーダーなし状態でカーソルを非表示（ロックなし）
        if (useBorderlessWindow)
        {
            Cursor.visible = false;
        }
        else
        {
            Cursor.visible = true;
        }
    }
    
    void ApplyDisplaySettings()
    {
        if (!enableScaling) return;
        
        // 現在の画面解像度を取得
        int currentWidth = Screen.currentResolution.width;
        int currentHeight = Screen.currentResolution.height;
        
        // カメラの設定（ドットバイドット表示のため、ビューポートは変更しない）
        if (mainCamera != null)
        {
            // カメラのビューポートをデフォルト（全体表示）に設定
            mainCamera.rect = new Rect(0, 0, 1, 1);
        }
        
        // Canvasの設定
        if (canvas != null && canvasRectTransform != null)
        {
            // Canvas Scalerを完全に無効化
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.enabled = false;
            }
            
            // CanvasのレンダーモードをScreen Space - Overlayに設定
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            // Canvasのアンカーポイントを左上に設定
            canvasRectTransform.anchorMin = new Vector2(0, 1); // 左上
            canvasRectTransform.anchorMax = new Vector2(0, 1); // 左上
            canvasRectTransform.pivot = new Vector2(0, 1); // 左上
            canvasRectTransform.anchoredPosition = Vector2.zero; // 左上に配置
            
            // Canvasのサイズを固定（ピクセル単位）
            canvasRectTransform.sizeDelta = new Vector2(targetWidth, targetHeight);
            
            // Canvasのスケールを1.0に固定
            canvasRectTransform.localScale = Vector3.one;
        }
        
        Debug.Log($"Dot-by-Dot Display Applied - Target: {targetWidth}x{targetHeight}, Current: {currentWidth}x{currentHeight}, No Scaling");
    }
    
    void ToggleWindowMode()
    {
        // ボーダーあり/なしを切り替え
        useBorderlessWindow = !useBorderlessWindow;
        
        if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
            if (useBorderlessWindow)
            {
                SetBorderlessWindow();
                PositionWindowToTopLeft();
            }
            else
            {
                SetNormalWindow();
            }
        }
        
        // カーソル表示状態を即座に更新（ロックなし）
        if (useBorderlessWindow)
        {
            Cursor.visible = false;
        }
        else
        {
            Cursor.visible = true;
        }
        
        Debug.Log($"Window mode toggled: Borderless = {useBorderlessWindow}");
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
    
    // ボーダーなしウィンドウを設定
    void SetBorderlessWindow()
    {
        #if UNITY_STANDALONE_WIN
        IntPtr hwnd = GetActiveWindow();
        if (hwnd != IntPtr.Zero)
        {
            // ウィンドウスタイルを取得
            int style = GetWindowLong(hwnd, GWL_STYLE);
            
            // キャプション、ボーダー、最小化/最大化ボタン、システムメニューを削除
            style &= ~(WS_CAPTION | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX | WS_SYSMENU);
            
            // 新しいスタイルを適用
            SetWindowLong(hwnd, GWL_STYLE, style);
            
            // ウィンドウを再描画
            SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
            
            Debug.Log("Borderless window set");
        }
        #endif
    }
    
    // 通常のウィンドウを設定
    void SetNormalWindow()
    {
        #if UNITY_STANDALONE_WIN
        IntPtr hwnd = GetActiveWindow();
        if (hwnd != IntPtr.Zero)
        {
            // ウィンドウスタイルを取得
            int style = GetWindowLong(hwnd, GWL_STYLE);
            
            // キャプション、ボーダー、最小化/最大化ボタン、システムメニューを復元
            style |= (WS_CAPTION | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX | WS_SYSMENU);
            
            // 新しいスタイルを適用
            SetWindowLong(hwnd, GWL_STYLE, style);
            
            // ウィンドウを再描画
            SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
            
            Debug.Log("Normal window set");
        }
        #endif
    }
    
    // ウィンドウを左上に配置
    void PositionWindowToTopLeft()
    {
        #if UNITY_STANDALONE_WIN
        IntPtr hwnd = GetActiveWindow();
        if (hwnd != IntPtr.Zero)
        {
            // ウィンドウを左上（0, 0）に配置
            SetWindowPos(hwnd, IntPtr.Zero, 0, 0, targetWidth, targetHeight, SWP_NOZORDER);
            
            Debug.Log($"Window positioned to top-left: {targetWidth}x{targetHeight}");
        }
        #endif
    }
    
    // 遅延してウィンドウ位置を設定
    IEnumerator SetWindowPositionDelayed()
    {
        // 数フレーム待機してウィンドウが作成されるのを待つ
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        
        // ボーダーなしウィンドウを設定
        SetBorderlessWindow();
        
        // さらに数フレーム待機
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        
        // 左上に配置
        PositionWindowToTopLeft();
        
        Debug.Log("Window setup completed");
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