using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeaconBillboard : MonoBehaviour
{
    public GameObject _player;
    private Vector3 _lookPos;

    private void Start()
    {
        if(_player == null)
            _player = GameObject.Find("Player");
    }

    private void Update()
    {
        _lookPos = _player.transform.position - transform.position;
        _lookPos.y = 0;
        transform.rotation = Quaternion.LookRotation(-_lookPos);
    }
}
