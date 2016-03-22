// ListExt.cs
// 
// Created by Alger Ortín Castellví on 13/10/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom extensions to the List and Array classes.
/// </summary>
public static class ListExt {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// STATIC EXTENSION METHODS											//
	//------------------------------------------------------------------//
	/// <summary>
	/// Searches for the specified object and returns the zero-based index of the first occurrence within the entire array.
	/// </summary>
	/// <returns>The zero-based index of the first occurrence of the target item within the array if found; otherwise, <c>–1</c>.</returns>
	/// <param name="_array">The array to be searched.</param>
	/// <param name="_target">The object to locate in the array. The value can be <c>null</c> for reference types.</param>
	/// <remarks>
	/// The array is searched forward starting at the first element and ending at the last element.
	/// This method performs a linear search; therefore, this method is an O(n) operation, where n is array's Length.
	/// </remarks>
	public static int IndexOf(this Array _array, object _target) {
		// Standard linear search
		for(int i = 0; i < _array.Length; i++) {
			if(_array.GetValue(i) == _target) return i;
		}

		return -1;
	}

	/// <summary>
	/// Randomize the order within a list.
	/// </summary>
	/// <param name="_list">The list to be shuffled.</param>
	/// <param name="_seed">Optionally provide a specific seed to the random generator.</param>
	public static void Shuffle<T>(this IList<T> _list, int _seed = int.MinValue) {
		int n = _list.Count;
		while(n > 1) {  
			n--;  
			if(_seed != int.MinValue) UnityEngine.Random.seed = _seed;
			int k = UnityEngine.Random.Range(0, n + 1);
			T value = _list[k];  
			_list[k] = _list[n];  
			_list[n] = value;  
		}  
	}

	/// <summary>
	/// Randomize the order within an array.
	/// </summary>
	/// <param name="_array">The array to be shuffled.</param>
	/// <param name="_seed">Optionally provide a specific seed to the random generator.</param>
	public static void Shuffle(this Array _array, int _seed = int.MinValue) {
		int n = _array.Length;
		while(n > 1) {  
			n--;  
			if(_seed != int.MinValue) UnityEngine.Random.seed = _seed;
			int k = UnityEngine.Random.Range(0, n + 1);
			object value = _array.GetValue(k);
			_array.SetValue(_array.GetValue(n), k);
			_array.SetValue(value, n);
		}  
	}

	/// <summary>
	/// Obtain a random value from the given list.
	/// </summary>
	/// <returns>A random value from the ones within the list.</returns>
	/// <param name="_list">The list conatining the candidate values.</param>
	public static T GetRandomValue<T>(this IList<T> _list) {
		// Check for empty lists
		if(_list.Count == 0) return default(T);
		return _list[UnityEngine.Random.Range(0, _list.Count)];
	}
	
	/// <summary>
	/// Obtain a random value from the given array.
	/// </summary>
	/// <returns>A random value from the ones within the array.</returns>
	/// <param name="_array">The array conatining the candidate values.</param>
	public static T GetRandomValue<T>(this Array _array) {
		// Check for empty arrays
		if(_array.Length == 0) return default(T);
		return (T)_array.GetValue(UnityEngine.Random.Range(0, _array.Length));
	}

	/// <summary>
	/// Obtain the first element of the list.
	/// </summary>
	/// <param name="_list">The list to be searched.</param>
	/// <returns>The first element of the list, <c>null</c> if the list is empty.</returns>
	public static T First<T>(this IList<T> _list) {
		// Check for empty lists
		if(_list.Count == 0) return default(T);
		return _list[0];
	}

	/// <summary>
	/// Obtain the first element of the array.
	/// </summary>
	/// <param name="_list">The array to be searched.</param>
	/// <returns>The first element of the array, <c>null</c> if the array is empty.</returns>
	public static T First<T>(this Array _array) {
		// Check for empty lists
		if(_array.Length == 0) return default(T);
		return (T)_array.GetValue(0);
	}

