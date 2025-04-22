using UnityEngine;

public class FishingRodController : MonoBehaviour
{
    public Rigidbody hookRigidbody;  // Assign Hook Rigidbody in Inspector
    public Transform rodTip;  // Assign Rod Tip in Inspector
    public float castForce = 10f;  // Adjust casting strength
    public float CastTriggerDegrees = 10f;
    private Vector3 previousPosition;
    private Vector3 rodVelocity;

    void Start()
    {
        previousPosition = rodTip.position;  // Store initial rod position
    }

    void Update()
    {
        // Calculate rod movement velocity
        rodVelocity = (rodTip.position - previousPosition) / Time.deltaTime;
        previousPosition = rodTip.position;

        // Only cast if the rod is moving forward (not backward)
        // Only allow casting when moving forward (not sideways or up)
        if (rodVelocity.magnitude > 3.0f && rodVelocity.z > 1.2f && Mathf.Abs(rodVelocity.x) < 1.0f)
        {
            CastLine();
        }


        // Reel in when holding Left Click
        if (Input.GetMouseButton(0))
        {
            ReelIn();
        }
    }

    void CastLine()
    {
        hookRigidbody.velocity = rodVelocity * 1.2f;  // Increased multiplier for more force
        hookRigidbody.drag = 0.5f;  // Lower drag to allow a longer cast
        Debug.Log("Casting Fishing Line!");
    }

    void ReelIn()
    {
        Vector3 pullDirection = (rodTip.position - hookRigidbody.position).normalized;
        hookRigidbody.AddForce(pullDirection * castForce * 0.5f, ForceMode.Acceleration);
        Debug.Log("Reeling In!");
    }
}
