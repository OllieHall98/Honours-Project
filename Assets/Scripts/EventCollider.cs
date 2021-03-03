using UnityEngine.Events;
using UnityEngine;
using NaughtyAttributes;

public class EventCollider : MonoBehaviour
{
    public UnityEvent e;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player") InvokeEvent();
    }

    [Button("Call Event")]
    void InvokeEvent()
    {
        e.Invoke();
    }
}
