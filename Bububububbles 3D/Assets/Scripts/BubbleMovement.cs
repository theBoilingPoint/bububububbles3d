using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleMovement : MonoBehaviour
{
    [Header("Toggle")]
    public bool movable = false;

    [Header("Floor")]
    [SerializeField] private LayerMask floorMask = ~0;
    [SerializeField] private float groundCheckHeight = 0.6f; // raycast start height above the bubble

    [Header("Motion")]
    [SerializeField] private float pauseBetweenMoves = 1.2f; // idle time between bursts
    [SerializeField] private float moveDuration = 0.6f;      // how long each burst lasts
    [SerializeField] private float moveAcceleration = 4.0f;  // m/s^2 added along the ground (independent of mass)
    [SerializeField] private float maxHorizontalSpeed = 2.0f;
    [SerializeField] private bool hardStopBetweenBursts = true; // zero horizontal speed after each burst

    private Rigidbody rb;
    private Coroutine moveLoop;
    private bool wasMovable;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        wasMovable = movable;
        if (movable && moveLoop == null) moveLoop = StartCoroutine(MoveLoop());
    }

    private void OnDisable()
    {
        if (moveLoop != null) { StopCoroutine(moveLoop); moveLoop = null; }
    }

    private void Update()
    {
        // Handle toggling at runtime
        if (movable != wasMovable)
        {
            if (movable && moveLoop == null) moveLoop = StartCoroutine(MoveLoop());
            if (!movable && moveLoop != null) { StopCoroutine(moveLoop); moveLoop = null; }
            wasMovable = movable;
        }
    }

    private IEnumerator MoveLoop()
    {
        var waitPause = new WaitForSeconds(pauseBetweenMoves);

        while (movable)
        {
            // 1) Pick a random direction on XZ
            float angle = Random.Range(0f, Mathf.PI * 2f);
            Vector3 dir = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));

            // 2) Project it onto the ground plane so we move along slopes, not into them
            Vector3 moveDir = ProjectOntoGround(dir).normalized;
            if (moveDir.sqrMagnitude < 1e-6f)
                moveDir = dir; // fallback if no ground raycast

            // 3) Accelerate for moveDuration seconds
            float t = 0f;
            var fixedWait = new WaitForFixedUpdate();
            while (t < moveDuration && movable)
            {
                t += Time.fixedDeltaTime;

                // Apply acceleration along the ground (mass-independent)
                rb.AddForce(moveDir * moveAcceleration, ForceMode.Acceleration);

                // Clamp horizontal speed
                Vector3 v = rb.velocity;
                Vector3 vXZ = new Vector3(v.x, 0f, v.z);
                float s = vXZ.magnitude;
                if (s > maxHorizontalSpeed)
                {
                    Vector3 cappedXZ = vXZ * (maxHorizontalSpeed / s);
                    rb.velocity = new Vector3(cappedXZ.x, v.y, cappedXZ.z);
                }

                yield return fixedWait;
            }

            // 4) Optional crisp stop so it feels “stepwise”
            if (hardStopBetweenBursts)
            {
                Vector3 v = rb.velocity;
                rb.velocity = new Vector3(0f, v.y, 0f);
            }

            // 5) Chill before the next burst
            yield return waitPause;
        }

        moveLoop = null;
    }

    private Vector3 ProjectOntoGround(Vector3 dir)
    {
        Vector3 origin = transform.position + Vector3.up * groundCheckHeight;
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, groundCheckHeight + 1f, floorMask, QueryTriggerInteraction.Ignore))
        {
            // Move along the surface tangent
            return Vector3.ProjectOnPlane(dir, hit.normal);
        }
        return dir;
    }
}
