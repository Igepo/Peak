using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveUp : MonoBehaviour
{
    [SerializeField] float height = 5.0f;
    [SerializeField] float forceMultiplier = 10.0f;

    [SerializeField] float distWanted = 0.82f;

    public Rigidbody rb;
    // Is the leg moving?
    public bool Moving;

    [SerializeField] Transform armature;
    [SerializeField] Transform nextStepTransform;
    // How long a step takes to complete
    [SerializeField] float moveDuration;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    void Update()
    {
        //Test1();
        MoveBody();
    }

    private void MoveBody()
    {
        //if (Moving) return;

        //float distFromTheGround = Vector3.Distance(armature.position, nextStepTransform.position);
        Vector3 distFromTheGround = armature.position - nextStepTransform.position;
        float distanceInY = Mathf.Abs(distFromTheGround.y);
        Debug.Log("Distance " + distanceInY);

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

    void Test1()
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.position, Vector3.down);
        Debug.DrawRay(transform.position, Vector3.down * distWanted, Color.red);

        if (Physics.Raycast(ray, out hit, height))
            {
            rb.AddForce(transform.up * (forceMultiplier / (hit.distance / 2)));
            }
    }
}
