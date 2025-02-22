using System;
using System.Collections;
using NaughtyAttributes;
using UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Weather
{
    public class WeatherController : MonoBehaviour
    {
        public static WeatherController Instance;
        
        public float transitionDuration = 5f;
        public WeatherTypeData startWeather = default;
        public Volume vol;
        public Font weatherFont;

        private WeatherTypeData _weatherTarget;
        private WeatherAudio _weatherAudio;
        
        private bool _coroutineExecuting;
        private static readonly int Density = Shader.PropertyToID("_Density");
        private static readonly int Speed = Shader.PropertyToID("_Speed");
        private static readonly int CloudColor = Shader.PropertyToID("_CloudColor");

        private float _timer = 0;

        public bool enabled;

        [Serializable]
        public class CloudClass
        {
            public Material material;
            
            [HideInInspector] public float startDensity;
            [HideInInspector] public float startSpeed;
            [HideInInspector] public Color startColor;
            
            [HideInInspector] public float currentDensity;
            [HideInInspector] public float currentSpeed;
            [HideInInspector] public Color currentColor;
        }
        
        [Serializable]
        public class SkyClass
        {
            [HideInInspector] public GradientSky obj;
            
            [HideInInspector] public Color startTop;
            [HideInInspector] public Color startMiddle;
        }

        [Serializable]
        public class FogClass
        {
            [HideInInspector] public Fog obj;
            
            [HideInInspector] public float startAttenuation;
        }
        
        [Serializable]
        public class SunClass
        {
            public HDAdditionalLightData obj;
            
            [HideInInspector] public Color startColor;
            [HideInInspector] public float startIntensity;
        }

        [Serializable]
        public class RainClass
        {
            public ParticleSystem lightPS, heavyPS, floorPS;
            [HideInInspector]public ParticleSystem.MainModule LightMain, HeavyMain, FloorMain;
            [HideInInspector]public ParticleSystem.EmissionModule LightEmission, HeavyEmission;
            
            [HideInInspector]public Color startColor;
            [HideInInspector]public float startIntensity;

            public void ConfigureRainSystem()
            {
                LightEmission = lightPS.emission;
                HeavyEmission = heavyPS.emission;

                LightMain = lightPS.main;
                HeavyMain = heavyPS.main;
                FloorMain = floorPS.main;
            }
        }
        
        [Serializable]
        public class BirdClass
        {
            public NVBoids Birds;
            
            [HideInInspector] public float StartAmount = 0;
            [HideInInspector] public float StartSpeed = 0;
        }
        

        public CloudClass lowCloud;
        public CloudClass highCloud;
        [HideInInspector] public SkyClass sky;
        [HideInInspector] public FogClass fog;
        public SunClass sun;
        public RainClass rain;
        public BirdClass birds;
        
        private void Awake()
        {
            Instance = this;

            enabled = true;
            
            _weatherAudio = GetComponent<WeatherAudio>();

            rain.ConfigureRainSystem();
            GetVolumeProfile();

        }

        private void Start()
        {
            _weatherAudio.SetRainIntensityRtcp(rain.LightEmission.rateOverTime.constant / 50);
            ResetWeather();
        }

        private void GetVolumeProfile()
        {
            var profile = vol.sharedProfile;

            if (profile.TryGet(out fog.obj))
            {
                fog.obj.meanFreePath.overrideState = true;
                fog.obj.baseHeight.overrideState = true;
            }
            else
            {
                Debug.LogError("Fog profile does not exist!");
            }

            if(profile.TryGet(out sky.obj))
            {
                sky.obj.top.overrideState = true;
                sky.obj.middle.overrideState = true;
            }
            else
            {
                Debug.LogError("Sky profile does not exist!");
            }
        }
        
        public void ChangeWeather(WeatherTypeData weather, float duration)
        {
            if (!enabled) return;
            
            //NotificationText.Instance.DisplayMessage("Changing weather to " + weather.weatherName, 2f);

            //Debug.Log("Changing weather to " + weather.name);

            _weatherTarget = weather;

            SetTransitionStartValues();

            if (!_coroutineExecuting)
            {
                StartCoroutine(ChangeWeatherTransition(duration));
            }
        }

        private void SetCloudStartValues(WeatherTypeData.Clouds cloudData, CloudClass cloud)
        {
            if (!cloudData.enabled) return;
            
            if (cloudData.changeDensity)
                cloud.startDensity = cloud.material.GetFloat((Density));

            if (cloudData.changeSpeed)
                cloud.startSpeed = cloud.material.GetFloat((Speed));

            if (cloudData.changeColor)
                cloud.startColor = cloud.material.GetColor((CloudColor));
        }
        
        private void SetTransitionStartValues()
        {
            _timer = 0f;
            
            SetCloudStartValues(_weatherTarget.highClouds, highCloud);
            SetCloudStartValues(_weatherTarget.lowClouds, lowCloud);
            
            if (_weatherTarget.fog.enabled)
            {
                fog.startAttenuation = fog.obj.meanFreePath.value;
            }

            if (_weatherTarget.sky.enabled)
            {
                sky.startTop = sky.obj.top.value;
                sky.startMiddle = sky.obj.middle.value;
            }

            if (_weatherTarget.sun.enabled)
            {
                sun.startColor = sun.obj.color;
                sun.startIntensity = sun.obj.intensity;
            }

            if (!_weatherTarget.rain.enabled) return;
            
            rain.startColor = rain.LightMain.startColor.color;

            rain.startIntensity = rain.HeavyEmission.rateOverTime.constant;

            //birds.StartAmount = birds.Birds.birdsNum;
            birds.StartSpeed = birds.Birds.birdSpeed;
            
            //birds.Birds.CreateFlock();
            //birds.Birds.CreateBird();
        }
        
        private IEnumerator ChangeWeatherTransition(float duration)
        {
            _coroutineExecuting = true;
            
            _timer = 0;

            while (_timer < duration)
            {
                LerpClouds(_weatherTarget.lowClouds, lowCloud, _timer / duration);
                LerpClouds(_weatherTarget.highClouds, highCloud, _timer / duration);
                
                if (_weatherTarget.fog.enabled)
                {
                    fog.obj.meanFreePath.value = Mathf.Lerp(fog.startAttenuation, _weatherTarget.fog.thickness, _timer / duration);
                }

                if (_weatherTarget.sky.enabled)
                {
                    sky.obj.top.value = Color.Lerp(sky.startTop, _weatherTarget.sky.top, _timer / duration);
                    sky.obj.middle.value = Color.Lerp(sky.startMiddle, _weatherTarget.sky.middle, _timer / duration);
                }

                if (_weatherTarget.sun.enabled)
                {
                    if (_weatherTarget.sun.changeColor)
                    {
                        sun.obj.color = Color.Lerp(sun.startColor, _weatherTarget.sun.color, _timer / duration);
                    }

                    if (_weatherTarget.sun.changeIntensity)
                    {
                        sun.obj.intensity = Mathf.Lerp(sun.startIntensity, _weatherTarget.sun.intensity, _timer / duration);
                    }
                }

                if (!_weatherTarget.rain.enabled) continue;
                
                if (_weatherTarget.rain.changeColor)
                {
                    rain.LightMain.startColor = Color.Lerp(rain.startColor, _weatherTarget.rain.color / 2, _timer / duration);
                    rain.HeavyMain.startColor = Color.Lerp(rain.startColor, _weatherTarget.rain.color, _timer / duration);
                    rain.FloorMain.startColor = Color.Lerp(rain.startColor, _weatherTarget.rain.color, _timer / duration);
                }

                if (_weatherTarget.rain.changeIntensity)
                {
                    rain.LightEmission.rateOverTime = Mathf.Lerp(rain.startIntensity,
                        _weatherTarget.rain.intensity * 50, _timer / duration);
                    rain.HeavyEmission.rateOverTime = Mathf.Lerp(rain.startIntensity,
                        _weatherTarget.rain.intensity * 50, _timer / duration);

                    _weatherAudio.SetRainIntensityRtcp(rain.LightEmission.rateOverTime.constant / 20);
                }

                //birds.Birds.birdsNum = (int)Mathf.Lerp(birds.StartAmount, _weatherTarget.birdNumber, _timer / duration);
                birds.Birds.birdSpeed = Mathf.Lerp(birds.StartSpeed, _weatherTarget.birdSpeed, _timer / duration);
                
                _timer += Time.deltaTime;
                yield return null;
            }
            
            _coroutineExecuting = false;
        }

        private void SetClouds(WeatherTypeData.Clouds cloudData, CloudClass cloud)
        {
            if (!cloudData.enabled) return;
            
            if (cloudData.changeDensity)
            {
                cloud.material.SetFloat(Density, cloudData.density);
                cloud.currentDensity = cloudData.density;
            }

            if (cloudData.changeSpeed)
            {
                cloud.material.SetFloat(Speed, cloudData.speed);
                cloud.currentSpeed = cloudData.speed;
            }

            if (!cloudData.changeColor) return;
            
            cloud.material.SetColor(CloudColor, cloudData.color);
            cloud.currentColor = cloudData.color;
        }
        
        private void LerpClouds(WeatherTypeData.Clouds cloudData, CloudClass cloud, float value)
        {
            if (!cloudData.enabled) return;
            
            if (cloudData.changeDensity)
            {
                cloud.currentDensity =
                    Mathf.Lerp(cloud.startDensity, cloudData.density, value);
                cloud.material.SetFloat(Density, cloud.currentDensity);
            }

            if (cloudData.changeSpeed)
            { 
                cloud.currentSpeed = Mathf.Lerp(cloud.startSpeed, cloudData.speed, value);
                cloud.material.SetFloat(Speed, cloud.currentSpeed);
            }

            if (!cloudData.changeColor) return;
            
            cloud.currentColor = Color.Lerp(cloud.startColor, cloudData.color, value);
            cloud.material.SetColor(CloudColor, cloud.currentColor);
        }
        
        public void ResetWeather()
        {
            _weatherTarget = startWeather;
            
            GetVolumeProfile();
            
            if (_weatherAudio == null)
            {
                _weatherAudio = GetComponent<WeatherAudio>();
            }


            SetClouds(_weatherTarget.lowClouds, lowCloud);
            SetClouds(_weatherTarget.highClouds, highCloud);
            
            if (_weatherTarget.fog.enabled)
            {
                if (fog.obj == null)
                {
                    Debug.Log("WARNING: Fog is not assigned in inspector");
                }
                fog.obj.meanFreePath.value = _weatherTarget.fog.thickness;
            }

            if (_weatherTarget.sky.enabled)
            {
                if (sky.obj == null)
                {
                    Debug.Log("WARNING: Sky is not assigned in inspector");
                }
                sky.obj.top.value = _weatherTarget.sky.top;
                sky.obj.middle.value = _weatherTarget.sky.middle;
            }

            if (_weatherTarget.sun.enabled)
            {
                if (_weatherTarget.sun.changeColor)
                {
                    sun.obj.color = _weatherTarget.sun.color;
                }

                if (_weatherTarget.sun.changeIntensity)
                {
                    sun.obj.intensity = _weatherTarget.sun.intensity;
                }
            }

            if(_weatherTarget.rain.enabled)
            {
                rain.ConfigureRainSystem();

                if (_weatherTarget.rain.changeColor)
                {
                    rain.LightMain.startColor = _weatherTarget.rain.color / 2;
                    rain.HeavyMain.startColor = _weatherTarget.rain.color;
                    rain.FloorMain.startColor = _weatherTarget.rain.color;
                }

                if (_weatherTarget.rain.changeIntensity)
                {
                    rain.LightEmission.rateOverTime = _weatherTarget.rain.intensity * 50;
                    rain.HeavyEmission.rateOverTime = _weatherTarget.rain.intensity * 50;
                    _weatherAudio.SetRainIntensityRtcp(_weatherTarget.rain.intensity);
                }
            }

            //birds.Birds.birdsNum = (int)_weatherTarget.birdNumber;
            birds.Birds.birdSpeed = _weatherTarget.birdSpeed;

            SetTransitionStartValues();

        }
    }
    
    #if UNITY_EDITOR
    
    [CustomEditor(typeof(WeatherController))]
    [CanEditMultipleObjects]
    public class WeatherControllerEditor : Editor
    {
        private SerializedProperty changeColour;

        private WeatherController script = null;

        GUILayoutOption labelWidth = GUILayout.MinWidth(40);
        GUILayoutOption fieldWidth = GUILayout.MaxWidth(150);

        public bool showObjectPanel = true;

 
        
        void OnEnable()
        {
            script = (WeatherController) target;
        }

        private int currentPickerWindow;
        
        public override void OnInspectorGUI()
        {
            GUILayout.BeginVertical();
            
            var style = new GUIStyle(GUI.skin.button) {alignment = TextAnchor.MiddleCenter, fontSize = 18, font = script.weatherFont};
            //Rect rect = new Rect(50, 15, 250, 50);
            
            GUILayout.BeginVertical();
            GUILayout.Space(15);
            GUILayout.MaxHeight(125);
            //EditorGUILayout.LabelField("Current Weather: " + script.startWeather.name, style, GUILayout.Height(35));
            GUI.backgroundColor = Color.gray;
            if (GUILayout.Button("Current Weather: " + script.startWeather.name, style, GUILayout.Height(50)))
            {
                currentPickerWindow = EditorGUIUtility.GetControlID((FocusType.Passive) + 100);
                EditorGUIUtility.ShowObjectPicker<WeatherTypeData>(script.startWeather, false,"", currentPickerWindow);
            }
            GUI.backgroundColor = Color.white;

            if (Event.current.commandName == "ObjectSelectorUpdated" &&
                EditorGUIUtility.GetObjectPickerControlID() == currentPickerWindow)
            {
                script.startWeather = (WeatherTypeData)EditorGUIUtility.GetObjectPickerObject();
                currentPickerWindow = -1;
                script.ResetWeather();
            }

            GUILayout.EndVertical();

            GUILayout.Space(15);
            
            showObjectPanel = EditorGUILayout.Foldout(showObjectPanel, "Instances to assign");
            
            GUILayout.Space(5);
            
            GUILayout.BeginHorizontal();
            if (showObjectPanel)
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Space(5);
                script.vol = (Volume) EditorGUILayout.ObjectField("Volume", script.vol, typeof(Volume), true); 
                GUILayout.Space(5);
                script.lowCloud.material = (Material)EditorGUILayout.ObjectField("Low Clouds", script.lowCloud.material, typeof(Material), true);
                script.highCloud.material = (Material)EditorGUILayout.ObjectField("High Clouds", script.highCloud.material, typeof(Material), true);
                GUILayout.Space(5);
                script.sun.obj = (HDAdditionalLightData)EditorGUILayout.ObjectField("Sun", script.sun.obj, typeof(HDAdditionalLightData), true);
                GUILayout.Space(5);
                script.rain.lightPS = (ParticleSystem)EditorGUILayout.ObjectField("Light Rain", script.rain.lightPS, typeof(ParticleSystem), true);
                script.rain.heavyPS = (ParticleSystem)EditorGUILayout.ObjectField("Heavy Rain", script.rain.heavyPS, typeof(ParticleSystem), true);
                script.rain.floorPS = (ParticleSystem)EditorGUILayout.ObjectField("Floor Rain", script.rain.floorPS, typeof(ParticleSystem), true);
                GUILayout.Space(5);
                script.birds.Birds = (NVBoids)EditorGUILayout.ObjectField("Birds", script.birds.Birds, typeof(NVBoids), true);
                GUILayout.Space(5);
                GUILayout.EndVertical();
                
            }

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            
            EditorUtility.SetDirty(script);
        }

    }
    #endif
    
}
