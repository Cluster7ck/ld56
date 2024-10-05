using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spider : MonoBehaviour
{
    private SpringJoint2D springJoint;
    private Vector3 initSprintJointPos;
    private LineRenderer lineRenderer;

    [SerializeField] private float radius = 5f; // Der Radius des Kreises
    [SerializeField] private float moveSpeed = 0.5f; // Geschwindigkeit der Bewegung
    [SerializeField] private float interval = 2f; // Zeitintervall, nach dem eine neue Position gewählt wird

    private Vector2 targetPosition; // Die Zielposition
    private Vector2 startPosition; // Startposition für Slerp
    private float timer = 0f; // Timer für das Intervall
    private bool moving = false;
    private float startTime = 0;

    // Start is called before the first frame update
    void Start() {
        lineRenderer = GetComponent<LineRenderer>();
        springJoint = GetComponent<SpringJoint2D>();
        initSprintJointPos = springJoint.connectedAnchor;
    }

    // Update is called once per frame
    void Update() {
        DrawSpiderWeb();
        JointJitter();    
    }

    private void DrawSpiderWeb() {
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, springJoint.connectedAnchor);
    }

    private void JointJitter() {
        timer += Time.deltaTime;

        if (timer >= interval) {
            startPosition = springJoint.connectedAnchor; 
            targetPosition = Random.insideUnitCircle * radius + (Vector2)initSprintJointPos;
            Debug.Log("Random Pos: " + targetPosition);
            timer = 0f;
            moving = true;
            startTime = Time.time;
        }

        if (moving) {
            float fracComplete = (Time.time - startTime) / moveSpeed;
            springJoint.connectedAnchor = Vector3.Slerp(startPosition, targetPosition, fracComplete);
            if (Vector2.Distance(springJoint.connectedAnchor, targetPosition) < 0.1f) moving = false;
        }
    }

}
