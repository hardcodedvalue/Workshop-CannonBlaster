using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(CircleCollider2D))]
public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField]
    private float _lifetime = 10f; // How long the projectile lasts before being destroyed
    [SerializeField]
    private bool _destroyOnCollision = true;
    
    [Header("Effects")]
    [SerializeField]
    private GameObject _impactEffect; // Optional impact effect prefab
    [SerializeField]
    private AudioClip _impactSound; // Optional impact sound

    [SerializeField]
    private Rigidbody2D _rigidBody;

    [SerializeField]
    private AudioSource _audioSource;
    
    private void Awake()
    {
        // Ensure gravity is enabled
        _rigidBody.gravityScale = 9.8f;
        
        // Prevent tunneling through colliders
        _rigidBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        
        // Destroy after lifetime
        Destroy(gameObject, _lifetime);
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Play impact sound
        float audioLength = 0f;

        if (_impactSound != null && _audioSource != null) {
            audioLength = _impactSound.length;

            if (!_audioSource.isPlaying) {
                _audioSource.PlayOneShot(_impactSound);
            }
        }
        
        // Spawn impact effect
        if (_impactEffect != null)
        {
            Instantiate(_impactEffect, transform.position, Quaternion.identity);
        }
        
        // Log what we hit for debugging
        Debug.Log($"Projectile hit: {collision.gameObject.name}");
        
        // Destroy projectile on collision if enabled
        if (_destroyOnCollision)
        {
            bool isBoxLayer = collision.gameObject.layer == LayerMask.NameToLayer("Box");

            if (!isBoxLayer) {
                return;
            }

            Destroy(gameObject, audioLength);
        }
    }

    // Public method to add force (alternative launch method)
    public void AddLaunchForce(Vector3 force)
    {
        // Convert Vector3 to Vector2 for 2D physics
        Vector2 force2D = new(force.x, force.y);
        _rigidBody.AddForce(force2D, ForceMode2D.Impulse);
        Debug.Log($"Projectile launched! Force: {force2D}");
    }
}