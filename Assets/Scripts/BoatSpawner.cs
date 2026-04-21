using System.Collections;
using UnityEngine;

public class BoatSpawner : MonoBehaviour
{
    [SerializeField] private Vector3 _boatSpawn;
    [SerializeField] private Vector3 _boatDespawn;
    [SerializeField] private float _spawnCooldown = 2f;
    [SerializeField] private float _speed = 5f;
    [SerializeField] private GameObject _boat;

    private bool _hasPlayedHorn = false;
    private Camera _mainCamera;

    private void Start()
    {
        _mainCamera = Camera.main;

        if (_boat != null)
        {
            _boat.transform.position = _boatSpawn;
        }

        StartCoroutine(BoatCycle());
    }

    private void Update()
    {
        if (_boat == null || _mainCamera == null || _hasPlayedHorn)
            return;

        if (IsBoatVisibleOnMainCamera())
        {
            AudioManager.Instance?.PlayShipHorn();
            _hasPlayedHorn = true;
        }
    }

    private IEnumerator BoatCycle()
    {
        while (true)
        {
            _hasPlayedHorn = false;

            while (_boat != null && Vector3.Distance(_boat.transform.position, _boatDespawn) > 0.1f)
            {
                _boat.transform.position = Vector3.MoveTowards(
                    _boat.transform.position,
                    _boatDespawn,
                    _speed * Time.deltaTime
                );

                yield return null;
            }

            yield return new WaitForSeconds(_spawnCooldown);

            if (_boat != null)
            {
                _boat.transform.position = _boatSpawn;
            }
        }
    }

    private bool IsBoatVisibleOnMainCamera()
    {
        Vector3 viewportPos = _mainCamera.WorldToViewportPoint(_boat.transform.position);

        return viewportPos.z > 0f &&
               viewportPos.x >= 0f && viewportPos.x <= 1f &&
               viewportPos.y >= 0f && viewportPos.y <= 1f;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(_boatSpawn, _boatDespawn);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(_boatSpawn, 0.5f);

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(_boatDespawn, 0.5f);
    }
}