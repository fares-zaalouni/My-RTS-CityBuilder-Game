using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
  [SerializeField] private InputActionAsset InputActionAsset;

  private InputAction _Click;
  private InputAction _RightClick;

  //Events
  public static event Action<Vector2> OnLeftClick;
  public static event Action<Vector2> OnRightClick;

  void Awake()
  {
    var inputActionMap = InputActionAsset.FindActionMap("Main");
    _Click = inputActionMap.FindAction("Click");
    _RightClick = inputActionMap.FindAction("RightClick");

  }

  private void OnEnable()
  {
    _Click.Enable();
    _RightClick.Enable();
  }

  void Update()
  {
    if (_Click.WasCompletedThisFrame())
    {
      OnLeftClick?.Invoke(Mouse.current.position.ReadValue());
    }
    if (_RightClick.WasCompletedThisFrame())
    {
      OnRightClick?.Invoke(Mouse.current.position.ReadValue());
    }
  }

  private void OnDisable()
  {
    _Click.Disable();
    _RightClick.Disable();
  }
}
