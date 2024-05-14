using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{

    public static InputManager Instance { get; private set; }

    private PlayerControls controls;
    public Vector2 Direction { get; set; }
    public event Action<Vector2> OnLook;
    public event Action OnInteract;
    public event Action<Vector3> MoveEvent;
    private void Awake()
    {
        if (Instance != null)
        {
            DestroyImmediate(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        controls = new PlayerControls();
        controls.Frogger.Enable();
        controls.Frogger.Move.performed += OnMove;
        controls.Frogger.Look.performed += ctx => OnLook?.Invoke(ctx.ReadValue<Vector2>());
        controls.Frogger.Interact.performed += ctx => OnInteract?.Invoke();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        Vector2 moveInput = context.ReadValue<Vector2>();
        Vector3 direction;
        Debug.Log(moveInput);
        if (Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y))
        {
            direction = new Vector3(Mathf.Sign(moveInput.x), 0, 0);
        }
        else if (Mathf.Abs(moveInput.y) > 0)
        {
            direction = new Vector3(0, Mathf.Sign(moveInput.y), 0);
        }
        else
        {
            direction = Vector3.zero;
        }

        MoveEvent?.Invoke(direction);
    }
    

    private void OnEnable()
    {
        controls.Enable();  // Ensure controls are enabled
    }

    private void OnDisable()
    {
        controls.Disable();  // Disable controls to clean up
    }
}
