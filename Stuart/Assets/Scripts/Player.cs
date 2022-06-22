using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [SerializeField] private float horizontalSpeed = 5.0f;
    [SerializeField] private float jumpSpeed = 5.0f;
    public float JumpSpeed => jumpSpeed;
    [SerializeField] private Transform groundProbe;
    [SerializeField] private Transform enterPoint;
    [SerializeField] private float groundProbeRadius = 5.0f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private LayerMask platformMask;
    [SerializeField] private int defaultLayer = 3;
    [SerializeField] private float maxJumpTime = 0.1f;
    [SerializeField] private float fallGravityScale = 5.0f;
    [SerializeField] private Vector3 currentVelocity;
    public Vector3 CurrentVelocity => currentVelocity;
    [SerializeField] private TimeUpdater timeUpdater;

    private bool isInputLocked => (inputLockTimer > 0);

    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    [SerializeField] private SpeechBalloon speechBalloon;

    private float jumpTime;
    [SerializeField] private float inputLockTimer = 0;
    [SerializeField] private bool onGround;
    public bool OnGround => onGround;
    [SerializeField] private bool onPlatform;
    public bool OnPlatform => onPlatform;
    [SerializeField] private bool jumping;
    public bool Jumping => jumping;
    [SerializeField] private bool gliding;
    public bool Gliding => gliding;

    [SerializeField] private bool isLocked;

    [SerializeField] private bool enteredScene;

    // World space UI components
    int textIndex;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        speechBalloon = GetComponentInChildren<SpeechBalloon>();
    }

    private void Update()
    {
        if (enteredScene && !isLocked)
        {
            UpdateMovement();
        }
    }

    private void UpdateMovement()
    {
        currentVelocity = rb.velocity;
        onGround = IsOnGround();
        onPlatform = IsOnPlatform();

        if (isInputLocked)
        {
            inputLockTimer -= Time.deltaTime;
            if (timeUpdater)
            {
                timeUpdater.SetScale(1.0f);
            }
        }
        else
        {
            float hAxis = Input.GetAxis("Horizontal");
            currentVelocity.x = hAxis * horizontalSpeed;

            if (timeUpdater != null)
            {
                if (Mathf.Abs(hAxis) > 0) timeUpdater.SetScale(1.0f);
                else timeUpdater.SetScale(0.0f);
            }

            if (onPlatform)
            {
                jumping = false;
                gliding = false;
                sprite.sortingOrder = defaultLayer - 2;
            }
            else if (onGround)
            {
                jumping = false;
                gliding = false;
                sprite.sortingOrder = defaultLayer;
            }
            
            if (Input.GetButtonDown("Jump"))
            {
                if (onGround || onPlatform)
                {
                    rb.gravityScale = 1.0f;
                    currentVelocity.y = jumpSpeed;
                    jumpTime = Time.time;
                }
                else
                {
                    if (currentVelocity.y <= 0)
                    {
                        gliding = true;
                        rb.gravityScale = 0.2f;
                        if (currentVelocity.y < -5 * fallGravityScale)
                            currentVelocity.y -= currentVelocity.y * 0.8f;
                    }
                }
            }
            else if (Input.GetButton("Jump"))
            {
                float elapsedTime = Time.time - jumpTime;
                if (elapsedTime > maxJumpTime && !gliding)
                {
                    rb.gravityScale = fallGravityScale;
                }

                if (currentVelocity.y >= 0 && !onPlatform && !onGround)
                    jumping = true;
                if (jumping && currentVelocity.y <= 0)
                {
                    gliding = true;
                    rb.gravityScale = 0.2f;
                }
            }
            else if (Input.GetButtonUp("Jump"))
            {
                jumping = false;

                if (gliding)
                    gliding = false;
            }
            else
            {
                if (!gliding)
                    rb.gravityScale = fallGravityScale;
            }

            rb.velocity = currentVelocity;

            if ((currentVelocity.x > 0) && (transform.right.x < 0))
            {
                transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            else if ((currentVelocity.x < 0) && (transform.right.x > 0))
            {
                transform.rotation = Quaternion.Euler(0, 180, 0);
            }
        }
    }

    public IEnumerator EnterScene(float timeToEnter)
    {
        yield return new WaitForSeconds(timeToEnter);
        do
        {
            rb.velocity = new Vector3(2, 0, 0);

            yield return null;
        }
        while(transform.position.x < enterPoint.position.x);
        
        rb.velocity = Vector3.zero;

        yield return new WaitForSeconds(1.0f);
        enteredScene = true;
    }

    public void Lock()
    {
        isLocked = true;

        rb.gravityScale = fallGravityScale;
    }

    public void Unlock()
    {
        isLocked = false;
    }
    
    private bool IsOnGround()
    {
        Collider2D collider = Physics2D.OverlapCircle(
            groundProbe.position, groundProbeRadius, groundMask);

        return (collider != null);
    }

    private bool IsOnPlatform()
    {
        Collider2D collider = Physics2D.OverlapCircle(
            groundProbe.position, groundProbeRadius, platformMask);

        return (collider != null && !jumping && !gliding);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundProbe != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(groundProbe.position, groundProbeRadius);
        }
    }

    public void Talk()
    {
        Debug.Log($"PLAYER TALKING {textIndex}");

        speechBalloon.ShowBalloon();

        textIndex++;
    }

    public void Listen()
    {
        Debug.Log("PLAYER LISTENING");

        speechBalloon.HideBalloon();
    }
}
