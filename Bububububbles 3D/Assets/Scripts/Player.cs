using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    [SerializeField] private BubblesManager bubblesManager;
    
    public static Player Instance { get; private set; }
    
    [HideInInspector] public bool hasCollidedWithBubbles = false;
    
    [Header("Anim/Curves")]
    [SerializeField] private float animSpeed = 1.5f;
    [SerializeField] private bool useCurves = true;
    [SerializeField] private float useCurvesHeight = 0.5f;

    [Header("Movement")]
    [SerializeField] private float forwardSpeed = 7.0f;
    [SerializeField] private float backwardSpeed = 2.0f;
    [SerializeField] private float rotateSpeed = 2.0f;
    [SerializeField] private float jumpPower = 3.0f;

    [Header("Input System (assign in Inspector)")]
    [Tooltip("Action (Value/Vector2). Bind WASD/Arrows/Gamepad stick.")]
    [SerializeField] private InputActionReference moveAction;   // expects Vector2
    [Tooltip("Action (Button). Bind Space / Gamepad South.")]
    [SerializeField] private InputActionReference jumpAction;   // expects Button

    private CapsuleCollider col;
    private Rigidbody rb;
    private Vector3 velocity;

    private float orgColHight;
    private Vector3 orgVectColCenter;
    private Animator anim;
    private AnimatorStateInfo currentBaseState;

    private GameObject cameraObject;

    static int idleState = Animator.StringToHash("Base Layer.Idle");
    static int locoState = Animator.StringToHash("Base Layer.Locomotion");
    static int jumpState = Animator.StringToHash("Base Layer.Jump");
    static int restState = Animator.StringToHash("Base Layer.Rest");

    // cached input each FixedUpdate
    private Vector2 moveVec;
    private bool jumpPressed;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    void Start()
    {
        anim = GetComponent<Animator>();
        col = GetComponent<CapsuleCollider>();
        rb  = GetComponent<Rigidbody>();
        cameraObject = GameObject.FindWithTag("MainCamera");
        orgColHight = col.height;
        orgVectColCenter = col.center;

        if (moveAction == null) Debug.LogWarning("[UnityChan] Move Action not assigned.", this);
        if (jumpAction == null) Debug.LogWarning("[UnityChan] Jump Action not assigned.", this);
    }

    private void Update()
    {
        hasCollidedWithBubbles = false;
    }
    
    void OnEnable()
    {
        if (moveAction != null) moveAction.action.Enable();
        if (jumpAction != null) jumpAction.action.Enable();
    }

    void OnDisable()
    {
        if (moveAction != null) moveAction.action.Disable();
        if (jumpAction != null) jumpAction.action.Disable();
    }

     void FixedUpdate()
    {
        moveVec = moveAction != null ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;
        jumpPressed = (jumpAction != null) && jumpAction.action.WasPressedThisFrame();

        float h = moveVec.x;
        float v = moveVec.y;

        anim.SetFloat("Speed", v);
        anim.SetFloat("Direction", h);
        anim.speed = animSpeed;
        currentBaseState = anim.GetCurrentAnimatorStateInfo(0);
        rb.useGravity = true;

        // movement vector (local Z forward)
        velocity = new Vector3(0, 0, v);
        velocity = transform.TransformDirection(velocity);

        if (v > 0.1f)       velocity *= forwardSpeed;
        else if (v < -0.1f) velocity *= backwardSpeed;
        
        if (jumpPressed)
        {
            if (currentBaseState.fullPathHash == locoState && !anim.IsInTransition(0))
            {
                rb.AddForce(Vector3.up * jumpPower, ForceMode.VelocityChange);
                anim.SetBool("Jump", true);
            }
        }

        transform.localPosition += velocity * Time.fixedDeltaTime;
        transform.Rotate(0, h * rotateSpeed, 0);

        if (currentBaseState.fullPathHash == locoState)
        {
            if (useCurves) resetCollider();
        }
        else if (currentBaseState.fullPathHash == jumpState)
        {
            if (!anim.IsInTransition(0))
            {
                if (useCurves)
                {
                    float jumpHeight = anim.GetFloat("JumpHeight");
                    float gravityControl = anim.GetFloat("GravityControl");
                    if (gravityControl > 0) rb.useGravity = false;

                    Ray ray = new Ray(transform.position + Vector3.up, -Vector3.up);
                    RaycastHit hitInfo;
                    if (Physics.Raycast(ray, out hitInfo))
                    {
                        if (hitInfo.distance > useCurvesHeight)
                        {
                            col.height = orgColHight - jumpHeight;
                            float adjCenterY = orgVectColCenter.y + jumpHeight;
                            col.center = new Vector3(0, adjCenterY, 0);
                        }
                        else
                        {
                            resetCollider();
                        }
                    }
                }
                anim.SetBool("Jump", false);
            }
        }
        else if (currentBaseState.fullPathHash == idleState)
        {
            if (useCurves) resetCollider();

            // pressing Jump while Idle sets "Rest"
            if (jumpPressed)
            {
                anim.SetBool("Rest", true);
            }
        }
        else if (currentBaseState.fullPathHash == restState)
        {
            if (!anim.IsInTransition(0))
            {
                anim.SetBool("Rest", false);
            }
        }
    }
    
    private void resetCollider()
    {
        col.height = orgColHight;
        col.center = orgVectColCenter;
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        GameObject other = collision.gameObject;
        BubblesManager mgr = FindObjectOfType<BubblesManager>();
        if (mgr == null)
        {
            Debug.LogError("BubblesManager is not found!");
            return;
        }
        
        if (other.CompareTag(Bubble.NormalBubble.ToString()))
        {
            ProgressBarFill.Instance.UpdateCurrentScore(bubblesManager.bubbleScoreMap[Bubble.NormalBubble]);
            mgr.RemoveBubble(other.gameObject);
            Destroy(other.gameObject);
            hasCollidedWithBubbles = true;
        }

        if (other.CompareTag(Bubble.AddTimeBubble.ToString()))
        {
            Timer.Instance.AddTime(bubblesManager.bubbleScoreMap[Bubble.AddTimeBubble]);
            mgr.RemoveBubble(other.gameObject);
            Destroy(other.gameObject);
            hasCollidedWithBubbles = true;
        }
        
        if (other.CompareTag(Bubble.DangerBubble.ToString()))
        {
            ProgressBarFill.Instance.UpdateCurrentScore(bubblesManager.bubbleScoreMap[Bubble.DangerBubble]);
            mgr.RemoveBubble(other.gameObject);
            Destroy(other.gameObject);
            hasCollidedWithBubbles = true;
        }
    }
}
