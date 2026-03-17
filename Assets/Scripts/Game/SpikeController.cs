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
    public bool pushAlongMovementDirection = true;
    public string playerTag = "Player";          // tag used to identify the player

    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = true;
        _rb.useGravity = false;
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
    }

    private void FixedUpdate()
    {
        Vector3 dir = moveDirection.sqrMagnitude > 0f ? moveDirection.normalized : Vector3.left;
        Vector3 targetPosition = _rb.position + (dir * speed * Time.fixedDeltaTime);
        _rb.MovePosition(targetPosition);

        if (dir.x < 0f && _rb.position.x < destroyX)
        {
            Destroy(gameObject);
        }
        else if (dir.x > 0f && _rb.position.x > destroyX)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        ApplyPush(collision.collider, ForceMode.VelocityChange, 2f);
    }

    private void OnCollisionStay(Collision collision)
    {
        ApplyPush(collision.collider, ForceMode.Acceleration, pushForce);
    }

    private Rigidbody ResolvePlayerRigidbody(Collider other)
    {
        if (other == null)
        {
            return null;
        }

        if (MatchesPlayer(other.gameObject))
        {
            return other.attachedRigidbody != null ? other.attachedRigidbody : other.GetComponent<Rigidbody>();
        }

        if (other.attachedRigidbody != null && MatchesPlayer(other.attachedRigidbody.gameObject))
        {
            return other.attachedRigidbody;
        }

        Transform root = other.transform.root;
        if (root != null && MatchesPlayer(root.gameObject))
        {
            return root.GetComponent<Rigidbody>();
        }

        return null;
    }

    private bool MatchesPlayer(GameObject candidate)
    {
        return candidate != null && (string.IsNullOrEmpty(playerTag) || candidate.CompareTag(playerTag));
    }

    private Vector3 GetPushDirection()
    {
        Vector3 direction = pushAlongMovementDirection && moveDirection.sqrMagnitude > Mathf.Epsilon
            ? moveDirection
            : pushDirection;

        return direction.sqrMagnitude > Mathf.Epsilon ? direction : Vector3.right;
    }

    private void ApplyPush(Collider other, ForceMode forceMode, float forceAmount)
    {
        Rigidbody playerRigidbody = ResolvePlayerRigidbody(other);
        if (playerRigidbody == null)
        {
            return;
        }

        Vector3 direction = GetPushDirection();
        playerRigidbody.AddForce(direction.normalized * forceAmount, forceMode);
    }
}

