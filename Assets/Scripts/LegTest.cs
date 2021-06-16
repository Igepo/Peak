using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegTest : MonoBehaviour
{
    // The position and rotation we want to stay in range of
    [SerializeField] Transform homeTransform;
    [SerializeField] Transform targetTransform;
    // Stay within this distance of home
    [SerializeField] float wantStepAtDistance;
    // How long a step takes to complete
    [SerializeField] float moveDuration;

    // Update is called once per frame
    void Update()
    {
        float distFromHome = Vector3.Distance(transform.position, homeTransform.position);
        Debug.Log("distFromHome = " + distFromHome);

        // If we are too far off in position or rotation
        if (distFromHome > wantStepAtDistance)
        {
            // Start the step coroutine
            targetTransform.position = homeTransform.position;
            Debug.Log("stop");

        }
    }
}
