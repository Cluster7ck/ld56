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

  [SerializeField] private GameObject deathAnimationPrefab;
  [SerializeField] private GameObject firstBulletTimeHelp;

  [SerializeField] private RectTransform relativeTarget;

    [SerializeField] private AudioClip jumpClip;

  private GameObject lastFrameCollision;

  public GameManager gameManager;
  private BoxCollider2D boxCollider;
  private State state = State.WaitInput;

  private bool dragging;
  private Vector3 dragStartPosScreen;

  private Vector3 initialVelocity;
  private Vector3 initialJumpPos;

  private Vector3 initialFallPos;
  private Vector3 initialFallVelocity;
  private GameObject lastFallCollision;
  private int sameCollisionFall;

  private float jumpTime;
  private bool hadFirstBulletTimeJump;

  private GameObject[] debugSpheres = new GameObject[12];

  private GameObject[] arcIndicators = new GameObject[14];

  private HitStop hitStop;

  private float _fallSpeedYDampingChangeThreshold;
  public State jumpState => state;
  public GameObject DeathAnimationPrefab => deathAnimationPrefab;

  private Vector2 prevPos;
  private bool lookRight = true;

  private void Awake()
  {
    gameManager = FindObjectOfType<GameManager>();
    hitStop = GetComponent<HitStop>();
        
  }

  // Start is called before the first frame update
  void Start()
  {
    boxCollider = GetComponent<BoxCollider2D>();
    for (int i = 0; i < debugSpheres.Length; i++)
    {
      var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
      go.name = $"{i}";
      go.transform.localScale = Vector3.one * 0.1f;
      debugSpheres[i] = go;
      debugSpheres[i].SetActive(false);
      _fallSpeedYDampingChangeThreshold = CameraManager.instance.fallSpeedYDampingChangeThreshold;
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


        if (inputThisFrame())
          {
            animator.SetBool("isAiming", true);
            state = State.PrepareJump;
            dragStartPosScreen = touchMousePosition();
            relativeTarget.gameObject.SetActive(true);
            relativeTarget.position = dragStartPosScreen;

            for (int i = 0; i < arcIndicators.Length; i++)
            {
              arcIndicators[i].gameObject.SetActive(true);
            }
        }
    }

    if (state == State.BulletTimeWaitInput)
    {
      if (inputStartedThisFrame())
      {
        
        hadFirstBulletTimeJump = true;
        state = State.BulletTimePrepareJump;
        dragStartPosScreen = touchMousePosition();
        relativeTarget.gameObject.SetActive(true);
        relativeTarget.position = dragStartPosScreen;
        for (int i = 0; i < arcIndicators.Length; i++)
        {
          arcIndicators[i].gameObject.SetActive(true);
        }
      }
    }


    if (state == State.BulletTimeWaitInput || state == State.BulletTimePrepareJump)
    {
      JumpingUpBulletTime();
    }

    if (state == State.PrepareJump || state == State.BulletTimePrepareJump)
    {
        
      Vector3 dragCurrentPosScreen = touchMousePosition();
      var dragScreenDelta = (dragStartPosScreen - dragCurrentPosScreen);

      var potentialVelocity = dragScreenDelta * velocityMul;
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

      if (inputEndedThisFrame())
      {
        for (int i = 0; i < arcIndicators.Length; i++)
        {
          arcIndicators[i].gameObject.SetActive(false);
        }
        
        relativeTarget.gameObject.SetActive(false);

        animator.SetBool("isAiming", false);
        animator.SetBool("isFlying", true);

        initialVelocity = potentialVelocity;
        initialJumpPos = potentialJumpPos;
        jumpTime = 0;
        hitStop.ForceReset();
        state = State.JumpingUp;
        AudioManager.Instance.PlaySound(jumpClip, 0f, -0.9f);
      }

      if (inputStartedThisFrame())
      {
        for (int i = 0; i < arcIndicators.Length; i++)
        {
          arcIndicators[i].gameObject.SetActive(false);
        }
        relativeTarget.gameObject.SetActive(false);

        state = State.WaitInput;
      }

      if (potentialVelocity.x > 0.05f)
      {
        lookRight = true;
      }
      else if (potentialVelocity.x < -0.05f)
      {
        lookRight = false;
      }
    }


    var localScale = transform.localScale;
    if (lookRight)
    {
      transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), localScale.y, localScale.z);
    }
    else
    {
      transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), localScale.y, localScale.z);
    }

    Vector2 currentPos = transform.position;
    Vector2 velocity = currentPos - prevPos;

    //For Camera
    // if we are falling past a certain threshold 
    if (velocity.y < _fallSpeedYDampingChangeThreshold && !CameraManager.instance.IsLerpingYDamping &&
        !CameraManager.instance.LerpedFromPlayerFalling)
    {
      CameraManager.instance.LerpYDamping(true);
    }

    // if we are standing still or moving up
    if (velocity.y >= 0f && !CameraManager.instance.IsLerpingYDamping && CameraManager.instance.LerpedFromPlayerFalling)
    {
      CameraManager.instance.LerpedFromPlayerFalling = false;
      CameraManager.instance.LerpYDamping(false);
    }

    prevPos = transform.position;
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
        if (nextState.Value == State.Bounce)
        {
          TransitionToBounce(initialFallVelocity, collision);
        }
        else if (nextState.Value == State.BulletTimeWaitInput)
        {
          TransitionToBulletTime(initialFallVelocity, collision);
        }
        state = nextState.Value;
        jumpTime = 0;
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
          TransitionToBulletTime(initialVelocity, collision);
        }

        jumpTime = 0;
      }
      else
      {
        transform.position = pos;
      }
    }
  }

  private void JumpingUpBulletTime()
  {
    // Predicted position
    jumpTime += Time.deltaTime;

    var pos = PredictProjectilePosAtT(jumpTime, initialVelocity, initialJumpPos, gravity * riseGravityMul);
    if (transform.position.y > pos.y)
    {
      foreach (var arcIndicator in arcIndicators)
      {
        arcIndicator.SetActive(false);
      }

      hitStop.ForceReset();
      TransitionToJumpingDown();
    }
    else
    {
      // collision
      var (nextState, _) = DoCollision(transform.position, pos);

      if (nextState.HasValue)
      {
        if (nextState.Value == State.Falling)
        {
          hitStop.ForceReset();
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

  public void Die(Vector3 respawnPos)
  {
    initialVelocity = Vector3.zero;
    initialJumpPos = respawnPos;
    initialFallPos = respawnPos;
    initialFallVelocity = Vector3.down * 0.05f;
    Instantiate(DeathAnimationPrefab, transform.position, Quaternion.identity);
    gameObject.SetActive(false);
  }

  private void TransitionToJumpingDown()
  {
    initialVelocity = PredictVelocityAtT(jumpTime, initialVelocity, gravity * riseGravityMul);
    initialJumpPos = transform.position;
    state = State.JumpingDown;
    jumpTime = 0;
  }

  public void TransitionToFalling()
  {
    foreach (var arcIndicator in arcIndicators)
    {
      if (arcIndicator)
      {
        arcIndicator.SetActive(false);
      }
    }
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
    //state = State.JumpingUp;
    state = State.BulletTimeWaitInput;

    hitStop.Stop(hitStopLength);
  }

  private void TransitionToBulletTime(Vector3 initialVelocity, Transform bounceable)
  {
    if (!hadFirstBulletTimeJump)
    {
      var go = Instantiate(firstBulletTimeHelp);
      go.transform.position = transform.position;
    }
    var shroom = bounceable.GetComponent<Shroom>();
    shroom.DoBounce();

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

    hitStop.BulletTime(bulletTimeLength);
  }

  private (State?, Transform? collision) DoCollision(Vector3 previousPos, Vector3 nextPos)
  {
    var position = transform.position;
    var dir = nextPos - previousPos;

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
        //Debug.Log("sameCollisionFall " + sameCollisionFall);
      }
      else
      {
        sameCollisionFall = 0;
      }

      if (sameCollisionFall > 10)
      {
        transform.position += dir * -1 * 2;
      }

      if (Unstuck(transform.position + dir, dir, out var unstuckOffset))
      {
        transform.position += dir + unstuckOffset;
      }
      else
      {
        var dist = hit.distance;
        var normDir = dir.normalized;
        var nextDir = normDir * Mathf.Clamp(dist - 0.03f, 0.03f, dist);
        transform.position += nextDir;
      }

      var dot = Vector3.Dot(hit.normal, Vector3.up);
      //Debug.Log($"{hit.transform.gameObject.name}: hd:{hit.distance}, hn:{hit.normal}, dot:{dot}, dir:{dir}");

      // We hit a wall from below
      if (previousPos.y < nextPos.y)
      {
        //Debug.Log("Wall from below");
        if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Shroom") || hit.transform.gameObject.layer == LayerMask.NameToLayer("BulletShroom")) {
            return (null, null);
        } else {
            lastFallCollision = hit.transform.gameObject;
            return (State.Falling, hit.transform);
        }
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
          //Debug.Log("Bounce");
          return (State.Bounce, hit.transform);
        }

        if (hit.transform.gameObject.layer == LayerMask.NameToLayer("BulletShroom"))
        {
          //Debug.Log("BulletTime");
          return (State.BulletTimeWaitInput, hit.transform);
        }

        // collide with ground
        //Debug.Log("Idle");
        return (State.WaitInput, hit.transform);
      }
      else
      {
        lastFallCollision = hit.transform.gameObject;
        if (collisionStateTransition != null)
        {
          return (collisionStateTransition.TransitionToFromSide, hit.transform);
        }

        //Debug.Log("Falling");
        return (State.Falling, hit.transform);
      }
    }

    return (null, null);
  }

  private bool Unstuck(Vector3 pos, Vector3 inDir, out Vector3 offset)
  {
    var outDir = -inDir;
    offset = Vector3.zero;
    int it = 0;
    while (Physics2D.BoxCast(pos + offset, boxCollider.size, 0, Vector3.zero, 0, collisionMask).collider != null)
    {
      offset += outDir * 0.03f;
      it++;
      if (it == 100) return false;
    }

    return true;
  }

  public void SetVelocityMultiplier(float sensitivity)
  {
    velocityMul = sensitivity.Remap01(0.01f, 0.4f);
    Debug.Log(velocityMul+"  , "+sensitivity);
  }

  private Vector3 PredictVelocityAtT(float time, Vector3 initialVel, Vector3 gravity)
  {
    return gravity * time + initialVel;
  }

  private Vector3 PredictProjectilePosAtT(float time, Vector3 initialVel, Vector3 initialPos, Vector3 gravity)
  {
    return gravity * (0.5f * time * time) + initialVel * time + initialPos;
  }

    public Animator Animator => animator;

    private bool inputStartedThisFrame() {
        bool _inputPressedThisFrame = false;
        if(Mouse.current != null) _inputPressedThisFrame = Mouse.current.leftButton.wasPressedThisFrame;
#if PLATFORM_ANDROID
        if (Touchscreen.current != null) _inputPressedThisFrame = Touchscreen.current.touches[0].press.wasPressedThisFrame;
#endif
        return _inputPressedThisFrame;
    }
    private bool inputThisFrame() {
        bool _inputThisFrame = false;
        if(Mouse.current != null) _inputThisFrame = Mouse.current.leftButton.isPressed;
#if PLATFORM_ANDROID
        if (Touchscreen.current != null) _inputThisFrame = Touchscreen.current.touches[0].press.isPressed;
#endif
        return _inputThisFrame;
    }
    private bool inputEndedThisFrame() {
        bool _inputReleasedThisFrame = false;
        if(Mouse.current != null) _inputReleasedThisFrame = Mouse.current.leftButton.wasReleasedThisFrame;
#if PLATFORM_ANDROID
        if (Touchscreen.current != null) _inputReleasedThisFrame = Touchscreen.current.touches[0].press.wasReleasedThisFrame;
#endif
        return _inputReleasedThisFrame;
    }
    private Vector2 touchMousePosition() {
        Vector2 pos = Vector2.zero;

        if(Mouse.current != null) pos = Mouse.current.position.value;
        
#if PLATFORM_ANDROID
        if (Touchscreen.current != null) pos = Touchscreen.current.touches[0].position.value;

#endif
        
        return pos;
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
  
  public static float Remap01(this float s, float b1, float b2)
  {
    return s.Remap(0, 1.0f, b1, b2);
  }
}