using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    #region Dependencies 
    private Camera mainCamera;
    private Rigidbody2D rb;
    private PlayerInputActions inputActions;
    #endregion

    #region Health & Stamina Variables
    [Header ("Health & Stamina")]
    [SerializeField] private float maxStamina = 60f;
    [SerializeField] private float staminaRegenerationRate = 12f;
    [SerializeField] private float staminaMultiplier = 84f;
    private float stamina = 60f;
    private bool staminaRegenerating = false;
    #endregion

    #region Movement Variables 
    [Header ("Movement")]
    [SerializeField] private float maxSwimSpeed = 16f;
    [SerializeField] private float swimForce = 10f;
    [SerializeField] private float drag = 2f;

    [SerializeField] private float rotationSpeed = 300f;

    [SerializeField] private float sprintBurstStrength = 36f;
    [SerializeField] private float sprintMultiplier = 1.8f;
    [SerializeField] private float sprintForceMultiplier = 1.5f;
    private bool sprintBurst = false;
    private bool sprinting = false;
    #endregion

    #region Inventory Variables
    [Header ("Inventory")]
    [SerializeField] private string[] collectableItems;
    private int[] itemsCollected;
    #endregion

    #region Awake 
    void Awake()
    {
        inputActions = new PlayerInputActions();
        inputActions.Player.Enable();
    }
    #endregion

    #region Start
    void Start()
    {
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody2D>();
        itemsCollected = new int[collectableItems.Length];
        InitialiseHealthStamina();
    }

    private void InitialiseHealthStamina()
    {
        stamina = maxStamina;
    }
    #endregion

    #region Update 
    void Update()
    {
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

        if (stamina <= 0) StopSprint();
        float currentMaxSpeed = sprinting && stamina > 0 ? maxSpeed * sprintMultiplier : maxSpeed;
        float currentForce = sprinting && stamina > 0 ? swimForce * sprintForceMultiplier : swimForce; 
        
        float distance = directionToMouse.magnitude;
        float targetSpeed = Mathf.Min(currentMaxSpeed, distance * currentForce);
        Vector2 targetVelocity = directionToMouse * targetSpeed;
        Vector2 velocityDifference = targetVelocity - rb.linearVelocity;
        if (!sprintBurst)
        {
            rb.AddForce(velocityDifference * currentForce - (rb.linearVelocity * drag));
            if (sprinting) TakeStamina(currentForce / staminaMultiplier);
        }
        else
        {
            rb.AddForce(VectorToMouse().normalized * sprintBurstStrength);
            TakeStamina(sprintBurstStrength / staminaMultiplier);
        }
    }
    #endregion

    #region Sprint 
    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            sprinting = true;
            StartCoroutine(SprintBurst());
        }
        else if (context.canceled)
        {
            StopSprint();
        }
    }
    private void StopSprint()
    {
        sprinting = false; 
        StartStaminaRegeneration();
    }
    
    private IEnumerator SprintBurst()
    {
        sprintBurst = true;
        yield return new WaitForSeconds(0.25f);
        sprintBurst = false;
    }
    #endregion

    #region Stamina
    private void TakeStamina(float staminaTaken)
    {
        stamina -= staminaTaken;
        stamina = Mathf.Clamp(stamina, 0f, maxStamina);
    }
    
    private void StartStaminaRegeneration()
    {
        if (!staminaRegenerating)
        { 
            staminaRegenerating = true; 
            StartCoroutine(WaitForStaminaRegeneration()); 
        }
    }
    private IEnumerator WaitForStaminaRegeneration()
    {
        yield return new WaitForSeconds(2f);
        StartCoroutine(RegenerateStamina());
    }

    private IEnumerator RegenerateStamina()
    {
        if (stamina >= maxStamina || sprinting){ staminaRegenerating = false; yield break; }
        stamina += staminaRegenerationRate / staminaMultiplier;
        stamina = Mathf.Clamp(stamina, 0f, maxStamina);
        yield return new WaitForFixedUpdate();
        StartCoroutine(RegenerateStamina());
    }
    #endregion

    #region Mouse Input 
    private Vector2 VectorToMouse()
    {
        Vector2 mousePosition = GetMouseWorldPosition();
        return mousePosition - rb.position;
    }

    private Vector2 GetMouseWorldPosition()
    {
        return mainCamera.ScreenToWorldPoint(inputActions.Player.MousePosition.ReadValue<Vector2>());
    }
    #endregion

    #region Item Collection 
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Collectable"))
        {
            for (int i = 0; i < collectableItems.Length; i++)
            {
                if (collision.name.Contains(collectableItems[i]))
                {
                    itemsCollected[i] += 1;
                    Destroy(collision.gameObject);
                    Debug.Log("Collected item: " + collision.name);
                }
            }
        }
    }
    #endregion

    #region Enable / Disable 
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
    #endregion
}