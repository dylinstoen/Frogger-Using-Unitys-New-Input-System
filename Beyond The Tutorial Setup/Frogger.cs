using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.Windows;
using static PlayerControls;
using static UnityEditor.Timeline.TimelinePlaybackControls;
[RequireComponent(typeof(SpriteRenderer))]
public class Frogger : MonoBehaviour
{
    private bool isMoving = false;
    public bool IsMoving { get { return isMoving; } }
    private SpriteRenderer spriteRenderer;
    private Camera mainCamera;
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite leapSprite;
    [SerializeField] private Sprite deadSprite;
    [SerializeField] private float movementSpeed = 5.0f;
    private Vector3 spawnPosition;
    private float farthestRow;
    private bool cooldown;
    private void Awake()
    {
        spawnPosition = transform.position;
        mainCamera = Camera.main; // Cache the main camera
    }

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    public void HandleMove(Vector3 input)
    {
        if (!isMoving)
        {
            StopAllCoroutines();
            StartCoroutine(Move(input));
        }  
    }

    public IEnumerator Move(Vector3 direction)
    {
        if (cooldown)
        {
            yield break;
        }
        Vector3 destination = transform.position + direction;
        // Check for barriers
        Collider2D barrier = Physics2D.OverlapBox(destination, Vector2.one * 0.5f, 0f, LayerMask.GetMask("Barrier"));
        if (barrier != null)
        {
            yield break; // Stop if there is a barrier
        }

        // Check for platforms
        Collider2D platform = Physics2D.OverlapBox(destination, Vector2.one * 0.5f, 0f, LayerMask.GetMask("Platform"));
        if (platform != null)
        {
            transform.SetParent(platform.transform);
        }
        else
        {
            transform.SetParent(null);
        }

        // Check for obstacles
        Collider2D obstacle = Physics2D.OverlapBox(destination, Vector2.one * 0.5f, 0f, LayerMask.GetMask("Obstacle"));
        if (obstacle != null && platform == null)
        {

            transform.position = destination;
            Death(); // Handle death, assume Death() is defined elsewhere
            yield break;
        }
        // Check if we have advanced to a farther row
        if (destination.y > farthestRow)
        {
            farthestRow = destination.y;
            GameManager.Instance.UpdateFurthestRow();
        }
        if (!isMoving)
        {
            isMoving = true;
            Vector3 startPosition = transform.position;
            float duration = 0.5f; // Duration of the move
            float elapsedTime = 0;
            spriteRenderer.sprite = leapSprite;
            while (elapsedTime < duration)
            {
                transform.position = Vector3.Lerp(startPosition, destination, elapsedTime / duration);
                elapsedTime += Time.deltaTime * movementSpeed;
                yield return null;
            }
            transform.position = destination;
            spriteRenderer.sprite = idleSprite;
            isMoving = false;
        }
    }

    private void Look(Vector2 lookInput)
    {
        Vector2 look = lookInput;
        if (look != Vector2.zero)
        {
            if (Mouse.current != null && Mouse.current.position.ReadValue() != Vector2.zero) // Checks if mouse is being used
            {
                Vector2 mousePosition = Mouse.current.position.ReadValue(); // Read the mouse position
                Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, mainCamera.nearClipPlane));
                mouseWorldPosition.z = 0;
                look = (mouseWorldPosition - transform.position).normalized; // Direction from the sprite to the mouse
            }
            else // Use joystick input directly from lookInput
            {
                Vector3 worldLookPoint = transform.position + (Vector3)look; // Convert look input to world point relative to the player
                look = (worldLookPoint - transform.position).normalized;
            }
            float angle = Mathf.Atan2(look.y, look.x) * Mathf.Rad2Deg; // Calculate the angle
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90)); // Apply rotation
        }
    }

    public void Respawn()
    {

        StopAllCoroutines(); // In case you respawn without dieing
        transform.SetPositionAndRotation(spawnPosition, Quaternion.identity);
        farthestRow = spawnPosition.y;
        // Reset sprite
        spriteRenderer.sprite = idleSprite;

        // Enable control
        gameObject.SetActive(true);
        enabled = true;
        cooldown = false;
    }

    public void Death()
    {
        // Stop animations
        StopAllCoroutines();

        // Disable control
        enabled = false;

        // Display death sprite
        transform.rotation = Quaternion.identity;
        spriteRenderer.sprite = deadSprite;

        // Update game state
        GameManager.Instance.Died();
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        bool hitObstacle = other.gameObject.layer == LayerMask.NameToLayer("Obstacle");
        bool onPlatform = transform.parent != null;

        if (enabled && hitObstacle && !onPlatform)
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
        // Subscribe to the input events through InputManager
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnLook += Look;
            InputManager.Instance.MoveEvent += HandleMove;

        }
        
    }

    private void OnDestroy()
    {
        if (spriteRenderer == null) // Ensure all component references are set up
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnLook -= Look;
            InputManager.Instance.MoveEvent -= HandleMove;

        }
        
    }

    private void OnDisable()
    {
        // Unsubscribe from the input events to prevent memory leaks
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnLook -= Look;
            InputManager.Instance.MoveEvent -= HandleMove;
        }
        
    }

}
