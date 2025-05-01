using UnityEngine;

public class FishingBobber : MonoBehaviour
{
    private Rigidbody bobberRigidbody;

    private void Awake()
    {
        bobberRigidbody = GetComponent<Rigidbody>();
    }

    public void OnReel()
    {
        bobberRigidbody.isKinematic = false;
    }

    public void OnCast()
    {
        bobberRigidbody.isKinematic = true;
    }
}
