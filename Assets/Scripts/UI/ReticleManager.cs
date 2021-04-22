using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReticleManager : MonoBehaviour
{
    public static ReticleManager Instance;
    
    public GameObject reticle;

    [SerializeField] private Sprite keyboardIcon, controllerIcon;

    private void Awake()
    {
        Instance = this;
    }

    public void ShowReticle()
    {
        reticle.GetComponent<Image>().sprite = Input.GetJoystickNames().Length > 0 ? controllerIcon : keyboardIcon;
        reticle.SetActive(true);
    }

    public void HideReticle() => reticle.SetActive(false);

}
