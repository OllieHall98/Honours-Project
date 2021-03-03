using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Weather
{
    [CreateAssetMenu(fileName = "WeatherTypeData", menuName = "ScriptableObjects/WeatherType", order = 1)]
    public class WeatherTypeData : ScriptableObject
    {
        public string weatherName = default;

         public Clouds lowClouds;
         public Clouds highClouds;
         public Fog fog;
         public Sky sky;
         public Sun sun;
         public Rain rain;
        
        [Serializable]
        public class Clouds
        {
            public bool enabled;
            public bool changeColor;
            public bool changeSpeed;
            public bool changeDensity;
            
            public float density   = 1.21f;
            public float speed = -0.25f;
            public Color color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        }
        
        [Serializable]
        public class Fog
        {
            public bool enabled;
            public float thickness = 400f;
        }

        [Serializable]
        public class Sky
        {
            public bool enabled;
            public Color top = new Color(1.0f, 1.0f, 1.0f);
            public Color middle = new Color(1.0f, 0.84f, 0.7f);
        }

        [Serializable]
        public class Sun
        {
            public bool enabled;
            public bool changeIntensity;
            public bool changeColor;
            public float intensity;
            public Color color;
        }


        [Serializable]
        public class Rain
        {
            public bool enabled;
            public bool changeIntensity;
            public bool changeColor;
            public Color color = new Color(1.0f, 1.0f, 1.0f);
            public float intensity;
        }

    }
    
    [CustomEditor(typeof(WeatherTypeData))]
    [CanEditMultipleObjects]
    public class WeatherTypeDataEditor : Editor
    {
        private SerializedProperty changeColour;

        private WeatherTypeData data = null;

        GUILayoutOption labelWidth = GUILayout.MinWidth(40);
        GUILayoutOption fieldWidth = GUILayout.MaxWidth(150);
        
       
        
        void OnEnable()
        {
            data = (WeatherTypeData) target;
        }

        public override void OnInspectorGUI()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            
            GUILayout.Space(5);

            var style = new GUIStyle(GUI.skin.textField) {alignment = TextAnchor.UpperCenter};

            GUILayout.BeginHorizontal();
            data.weatherName = EditorGUILayout.TextField(data.weatherName, style);
            GUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            CloudSection(data.highClouds, "High Clouds");
            GUILayout.FlexibleSpace();
            CloudSection(data.lowClouds, "Low Clouds");
            GUILayout.FlexibleSpace();
            FogSection();
            GUILayout.FlexibleSpace();
            SkySection();
            GUILayout.FlexibleSpace();
            SunSection();
            GUILayout.FlexibleSpace();
            RainSection();
            
            GUILayout.EndVertical();
            
            EditorUtility.SetDirty(data);
        }

        void CloudSection(WeatherTypeData.Clouds clouds, string cloudType)
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            
            GUILayout.BeginHorizontal();
            //EditorGUILayout.LabelField(cloudType, labelWidth);
            clouds.enabled = EditorGUILayout.ToggleLeft(cloudType, clouds.enabled, fieldWidth);
            GUILayout.EndHorizontal();

            if (clouds.enabled)
            {
                GUILayout.BeginVertical();
                GUILayout.Space(5);
                
                GUILayout.BeginHorizontal();
                clouds.changeColor = EditorGUILayout.ToggleLeft("Color", clouds.changeColor, labelWidth);
                if (clouds.changeColor)
                {
                    clouds.color = EditorGUILayout.ColorField(clouds.color, fieldWidth);
                }
                GUILayout.EndHorizontal();
                
                GUILayout.Space(2);
                
                GUILayout.BeginHorizontal();
                clouds.changeDensity = EditorGUILayout.ToggleLeft("Density", clouds.changeDensity, labelWidth);
                if (clouds.changeDensity)
                {
                    clouds.density = EditorGUILayout.Slider(clouds.density, 0, 3, fieldWidth);
                }
                GUILayout.EndHorizontal();
                
                GUILayout.Space(2);
                
                GUILayout.BeginHorizontal();
                clouds.changeSpeed = EditorGUILayout.ToggleLeft("Speed", clouds.changeSpeed, labelWidth);
                if (clouds.changeSpeed)
                {
                    clouds.speed = EditorGUILayout.Slider(clouds.speed, -1, 1, fieldWidth);
                }
                GUILayout.EndHorizontal();
                
                GUILayout.Space(5);
                GUILayout.EndVertical();
            }
            
            
            
            GUILayout.EndVertical();
            
        }
        
        void FogSection()
        { 
            GUILayout.BeginVertical(EditorStyles.helpBox);
            
            GUILayout.BeginHorizontal();
            data.fog.enabled = EditorGUILayout.ToggleLeft("Fog", data.fog.enabled, fieldWidth);
            GUILayout.EndHorizontal();

            if (data.fog.enabled)
            {
                GUILayout.BeginVertical();
                GUILayout.Space(5);
                
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Thickness", labelWidth);
                
                    data.fog.thickness = EditorGUILayout.FloatField( data.fog.thickness, fieldWidth);
                    GUILayout.EndHorizontal();
                
                
                GUILayout.Space(5);
                GUILayout.EndVertical();
            }
            
            GUILayout.EndVertical();
        }
        
        void SkySection()
        { 
            GUILayout.BeginVertical(EditorStyles.helpBox);
            
            GUILayout.BeginHorizontal();
            data.sky.enabled = EditorGUILayout.ToggleLeft("Sky", data.sky.enabled, fieldWidth);
            GUILayout.EndHorizontal();

            if (data.sky.enabled)
            {
                GUILayout.BeginVertical();
                GUILayout.Space(5);
                
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Middle Color", labelWidth);
                data.sky.middle = EditorGUILayout.ColorField(data.sky.middle, fieldWidth);
                GUILayout.EndHorizontal();
                
                GUILayout.Space(5);
                
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Top Color", labelWidth);
                data.sky.top = EditorGUILayout.ColorField(data.sky.top, fieldWidth);
                GUILayout.EndHorizontal();

                GUILayout.Space(5);
                GUILayout.EndVertical();
            }
            
            GUILayout.EndVertical();
        }
        
        void SunSection()
        { 
            GUILayout.BeginVertical(EditorStyles.helpBox);
            
            GUILayout.BeginHorizontal();
            data.sun.enabled = EditorGUILayout.ToggleLeft("Sun", data.sun.enabled, fieldWidth);
            GUILayout.EndHorizontal();

            if (data.sun.enabled)
            {
                GUILayout.BeginVertical();
                GUILayout.Space(5);
                
                GUILayout.BeginHorizontal();
                data.sun.changeIntensity = EditorGUILayout.ToggleLeft("Intensity", data.sun.changeIntensity, labelWidth);
                if (data.sun.changeIntensity)
                {
                    data.sun.intensity = EditorGUILayout.Slider(data.sun.intensity, 0, 120000, fieldWidth);
                }
                GUILayout.EndHorizontal();
                
                GUILayout.BeginHorizontal();
                data.sun.changeColor = EditorGUILayout.ToggleLeft("Color", data.sun.changeColor, labelWidth);
                if (data.sun.changeColor)
                {
                    data.sun.color = EditorGUILayout.ColorField(data.sun.color, fieldWidth);
                }
                GUILayout.EndHorizontal();
                
                GUILayout.Space(5);
                GUILayout.EndVertical();
            }
            
            GUILayout.EndVertical();
        }
        
        void RainSection()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            
            data.rain.enabled = EditorGUILayout.ToggleLeft("Rain", data.rain.enabled);
            
            if (data.rain.enabled)
            {
                GUILayout.BeginVertical();
                GUILayout.Space(5);
                
                GUILayout.BeginHorizontal();
                data.rain.changeIntensity = EditorGUILayout.ToggleLeft("Intensity", data.rain.changeIntensity, labelWidth);
                if (data.rain.changeIntensity)
                {
                    data.rain.intensity = EditorGUILayout.Slider(data.rain.intensity, 0, 100, fieldWidth);
                }
                GUILayout.EndHorizontal();
                
                GUILayout.BeginHorizontal();
                data.rain.changeColor = EditorGUILayout.ToggleLeft("Color", data.rain.changeColor, labelWidth);
                if (data.rain.changeColor)
                {
                    data.rain.color = EditorGUILayout.ColorField(data.rain.color, fieldWidth);
                }
                GUILayout.EndHorizontal();
                
                GUILayout.Space(5);
                GUILayout.EndVertical();
            }
            
            GUILayout.EndVertical();
        }
        
        
    }
    
}
