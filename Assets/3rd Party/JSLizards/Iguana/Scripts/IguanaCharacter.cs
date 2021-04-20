using System;
using UnityEngine;
using System.Collections;

public class IguanaCharacter : MonoBehaviour {
	Animator iguanaAnimator;

	public static IguanaCharacter Instance;

	public bool released = false;
	
	private void Awake()
	{
		Instance = this;
		released = false;
	}

	void Start () {
		iguanaAnimator = GetComponent<Animator> ();
	}
	
	public void Attack(){
		iguanaAnimator.SetTrigger("Attack");
	}
	
	public void Hit(){
		iguanaAnimator.SetTrigger("Hit");
	}
	
	public void Eat(){
		iguanaAnimator.SetTrigger("Eat");
	}

	public void Death(){
		iguanaAnimator.SetTrigger("Death");
	}

	public void Rebirth(){
		iguanaAnimator.SetTrigger("Rebirth");
	}

	public void RELEASEME()
	{
		iguanaAnimator.SetFloat("Forward", 15.0f);
		//released = true;
	}

	private void Update()
	{
		if (!released) return;

		transform.position += Vector3.forward * 0.3f;
	}


	public void Move(float v,float h){
		iguanaAnimator.SetFloat ("Forward", v);
		iguanaAnimator.SetFloat ("Turn", h);
	}
	
	
	
	
}
