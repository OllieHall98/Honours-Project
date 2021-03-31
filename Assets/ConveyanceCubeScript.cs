using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public enum CubeState
{
    Positive,
    Negative
};

public class ConveyanceCubeScript : MonoBehaviour
{
    public static ConveyanceCubeScript Instance;
    
    private CubeState _currentState;
    private GameObject _player;
    
    [Header("ScriptableObjects")]
    public ConveyanceCubePreset positivePreset;
    public ConveyanceCubePreset negativePreset;
    
    private MoodReceptorScript _currentReceptorScript;
    public Dictionary<Transform, MoodReceptorScript> MoodReceptorDictionary;

    #region visuals

    [Header("Assigned objects")]
    [Space(10)]
    public GameObject hitEffect;
    public LineRenderer line;
    private GameObject _positiveEffect, _negativeEffect;
    private ParticleSystem[] _positiveParticles, _negativeParticles;
    private ParticleSystemRenderer _hitParticles;
    private Light _hitLight;
        
    #endregion
    
    #region pickup

        public Transform objectHolder;
    
        [HideInInspector] public bool isHeld;
    
        private bool _coroutineExecuting;
        [Header("Values")]
        [Space(10)]
        public float lerpDuration = 0.5f;
        
        private BoxCollider _boxCollider;
        
    #endregion

    #region raycast

        [SerializeField] private float rayDistance = 10;
        [SerializeField] private Vector3 raycastOffset = new Vector3(0,2.5f, 0);
        
    #endregion

    
    
    private void Awake()
    {
        Instance = this;
        MoodReceptorDictionary = new Dictionary<Transform, MoodReceptorScript>();
        
        _positiveEffect = transform.Find("Positive").gameObject;
        _negativeEffect = transform.Find("Negative").gameObject;
        _player = GameObject.Find("Player");
        
        GetComponents();
    }

    private void Start()
    {
        InitialiseLineRenderer();
        SetBeamActive(false);
    }

    private void GetComponents()
    {
        _boxCollider = GetComponent<BoxCollider>();
        
        _hitParticles = hitEffect.GetComponent<ParticleSystemRenderer>();
        _hitLight = hitEffect.GetComponentInChildren<Light>();

        _positiveParticles = _positiveEffect.GetComponentsInChildren<ParticleSystem>();
        _negativeParticles = _negativeEffect.GetComponentsInChildren<ParticleSystem>();
    }

    private void InitialiseLineRenderer()
    {
        line.positionCount = 2;
        var initLaserPositions = new Vector3[ 2 ] { Vector3.zero, Vector3.zero };
        line.SetPositions( initLaserPositions );
    }

    private void Update()
    {
        if (!isHeld) return;
        
        if (Input.GetKeyDown(KeyCode.Alpha1)) ChangeState(CubeState.Positive);
        if (Input.GetKeyDown(KeyCode.Alpha2)) ChangeState(CubeState.Negative);
    }

    private void LateUpdate()
    {
        if (!isHeld) return;

        if (Input.GetButton("Fire1"))
            FireBeam();
        else
            SetBeamActive(false);
    }
    
    private void SetBeamActive(bool value)
    {
        line.enabled = value;
        hitEffect.SetActive(value);
    }

    private void FireBeam()
    {
        SetBeamActive(true);
        
        var cubeTransform = transform;
        var cubePosition = cubeTransform.position;
        var hitPosition = cubePosition + (cubeTransform.forward + raycastOffset) * rayDistance;

        if (!Physics.Raycast(transform.position, transform.forward + raycastOffset, out var hit, rayDistance))
            goto setBeamHit;

        if (!hit.collider.CompareTag($"MoodReceptor"))
            goto setBeamHit;

        _currentReceptorScript = MoodReceptorDictionary[hit.transform];

        if (!_currentReceptorScript.executing)
            _currentReceptorScript.DetermineValueChange(_currentState);

        // Set hit point with fixed Y position
        hitPosition = new Vector3(hit.point.x, hitPosition.y, hit.point.z);

        setBeamHit:
            SetBeamHitPosition(cubePosition, hitPosition);
    }

    private void SetBeamHitPosition(Vector3 cubePosition, Vector3 hitPosition)
    {
        hitEffect.transform.position = hitPosition;
        line.SetPosition(0, cubePosition);
        line.SetPosition(1, hitPosition);
    }
    

    public void Pickup()
    {
        _boxCollider.enabled = false;

        if (_coroutineExecuting) return;
        
        StartCoroutine(MoveToPlayer());
    }

    private IEnumerator MoveToPlayer()
    {
        _coroutineExecuting = true;
        
        var cubeTransform = transform;
        var startPosition = cubeTransform.position;

        cubeTransform.parent = _player.transform;

        cubeTransform.localRotation = Quaternion.identity;


        float elapsedTime = 0;

        while (elapsedTime < lerpDuration)
        {
            transform.position = Vector3.Lerp(startPosition, objectHolder.position, elapsedTime / lerpDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // for (_ft = 0f; _ft <= 1; _ft += Time.deltaTime / lerpDuration)
        // {
        //     transform.position = Vector3.Lerp(startPosition, objectHolder.position, _ft);
        //     yield return null;
        // }
        
        isHeld = true;
        
        _coroutineExecuting = false;
    }

    private void ChangeState(CubeState state)
    {
        if (_currentState == state)
            return;
        
        if (state == CubeState.Positive)
        {
            line.material = positivePreset.coreMaterial;
            _hitParticles.trailMaterial = positivePreset.coreMaterial;
            _hitLight.color = positivePreset.color;

            StopParticleEffects(_negativeEffect);
            StartParticleEffects(_positiveEffect);
            
        }
        else
        {
            line.material = negativePreset.coreMaterial;
            _hitParticles.trailMaterial = negativePreset.coreMaterial;
            _hitLight.color = negativePreset.color;
            
            StopParticleEffects(_positiveEffect);
            StartParticleEffects(_negativeEffect);
        }

        _currentState = state;
    }

    private void StopParticleEffects(GameObject particleGroup)
    {
        if (particleGroup == _positiveEffect)
        {
            foreach (var ps in _positiveParticles)
                ps.Stop();
        }
        else
        {
            foreach (var ps in _negativeParticles)
                ps.Stop();
        }

        particleGroup.SetActive(false);
    }

    private void StartParticleEffects(GameObject particleGroup)
    {
        particleGroup.SetActive(true);
        
        if (particleGroup == _positiveEffect)
        {
            foreach (var ps in _positiveParticles)
                ps.Play();
        }
        else
        {
            foreach (var ps in _negativeParticles)
                ps.Play();
        }
    }
}
