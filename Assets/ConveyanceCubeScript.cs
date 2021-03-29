using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class ConveyanceCubeScript : MonoBehaviour
{
    private enum CubeState
    {
        Positive,
        Negative
    };

    private CubeState _currentState;
    
    public static ConveyanceCubeScript Instance;
    
    private GameObject _player;
    
    #region pickup
        
        [SerializeField] private bool coroutineExecuting;    
    
        private float _ft;
        
        public float lerpDuration = 0.5f;

        public Transform playerPickupTransform;
        
        private BoxCollider _boxCollider;
        
    #endregion

    #region raycast

        public Vector3 raycastOffset;
        
    #endregion

    #region visuals

        public GameObject positiveEffect, negativeEffect;
        
        private ParticleSystem[] _positiveParticles, _negativeParticles;
        
        public GameObject hitEffect;
        
        private ParticleSystemRenderer _hitParticles;
        
        private Light _hitLight;
        
        public LineRenderer line;
        
        public Material negativeMat, positiveMat;

        public Color negativeLightColor, positiveLightColor;
        
    #endregion
    
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _player = GameObject.Find("Player");
        _boxCollider = GetComponent<BoxCollider>();
        
        _hitParticles = hitEffect.GetComponent<ParticleSystemRenderer>();
        _hitLight = hitEffect.GetComponentInChildren<Light>();

        _positiveParticles = positiveEffect.GetComponentsInChildren<ParticleSystem>();
        _negativeParticles = negativeEffect.GetComponentsInChildren<ParticleSystem>();
        
        line.positionCount = 2;
        
        Vector3[] initLaserPositions = new Vector3[ 2 ] { Vector3.zero, Vector3.zero };
        line.SetPositions( initLaserPositions );
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ChangeState(CubeState.Positive);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ChangeState(CubeState.Negative);
        }
        
        FireBeam();
    }

    private void FireBeam()
    {
        var cubePosition = transform.position;
        line.SetPosition(0, cubePosition);

        var hitPosition = cubePosition + ((transform.forward * 2) + raycastOffset);
        hitEffect.transform.position = hitPosition;
        line.SetPosition(1, hitPosition);

        float hitY = hitPosition.y;

        if (!Physics.Raycast(transform.position, transform.forward + raycastOffset, out var hit, 15f)) return;

        if (hit.collider.name == "Terrain")
            return;

        var position = hit.point;
        position = new Vector3(position.x, hitY, position.z);
        hitEffect.transform.position = position;
        line.SetPosition(1, position);
    }

    public void Pickup()
    {
        _boxCollider.enabled = false;
        StartCoroutine(MoveToPlayer());
    }

    private IEnumerator MoveToPlayer()
    {
        coroutineExecuting = true;
        
        var cubeTransform = transform;
        var startPosition = cubeTransform.position;

        cubeTransform.parent = _player.transform;

        cubeTransform.localRotation = Quaternion.identity;

        for (_ft = 0f; _ft <= 1; _ft += Time.deltaTime / lerpDuration)
        {
            transform.position = Vector3.Lerp(startPosition, playerPickupTransform.position, _ft);
            yield return null;
        }
        
        coroutineExecuting = false;
    }

    private void ChangeState(CubeState state)
    {
        if (_currentState == state)
            return;
        
        if (state == CubeState.Positive)
        {
            line.material = positiveMat;
            _hitParticles.trailMaterial = positiveMat;
            _hitLight.color = positiveLightColor;

            StopParticleEffects(negativeEffect);
            StartParticleEffects(positiveEffect);
            
        }
        else
        {
            line.material = negativeMat;
            _hitParticles.trailMaterial = negativeMat;
            _hitLight.color = negativeLightColor;
            
            StopParticleEffects(positiveEffect);
            StartParticleEffects(negativeEffect);
        }

        _currentState = state;
    }

    private void StopParticleEffects(GameObject particleGroup)
    {
        if (particleGroup == positiveEffect)
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
        
        if (particleGroup == positiveEffect)
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
