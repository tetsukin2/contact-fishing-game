using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FishingBobber : MonoBehaviour
{
    [SerializeField] private float CastMaxLineLength = 5f;
    [SerializeField] private float ReeledMaxLineLength = 0.5f;

    //[SerializeField] private Transform _fishingRodTip;

    public UnityEvent HasHitWater = new();

    private bool _hasBeenCast = false;  // Flag to check if the bobber has been cast, so reeling doesn't stick

    private SpringJoint hookSpringJoint;
    private Rigidbody bobberRigidbody;

    private void Awake()
    {
        bobberRigidbody = GetComponent<Rigidbody>();
        hookSpringJoint = GetComponent<SpringJoint>();
    }

    // Start is called before the first frame update
    void Start()
    {
        // Setup line length
        hookSpringJoint.maxDistance = ReeledMaxLineLength;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the bobber collides with the water surface
        if (_hasBeenCast && collision.gameObject.CompareTag("Water"))
        {
            HasHitWater.Invoke();
            Debug.Log("Water Hit");
            bobberRigidbody.isKinematic = true;  // Stop the bobber from moving
        }
    }

    public void Reel()
    {
        _hasBeenCast = false;
        bobberRigidbody.isKinematic = false;
        // Set the maximum distance to the reeling length
        hookSpringJoint.maxDistance = ReeledMaxLineLength;
    }

    public void Cast(float castForce, float drag)
    {
        _hasBeenCast = true;
        // Set the maximum distance to the reeling length
        hookSpringJoint.maxDistance = CastMaxLineLength;
        //bobberRigidbody.velocity = transform.forward * castForce;
        //bobberRigidbody.drag = drag;  // Lower drag to allow a longer cast
        bobberRigidbody.AddForce(transform.forward * castForce, ForceMode.Impulse);
    }
}
