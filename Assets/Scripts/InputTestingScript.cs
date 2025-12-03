using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using MoreMountains.Feedbacks;

public class InputTestingScript : MonoBehaviour
{
    InputAction clickAction,clickLocation;

    public GameObject ballObject;
    public Transform ballSpawnPoint;
    public Camera testCam;

    public float shootForce = 1f; //ball lauunch speed

    //trajectory variables
    public LineRenderer lineRenderer; //used for displaying trajectory

    public float timeStep= 0.05f;
    public float projectileMass = 1f;
    public float bounceDamping = 0.8f; // how much speed is lost on bounce

    public int maxSteps = 200;         // max number of physics steps to simulate

    public bool holdingBall = false;

    public MMF_Player holdFeedback,launchFeedback;

    private bool stopFeedbackLoop;

    void Start()
    {
        clickAction = InputSystem.actions.FindAction("Click");
        clickLocation = InputSystem.actions.FindAction("Point");
        HideTrajectory();
    }

    void Update()
    {
        if (!Gamemanager.Instance.gameEnded)
        {
            HandleBallLogic();
        }

    }

    public void HandleBallLogic()
    {
        Ray ray = testCam.ScreenPointToRay(clickLocation.ReadValue<Vector2>()); //get the click point on screen

        Vector3 targetDir = ray.direction.normalized;

       if(clickAction.IsPressed()) //new input system reads mouse and touch controls
        {
            //lock out of feedback loop
            if (!stopFeedbackLoop)
            {
                holdFeedback?.PlayFeedbacks();
                stopFeedbackLoop = true;
            }           
            // ready to shoot ball state
            holdingBall = true;
            ResetBallPosition();
            DrawTrajectory(targetDir);
        }

        if (clickAction.WasReleasedThisFrame())
        {
            stopFeedbackLoop = false;
            launchFeedback?.PlayFeedbacks();
            ballObject.GetComponent<Rigidbody>().AddForce(targetDir * shootForce, ForceMode.Impulse); // update to aim at the location of touch or cursor
            HideTrajectory();
            holdingBall = false;
        }
    }

    //reset ball to starting position and stops all rigidbody movenet
    public void ResetBallPosition()
    {
        ballObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        ballObject.transform.position = ballSpawnPoint.position;
        ballObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
    }


    public void DrawTrajectory(Vector3 direction)
    {
        Vector3 pos = ballSpawnPoint.position;
        Vector3 velocity = direction * shootForce;

        List<Vector3> points = new List<Vector3>();
        points.Add(pos);

        for (int i = 0; i < maxSteps; i++)
        {
            Vector3 nextPos = pos + velocity * timeStep + 0.5f * Physics.gravity * timeStep * timeStep;

            // Raycast from pos → nextPos to detect walls
            if (Physics.Raycast(pos, velocity.normalized, out RaycastHit hit, (nextPos - pos).magnitude))
            {
                // add hit point to line
                points.Add(hit.point);

                // Reflect velocity
                velocity = Vector3.Reflect(velocity, hit.normal) * bounceDamping;

                // Move start slightly away from wall to avoid sticking
                pos = hit.point + hit.normal * 0.01f;

                continue;
            }

            // No hit > continue arc
            points.Add(nextPos);

            // Update velocity
            velocity += Physics.gravity * timeStep;

            pos = nextPos;
        }
        // Render points
        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
    }

    //sets positions 0 so no line is rendered
    public void HideTrajectory()
    {
        lineRenderer.positionCount = 0;
    }
}
