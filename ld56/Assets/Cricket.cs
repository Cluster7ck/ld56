using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.ReorderableList;
using UnityEngine;
using UnityEngine.InputSystem;

public enum State
{
  Idle,
  PrepareJump,
  JumpingUp,
  JumpingDown,
  Falling,
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
  [SerializeField] private AnimationCurve jumpCurve;

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

    for (int i = 0; i < 12; i++)
    {
      var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
      go.name = $"{i}";
      go.transform.localScale = Vector3.one * 0.1f;
      debugSpheres[i] = go;
    }
    
    for (int i = 0; i < 12; i++)
    {
      var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
      go.name = $"arcIndicator{i}";
      go.transform.localScale = Vector3.one * 0.1f;
      arcIndicators[i] = go;
    }
  }

  // Update is called once per frame
  void Update()
  {
    if (Input.GetKeyDown(KeyCode.Space))
    {
      state = State.Idle;
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
    else if (state == State.JumpingUp)
    {
      // Predicted position
      jumpTime += Time.deltaTime;

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
        var nextState = DoCollision(transform.position, pos);

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
      jumpTime += Time.deltaTime;
      var pos = PredictProjectilePosAtT(jumpTime, initialFallVelocity, initialFallPos, gravity * fallGravityMul);

      var nextState = DoCollision(transform.position, pos);
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
      jumpTime += Time.deltaTime;

      var pos = PredictProjectilePosAtT(jumpTime, initialVelocity, initialJumpPos, gravity * fallGravityMul);
      // collision
      var nextState = DoCollision(transform.position, pos);


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

  private State? DoCollision(Vector3 previousPos, Vector3 nextPos)
  {
    var extents = boxCollider.size / 2;

    // up
    if (previousPos.y < nextPos.y)
    {
      var upDistance = nextPos.y - transform.position.y;
      var leftUp = transform.position + (Vector3.up * extents.y) + (Vector3.left * extents.x);
      var middleUp = transform.position + (Vector3.up * extents.y);
      var rightUp = transform.position + (Vector3.up * extents.y) + (Vector3.right * extents.x);
      debugSpheres[3].transform.position = leftUp;
      debugSpheres[4].transform.position = middleUp;
      debugSpheres[5].transform.position = rightUp;

      var checks = new[] { leftUp, middleUp, rightUp };
      for (var i = 0; i < checks.Length; i++)
      {
        var origin = checks[i];
        var res = Physics2D.Raycast(origin, Vector2.up, upDistance * collisionCheckExtents);
        if (res.collider != null)
        {
          Debug.Log($"UpCollision {res.collider.gameObject.name}");
          debugSpheres[i + 3].GetComponent<MeshRenderer>().material.color = Color.red;
          return State.Falling;
        }
      }
    }

    // right
    if (previousPos.x < nextPos.x)
    {
      var rightDistance = nextPos.x - transform.position.x;
      var upRight = transform.position + (Vector3.up * extents.y) + (Vector3.right * extents.x);
      var middleRight = transform.position + (Vector3.right * extents.x);
      var downRight = transform.position + (Vector3.down * extents.y) + (Vector3.right * extents.x);
      debugSpheres[6].transform.position = upRight;
      debugSpheres[7].transform.position = middleRight;
      debugSpheres[8].transform.position = downRight;

      var checks = new[] { upRight, middleRight, downRight };
      State? nextState = null;
      for (var i = 0; i < checks.Length; i++)
      {
        var origin = checks[i];
        var res = Physics2D.Raycast(origin, Vector2.right, Mathf.Abs(rightDistance * collisionCheckExtents * 2));
        if (res.collider != null)
        {
          Debug.Log($"RightCollision {res.collider.gameObject.name} {i} {origin}");
          debugSpheres[i + 6].GetComponent<MeshRenderer>().material.color = Color.red;
          return State.Falling;
        }
      }
    }

    // left
    if (previousPos.x > nextPos.x)
    {
      var leftDistance = transform.position.x - nextPos.x;
      var upLeft = transform.position + (Vector3.up * extents.y) + (Vector3.left * extents.x);
      var middleLeft = transform.position + (Vector3.left * extents.x);
      var downLeft = transform.position + (Vector3.down * extents.y) + (Vector3.left * extents.x);
      debugSpheres[9].transform.position = upLeft;
      debugSpheres[10].transform.position = middleLeft;
      debugSpheres[11].transform.position = downLeft;

      var checks = new[] { upLeft, middleLeft, downLeft };
      State? nextState = null;
      for (var i = 0; i < checks.Length; i++)
      {
        var origin = checks[i];
        var res = Physics2D.Raycast(origin, Vector2.left, Mathf.Abs(leftDistance * collisionCheckExtents * 2));
        if (res.collider != null)
        {
          Debug.Log($"LeftCollision {res.collider.gameObject.name}");
          debugSpheres[i + 9].GetComponent<MeshRenderer>().material.color = Color.red;
          return State.Falling;
        }
      }
    }

    // down
    if (previousPos.y > nextPos.y)
    {
      //Debug.Log("DownCheck");
      var downDistance = transform.position.y - nextPos.y;
      var leftDown = transform.position + (Vector3.down * extents.y) + (Vector3.left * extents.x);
      var middleDown = transform.position + (Vector3.down * extents.y);
      var rightDown = transform.position + (Vector3.down * extents.y) + (Vector3.right * extents.x);
      debugSpheres[0].transform.position = leftDown;
      debugSpheres[1].transform.position = middleDown;
      debugSpheres[2].transform.position = rightDown;

      var checks = new[] { leftDown, middleDown, rightDown };
      for (var i = 0; i < checks.Length; i++)
      {
        var origin = checks[i];
        var res = Physics2D.Raycast(origin, Vector2.down, downDistance * collisionCheckExtents);
        if (res.collider != null)
        {
          Debug.Log($"DownCollision {res.collider.gameObject.name}");
          debugSpheres[i].GetComponent<MeshRenderer>().material.color = Color.red;
          return State.Idle;
        }
      }
    }

    return null;
  }

  private Vector3[] Projectile(Vector3 pos, Vector3 angle, float force)
  {
    return new[] { Vector3.one };
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