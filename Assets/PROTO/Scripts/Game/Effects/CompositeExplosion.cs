// CompositeExplosion.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 05/05/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

#region INCLUDES AND PREPROCESSOR --------------------------------------------------------------------------------------
using UnityEngine;
using System.Collections;
#endregion

#region CLASSES --------------------------------------------------------------------------------------------------------
/// <summary>
/// Visual effect to trigger a big explosion composed of multiple smaller explosions.
/// </summary>
public class CompositeExplosion : MonoBehaviour {
	#region EXPOSED MEMBERS --------------------------------------------------------------------------------------------
	public GameObject explosionPrefab = null;
	public int explosionsAmount = 8;
	public Range delayRange = new Range(0f, 0.25f);
	public Vector3 spawnOffset = Vector3.zero;	// Offset from parent object's position where the explosions will spawn
	public Vector3 spawnArea = Vector3.one;		// Area around the spawn point where the explosions will spawn
	public Range scaleRange = new Range(1f, 5f);
	public Range rotationRange = new Range(0f, 360f);
	#endregion

	#region PUBLIC METHODS ---------------------------------------------------------------------------------------------
	/// <summary>
	/// Initialization, must add it so Unity realizes it is a component -_-
	/// </summary>
	void Start() {
		// Nothing to do
	}

	/// <summary>
	/// Trigger the explosion with the values from the inspector.
	/// </summary>
	public void Explode() {
		// Just do it using default values
		Explode(explosionsAmount, delayRange, spawnOffset, spawnArea, scaleRange, rotationRange);
	}

	/// <summary>
	/// Trigger the explosion using custom values.
	/// </summary>
	/// <param name="_iAmount">Amount of small explosions to be triggered.</param>
	/// <param name="_delayRange">Random delay range for each small explosion.</param>
	/// <param name="_spawnOffset">Offset from parent object's position where the explosions will spawn.</param>
	/// <param name="_spawnArea">Area around the spawn point where the explosions will spawn.</param>
	/// <param name="_scaleRange">Random scale range for each small explosion.</param>
	/// <param name="_rotationRange">Random rotation range for each small explosion.</param>
	public void Explode(int _iAmount, Range _delayRange, Vector3 _spawnOffset, Vector3 _spawnArea, Range _scaleRange, Range _rotationRange) {
		// Some checks
		DebugUtils.Assert(explosionPrefab != null, "Required component!");
		
		// Launch as much single explosions as needed
		for(int i = 0; i < explosionsAmount; i++) {
			StartCoroutine(SingleExplosion(_delayRange, _spawnOffset, _spawnArea, _scaleRange, _rotationRange));
		}
	}
	#endregion

	#region INTERNAL UTILS ---------------------------------------------------------------------------------------------
	/// <summary>
	/// Launch a single explosion using custom values.
	/// </summary>
	/// <param name="_delayRange">Random delay range for each small explosion.</param>
	/// <param name="_spawnOffset">Offset from parent object's position where the explosions will spawn.</param>
	/// <param name="_spawnArea">Area around the spawn point where the explosions will spawn.</param>
	/// <param name="_scaleRange">Random scale range for each small explosion.</param>
	/// <param name="_rotationRange">Random rotation range for each small explosion.</param>
	private IEnumerator SingleExplosion(Range _delayRange, Vector3 _spawnOffset, Vector3 _spawnArea, Range _scaleRange, Range _rotationRange) {
		// First of all, wait a random delay amount
		yield return new WaitForSeconds(_delayRange.GetRandom());

		// New prefab instance
		GameObject exp = (GameObject)Object.Instantiate(explosionPrefab);
		
		// Random position within range
		Vector3 p = transform.position + _spawnOffset;
		p.x += Random.Range(-_spawnArea.x, _spawnArea.x);
		p.y += Random.Range(-_spawnArea.y, _spawnArea.y);
		p.z += Random.Range(-_spawnArea.z, _spawnArea.z);
		exp.transform.position = p;
		
		// Random scale within range
		exp.transform.localScale = Vector3.one * _scaleRange.GetRandom();
		
		// Random rotation within range
		exp.transform.Rotate(0, 0, _rotationRange.GetRandom());
	}
	#endregion
}
#endregion