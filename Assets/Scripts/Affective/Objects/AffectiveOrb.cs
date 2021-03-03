using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AffectiveOrb : ObjectState
{
    private Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    public override void Neutral_State() { anim.SetTrigger("Neutral"); }
    public override void Joy_State() { anim.SetTrigger("Float"); }
    public override void Sadness_State() { anim.SetTrigger("Sad"); }
    public override void Fear_State(){ anim.SetTrigger("Shiver"); }
    public override void Anger_State(){ anim.SetTrigger("Angry"); }
    public override void Disgust_State(){ anim.SetTrigger("Disgust"); }
    public override void Surprise_State(){ anim.SetTrigger("Surprise"); }
}
