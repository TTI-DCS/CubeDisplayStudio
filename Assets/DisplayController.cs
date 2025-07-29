using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DisplayController : MonoBehaviour
{
    [Header("表示切り替え設定")]
    [SerializeField] private GameObject[] displayObjects; // 表示切り替えするゲームオブジェクトの配列
    [SerializeField] private float switchInterval = 2.0f; // 切り替え間隔（秒）
    [SerializeField] private bool autoStart = true; // 自動開始するかどうか
    
    private int currentIndex = 0; // 現在表示中のオブジェクトのインデックス
    private Coroutine switchCoroutine; // 切り替えコルーチン
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 初期化
        InitializeDisplay();
        
        // 自動開始が有効な場合、切り替えを開始
        if (autoStart)
        {
            StartSwitching();
        }
    }
    
    // 表示の初期化
    private void InitializeDisplay()
    {
        if (displayObjects == null || displayObjects.Length == 0)
        {
            Debug.LogWarning("DisplayController: 表示オブジェクトが設定されていません。");
            return;
        }
        
        // 最初のオブジェクト以外を非表示にする
        for (int i = 0; i < displayObjects.Length; i++)
        {
            if (displayObjects[i] != null)
            {
                displayObjects[i].SetActive(i == 0);
            }
        }
        
        currentIndex = 0;
    }
    
    // 表示切り替えを開始
    public void StartSwitching()
    {
        if (displayObjects == null || displayObjects.Length <= 1)
        {
            Debug.LogWarning("DisplayController: 切り替え可能なオブジェクトが不足しています。");
            return;
        }
        
        if (switchCoroutine != null)
        {
            StopCoroutine(switchCoroutine);
        }
        
        switchCoroutine = StartCoroutine(SwitchDisplayCoroutine());
    }
    
    // 表示切り替えを停止
    public void StopSwitching()
    {
        if (switchCoroutine != null)
        {
            StopCoroutine(switchCoroutine);
            switchCoroutine = null;
        }
    }
    
    // 表示切り替えのコルーチン
    private IEnumerator SwitchDisplayCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(switchInterval);
            SwitchToNextObject();
        }
    }
    
    // 次のオブジェクトに切り替え
    public void SwitchToNextObject()
    {
        if (displayObjects == null || displayObjects.Length <= 1)
            return;
        
        // 現在のオブジェクトを非表示
        if (displayObjects[currentIndex] != null)
        {
            displayObjects[currentIndex].SetActive(false);
        }
        
        // 次のインデックスに移動
        currentIndex = (currentIndex + 1) % displayObjects.Length;
        
        // 新しいオブジェクトを表示
        if (displayObjects[currentIndex] != null)
        {
            displayObjects[currentIndex].SetActive(true);
        }
        
        Debug.Log($"DisplayController: オブジェクト {currentIndex} に切り替えました。");
    }
    
    // 前のオブジェクトに切り替え
    public void SwitchToPreviousObject()
    {
        if (displayObjects == null || displayObjects.Length <= 1)
            return;
        
        // 現在のオブジェクトを非表示
        if (displayObjects[currentIndex] != null)
        {
            displayObjects[currentIndex].SetActive(false);
        }
        
        // 前のインデックスに移動
        currentIndex = (currentIndex - 1 + displayObjects.Length) % displayObjects.Length;
        
        // 新しいオブジェクトを表示
        if (displayObjects[currentIndex] != null)
        {
            displayObjects[currentIndex].SetActive(true);
        }
        
        Debug.Log($"DisplayController: オブジェクト {currentIndex} に切り替えました。");
    }
    
    // 特定のインデックスのオブジェクトに切り替え
    public void SwitchToObject(int index)
    {
        if (displayObjects == null || index < 0 || index >= displayObjects.Length)
        {
            Debug.LogWarning($"DisplayController: 無効なインデックス {index} です。");
            return;
        }
        
        // 現在のオブジェクトを非表示
        if (displayObjects[currentIndex] != null)
        {
            displayObjects[currentIndex].SetActive(false);
        }
        
        // 指定されたインデックスに切り替え
        currentIndex = index;
        
        // 新しいオブジェクトを表示
        if (displayObjects[currentIndex] != null)
        {
            displayObjects[currentIndex].SetActive(true);
        }
        
        Debug.Log($"DisplayController: オブジェクト {currentIndex} に切り替えました。");
    }
    
    // 切り替え間隔を変更
    public void SetSwitchInterval(float interval)
    {
        switchInterval = Mathf.Max(0.1f, interval); // 最小0.1秒
        
        // 現在実行中のコルーチンを再起動
        if (switchCoroutine != null)
        {
            StopSwitching();
            StartSwitching();
        }
    }
    
    // 現在のインデックスを取得
    public int GetCurrentIndex()
    {
        return currentIndex;
    }
    
    // オブジェクト数を取得
    public int GetObjectCount()
    {
        return displayObjects != null ? displayObjects.Length : 0;
    }
    
    // 現在表示中のオブジェクトを取得
    public GameObject GetCurrentObject()
    {
        if (displayObjects != null && currentIndex >= 0 && currentIndex < displayObjects.Length)
        {
            return displayObjects[currentIndex];
        }
        return null;
    }
}
