using Affective;
using UnityEngine;
using Weather;

public class AffectiveWeather : ObjectState
{
    public WeatherTypeData neutralWeather;
    public WeatherTypeData joyWeather;
    public WeatherTypeData sadWeather;
    public WeatherTypeData fearWeather;

    public override void Neutral_State()
    {
        WeatherController.Instance.ChangeWeather(neutralWeather, 10f); 
    }

    public override void Joy_State()
    {
        WeatherController.Instance.ChangeWeather(joyWeather, 10f);
    }

    public override void Sadness_State()
    {
        WeatherController.Instance.ChangeWeather(
            AffectiveManager.Instance.currentAffectiveMode == AffectiveManager.AffectiveMode.Reinforce
                ? sadWeather
                : joyWeather, 10f);
    }
    public override void Fear_State()     
    {
        WeatherController.Instance.ChangeWeather(
            AffectiveManager.Instance.currentAffectiveMode == AffectiveManager.AffectiveMode.Reinforce
                ? fearWeather
                : joyWeather, 10f);
    }

    public override void Disgust_State()
    {
        WeatherController.Instance.ChangeWeather(neutralWeather, 10f); 
    }

    public override void Anger_State()
    {
        WeatherController.Instance.ChangeWeather(
            AffectiveManager.Instance.currentAffectiveMode == AffectiveManager.AffectiveMode.Reinforce
                ? sadWeather
                : joyWeather, 10f);
    }

    public override void Surprise_State()
    {
        WeatherController.Instance.ChangeWeather(neutralWeather, 10f); 
    }
    
}