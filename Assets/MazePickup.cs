using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazePickup : MonoBehaviour
{
    public static MazePickup Instance;
    
    [SerializeField] private Animator mazeStartAnimator;
    private static readonly int Start1 = Animator.StringToHash("Start");

    [SerializeField] private AK.Wwise.Event showSymbol;
    [SerializeField] private AK.Wwise.Event pickupPaper;
    
    [SerializeField] private GameObject paper, heldPaper;

    private void Awake()
    {
        Instance = this;
    }

    public void Pickup()
    {
        pickupPaper.Post(gameObject);
        
        paper.SetActive(false);
        heldPaper.SetActive(true);

        StartCoroutine(MazeSymbolCutscene());
    }

    IEnumerator MazeSymbolCutscene()
    {
        PlayerStateScript.Instance.SetMovementActive(false, true);
        UIVisibilityScript.Instance.HideUI(0.5f);
        BlackBarTransitioner.Instance.Show(1.0f);
        
        yield return new WaitForSecondsRealtime(1.0f);
        
        PlayerStateScript.Instance.SetMovementActive(false, false);
        
        mazeStartAnimator.SetTrigger(Start1);
        
        showSymbol.Post(gameObject);
        
        yield return new WaitForSecondsRealtime(5.0f);

        PlayerStateScript.Instance.SetMovementActive(true, true);
        BlackBarTransitioner.Instance.Hide(2f);
        UIVisibilityScript.Instance.ShowUI(2.0f);

    }
}
