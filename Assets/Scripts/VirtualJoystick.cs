using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    // Events
    public event Action<Vector2> OnJoystickMoved;
    public event Action OnJoystickPressed;
    public event Action OnJoystickReleased;
    
    [Header("Joystick Settings")]
    [SerializeField] private float joystickRadius = 50f;
    [SerializeField] private float deadZone = 0.1f;
    
    [Header("Visual Components")]
    [SerializeField] private RectTransform joystickBackground;
    [SerializeField] private RectTransform joystickHandle;
    [SerializeField] private CanvasGroup canvasGroup;
    
    [Header("Fade Settings")]
    [SerializeField] private float fadeInDuration = 0.2f;
    [SerializeField] private float fadeOutDuration = 0.3f;
    [SerializeField] private float activeAlpha = 0.7f;
    
    private Vector2 inputVector;
    private Vector2 joystickCenter;
    private bool isPressed = false;
    private Camera uiCamera;
    private Canvas parentCanvas;
    
    
    // Properties
    public Vector2 Direction => inputVector;
    public float Magnitude => inputVector.magnitude;
    public bool IsPressed => isPressed;

    private void Awake()
    {
        parentCanvas = GetComponentInParent<Canvas>();
        
        if (parentCanvas.renderMode == RenderMode.ScreenSpaceCamera)
            uiCamera = parentCanvas.worldCamera;
        else if (parentCanvas.renderMode == RenderMode.WorldSpace)
            uiCamera = Camera.main;
        
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
        
        SetJoystickVisibility(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Position the joystick a touch place
        Vector2 touchPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform as RectTransform, 
            eventData.position, 
            uiCamera, 
            out touchPosition
        );
        
        joystickBackground.anchoredPosition = touchPosition;
        joystickHandle.anchoredPosition = touchPosition;
        joystickCenter = touchPosition;
        
        SetJoystickVisibility(true);
        FadeIn();
        
        isPressed = true;
        OnJoystickPressed?.Invoke();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isPressed) return;
        
        Vector2 currentTouchPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform as RectTransform,
            eventData.position,
            uiCamera,
            out currentTouchPosition
        );
        
        Vector2 direction = currentTouchPosition - joystickCenter;
        
        if (direction.magnitude > joystickRadius) {
            direction = direction.normalized * joystickRadius;
        }
        
        joystickHandle.anchoredPosition = joystickCenter + direction;
        
        inputVector = direction / joystickRadius;
        
        if (inputVector.magnitude < deadZone) {
            inputVector = Vector2.zero;
        }
        
        OnJoystickMoved?.Invoke(inputVector);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Reset
        inputVector = Vector2.zero;
        isPressed = false;
        joystickHandle.anchoredPosition = joystickCenter;
        
        // hide joystick
        FadeOut();
        
        OnJoystickReleased?.Invoke();
        OnJoystickMoved?.Invoke(Vector2.zero);
    }
    
    private void SetJoystickVisibility(bool visible)
    {
        joystickBackground.gameObject.SetActive(visible);
        joystickHandle.gameObject.SetActive(visible);
    }
    
    private void FadeIn()
    {
        StopAllCoroutines();
        StartCoroutine(FadeCoroutine(canvasGroup.alpha, activeAlpha, fadeInDuration));
    }
    
    private void FadeOut()
    {
        StopAllCoroutines();
        StartCoroutine(FadeCoroutine(canvasGroup.alpha, 0f, fadeOutDuration, () => {
            SetJoystickVisibility(false);
        }));
    }
    
    private IEnumerator FadeCoroutine(float startAlpha, float targetAlpha, float duration, System.Action onComplete = null)
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            
            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            
            yield return null;
        }
        
        if (canvasGroup != null)
            canvasGroup.alpha = targetAlpha;
        
        onComplete?.Invoke();
    }
    
    public Vector2 GetInputDirection()
    {
        return inputVector;
    }
    
    public float GetHorizontalInput()
    {
        return inputVector.x;
    }
    
    public float GetVerticalInput()
    {
        return inputVector.y;
    }
}