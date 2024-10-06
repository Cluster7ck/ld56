using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public enum State
{
  NoInput,
  WaitInput,
  PrepareJump,
  JumpingUp,
  JumpingDown,
  Falling,
  BulletTimeWaitInput,
  BulletTimePrepareJump,
  Bounce,
  DoubleJumping,
  Dieing
}

public class Cricket : MonoBehaviour
{
  [SerializeField] private Vector3 gravity;
  [SerializeField] private float velocityMul = 1;
  [SerializeField] private float maxVelocityMagnitude = 45;
  [SerializeField] private float riseGravityMul = 1;
  [SerializeField] private float fallGravityMul = 1;
  [SerializeField] private LayerMask collisionMask;

  [SerializeField] private float bounceStrength;
  [SerializeField] private float bounceMinForwardVelocity;

  [SerializeField] private float hitStopLength;

  [SerializeField] private float bulletTimeLength;

  [SerializeField] private GameObject arcIndicatorPrefab;
  [SerializeField] private Vector2 arcIndicatorSize;
  [SerializeField] private ParticleSystem jumpParticleSystem;
  [SerializeField] private Animator animator;

  public GameManager gameManager;
  private Camera camera;
  private BoxCollider2D boxCollider;
  private State state = State.WaitInput;

  private bool dragging;
  private Vector3 dragStartPosScreen;
  private Vector3 dragStartPosWorld;
  private Vector3 dragCurrentPosScreen;

  private Vector3 initialVelocity;
  private Vector3 initialJumpPos;

  private Vector3 initialFallPos;
  private Vector3 initialFallVelocity;
  private GameObject lastFallCollision;
  private int sameCollisionFall;

  private float jumpTime;

  private float timeSinceBulletTime = 0;

  private Vector3 startPos;

  private GameObject[] debugSpheres = new GameObject[12];

  private GameObject[] arcIndicators = new GameObject[7];

  private HitStop hitStop;


  private Rigidbody2D rb;
  private bool lookRight = true;

  private void Awake()
  {
    gameManager = FindObjectOfType<GameManager>();
    hitStop = GetComponent<HitStop>();
    rb = GetComponent<Rigidbody2D>();
  }

  // Start is called before the first frame update
  void Start()
  {
    camera = Camera.main;
    boxCollider = GetComponent<BoxCollider2D>();
    startPos = transform.position;

    for (int i = 0; i < debugSpheres.Length; i++)
    {
      var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
      go.name = $"{i}";
      go.transform.localScale = Vector3.one * 0.1f;
      debugSpheres[i] = go;
      debugSpheres[i].SetActive(false);
    }

    for (int i = 0; i < arcIndicators.Length; i++)
    {
      var go = Instantiate(arcIndicatorPrefab);
      go.name = $"arcIndicator{i}";
      go.transform.localScale = Vector3.one * (i / (arcIndicators.Length * 1.0f)).Remap(0, 1, arcIndicatorSize.x, arcIndicatorSize.y);
      arcIndicators[i] = go;
      if (i == 0)
      {
        jumpParticleSystem.transform.SetParent(go.transform);
        jumpParticleSystem.gameObject.SetActive(true);
        jumpParticleSystem.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
      }
      go.SetActive(false);
    }
  }

