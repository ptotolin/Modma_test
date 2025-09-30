using UnityEngine;
using System.Diagnostics;

public class FPSCounter : MonoBehaviour
{
    [Header("FPS Settings")]
    [SerializeField] private float updateInterval = 0.5f;
    
    private float currentFPS = 0f;
    private int frameCount = 0;
    private Stopwatch stopwatch;
    
    public float CurrentFPS => currentFPS;
    
    // Instant FPS using high-precision Stopwatch
    public float InstantFPS => 1f / Time.unscaledDeltaTime;
    
    private void Start()
    {
        stopwatch = Stopwatch.StartNew();
    }
    
    private void Update()
    {
        frameCount++;
        
        // High precision timing using Stopwatch
        double elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
        
        if (elapsedSeconds >= updateInterval)
        {
            currentFPS = frameCount / (float)elapsedSeconds;
            frameCount = 0;
            stopwatch.Restart();
        }
    }
    
    /// <summary>
    /// Get current FPS as integer
    /// </summary>
    public int GetFPSAsInt()
    {
        return Mathf.RoundToInt(currentFPS);
    }
    
    /// <summary>
    /// Get current FPS as formatted string
    /// </summary>
    public string GetFPSAsString(int decimals = 1)
    {
        return currentFPS.ToString($"F{decimals}");
    }
    
    /// <summary>
    /// Reset FPS counter
    /// </summary>
    public void Reset()
    {
        currentFPS = 0f;
        frameCount = 0;
        stopwatch?.Restart();
    }
}
