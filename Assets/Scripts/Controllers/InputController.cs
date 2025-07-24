using UnityEngine;
using System;
using Unity.VisualScripting;

public class InputController : MonoBehaviour, IController
{
    [Header("Input Settings")]
    [SerializeField] private LayerMask gemLayerMask = 1;
    [SerializeField] private float swipeThreshold = 0.5f;

    private Camera mainCamera;
    private bool inputEnabled = true;
    private Vector3 startTouchPosition;
    private Vector3 endTouchPosition;
    private GemView selectedGem;

    public event Action<Vector2Int, Vector2Int> OnSwipeDetected;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    public void Initialize()
    {
        EnableInput();
    }

    public void EnableInput()
    {
        inputEnabled = true;
    }

    public void DisableInput()
    {
        inputEnabled = false;
        selectedGem = null;
    }

    private void Update()
    {
        if (!inputEnabled) return;

        HandleInput();
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleTouchStart(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            HandleTouchEnd(Input.mousePosition);
        }
    }

    private void HandleTouchStart(Vector3 screenPosition)
    {
        startTouchPosition = screenPosition;
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
        worldPosition.z = 0;

        RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero, Mathf.Infinity, gemLayerMask);
        if (hit.collider != null)
        {
            selectedGem = hit.collider.GetComponent<GemView>();
            if (selectedGem != null)
            {
                selectedGem.Select();
            }
        }
    }

    private void HandleTouchEnd(Vector3 screenPosition)
    {
        if (selectedGem == null) return;

        endTouchPosition = screenPosition;
        Vector3 swipeDirection = endTouchPosition - startTouchPosition;

        if (swipeDirection.magnitude >= swipeThreshold * Screen.dpi / 160f)
        {
            Vector2Int swipeDir = GetSwipeDirection(swipeDirection);
            Vector2Int currentPos = selectedGem.GridPosition;
            Vector2Int targetPos = currentPos + swipeDir;

            OnSwipeDetected?.Invoke(currentPos, targetPos);
        }

        selectedGem.Deselect();
        selectedGem = null;
    }

    private Vector2Int GetSwipeDirection(Vector3 swipe)
    {
        Vector2 normalizedSwipe = swipe.normalized;

        if (Mathf.Abs(normalizedSwipe.x) > Mathf.Abs(normalizedSwipe.y))
        {
            return normalizedSwipe.x > 0 ? Vector2Int.right : Vector2Int.left;
        }
        else
        {
            return normalizedSwipe.y > 0 ? Vector2Int.up : Vector2Int.down;
        }
    }

    public void Cleanup()
    {
        DisableInput();
    }
}