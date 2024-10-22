using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Weather;

public class PortalScript : MonoBehaviour
{
    public static PortalScript Instance;
    
    [SerializeField] private GameObject portalLight;

    [SerializeField] private GameObject portalEntryCollider;

    [SerializeField] private AK.Wwise.Event allRelicsCollected;
    [SerializeField] private AK.Wwise.Event portalHum;
    [SerializeField] private AK.Wwise.Event portalOpen;

    [SerializeField] private WeatherTypeData sunset;
    
    private Animator cutsceneAnimator;
    private static readonly int StartCutscene = Animator.StringToHash("StartCutscene");

    private void Awake()
    {
        Instance = this;

        cutsceneAnimator = GetComponent<Animator>();
        
        portalEntryCollider.SetActive(false);
        
        Enable();
    }

    public void Enable()
    {
        portalLight.GetComponent<MeshRenderer>().enabled = true;
        //portalLight.SetActive(true);
    }
    
    public void Disable()
    {
        portalLight.GetComponent<MeshRenderer>().enabled = false;
        //portalLight.SetActive(false);
    }

    public void OpenPortal()
    {
        AffectiveWeather.Instance.canChangeWeather = false;
        WeatherController.Instance.ChangeWeather(sunset, 1.0f);
        
        StartCoroutine(PortalCutscene());
    }

    IEnumerator PortalCutscene()
    {
        GetComponent<Animator>().enabled = true;
        
        cutsceneAnimator.SetTrigger(StartCutscene);

        allRelicsCollected.Post(gameObject);
        
        yield return new WaitForSecondsRealtime(6.7f);
        
        portalOpen.Post(gameObject);
        
        yield return new WaitForSecondsRealtime(1.8f);
        
        portalHum.Post(portalLight);
        
        yield return new WaitForSecondsRealtime(4.5f);
        
        
        
        portalEntryCollider.SetActive(true);
        
        PlayerStateScript.Instance.SetRaycastState(RaycastState.Enabled);
        PlayerStateScript.Instance.SetMovementActive(true, true);
        BlackBarTransitioner.Instance.Hide(2f);
        UIVisibilityScript.Instance.ShowUI(2.0f);
    }
    
}
