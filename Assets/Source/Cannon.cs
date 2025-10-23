using UnityEngine;
using UnityEngine.InputSystem;

public class Cannon : MonoBehaviour
{
    [Header("Cannon Settings")]
    [SerializeField]
    private Transform _cannonBarrel; // The barrel that rotates
    [SerializeField]
    private Transform _shootPoint; // Where projectiles spawn from
    [SerializeField]
    private GameObject _projectilePrefab; // Ball/projectile prefab
    
    [Header("Rotation Settings")]
    [SerializeField]
    private float _rotationSpeed = 50f;
    [SerializeField]
    private float _minAngle = -45f; // Minimum rotation angle
    [SerializeField]
    private float _maxAngle = 45f;  // Maximum rotation angle
    
    [Header("Shooting Settings")]
    [SerializeField]
    private float _shootForce = 120f;
    [SerializeField]
    private float _shootCooldown = .25f;
    
    private float _currentRotation = 0f;
    private float _lastShootTime = 0f;
    
    // Input System actions - using direct key bindings for simplicity
    private InputAction _rotateUpAction;
    private InputAction _rotateDownAction;
    private InputAction _shootAction;
    
    private void Awake()
    {
        // Create input actions directly
        _rotateUpAction = new InputAction("RotateUp", binding: "<Keyboard>/w");
        _rotateDownAction = new InputAction("RotateDown", binding: "<Keyboard>/s");
        _shootAction = new InputAction("Shoot", binding: "<Keyboard>/space");
    }
    
    private void OnEnable()
    {
        // Enable input actions
        _rotateUpAction?.Enable();
        _rotateDownAction?.Enable();
        _shootAction?.Enable();
        
        // Subscribe to shoot action
        _shootAction.performed += OnShoot;
    }
    
    private void OnDisable()
    {
        // Unsubscribe from shoot action
        _shootAction.performed -= OnShoot;
        
        // Disable input actions
        _rotateUpAction?.Disable();
        _rotateDownAction?.Disable();
        _shootAction?.Disable();
    }
    
    private void Start()
    {
        // If no barrel is assigned, use this transform
        if (_cannonBarrel == null)
            _cannonBarrel = transform;
            
        // If no shoot point is assigned, create one
        if (_shootPoint == null)
        {
            GameObject shootPointObj = new GameObject("ShootPoint");
            shootPointObj.transform.SetParent(_cannonBarrel);
            shootPointObj.transform.localPosition = Vector3.right * 2f; // 2 units to the right (2D forward)
            _shootPoint = shootPointObj.transform;
        }
        
        // Set initial rotation
        UpdateCannonRotation();
    }
    
    private void Update()
    {
        HandleRotation();
    }
    
    private void HandleRotation()
    {
        float rotationInput = 0f;
        
        // Check for W key (rotate up)
        if (_rotateUpAction.IsPressed())
        {
            rotationInput = 1f;
        }
        // Check for S key (rotate down)
        else if (_rotateDownAction.IsPressed())
        {
            rotationInput = -1f;
        }
        
        if (Mathf.Abs(rotationInput) > 0.1f)
        {
            // Update current rotation
            _currentRotation += rotationInput * _rotationSpeed * Time.deltaTime;
            
            // Clamp rotation within limits
            _currentRotation = Mathf.Clamp(_currentRotation, _minAngle, _maxAngle);
            
            // Apply rotation to cannon barrel
            UpdateCannonRotation();
        }
    }
    
    private void UpdateCannonRotation()
    {
        if (_cannonBarrel != null)
        {
            // For 2D, rotate around Z-axis (XY plane rotation)
            _cannonBarrel.localRotation = Quaternion.Euler(0f, 0f, _currentRotation);
        }
    }
    
    private void OnShoot(InputAction.CallbackContext context)
    {
        // Check cooldown
        if (Time.time - _lastShootTime < _shootCooldown)
            return;
            
        Shoot();
        _lastShootTime = Time.time;
    }
    
    private void Shoot()
    {
        if (_projectilePrefab == null)
        {
            Debug.LogWarning("No projectile prefab assigned to cannon!");
            return;
        }
        
        if (_shootPoint == null)
        {
            Debug.LogWarning("No shoot point assigned to cannon!");
            return;
        }
        
        // Instantiate projectile
        GameObject projectileObj = Instantiate(_projectilePrefab, _shootPoint.position, _shootPoint.rotation);
        
        // Calculate 2D shoot direction based on cannon rotation
        // Convert angle to radians and calculate direction in XY plane
        float angleInRadians = _currentRotation * Mathf.Deg2Rad;
        Vector3 shootDirection = new Vector3(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians), 0f).normalized;
        
        // Try to get the Projectile component first
        Projectile projectile = projectileObj.GetComponent<Projectile>();
        if (projectile != null)
        {
            // Use the projectile's launch method
            projectile.AddLaunchForce(shootDirection * _shootForce);
        }
        else
        {
            Debug.LogError("The instantiated projectile does not have a Projectile component!");
        }
        
        // Optional: Add some visual/audio feedback
        Debug.Log($"Cannon fired! Angle: {_currentRotation:F1}°");
    }
    
    // Public method to manually set rotation (useful for AI or other scripts)
    public void SetRotation(float angle)
    {
        _currentRotation = Mathf.Clamp(angle, _minAngle, _maxAngle);
        UpdateCannonRotation();
    }
    
    // Public method to get current rotation
    public float GetRotation()
    {
        return _currentRotation;
    }
}