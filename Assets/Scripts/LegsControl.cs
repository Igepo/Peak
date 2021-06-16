using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class LegsControl : MonoBehaviour
{
    [Header("Leg Settings")]
    public GameObject pt;
    // The position and rotation we want to stay in range of
    [SerializeField] Transform nextStepTransform;
    [SerializeField] Transform targetTransform;
    // Stay within this distance of home
    [SerializeField] float wantStepAtDistance;
    // How long a step takes to complete
    [SerializeField] float moveDuration;
    // Is the leg moving?
    public bool Moving;
    // Fraction of the max distance from home we want to overshoot by
    [SerializeField] float stepOvershootFraction;

    [SerializeField] LegsControl frontLeftLegStepper;
    [SerializeField] LegsControl frontRightLegStepper;
    [SerializeField] LegsControl backLeftLegStepper;
    [SerializeField] LegsControl backRightLegStepper;

    [SerializeField] Transform armature;

    [SerializeField] float distWanted = 0.82f;

    void Awake()
    {
        StartCoroutine(LegUpdateCoroutine());
    }
    void Update()
    {
        LegAttachFloor();
        LegNextStep();
        //MoveBody(); 
    }

    void LegAttachFloor()
    {
        Debug.DrawRay(transform.position, Vector3.down * 5, Color.cyan);

        RaycastHit hit;
        Ray ray = new Ray(transform.position, Vector3.down);

        int layer_mask = LayerMask.GetMask("Default");

        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            //print("Rayon collision");
            //print("Distance =" + hit.distance);
            pt.transform.position = hit.point;
        }
    }

    private void MoveBody()
    {
        if (Moving) return;
        
        //float distFromTheGround = Vector3.Distance(armature.position, nextStepTransform.position);
        Vector3 distFromTheGround = armature.position - nextStepTransform.position;
        float distanceInY = Mathf.Abs(distFromTheGround.y);
        //Debug.Log("Distance " + distanceInY);

        // If we are too far off in position or rotation
        if (distanceInY < distWanted)
        {
            StartCoroutine(MoveBodyUp());
        }
    }
    IEnumerator MoveBodyUp()
    {
        // Indicate we're moving (used later)
        Moving = true;

        // Store the initial conditions
       // Quaternion startRot = armature.rotation;
        Vector3 startPoint = armature.position;

        //Quaternion endRot = homeTransform.rotation;
        Vector3 endPoint = new Vector3(armature.position.x, distWanted, armature.position.z);

        // Time since step started
        float timeElapsed = 0;

        // Here we use a do-while loop so the normalized time goes past 1.0 on the last iteration,
        // placing us at the end position before ending.
        do
        {
            // Add time since last frame to the time elapsed
            timeElapsed += Time.deltaTime;

            float normalizedTime = timeElapsed / moveDuration;

            // Interpolate position and rotation
            armature.position = Vector3.Lerp(startPoint, endPoint, normalizedTime);
            //transform.rotation = Quaternion.Slerp(startRot, endRot, normalizedTime);

            // Wait for one frame
            yield return null;
        }
        while (timeElapsed < moveDuration);

        // Done moving
        Moving = false;
    }

    IEnumerator MoveToHome()
    {
        Moving = true;

        Vector3 startPoint = targetTransform.position;
        Quaternion startRot = targetTransform.rotation;

        Quaternion endRot = nextStepTransform.rotation;

        // Directional vector from the foot to the home position
        Vector3 towardHome = (nextStepTransform.position - targetTransform.position);
        // Total distnace to overshoot by   
        float overshootDistance = wantStepAtDistance * stepOvershootFraction;
        Vector3 overshootVector = towardHome * overshootDistance;
        // Since we don't ground the point in this simplified implementation,
        // we restrict the overshoot vector to be level with the ground
        // by projecting it on the world XZ plane.
        overshootVector = Vector3.ProjectOnPlane(overshootVector, Vector3.up);

        // Apply the overshoot
        Vector3 endPoint = nextStepTransform.position + overshootVector;

        // We want to pass through the center point
        Vector3 centerPoint = (startPoint + endPoint) / 2;
        // But also lift off, so we move it up by half the step distance (arbitrarily)
        centerPoint += nextStepTransform.up * Vector3.Distance(startPoint, endPoint) / 2f;

        float timeElapsed = 0;
        do
        {
            timeElapsed += Time.deltaTime;
            float normalizedTime = timeElapsed / moveDuration;
            normalizedTime = Easing.InOutCubic(normalizedTime);

            // Quadratic bezier curve
            targetTransform.position =
              Vector3.Lerp(
                Vector3.Lerp(startPoint, centerPoint, normalizedTime),
                Vector3.Lerp(centerPoint, endPoint, normalizedTime),
                normalizedTime
              );

            targetTransform.rotation = Quaternion.Slerp(startRot, endRot, normalizedTime);

            yield return null;
        }
        while (timeElapsed < moveDuration);

        Moving = false;
    }

    // Only allow diagonal leg pairs to step together
    IEnumerator LegUpdateCoroutine()
    {
        // Run continuously
        while (true)
        {
            // Try moving one diagonal pair of legs
            do
            {
                frontLeftLegStepper.LegNextStep();
                backRightLegStepper.LegNextStep();
                // Wait a frame
                yield return null;

                // Stay in this loop while either leg is moving.
                // If only one leg in the pair is moving, the calls to TryMove() will let
                // the other leg move if it wants to.
            } while (backRightLegStepper.Moving || frontLeftLegStepper.Moving);

            // Do the same thing for the other diagonal pair
            do
            {
                frontRightLegStepper.LegNextStep();
                backLeftLegStepper.LegNextStep();
                yield return null;
            } while (backLeftLegStepper.Moving || frontRightLegStepper.Moving);
        }
    }

    void LegNextStep()
    {
        if (Moving) return;

        float distFromStart = Vector3.Distance(targetTransform.position, nextStepTransform.position);
        //Debug.Log("distFromHome = " + distFromStart);

        // If we are too far off in position or rotation
        if (distFromStart > wantStepAtDistance)
        {
            // Start the step coroutine
            //targetTransform.position = nextStepTransform.position;
            // Debug.Log(distFromStart);
            StartCoroutine(MoveToHome());   
        }
    }
}
