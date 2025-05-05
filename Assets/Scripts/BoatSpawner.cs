using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatSpawner : MonoBehaviour
{
    [SerializeField] private Vector3 _boatSpawn; // Starting position of the boat
    [SerializeField] private Vector3 _boatDespawn; // Ending position of the boat
    [SerializeField] private float _spawnCooldown = 2f; // Time to wait before respawning the boat
    [SerializeField] private float _speed = 5f; // Speed of the boat
    [SerializeField] private GameObject _boat; // Boat prefab or object

    //private GameObject _boat; // Reference to the currently spawned boat

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(BoatCycle());
    }

    private void OnDrawGizmosSelected()
    {
        // Draw a line from the boat spawn position to the despawn position
        Gizmos.color = Color.red;
        Gizmos.DrawLine(_boatSpawn, _boatDespawn);
        // Draw a sphere at the boat spawn position
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(_boatSpawn, 0.5f);
        // Draw a sphere at the despawn position
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(_boatDespawn, 0.5f);
    }

    private IEnumerator BoatCycle()
    {
        while (true)
        {
            // Spawn the boat at the starting position
            //if (_boat == null)
            //{
            //    _boat = Instantiate(_boat, _boatSpawn, Quaternion.identity);
            //}
            //else
            //{
            //    _boat.transform.position = _boatSpawn;
            //}

            // Move the boat to the despawn position
            while (Vector3.Distance(_boat.transform.position, _boatDespawn) > 0.1f)
            {
                _boat.transform.position = Vector3.MoveTowards(
                    _boat.transform.position,
                    _boatDespawn,
                    _speed * Time.deltaTime
                );
                yield return null;
            }

            // Wait for the cooldown before restarting the cycle
            yield return new WaitForSeconds(_spawnCooldown);

            _boat.transform.position = _boatSpawn; // Reset the boat position
        }
    }
}