  void Update()
  {
    if (Input.GetKeyDown(KeyCode.Space))
    {
      state = State.BulletTimeWaitInput;
      transform.position = startPos;
    }

    if (Input.GetKeyDown(KeyCode.A))
    {
      state = State.WaitInput;
      foreach (var sp in debugSpheres)
      {
        sp.GetComponent<MeshRenderer>().material.color = Color.gray;
      }
    }

    if (state == State.NoInput)
    {
      jumpTime += Time.deltaTime;
      var pos = PredictProjectilePosAtT(jumpTime, initialFallVelocity, initialFallPos, gravity * fallGravityMul);

      var (nextState, _) = DoCollision(transform.position, pos);
      if (!nextState.HasValue)
      {
        transform.position = pos;
      }
    }

    if (state == State.WaitInput)
    {
      animator.SetBool("isFlying", false);
      animator.SetBool("isAiming", false);
      if (Mouse.current.leftButton.wasPressedThisFrame)
      {
        animator.SetBool("isAiming", true);
        state = State.PrepareJump;
        dragStartPosScreen = Mouse.current.position.value;
        dragStartPosWorld = camera.ScreenToWorldPoint(dragStartPosScreen);

        for (int i = 0; i < arcIndicators.Length; i++)
        {
          arcIndicators[i].gameObject.SetActive(true);
        }
      }
    }

    if (state == State.BulletTimeWaitInput)
    {
      if (Mouse.current.leftButton.wasPressedThisFrame)
      {
        state = State.BulletTimePrepareJump;
        dragStartPosScreen = Mouse.current.position.value;
        dragStartPosWorld = camera.ScreenToWorldPoint(dragStartPosScreen);
        for (int i = 0; i < arcIndicators.Length; i++)
        {
          arcIndicators[i].gameObject.SetActive(true);
        }
      }
    }

    if (rb.velocity.x > 0.05f) {
        lookRight = true;
    } else if (rb.velocity.x < -0.05f) {
        lookRight = false;
    }

    if (state == State.PrepareJump || state == State.BulletTimePrepareJump)
    {
      dragCurrentPosScreen = Mouse.current.position.value;
      var dragCurrentPosWorld = camera.ScreenToWorldPoint(dragCurrentPosScreen);
      var dragDelta = (dragStartPosWorld - dragCurrentPosWorld);

      var potentialVelocity = dragDelta * velocityMul;
      var potentialVelocityNorm = potentialVelocity.normalized;
      var mag = Mathf.Min(potentialVelocity.magnitude, maxVelocityMagnitude);
      potentialVelocity = potentialVelocityNorm * mag;
      var potentialJumpPos = transform.position;

      var fo = jumpParticleSystem.forceOverLifetime;
      fo.xMultiplier = 0;
      fo.yMultiplier = -potentialVelocity.magnitude * potentialVelocity.magnitude.Remap(0, maxVelocityMagnitude, 0.5f, 2);

      float dt = 0.03f;
      float t = dt;
      for (int i = 0; i < arcIndicators.Length; i++)
      {
        arcIndicators[i].transform.position = PredictProjectilePosAtT(t, potentialVelocity, potentialJumpPos, gravity * riseGravityMul);
        t += dt;
      }

      var lastTarget = PredictProjectilePosAtT(t, potentialVelocity, potentialJumpPos, gravity * riseGravityMul);
      for (int i = 0; i < arcIndicators.Length - 1; i++)
      {
        arcIndicators[i].transform.up = arcIndicators[i + 1].transform.position - arcIndicators[i].transform.position;
        t += dt;
      }

      arcIndicators[^1].transform.up = lastTarget - arcIndicators[^1].transform.position;

      if (Mouse.current.leftButton.wasReleasedThisFrame)
      {
        for (int i = 0; i < arcIndicators.Length; i++)
        {
          arcIndicators[i].gameObject.SetActive(false);
        }

        animator.SetBool("isAiming", false);
        animator.SetBool("isFlying", true);

        initialVelocity = potentialVelocity;
        initialJumpPos = potentialJumpPos;
        state = State.JumpingUp;
      }

      if (Mouse.current.rightButton.wasPressedThisFrame)
      {
        for (int i = 0; i < arcIndicators.Length; i++)
        {
          arcIndicators[i].gameObject.SetActive(false);
        }

        state = State.WaitInput;
      }
      
      if (potentialVelocity.x > 0.05f) {
         lookRight = true;
      } else if (potentialVelocity.x < -0.05f) {
         lookRight = false;
      }
    }

   
    
    if(lookRight) {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        } else {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    
  }

  void FixedUpdate()
  {
    if (state == State.JumpingUp)
    {
      // Predicted position
      jumpTime += Time.fixedDeltaTime * Time.timeScale;

      var pos = PredictProjectilePosAtT(jumpTime, initialVelocity, initialJumpPos, gravity * riseGravityMul);
      if (transform.position.y > pos.y)
      {
        initialVelocity = PredictVelocityAtT(jumpTime, initialVelocity, gravity * riseGravityMul);
        initialJumpPos = transform.position;
        state = State.JumpingDown;
        jumpTime = 0;
      }
      else
      {
        // collision
        var (nextState, _) = DoCollision(transform.position, pos);

        if (jumpTime > 0.1f && nextState.HasValue)
        {
          if (nextState.Value == State.Falling)
          {
            TransitionToFalling();
          }
          else if (nextState.Value == State.WaitInput)
          {
            state = nextState.Value;
          }

          jumpTime = 0;
        }
        else
        {
          transform.position = pos;
        }
      }
    }
    else if (state == State.Falling)
    {
      jumpTime += Time.fixedDeltaTime * Time.timeScale;
      var pos = PredictProjectilePosAtT(jumpTime, initialFallVelocity, initialFallPos, gravity * fallGravityMul);

      var (nextState, collision) = DoCollision(transform.position, pos);
      if (nextState.HasValue)
      {
        state = nextState.Value;
        jumpTime = 0;
        if (nextState.Value == State.Bounce)
        {
          TransitionToBounce(initialFallVelocity, collision);
        }
        else if (nextState.Value == State.BulletTimeWaitInput)
        {
          TransitionToBulletTime(initialFallVelocity);
        }
      }
      else
      {
        transform.position = pos;
      }
    }

    if (state == State.JumpingDown)
    {
      // Predicted position
      jumpTime += Time.fixedDeltaTime * Time.timeScale;

      var pos = PredictProjectilePosAtT(jumpTime, initialVelocity, initialJumpPos, gravity * fallGravityMul);
      // collision
      var (nextState, collision) = DoCollision(transform.position, pos);

      if (nextState.HasValue)
      {
        if (nextState.Value == State.Falling)
        {
          TransitionToFalling();
        }
        else if (nextState.Value == State.WaitInput)
        {
          state = nextState.Value;
        }
        else if (nextState.Value == State.Bounce)
        {
          TransitionToBounce(initialVelocity, collision);
        }
        else if (nextState.Value == State.BulletTimeWaitInput)
        {
          TransitionToBulletTime(initialVelocity);
        }

        jumpTime = 0;
      }
      else
      {
        transform.position = pos;
      }
    }
    else if (state == State.BulletTimeWaitInput || state == State.BulletTimePrepareJump)
    {
      JumpingUp();
    }
  }

  private void JumpingUp()
  {
    // Predicted position
    jumpTime += Time.fixedDeltaTime * Time.timeScale;

    var pos = PredictProjectilePosAtT(jumpTime, initialVelocity, initialJumpPos, gravity * riseGravityMul);
    if (transform.position.y > pos.y)
    {
      TransitionToJumpingDown();
    }
    else
    {
      // collision
      var (nextState, _) = DoCollision(transform.position, pos);

      if (jumpTime > 0.1f && nextState.HasValue)
      {
        if (nextState.Value == State.Falling)
        {
          TransitionToFalling();
        }
        else if (nextState.Value == State.WaitInput)
        {
          state = nextState.Value;
          jumpTime = 0;
        }
      }
      else
      {
        transform.position = pos;
      }
    }
  }

  private void TransitionToJumpingDown()
  {
    initialVelocity = PredictVelocityAtT(jumpTime, initialVelocity, gravity * riseGravityMul);
    initialJumpPos = transform.position;
    state = State.JumpingDown;
    jumpTime = 0;
  }

  private void TransitionToFalling()
  {
    lastFallCollision = null;
    initialFallPos = transform.position;
    initialFallVelocity = Vector3.down * 0.05f;
    state = State.Falling;
    jumpTime = 0;
  }

  private void TransitionToBounce(Vector3 initialVelocity, Transform bounceable)
  {
    var shroom = bounceable.GetComponent<Shroom>();
    shroom.DoBounce();
    var bounceStrength = shroom.BounceStrength;

    jumpTime = 0;
    float clampedX = 0;
    if (initialVelocity.x < 0)
    {
      clampedX = Mathf.Clamp(initialVelocity.x, float.NegativeInfinity, -bounceMinForwardVelocity);
    }
    else
    {
      clampedX = Mathf.Clamp(initialVelocity.x, bounceMinForwardVelocity, float.PositiveInfinity);
    }

    this.initialVelocity = new Vector3(clampedX, bounceStrength, 0);
    initialJumpPos = transform.position;
    state = State.JumpingUp;

    hitStop.Stop(hitStopLength);
  }

  private void TransitionToBulletTime(Vector3 initialVelocity)
  {
    jumpTime = 0;
    float clampedX = 0;
    if (initialVelocity.x < 0)
    {
      clampedX = Mathf.Clamp(initialVelocity.x, float.NegativeInfinity, -bounceMinForwardVelocity);
    }
    else
    {
      clampedX = Mathf.Clamp(initialVelocity.x, bounceMinForwardVelocity, float.PositiveInfinity);
    }

    this.initialVelocity = new Vector3(clampedX, bounceStrength, 0);
    initialJumpPos = transform.position;
    state = State.BulletTimeWaitInput;
    timeSinceBulletTime = 0;

    hitStop.BulletTime(bulletTimeLength);
  }

  private (State?, Transform? collision) DoCollision(Vector3 previousPos, Vector3 nextPos)
  {
    var position = transform.position;
    var dir = nextPos - previousPos;

    //if (nextPos.x < previousPos.x && (nextPos.x - extents.x) < cameraLeft.x)
    //{
    //  var leftDir = Vector3.left * Mathf.Abs((cameraLeft.x + extents.x) - previousPos.x);
    //  var newLeftDir = Vector3.Project(leftDir, dir);
    //  transform.position += newLeftDir;
    //  return State.Falling;
    //}

    var hit = Physics2D.BoxCast(position, boxCollider.size, 0, dir, dir.magnitude, collisionMask);

    if (hit.collider != null)
    {
      var interactable = hit.transform.GetComponent<Collidable>();
      var collisionStateTransition = hit.transform.GetComponent<CollisionStateTransition>();
      if (interactable)
      {
        interactable.Collide(this);
      }

      // When no transition requested just to the interaction and return
      if (collisionStateTransition != null && !collisionStateTransition.doTransition)
      {
        return (collisionStateTransition.TransitionToFromTop, hit.transform);
      }

      if (state == State.Falling && hit.transform.gameObject == lastFallCollision)
      {
        sameCollisionFall++;
        Debug.Log("sameCollisionFall "+sameCollisionFall); 
      }
      else
      {
        sameCollisionFall = 0;
      }

      if (sameCollisionFall > 10)
      {
        transform.position += dir * -1 * 2;
      }

      if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Bad"))
      {
        Debug.Log("Bad hit");
      }

      var unstuckOffset = Unstuck(transform.position, dir);

      var dist = hit.distance;
      var normDir = dir.normalized;
      transform.position += normDir * Mathf.Clamp(dist - 0.01f, 0.01f, dist);

      var dot = Vector3.Dot(hit.normal, Vector3.up);
      //Debug.Log($"{hit.transform.gameObject.name}: hd:{hit.distance}, hn:{hit.normal}, dot:{dot}, dir:{dir}");
      
      // We hit a wall from below
      if (previousPos.y < nextPos.y)
      {
        Debug.Log("Wall from below");
        lastFallCollision = hit.transform.gameObject;
        return (State.Falling, hit.transform);
      }

      if (dot != 0)
      {
        sameCollisionFall = 0;
        if (collisionStateTransition != null)
        {
          return (collisionStateTransition.TransitionToFromTop, hit.transform);
        }

        if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Shroom"))
        {
          Debug.Log("Bounce");
          return (State.Bounce, hit.transform);
        }

        if (hit.transform.gameObject.layer == LayerMask.NameToLayer("BulletShroom"))
        {
          Debug.Log("BulletTime");
          return (State.BulletTimeWaitInput, hit.transform);
        }

        // collide with ground
        Debug.Log("Idle");
        return (State.WaitInput, hit.transform);
      }
      else
      {
        lastFallCollision = hit.transform.gameObject;
        if (collisionStateTransition != null)
        {
          return (collisionStateTransition.TransitionToFromSide, hit.transform);
        }

        Debug.Log("Falling");
        return (State.Falling, hit.transform);
      }
    }

    return (null, null);
  }

  private Vector3 Unstuck(Vector3 pos, Vector3 inDir)
  {
    var outDir = -inDir;
    var offset = Vector3.zero;
    RaycastHit2D hit;
    int it = 0;
    while ((hit = Physics2D.BoxCast(pos + offset, boxCollider.size, 0, Vector3.zero)).collider != null)
    {
      offset += outDir * 0.02f;
      it++;
      if (it == 100) break;
    }
    Debug.DrawLine(pos, pos+offset, Color.red, 30f);
    Debug.Log("It: "+it+" Off: "+offset);
    return offset;
  }

  public void SetState(State newState)
  {
    state = newState;
    jumpTime = 0;
  }

  public State jumpState
  {
    get { return state; }
  }

  private Vector3 PredictVelocityAtT(float time, Vector3 initialVel, Vector3 gravity)
  {
    return gravity * time + initialVel;
  }

  private Vector3 PredictProjectilePosAtT(float time, Vector3 initialVel, Vector3 initialPos, Vector3 gravity)
  {
    return gravity * (0.5f * time * time) + initialVel * time + initialPos;
  }
}

public static class Extensions
{
  public static float Remap(this float s, float a1, float a2, float b1, float b2)
  {
    return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
  }

  public static float Remap(this float s, float a2, float b2)
  {
    return s.Remap(0, a2, 0, b2);
  }
}