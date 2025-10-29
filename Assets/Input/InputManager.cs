using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    // Singleton
    public static InputManager Instance { get; private set; }
    
    [SerializeField] private InputActionAsset inputActionAsset;

    private InputAction _click;
    private InputAction _rightClick;
    private InputAction _sClick;
    private InputAction _flowFieldRequest;

    // Events
    public static event Action<Vector2> OnLeftClick;
    public static event Action<Vector2> OnRightClick;
    public static event Action OnSpawnUnit;
    public static event Action OnFlowFieldRequest;

    void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // Get actions
        var inputActionMap = inputActionAsset.FindActionMap("Main");
        _click = inputActionMap.FindAction("Click");
        _rightClick = inputActionMap.FindAction("RightClick");
        _sClick = inputActionMap.FindAction("SpawnUnit");
        _flowFieldRequest = inputActionMap.FindAction("FlowFieldRequest");

        // Subscribe to callbacks (better than Update polling)
        _click.performed += OnClickPerformed;
        _rightClick.performed += OnRightClickPerformed;
        _sClick.performed += OnSpawnPerformed;
        _flowFieldRequest.performed += OnFlowFieldRequestPerformed;
    }

    private void OnEnable()
    {
        _click?.Enable();
        _rightClick?.Enable();
        _sClick?.Enable();
        _flowFieldRequest?.Enable();
    }

    private void OnDisable()
    {
        _click?.Disable();
        _rightClick?.Disable();
        _sClick?.Disable(); 
        _flowFieldRequest?.Disable();
    }

    private void OnDestroy()
    {
        _click.performed -= OnClickPerformed;
        _rightClick.performed -= OnRightClickPerformed;
        _sClick.performed -= OnSpawnPerformed;
        _flowFieldRequest.performed -= OnFlowFieldRequestPerformed;
    }

    private void OnClickPerformed(InputAction.CallbackContext context)
    {
        if (Mouse.current != null) // ← Null check
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            OnLeftClick?.Invoke(mousePos);
        }
    }

    private void OnRightClickPerformed(InputAction.CallbackContext context)
    {
        if (Mouse.current != null)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            OnRightClick?.Invoke(mousePos);
        }
    }

    private void OnSpawnPerformed(InputAction.CallbackContext context)
    {
        OnSpawnUnit?.Invoke();
    }

    private void OnFlowFieldRequestPerformed(InputAction.CallbackContext context)
    {
        OnFlowFieldRequest?.Invoke();
    }
}