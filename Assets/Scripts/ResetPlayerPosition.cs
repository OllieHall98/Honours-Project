using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class ResetPlayerPosition : MonoBehaviour
{
    [SerializeField] private Transform playerStartTransform;
    

    [Button("Reset Position")]
    public void ResetPosition()
    {
        transform.position = playerStartTransform.position;
        transform.rotation = playerStartTransform.rotation;
    }
}
