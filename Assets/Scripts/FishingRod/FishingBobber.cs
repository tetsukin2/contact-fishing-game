using UnityEngine;

public class FishingBobber : MonoBehaviour
{
    private Rigidbody bobberRigidbody;

    private void Awake()
    {
        bobberRigidbody = GetComponent<Rigidbody>();
    }

    public void OnReel(float reelForce)
    {
        bobberRigidbody.isKinematic = false;
        bobberRigidbody.AddForce(Vector3.up * reelForce, ForceMode.Impulse);
    }

    public void OnCast()
    {
        bobberRigidbody.isKinematic = true;
    }
}
