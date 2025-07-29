using UnityEngine;
using UnityEngine.UI;

public class HeartBeatAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float pulseSpeed = 2f;        // 鼓動の速度
    [SerializeField] private float minScale = 0.8f;        // 最小スケール
    [SerializeField] private float maxScale = 1.2f;        // 最大スケール
    [SerializeField] private AnimationCurve pulseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // アニメーションカーブ
    
    private Image uiImage;
    private RectTransform rectTransform;
    private Vector3 originalScale;
    private float animationTime;
    
    void Start()
    {
        // UI Imageコンポーネントを取得
        uiImage = GetComponent<Image>();
        if (uiImage == null)
        {
            Debug.LogError("HeartBeatAnimation: Image component not found on this GameObject!");
            enabled = false;
            return;
        }
        
        // RectTransformを取得
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            Debug.LogError("HeartBeatAnimation: RectTransform component not found on this GameObject!");
            enabled = false;
            return;
        }
        
        // 元のスケールを保存
        originalScale = rectTransform.localScale;
        animationTime = 0f;
    }

    void Update()
    {
        // アニメーション時間を更新
        animationTime += Time.deltaTime * pulseSpeed;
        
        // 0-1の範囲でループ
        float normalizedTime = Mathf.PingPong(animationTime, 1f);
        
        // アニメーションカーブを使用してスケールを計算
        float curveValue = pulseCurve.Evaluate(normalizedTime);
        float currentScale = Mathf.Lerp(minScale, maxScale, curveValue);
        
        // スケールを適用
        rectTransform.localScale = originalScale * currentScale;
    }
    
    // アニメーションの一時停止/再開
    public void PauseAnimation()
    {
        enabled = false;
    }
    
    public void ResumeAnimation()
    {
        enabled = true;
    }
    
    // アニメーション設定の変更
    public void SetPulseSpeed(float speed)
    {
        pulseSpeed = speed;
    }
    
    public void SetScaleRange(float min, float max)
    {
        minScale = min;
        maxScale = max;
    }
}
