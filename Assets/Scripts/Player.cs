using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    private Camera mainCamera;
    private Rigidbody2D rb;
    private PlayerInputActions inputActions;

    [SerializeField] private float maxSwimSpeed = 8f;
    [SerializeField] private float swimForce = 10f;
    [SerializeField] private float drag = 2f;

    [SerializeField] private float rotationSpeed = 300f;

    [SerializeField] private float sprintBurstStrength = 20f;
    [SerializeField] private float sprintMultiplier = 1.8f;
    [SerializeField] private float sprintForceMultiplier = 1.5f;
    private bool sprintBurst = false;
    private bool sprinting = false;

    void Awake()
    {
        inputActions = new PlayerInputActions();
        inputActions.Player.Enable();
    }

    void Start()
    {
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        ToMouseMovement(maxSwimSpeed);
    }

    private void ToMouseMovement(float maxSpeed)
    {
        Vector2 directionToMouse = VectorToMouse();
        if (directionToMouse.sqrMagnitude < 0.01f) return; //if close enough to mouse, no movement 

        directionToMouse = directionToMouse.normalized;
        float mouseAngle = Mathf.Atan2(directionToMouse.y, directionToMouse.x) * Mathf.Rad2Deg;
        float newAngle = Mathf.MoveTowardsAngle(rb.rotation, mouseAngle, rotationSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(newAngle);

        float currentMaxSpeed = sprinting ? maxSpeed * sprintMultiplier : maxSpeed;
        float currentForce = sprinting ? swimForce * sprintForceMultiplier : swimForce; 
        
        float distance = directionToMouse.magnitude;
        float targetSpeed = Mathf.Min(currentMaxSpeed, distance * currentForce);
        Vector2 targetVelocity = directionToMouse * targetSpeed;
        Vector2 velocityDifference = targetVelocity - rb.linearVelocity;
        if (!sprintBurst)
        {
            rb.AddForce(velocityDifference * currentForce);
            rb.AddForce(-rb.linearVelocity * drag);
        }
        else
        {
            rb.AddForce(VectorToMouse().normalized * sprintBurstStrength);
        }
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            sprinting = true;
            StartCoroutine(SprintBurst());
        }
        else if (context.canceled)
        {
            sprinting = false;
        }
    }

    private IEnumerator SprintBurst()
    {
        sprintBurst = true;
        yield return new WaitForSeconds(0.25f);
        sprintBurst = false;
    }

    private Vector2 VectorToMouse()
    {
        Vector2 mousePosition = GetMouseWorldPosition();
        return mousePosition - rb.position;
    }

    private Vector2 GetMouseWorldPosition()
    {
        return mainCamera.ScreenToWorldPoint(inputActions.Player.MousePosition.ReadValue<Vector2>());
    }

    void OnEnable()
    {
        inputActions.Player.Sprint.started += OnSprint;
        inputActions.Player.Sprint.canceled += OnSprint;
    }
    void OnDisable()
    {
        inputActions.Player.Sprint.started -= OnSprint;
        inputActions.Player.Sprint.canceled -= OnSprint;
    }
}