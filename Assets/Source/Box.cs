using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(AudioSource))]
public class Box : MonoBehaviour
{
    [Header("Box Physics Settings")]
    [SerializeField]
    private float _mass = 2f;
    [SerializeField]
    private float _drag = 0.5f;
    [SerializeField]
    private float _angularDrag = 2f;
    
    [Header("Topple Settings")]
    [SerializeField]
    private float _toppleThreshold = 30f; // Angle in degrees before considered toppled
    [SerializeField]
    private float _fadeOutDuration = 0.75f; // Time to fade out after settling
    [SerializeField]
    private float _settleThreshold = 0.1f; // Velocity threshold to consider "settled"
    [SerializeField]
    private float _settleTime = 1f; // Time box must be still to be considered settled
    
    [Header("Material Settings")]
    [SerializeField]
    private PhysicsMaterial2D _boxMaterial;
    [SerializeField]
    private float _bounciness = 0.2f;
    [SerializeField]
    private float _friction = 0.4f;
    
    [Header("Effects")]
    [SerializeField]
    private AudioClip _impactSound; // Sound when hit
    
    [Header("Scoring")]
    [SerializeField]
    private int _pointValue = 100; // Points awarded when toppled
    
    private Rigidbody2D _rb;
    private BoxCollider2D _boxCollider;
    private AudioSource _audioSource;
    private bool _isToppled = false;
    private bool _hasBeenToppled = false;
    private bool _isSettled = false;
    private float _settleTimer = 0f;
    private SpriteRenderer _spriteRenderer;
    
    private void Awake()
    {
        // Get components
        _rb = GetComponent<Rigidbody2D>();
        _boxCollider = GetComponent<BoxCollider2D>();
        _audioSource = GetComponent<AudioSource>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Make the box sleep so it doesn't move initially
        _rb.Sleep();
    }
    
    private void Start()
    {
        SetupPhysics();
    }
    
    private void SetupPhysics()
    {
        // Configure rigidbody
        _rb.mass = _mass;
        _rb.linearDamping = _drag;
        _rb.angularDamping = _angularDrag;
        _rb.gravityScale = 1f;
        
        // Prevent tunneling through colliders
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        
        // Create physics material if none assigned
        if (_boxMaterial == null)
        {
            _boxMaterial = new PhysicsMaterial2D("BoxMaterial");
            _boxMaterial.bounciness = _bounciness;
            _boxMaterial.friction = _friction;
        }
        
        // Apply material to collider
        _boxCollider.sharedMaterial = _boxMaterial;
    }
    
    private void Update()
    {
        CheckIfToppled();
        
        if (_hasBeenToppled && !_isSettled)
        {
            CheckIfSettled();
        }
    }
    
    private void CheckIfToppled()
    {
        // Get the current rotation angle around Z-axis
        float currentAngle = Mathf.Abs(transform.eulerAngles.z);
        
        // Normalize angle to 0-180 range
        if (currentAngle > 180f)
            currentAngle = 360f - currentAngle;
        
        // Check if box is toppled
        bool wasToppled = _isToppled;
        _isToppled = currentAngle > _toppleThreshold;
        
        // If just toppled
        if (_isToppled && !wasToppled && !_hasBeenToppled)
        {
            OnToppled();
        }
    }
    
    private void OnToppled()
    {
        _hasBeenToppled = true;
        
        // Award points (you can hook this up to a score manager)
        Debug.Log($"Box toppled! +{_pointValue} points");

        if (ScoreManager.Instance) {
            ScoreManager.Instance.AddScore(PointValue);
        }

        // Don't start fade immediately - wait for box to settle
    }
    
    private void CheckIfSettled()
    {
        // Check if box is moving slowly enough to be considered settled
        float velocity = _rb.linearVelocity.magnitude;
        float angularVelocity = Mathf.Abs(_rb.angularVelocity);
        
        bool isStill = velocity < _settleThreshold && angularVelocity < _settleThreshold;
        
        if (isStill)
        {
            _settleTimer += Time.deltaTime;
            
            if (_settleTimer >= _settleTime)
            {
                _isSettled = true;
                Debug.Log("Box has settled - starting fade out");
                StartCoroutine(FadeOutAndDestroy());
            }
        }
        else
        {
            // Reset timer if box starts moving again
            _settleTimer = 0f;
        }
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Play impact sound on collision
        if (_impactSound != null && _audioSource != null)
        {
            // Only play if collision is strong enough
            float impactForce = collision.relativeVelocity.magnitude;
            if (impactForce > 2f) {
                if (!_audioSource.isPlaying) {
                    _audioSource.PlayOneShot(_impactSound, Mathf.Clamp01(impactForce / 10f));
                }
            }
        }
    }
    
    private System.Collections.IEnumerator FadeOutAndDestroy()
    {
        float elapsedTime = 0f;
        Color originalColor = Color.white;
        
        // Get original color from sprite renderer if available
        if (_spriteRenderer != null)
        {
            originalColor = _spriteRenderer.color;
        }
        
        while (elapsedTime < _fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / _fadeOutDuration);
            
            // Fade sprite renderer if available
            if (_spriteRenderer != null)
            {
                Color newColor = originalColor;
                newColor.a = alpha;
                _spriteRenderer.color = newColor;
            }
            
            yield return null;
        }
        
        // Destroy the box
        Destroy(gameObject);
    }
    
    public void ApplyForce(Vector2 force, Vector2 position)
    {
        _rb.AddForceAtPosition(force, position);
    }
    
    public void ApplyForce(Vector2 force)
    {
        _rb.AddForce(force, ForceMode2D.Impulse);
    }
    
    // Properties for external scripts
    public bool IsToppled => _isToppled;
    public bool HasBeenToppled => _hasBeenToppled;
    public int PointValue => _pointValue;
    
    // Public methods for game management
    public void SetMass(float newMass)
    {
        _mass = newMass;
        _rb.mass = _mass;
    }
    
    public void SetToppleThreshold(float newThreshold)
    {
        _toppleThreshold = newThreshold;
    }
    
    // Gizmo for visualizing topple threshold in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 pos = transform.position;
        
        // Draw lines showing topple threshold
        float rad = Mathf.Deg2Rad * _toppleThreshold;
        Vector3 rightLimit = pos + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0) * 2f;
        Vector3 leftLimit = pos + new Vector3(-Mathf.Cos(rad), Mathf.Sin(rad), 0) * 2f;
        
        Gizmos.DrawLine(pos, rightLimit);
        Gizmos.DrawLine(pos, leftLimit);
    }
}