	/// <summary>
	/// Obtain the last element of the list.
	/// </summary>
	/// <param name="_list">The list to be searched.</param>
	/// <returns>The last element of the list, <c>null</c> if the list is empty.</returns>
	public static T Last<T>(this IList<T> _list) {
		// Check for empty lists
		if(_list.Count == 0) return default(T);
		return _list[_list.Count - 1];
	}

	/// <summary>
	/// Obtain the last element of the array.
	/// </summary>
	/// <param name="_list">The array to be searched.</param>
	/// <returns>The last element of the array, <c>null</c> if the list is empty.</returns>
	public static T Last<T>(this Array _array) {
		// Check for empty lists
		if(_array.Length == 0) return default(T);
		return (T)_array.GetValue(_array.Length - 1);
	}

	/// <summary>
	/// Sorts a string list alphanumerically.
	/// See http://www.dotnetperls.com/alphanumeric-sorting.
	/// </summary>
	/// <param name="_list">The list to be sorted.</param>
	public static void SortAlphanumeric(this List<string> _list) {
		AlphanumComparatorFast comparer = new AlphanumComparatorFast();
		_list.Sort((x, y) => comparer.Compare(x, y));
	}

	/// <summary>
	/// Sorts a string array alphanumerically.
	/// See http://www.dotnetperls.com/alphanumeric-sorting.
	/// </summary>
	/// <param name="_array">The array to be sorted.</param>
	public static void SortAlphanumeric(this string[] _array) {
		Array.Sort(_array, new AlphanumComparatorFast());
	}
}

/// <summary>
/// Auxiliar class used for the Alphanumeric sorting.
/// See http://www.dotnetperls.com/alphanumeric-sorting.
/// </summary>
public class AlphanumComparatorFast : IComparer<string> {
	/// <summary>
	/// Compare the two given objects
	/// </summary>
	/// <param name="_obj1">First object to be compared.</param>
	/// <param name="_obj2">Second object to be compared.</param>
	public int Compare(string _s1, string _s2) {		
		if(_s1 == null) {
			return 0;
		}

		if(_s2 == null) {
			return 0;
		}

		int len1 = _s1.Length;
		int len2 = _s2.Length;
		int marker1 = 0;
		int marker2 = 0;

		// Walk through two the strings with two markers.
		while(marker1 < len1 && marker2 < len2) {
			char ch1 = _s1[marker1];
			char ch2 = _s2[marker2];

			// Some buffers we can build up characters in for each chunk.
			char[] space1 = new char[len1];
			int loc1 = 0;
			char[] space2 = new char[len2];
			int loc2 = 0;

			// Walk through all following characters that are digits or
			// characters in BOTH strings starting at the appropriate marker.
			// Collect char arrays.
			do {
				space1[loc1++] = ch1;
				marker1++;

				if(marker1 < len1) {
					ch1 = _s1[marker1];
				}
				else {
					break;
				}
			} while (char.IsDigit(ch1) == char.IsDigit(space1[0]));

			do {
				space2[loc2++] = ch2;
				marker2++;

				if(marker2 < len2) {
					ch2 = _s2[marker2];
				}
				else {
					break;
				}
			} while (char.IsDigit(ch2) == char.IsDigit(space2[0]));

			// If we have collected numbers, compare them numerically.
			// Otherwise, if we have strings, compare them alphabetically.
			string str1 = new string(space1);
			string str2 = new string(space2);

			int result;

			if(char.IsDigit(space1[0]) && char.IsDigit(space2[0])) {
				int thisNumericChunk = int.Parse(str1);
				int thatNumericChunk = int.Parse(str2);
				result = thisNumericChunk.CompareTo(thatNumericChunk);
			}
			else {
				result = str1.CompareTo(str2);
			}

			if(result != 0) {
				return result;
			}
		}
		return len1 - len2;
	}
}