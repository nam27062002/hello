// RandomExt.cs
// Hungry Dragon
// 
// Created by Marc Saña on 03/05/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using System;
using System.Reflection;
using System.Globalization;

using UnityEngine;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
public static class RandomExt {
	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	private static System.Random s_rand = new System.Random();
	private static byte[] s_buf = new byte[8];

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// 
	/// </summary>
	/// <param name="_min"></param>
	/// <param name="_max"></param>
	public static long Range(long _min, long _max) {
        if (_min == _max) {
            return _min;
        } else {
            s_rand.NextBytes(s_buf);
            long longRand = BitConverter.ToInt64(s_buf, 0);
            return (Math.Abs(longRand % (_max - _min)) + _min);
        }
	}

	/// <summary>
	/// Get a string representation of this Random.State.
	/// Random.State is a private struct that can't be easily serialized, so use reflection to do so.
	/// We know the internal structure of Random.State thanks to unofficial 
	/// UnityDecompiled repo (https://github.com/MattRix/UnityDecompiled/blob/master/UnityEngine/UnityEngine/Random.cs)
	/// </summary>
	/// <returns>A string representation of the Random State.</returns>
	/// <param name="_s">Random state to be stringified.</param>
	public static string Serialize(this UnityEngine.Random.State _s) {
		// A random state is composed of 4 int seeds named "s0", "s1", "s2", "s3"
		FieldInfo prop = null;
		string[] seeds = new string[4];
		for(int i = 0; i < 4; ++i) {
			prop = _s.GetType().GetField("s" + i, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			seeds[i] = ((int)prop.GetValue(_s)).ToString(CultureInfo.InvariantCulture);
		}

		// Join them in a single string
		return string.Join("|", seeds);
	}
		
	/// <summary>
	/// Initialize this Random.State with the data from a string representation of a Random.State.
	/// Random.State is a private struct that can't be easily serialized, so use reflection to do so.
	/// We know the internal structure of Random.State thanks to unofficial 
	/// UnityDecompiled repo (https://github.com/MattRix/UnityDecompiled/blob/master/UnityEngine/UnityEngine/Random.cs)
	/// </summary>
	/// <param name="_s">the Random State where the parsed values will be stored.</param>
	/// <param name="_string">String to be parsed.</param>
	public static void Deserialize(this UnityEngine.Random.State _s, string _string) {
		// Split input string
		string[] seeds = _string.Split('|');

		// A random state is composed of 4 int seeds named "s0", "s1", "s2", "s3"
		// Initialize them with the values read from the string
		FieldInfo prop = null;
		int seedValue = 0;
		for(int i = 0; i < 4; ++i) {
			// Do we have data for that seed
			if(i < seeds.Length) {
				// Is data for this seed properly formatted?
				if(int.TryParse(seeds[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out seedValue)) {
					// Initialize the seed using reflection
					prop = _s.GetType().GetField("s" + i, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
					prop.SetValue(_s, seedValue);
				}
			}
		}
	}
}
