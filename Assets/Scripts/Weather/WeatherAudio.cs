using UnityEngine;

namespace Weather
{
    public class WeatherAudio : MonoBehaviour
    {
        public AK.Wwise.Event startRainEvent;
        public AK.Wwise.RTPC rainIntensityRtcp;
    
        private void Start()
        {
            startRainEvent.Post(gameObject);
        }

        public void SetRainIntensityRtcp(float value)
        {
            rainIntensityRtcp.SetValue(gameObject, value);
        }
    }
}
