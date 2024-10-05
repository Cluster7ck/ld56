using UnityEngine;
using UnityEngine.InputSystem;

public enum State
{
  Idle,
  PrepareJump,
  JumpingUp,
  JumpingDown,
  Falling,
  BulletTime,
  Bounce,
  DoubleJumping,
  Dieing
}

public class Cricket : MonoBehaviour
{
  [SerializeField] private Vector3 gravity;
  [SerializeField] private float velocityMul = 1;
  [SerializeField] private float riseGravityMul = 1;
  [SerializeField] private float fallGravityMul = 1;
  [SerializeField] private float collisionCheckExtents;
  [SerializeField] private LayerMask collisionMask;
  [SerializeField] private float bounceStrength;

  private Camera camera;
  private BoxCollider2D boxCollider;
  private State state = State.Idle;

  private bool dragging;
  private Vector3 dragStartPosScreen;
  private Vector3 dragStartPosWorld;
  private Vector3 dragCurrentPosScreen;

  private Vector3 initialVelocity;
  private Vector3 initialJumpPos;

  private Vector3 initialFallPos;
  private Vector3 initialFallVelocity;

  private float jumpTime;
  private float maxJumpTime;

  private Vector3 startPos;

  private GameObject[] debugSpheres = new GameObject[12];

  private GameObject[] arcIndicators = new GameObject[7];


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
      var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
      go.name = $"arcIndicator{i}";
      go.transform.localScale = Vector3.one * 0.1f;
      arcIndicators[i] = go;
    }
  }

  void Update()
  {
    if (Input.GetKeyDown(KeyCode.Space))
    {
      state = State.BulletTime;
      transform.position = startPos;
    }

    if (Input.GetKeyDown(KeyCode.A))
    {
      state = State.Idle;
      foreach (var sp in debugSpheres)
      {
        sp.GetComponent<MeshRenderer>().material.color = Color.gray;
      }
    }

    if (state == State.Idle)
    {
      if (Mouse.current.leftButton.wasPressedThisFrame)
      {
        state = State.PrepareJump;
        dragStartPosScreen = Mouse.current.position.value;
        dragStartPosWorld = camera.ScreenToWorldPoint(dragStartPosScreen);
        for (int i = 0; i < arcIndicators.Length; i++)
        {
          arcIndicators[i].gameObject.SetActive(true);
        }
      }
    }

    if (state == State.PrepareJump)
    {
      if (Mouse.current.leftButton.isPressed)
      {
        dragCurrentPosScreen = Mouse.current.position.value;
      }

      var dragCurrentPosWorld = camera.ScreenToWorldPoint(dragCurrentPosScreen);
      var dragDelta = (dragStartPosWorld - dragCurrentPosWorld);

      initialVelocity = dragDelta * velocityMul;
      initialJumpPos = transform.position;

      Debug.Log(initialVelocity);
      float dt = 0.03f;
      float t = dt;
      for (int i = 0; i < arcIndicators.Length; i++)
      {
        arcIndicators[i].transform.position = PredictProjectilePosAtT(t, initialVelocity, initialJumpPos, gravity * riseGravityMul);
        t += dt;
      }

      if (Mouse.current.leftButton.wasReleasedThisFrame)
      {
        for (int i = 0; i < arcIndicators.Length; i++)
        {
          arcIndicators[i].gameObject.SetActive(false);
        }

        state = State.JumpingUp;
      }
    }
  }

  void FixedUpdate()
  {
    if (state == State.JumpingUp)
    {
      // Predicted position
      jumpTime += Time.fixedDeltaTime;

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
        var nextState = DoCollision2(transform.position, pos);

        if (nextState.HasValue)
        {
          if (nextState.Value == State.Falling)
          {
            initialFallPos = transform.position;
            initialFallVelocity = Vector3.down * 0.05f;
            state = nextState.Value;
          }
          else if (nextState.Value == State.Idle)
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
      jumpTime += Time.fixedDeltaTime;
      var pos = PredictProjectilePosAtT(jumpTime, initialFallVelocity, initialFallPos, gravity * fallGravityMul);

      var nextState = DoCollision2(transform.position, pos);
      if (nextState.HasValue)
      {
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
      jumpTime += Time.fixedDeltaTime;

      var pos = PredictProjectilePosAtT(jumpTime, initialVelocity, initialJumpPos, gravity * fallGravityMul);
      // collision
      var nextState = DoCollision2(transform.position, pos);

      if (nextState.HasValue)
      {
        if (nextState.Value == State.Falling)
        {
          initialFallPos = transform.position;
          initialFallVelocity = Vector3.down * 0.05f;
          state = nextState.Value;
        }
        else if (nextState.Value == State.Idle)
        {
          state = nextState.Value;
        }
        else if(nextState.Value == State.Bounce)
        {
          jumpTime = 0;
          initialVelocity = new Vector3(initialVelocity.x, bounceStrength, 0);
          initialJumpPos = transform.position;
          state = State.JumpingUp;
          //state = nextState.Value;
        }

        jumpTime = 0;
      }
      else
      {
        transform.position = pos;
      }
    }
  }

  private State? DoCollision2(Vector3 previousPos, Vector3 nextPos)
  {
    var extents = boxCollider.size / 2;
    var cameraLeft = camera.ScreenToWorldPoint(new Vector3(0, 1, 0));
    var cameraLeft2 = camera.ScreenToWorldPoint(new Vector3(0, 0, 0));
    Debug.DrawLine(cameraLeft, cameraLeft2, Color.cyan);
    var position = transform.position;
    var dir = nextPos - previousPos;
    
    if (nextPos.x < previousPos.x && (nextPos.x-extents.x) < cameraLeft.x)
    {
      Debug.Log("What");
      var leftDir = Vector3.left * Mathf.Abs((cameraLeft.x + extents.x) - previousPos.x);
      var newLeftDir = Vector3.Project(leftDir, dir);
      Debug.DrawLine(previousPos, previousPos+Vector3.up, Color.blue, 30f);
      Debug.DrawLine(previousPos, previousPos+leftDir, Color.green, 30f);
      Debug.DrawLine(previousPos, previousPos+newLeftDir, Color.red, 30f);
      transform.position += newLeftDir;
      return State.Falling;
    }
    
    var hit = Physics2D.BoxCast(position, boxCollider.size, 0, dir, dir.magnitude, collisionMask);

    if (hit.collider != null)
    {
      if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Bad"))
      {
        Debug.Log("Bad hit");
      }

      var dist = hit.distance;
      var normDir = dir.normalized;
      transform.position += normDir * Mathf.Clamp(dist - 0.005f, 0.005f, dist);

      var dot = Vector3.Dot(hit.normal, Vector3.up) - 1.0f;
      //Debug.Log($"{hit.transform.gameObject.name}: {hit.distance}, {hit.normal}, {dot}");
      if (Mathf.Abs(dot) < Mathf.Epsilon)
      {
        if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Shroom"))
        {
          return State.Bounce;
        }
        // collide with ground
        return State.Idle;
      }
      else
      {
        return State.Falling;
      }
    }

    return null;
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