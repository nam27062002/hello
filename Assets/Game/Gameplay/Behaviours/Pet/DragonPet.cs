﻿using UnityEngine;
using System.Collections;

public class DragonPet : MonoBehaviour {

	// Use this for initialization
	void Start () {
		// InstanceManager.pet = this;

		Animator animator = transform.Find("view").GetComponent<Animator>();
		animator.speed = 2;
	}
}
