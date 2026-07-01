using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class ClickCountEventTrigger : MonoBehaviour
{
    // ========================= BASE EVENT =========================

    [Header("Base Touch Event")]
    public UnityEvent OnTouched;

    // ========================= CLICK COUNT EVENTS =========================

    [System.Serializable]
    public class ClickEvent
    {
        [Header("Click Condition")]
        public int numberOfClicks = 1;

        public UnityEvent onClickReached;

        [Header("Trigger Settings")]
        public bool allowMultipleTriggers = true;

        [HideInInspector] public int currentClicks;
        [HideInInspector] public bool hasTriggered;
    }

    [Header("Click Events")]
    [SerializeField] private List<ClickEvent> clickEvents = new List<ClickEvent>();

    // ========================= SETTINGS =========================

    [Header("References")]
    [SerializeField] private Camera targetCamera;

    [Header("Behavior")]
    [SerializeField] private bool ignoreUI = true;

    // ========================= INTERNAL =========================

    private Collider cachedCollider;

    // ========================= LIFECYCLE =========================

    private void Awake()
    {
        cachedCollider = GetComponent<Collider>();

        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    private void OnEnable()
    {
        ResetAll();
    }

    private void Update()
    {
        if (targetCamera == null)
            return;

        // -------- NEW INPUT SYSTEM (Primary) --------
        bool inputTriggered = false;
        Vector2 screenPos = Vector2.zero;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            inputTriggered = true;
            screenPos = Mouse.current.position.ReadValue();
        }

        if (!inputTriggered && Touchscreen.current != null)
        {
            var touch = Touchscreen.current.primaryTouch;

            if (touch.press.wasPressedThisFrame)
            {
                inputTriggered = true;
                screenPos = touch.position.ReadValue();
            }
        }

        // -------- FALLBACK (CRITICAL FIX) --------
        if (!inputTriggered && Input.GetMouseButtonDown(0))
        {
            inputTriggered = true;
            screenPos = Input.mousePosition;
        }

        if (inputTriggered)
        {
            ProcessPointer(screenPos);
        }
    }

    // ========================= INPUT PROCESSING =========================

    private void ProcessPointer(Vector2 screenPosition)
    {
        if (ignoreUI && EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        Ray ray = targetCamera.ScreenPointToRay(screenPosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, 1000f))
            return;

        if (hit.collider != cachedCollider)
            return;

        HandleClick();
    }

    private void HandleClick()
    {
        OnTouched?.Invoke();

        foreach (var entry in clickEvents)
        {
            if (!entry.allowMultipleTriggers && entry.hasTriggered)
                continue;

            entry.currentClicks++;

            if (entry.currentClicks >= entry.numberOfClicks)
            {
                entry.hasTriggered = true;
                entry.onClickReached?.Invoke();

                entry.currentClicks = entry.allowMultipleTriggers ? 0 : entry.numberOfClicks;
            }
        }
    }

    // ========================= PUBLIC API =========================

    public void ResetAll()
    {
        foreach (var entry in clickEvents)
        {
            entry.currentClicks = 0;
            entry.hasTriggered = false;
        }
    }
}