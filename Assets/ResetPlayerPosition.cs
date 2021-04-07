using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class ResetPlayerPosition : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [Button("Reset Position")]
    public void ResetPosition()
    {
        gameObject.transform.position = new Vector3(230.9f, 62.629f, 221.53f);
    }
}
