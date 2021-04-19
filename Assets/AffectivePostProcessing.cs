using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Affective;

public class AffectivePostProcessing : ObjectState
{
    [SerializeField] private PostProcessingPreset neutralPreset;
    [SerializeField] private PostProcessingPreset joyPreset;
    [SerializeField] private PostProcessingPreset sadnessPreset;
    [SerializeField] private PostProcessingPreset fearPreset;
    [SerializeField] private PostProcessingPreset surprisePreset;
    [SerializeField] private PostProcessingPreset disgustPreset;
    [SerializeField] private PostProcessingPreset angerPreset;
    

    public override void Neutral_State()
    {
        PostProcessController.Instance.ChangePostProcessing(neutralPreset ,10f);
    }

    public override void Joy_State()
    {
        PostProcessController.Instance.ChangePostProcessing(joyPreset ,10f);
    }

    public override void Sadness_State()
    {
        PostProcessController.Instance.ChangePostProcessing(
            AffectiveManager.Instance.currentAffectiveMode == AffectiveManager.AffectiveMode.Reinforce
                ? sadnessPreset
                : joyPreset, 10f);
    }
    public override void Fear_State()     
    {
        PostProcessController.Instance.ChangePostProcessing(
            AffectiveManager.Instance.currentAffectiveMode == AffectiveManager.AffectiveMode.Reinforce
                ? fearPreset
                : joyPreset, 10f);
    }
    public override void Disgust_State()
    {
        PostProcessController.Instance.ChangePostProcessing(
            AffectiveManager.Instance.currentAffectiveMode == AffectiveManager.AffectiveMode.Reinforce
                ? disgustPreset
                : joyPreset, 10f);
    }
    public override void Anger_State()
    {
        PostProcessController.Instance.ChangePostProcessing(
            AffectiveManager.Instance.currentAffectiveMode == AffectiveManager.AffectiveMode.Reinforce
                ? angerPreset
                : joyPreset, 10f);
    }
    public override void Surprise_State()
    {
        PostProcessController.Instance.ChangePostProcessing(surprisePreset ,10f);
    }
}
