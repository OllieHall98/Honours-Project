using Affective;
using UnityEngine;
using Weather;

public class AffectiveWeather : ObjectState
{
    public WeatherTypeData neutralWeather;
    public WeatherTypeData joyWeather;
    public WeatherTypeData sadWeather;
    //public WeatherTypeData angryWeather;
    
    
    public override void Neutral_State()
    {
        WeatherController.Instance.ChangeWeather(neutralWeather);
        Debug.Log("Setting weather to neutral");
    }

    public override void Joy_State()
    {
        WeatherController.Instance.ChangeWeather(joyWeather);
        Debug.Log("Setting weather to joyful");
        
    }

    public override void Sadness_State()
    {
        WeatherController.Instance.ChangeWeather(sadWeather);
        Debug.Log("Setting weather to sad");
    }
    public override void Fear_State() { Debug.Log(this.gameObject.name + " is fearful"); }
    public override void Disgust_State() { Debug.Log(this.gameObject.name + " is disgusted"); }
    public override void Anger_State() { Debug.Log(this.gameObject.name + " is angry"); }
    public override void Surprise_State() { Debug.Log(this.gameObject.name + " is surprised"); }
}