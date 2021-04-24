using Affective;
using UnityEngine;

public class AffectiveOrb : ObjectState
{
    private Animator _anim;
    private static readonly int Neutral = Animator.StringToHash("Neutral");
    private static readonly int F = Animator.StringToHash("Float");
    private static readonly int Sad = Animator.StringToHash("Sad");
    private static readonly int Shiver = Animator.StringToHash("Shiver");
    private static readonly int Angry = Animator.StringToHash("Angry");
    private static readonly int Disgust = Animator.StringToHash("Disgust");
    private static readonly int Surprise = Animator.StringToHash("Surprise");

    private void Start()
    {
        _anim = GetComponent<Animator>();
    }

    public override void Neutral_State() { _anim.SetTrigger(Neutral); }
    public override void Joy_State() { _anim.SetTrigger(F); }
    public override void Sadness_State() { _anim.SetTrigger(Sad); }
    public override void Fear_State(){ _anim.SetTrigger(Shiver); }
    public override void Anger_State(){ _anim.SetTrigger(Angry); }
    public override void Disgust_State(){ _anim.SetTrigger(Disgust); }
    public override void Surprise_State(){ _anim.SetTrigger(Surprise); }
}
