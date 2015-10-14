// ListExt.cs
// 
// Created by Alger Ortín Castellví on 13/10/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom extensions to the List class.
/// </summary>
public static class ListExt {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// STATIC EXTENSION METHODS											//
	//------------------------------------------------------------------//
	/// <summary>
	/// Randomize the order within the list.
	/// </summary>
	/// <param name="_list">The list to be shuffled.</param>
	/// <param name="_seed">Optionally provide a specific seed to the random generator.</param>
	public static void Shuffle<T>(this IList<T> _list, int _seed = int.MinValue) {
		int n = _list.Count;
		while(n > 1) {  
			n--;  
			if(_seed != int.MinValue) Random.seed = _seed;
			//int k = rng.Next(n + 1);  
			int k = Random.Range(0, n + 1);
			T value = _list[k];  
			_list[k] = _list[n];  
			_list[n] = value;  
		}  
	}
}
