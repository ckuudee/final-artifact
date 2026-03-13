using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SpikeController : MonoBehaviour
{
    [Header("Spike Movement")]
    public float speed = 5f;
    public Vector3 moveDirection = Vector3.left; // world-space movement direction
    public float destroyX = -20f;                // X position at which to destroy (see logic below)

    [Header("Spike Hit Behaviour")]
    public float pushForce = 3f;
    public Vector3 pushDirection = Vector3.left; // direction to push the player
    public string playerTag = "Player";          // tag used to identify the player

    private Rigidbody _rb;
    private bool _hasHitPlayer;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        // Let the script control movement; disable physics forces if desired.
        _rb.isKinematic = true;
        _rb.useGravity = false;
    }

    private void Update()
    {
        // Move in configurable world-space direction
        Vector3 dir = moveDirection.sqrMagnitude > 0f ? moveDirection.normalized : Vector3.left;
        transform.Translate(dir * speed * Time.deltaTime, Space.World);

        // Simple X-based destroy logic that works for both left and right movement:
        // - If moving with negative X (left), destroy when x < destroyX
        // - If moving with positive X (right), destroy when x > destroyX
        if (dir.x < 0f && transform.position.x < destroyX)
        {
            Destroy(gameObject);
        }
        else if (dir.x > 0f && transform.position.x > destroyX)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_hasHitPlayer)
            return;

        // If a player tag is specified, filter by tag
        if (!string.IsNullOrEmpty(playerTag) && !other.CompareTag(playerTag))
            return;

        Rigidbody otherRb = other.attachedRigidbody;
        if (otherRb != null)
        {
            _hasHitPlayer = true;
            otherRb.AddForce(pushDirection.normalized * pushForce, ForceMode.Impulse);
            Destroy(gameObject, 0.1f);
        }
    }
}

