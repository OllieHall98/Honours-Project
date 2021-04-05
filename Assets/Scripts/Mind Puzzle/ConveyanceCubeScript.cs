using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using UI;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;

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

    [SerializeField] private string cubePickupSentence;

    [SerializeField] private AK.Wwise.Event startFireBeam;
    [SerializeField] private AK.Wwise.Event stopFireBeam;
    
    private bool _firstFire;

    private bool firing = false;
    
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

        _positiveEffect = transform.Find("Positive").gameObject;
        _negativeEffect = transform.Find("Negative").gameObject;
        _player = GameObject.Find("Player");
        
        GetComponents();
        _firstFire = false;
    }

    private void Start()
    {
        Instance = this;
        
        MoodReceptorDictionary = new Dictionary<Transform, MoodReceptorScript>();
        
        InitialiseLineRenderer();
        SetBeamActive(false);
        
        SetPositiveVisuals();
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
        
        ChangeState(DetermineCurrentEmotiveState());
    }

    private static CubeState DetermineCurrentEmotiveState()
    {
        if (AffectiveManager.Instance == null) return CubeState.Positive;
        
        return AffectiveManager.Instance.GetCurrentEmotion() switch
        {
            "joy" => CubeState.Positive,
            "surprise" => CubeState.Positive,
            "neutral" => CubeState.Positive,
            "anger" => CubeState.Negative,
            "sadness" => CubeState.Negative,
            "fear" => CubeState.Negative,
            "disgust" => CubeState.Negative,
            _ => CubeState.Positive
        };
    }

    private void LateUpdate()
    {
        if (!isHeld) return;

        if (Input.GetButton("Fire1"))
            FireBeam();
        else
        {
            if (firing)
            {
                stopFireBeam.Post(gameObject);
                firing = false;
            }
            SetBeamActive(false);
        }
    }
    
    private void SetBeamActive(bool value)
    {
        line.enabled = value;
        hitEffect.SetActive(value);
    }

    private void FireBeam()
    {
        if (!_firstFire)
        {
            NotificationText.Instance.StopDisplayingText(TransitionType.Fade);
            _firstFire = true;
        }

        if (!firing)
        {
            startFireBeam.Post(gameObject);
            firing = true;
        }
        
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
       

        isHeld = true;
        
        NotificationText.Instance.DisplayMessage(TransitionType.Float ,cubePickupSentence, 0f);
        // PlayerStateScript.Instance.SetMovementActive(false, true);
        //
        // NotificationText.Instance.DisplayMessage(TransitionType.Float ,cubePickupSentence, 0f);
        //
        // _boxCollider.enabled = false;
        //
        // if (_coroutineExecuting) return;
        //
        // //var fdsf = transform.position;
        // transform.parent = _player.transform;
        // ///transform.position = fdsf;
        // StartCoroutine(MoveToPlayer());
    }

    private IEnumerator MoveToPlayer()
    {
        _coroutineExecuting = true;
        
        var cubeTransform = transform;
        var startPosition = cubeTransform.position;
        float elapsedTime = 0;

        cubeTransform.localRotation = Quaternion.identity;

        while (elapsedTime < lerpDuration)
        {
            transform.position = Vector3.Lerp(startPosition, objectHolder.position, elapsedTime / lerpDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
 
        isHeld = true;
        _coroutineExecuting = false;
    }

    private void ChangeState(CubeState state)
    {
        if (_currentState == state)
            return;
        
        if (state == CubeState.Positive)
            SetPositiveVisuals();
        else
            SetNegativeVisuals();

        _currentState = state;
    }

    private void SetNegativeVisuals()
    {
        line.material = negativePreset.coreMaterial;
        _hitParticles.trailMaterial = negativePreset.coreMaterial;
        _hitLight.color = negativePreset.color;

        StopParticleEffects(_positiveEffect);
        StartParticleEffects(_negativeEffect);
    }

    private void SetPositiveVisuals()
    {
        line.material = positivePreset.coreMaterial;
        _hitParticles.trailMaterial = positivePreset.coreMaterial;
        _hitLight.color = positivePreset.color;

        StopParticleEffects(_negativeEffect);
        StartParticleEffects(_positiveEffect);
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
