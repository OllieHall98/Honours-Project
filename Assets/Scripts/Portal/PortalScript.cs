using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalScript : MonoBehaviour
{
    public static PortalScript Instance;
    
    [SerializeField] private GameObject portalLight;

    [SerializeField] private GameObject portalEntryCollider;
    
    private Animator cutsceneAnimator;
    private static readonly int StartCutscene = Animator.StringToHash("StartCutscene");

    private void Awake()
    {
        Instance = this;

        cutsceneAnimator = GetComponent<Animator>();
        
        portalEntryCollider.SetActive(false);
        
        Disable();
    }

    public void Enable()
    {
        portalLight.SetActive(true);
    }
    
    public void Disable()
    {
        portalLight.SetActive(false);
    }

    public void OpenPortal()
    {
        StartCoroutine(PortalCutscene());
    }

    IEnumerator PortalCutscene()
    {
        cutsceneAnimator.SetTrigger(StartCutscene);

        yield return new WaitForSecondsRealtime(14f);
        
        portalEntryCollider.SetActive(true);
        
        PlayerStateScript.Instance.SetRaycastState(RaycastState.Enabled);
        PlayerStateScript.Instance.SetMovementActive(true, true);
        BlackBarTransitioner.Instance.Hide(2f);
        UIVisibilityScript.Instance.ShowUI(2.0f);
    }
    
}
