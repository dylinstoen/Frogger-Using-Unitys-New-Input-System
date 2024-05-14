using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class Frogger : MonoBehaviour
{
    
    private PlayerControls playerControls;
    private PlayerControls.FroggerActions froggerActions;
    private Camera mainCamera;
    private bool isMoving = false;
    private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite leapSprite;
    [SerializeField] private Sprite deadSprite;
    private Queue<Vector3> movementQueue = new Queue<Vector3>();
    [SerializeField] private float movementSpeed = 5.0f;  // Speed of movement
    private Vector3 spawnPosition;
    private float farthestRow;

    private void Awake()
    {
        playerControls = new PlayerControls();
        froggerActions = playerControls.Frogger;
        mainCamera = Camera.main; // Cache the main camera
        spawnPosition = transform.position;
    }

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
 

    public void Death()
    {
        //...
        StopAllCoroutines();

        transform.rotation = Quaternion.identity;
        spriteRenderer.sprite = deadSprite;
        enabled = false; // Disable script
        
        GameManager.Instance.Died();
    }

    public void Respawn()
    {

        StopAllCoroutines(); // In case you respawn without dieing

        transform.rotation = Quaternion.identity; // In case you respawn without dieing
        transform.position = spawnPosition;
        farthestRow = spawnPosition.y;
        spriteRenderer.sprite = idleSprite;
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
        enabled = true;
    }

    void Update()
    {
        
        ProcessLook();
        if (!isMoving && movementQueue.Count > 0)
        {
            Vector3 direction = movementQueue.Dequeue();
            Vector3 destination = transform.position + direction;
            Collider2D barrier = Physics2D.OverlapBox(destination, Vector2.zero, 0f, LayerMask.GetMask("Barrier"));
            if (barrier != null)
            {
                return;
            }
            Collider2D platform = Physics2D.OverlapBox(destination, Vector2.zero, 0f, LayerMask.GetMask("Platform"));
            Collider2D obstacle = Physics2D.OverlapBox(destination, Vector2.zero, 0f, LayerMask.GetMask("Obstacle"));
            if (platform != null)
            {
                transform.SetParent(platform.transform);
            }
            else
            {
                transform.SetParent(null);
            }
            if (obstacle != null && platform == null)
            {
                transform.position = destination;
                Death();
            }
            else
            {
                if (destination.y > farthestRow)
                {
                    farthestRow = destination.y;
                    GameManager.Instance.NewFarthestRow();
                }
                StartCoroutine(Move(direction));
            }
            

        }
    }
    private void ProcessLook()
    {
        Vector2 look = Vector2.zero;
        if (froggerActions.Look.triggered) // // Use keyboard input
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue(); // Read the mouse position
            Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(mousePosition); // Convert to world position
            mouseWorldPosition.z = 0;
            look = (mouseWorldPosition - transform.position).normalized; // Direction from the sprite to the mouse
        }
        else // Use joystick input
        {
            look = froggerActions.Look.ReadValue<Vector2>();
            if (look != Vector2.zero)
            {
                look = (Vector2)(transform.position + (Vector3)look) - (Vector2)transform.position;
            }
        }
        if (look != Vector2.zero)
        {
            float angle = Mathf.Atan2(look.y, look.x) * Mathf.Rad2Deg; // Calculate the angle
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90)); // Apply rotation
        }
    }

    private IEnumerator Move(Vector3 direction)
    {
        isMoving = true;
        Vector3 startPosition = transform.localPosition;  // Use local position
        Vector3 endPosition = startPosition + direction;

        float journeyLength = Vector3.Distance(startPosition, endPosition);
        float startTime = Time.time;
        spriteRenderer.sprite = leapSprite;

        while (isMoving && Vector3.Distance(transform.localPosition, endPosition) > 0)
        {
            float distCovered = (Time.time - startTime) * movementSpeed;
            float fractionOfJourney = distCovered / journeyLength;
            transform.localPosition = Vector3.Lerp(startPosition, endPosition, fractionOfJourney);
            yield return null;
        }

        transform.localPosition = endPosition;  // Snap to grid position locally
        spriteRenderer.sprite = idleSprite;
        isMoving = false;
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        Vector2 moveInput = context.ReadValue<Vector2>();
        Vector3 direction = new Vector3(moveInput.x, moveInput.y, 0);
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y)) // Normalize and ensure non-diagonal movement by selecting one major direction
        {
            direction = new Vector3(direction.x > 0 ? 1 : -1, 0, 0);
        }
        else if (Mathf.Abs(direction.y) > Mathf.Abs(direction.x))
        {
            direction = new Vector3(0, direction.y > 0 ? 1 : -1, 0);
        }
        else
        {
            return; // If movement is diagonal or no input, do not enqueue any movement
        }

        if (direction != Vector3.zero && !isMoving)
        {
            movementQueue.Enqueue(direction);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (enabled && other.gameObject.layer == LayerMask.NameToLayer("Obstacle") && transform.parent == null) // if we hit an obstacle and were not on a platform (platforms set frogger to child) then we die
        {
            Death();
        }
    }


 
    private void OnEnable()
    {
        isMoving = false;  // Reset movement state        
        if (spriteRenderer == null) // Ensure all component references are set up
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        froggerActions.Enable(); // Ensure the input actions are enabled
        froggerActions.Move.performed += OnMovePerformed; // Subscribe to the move action
    }
    private void OnDisable()
    {
        froggerActions.Move.performed -= OnMovePerformed; // Unsubscribe to prevent memory leaks
        froggerActions.Disable(); // Disable the input actions
    }

}
