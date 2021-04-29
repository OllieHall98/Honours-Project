using System;
using Affective;
using UnityEngine;
using Weather;

public class AffectiveWeather : ObjectState
{
    public static AffectiveWeather Instance;
    
    public WeatherTypeData neutralWeather;
    public WeatherTypeData joyWeather;
    public WeatherTypeData sadWeather;
    public WeatherTypeData fearWeather;

    public bool canChangeWeather;

    public override void Awake()
    {
        base.Awake();
        Instance = this;
        canChangeWeather = true;
    }

    public override void Neutral_State()
    {
        if(canChangeWeather)
            WeatherController.Instance.ChangeWeather(neutralWeather, 7f); 
    }

    public override void Joy_State()
    {
        if(canChangeWeather)
            WeatherController.Instance.ChangeWeather(joyWeather, 7f);
    }

    public override void Sadness_State()
    {
        if(canChangeWeather)
            WeatherController.Instance.ChangeWeather(
            AffectiveManager.Instance.currentAffectiveMode == AffectiveManager.AffectiveMode.Reinforce
                ? sadWeather
                : joyWeather, 7f);
    }
    public override void Fear_State()     
    {
        if(canChangeWeather)
            WeatherController.Instance.ChangeWeather(
            AffectiveManager.Instance.currentAffectiveMode == AffectiveManager.AffectiveMode.Reinforce
                ? fearWeather
                : joyWeather, 7f);
    }

    public override void Disgust_State()
    {
        if(canChangeWeather)
            WeatherController.Instance.ChangeWeather(neutralWeather, 7f); 
    }

    public override void Anger_State()
    {
        if(canChangeWeather)
            WeatherController.Instance.ChangeWeather(
            AffectiveManager.Instance.currentAffectiveMode == AffectiveManager.AffectiveMode.Reinforce
                ? sadWeather
                : joyWeather, 7f);
    }

    public override void Surprise_State()
    {
        if(canChangeWeather)
            WeatherController.Instance.ChangeWeather(neutralWeather, 7f); 
    }
    
}