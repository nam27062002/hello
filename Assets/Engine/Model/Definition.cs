// Def.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 03/12/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Generic base class for a definition.
/// To be inherited adding as many fields as needed.
/// Feel free to override IEquatable/IComparable implementations as needed.
/// By default, alphanumeric sku sorting will be used to determine order between definitions.
/// See https://msdn.microsoft.com/en-us/library/4d7sx9hd(v=vs.110).aspx
/// See https://msdn.microsoft.com/en-us/library/ms131190(v=vs.110).aspx
/// </summary>
[Serializable]
public class Definition : IEquatable<Definition>, IComparable<Definition> {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	[SerializeField] private string m_sku = "";
	public string sku { get { return m_sku; }}

	//------------------------------------------------------------------//
	// IEquatable IMPLEMENTATION										//
	//------------------------------------------------------------------//
	/// <summary>
	/// Determines whether the specified <see cref="Definition"/> is equal to the current <see cref="Definition"/>.
	/// </summary>
	/// <param name="_other">The <see cref="Definition"/> to compare with the current <see cref="Definition"/>.</param>
	/// <returns><c>true</c> if the specified <see cref="Definition"/> is equal to the current <see cref="Definition"/>; otherwise, <c>false</c>.</returns>
	public bool Equals(Definition _other) {
		if(_other == null) return false;

		if(this.sku == _other.sku) {
			return true;
		} else {
			return false;
		}
	}

	/// <summary>
	/// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="System.Object"/>.
	/// Override this to avoid warning CS0660.
	/// </summary>
	/// <param name="obj">The <see cref="System.Object"/> to compare with the current <see cref="System.Object"/>.</param>
	/// <returns><c>true</c> if the specified <see cref="System.Object"/> is equal to the current <see cref="System.Object"/>; otherwise, <c>false</c>.</returns>
	public override bool Equals(object obj) {
		// Pass obj or null for non-Definition types to our Equals(Definition) override
		return Equals(obj as Definition);
	}

	/// <summary>
	/// Serves as a hash function for a <see cref="Definition"/> object.
	/// </summary>
	/// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a hash table.</returns>
	public override int GetHashCode() {
		return this.sku.GetHashCode();
	}

	/// <summary>
	/// Equality operator for <see cref="Definition"/>.
	/// </summary>
	/// <returns><c>true</c> if both definitions are equivalent, <c>false</c> otherwise.</returns>
	/// <param name="_def1">The first <see cref="Definition"/> to be compared.</param>
	/// <param name="_def2">The second <see cref="Definition"/> to be compared.</param>
	public static bool operator == (Definition _def1, Definition _def2) {
		if(((object)_def1) == null || ((object)_def2) == null) {
			return UnityEngine.Object.Equals(_def1, _def2);
		}

		return _def1.Equals(_def2);
	}

	/// <summary>
	/// Difference operator for <see cref="Definition"/>.
	/// </summary>
	/// <returns><c>true</c> if both definitions are different, <c>false</c> otherwise.</returns>
	/// <param name="_def1">The first <see cref="Definition"/> to be compared.</param>
	/// <param name="_def2">The second <see cref="Definition"/> to be compared.</param>
	public static bool operator != (Definition _def1, Definition _def2) {
		if(((object)_def1) == null || ((object)_def2) == null) {
			return !UnityEngine.Object.Equals(_def1, _def2);
		}

		return !(_def1.Equals(_def2));
	}

	//------------------------------------------------------------------//
	// IComparable IMPLEMENTATION										//
	//------------------------------------------------------------------//
	/// <summary>
	/// Compares the current instance with another object of the same type and returns 
	/// an integer that indicates whether the current instance precedes, follows, 
	/// or occurs in the same position in the sort order as the other object.
	/// </summary>
	/// <returns>
	/// Less than zero if this instance precedes <paramref name="_other"/> in the sort order;
	/// zero if this instance occurs in the same position in the sort order as <paramref name="_other"/>; 
	/// and greater than zero if this instance follows <paramref name="_other"/> in the sort order.
	/// </returns>
	/// <param name="_other">An object to compare with this instance.</param>
	public int CompareTo(Definition _other) {
		// If other is not a valid object reference, this instance is greater.
		if(_other == null) return 1;

		// Use standard alphanumeric order with skus
		return sku.CompareTo(_other.sku);
	}

	/// <summary>
	/// Greater than operator for <see cref="Definition"/>.
	/// </summary>
	/// <returns><c>true</c> if this <paramref name="_def1"/> is considered greater or equal to <paramref name="_def2"/>, <c>false</c> otherwise.</returns>
	/// <param name="_def1">The first <see cref="Definition"/> to be compared.</param>
	/// <param name="_def2">The second <see cref="Definition"/> to be compared.</param>
	public static bool operator >= (Definition _def1, Definition _def2) {
		return _def1.CompareTo(_def2) == 1;
	}

	/// <summary>
	/// Less than operator for <see cref="Definition"/>.
	/// </summary>
	/// <returns><c>true</c> if this <paramref name="_def1"/> is considered less or equal to <paramref name="_def2"/>, <c>false</c> otherwise.</returns>
	/// <param name="_def1">The first <see cref="Definition"/> to be compared.</param>
	/// <param name="_def2">The second <see cref="Definition"/> to be compared.</param>
	public static bool operator <= (Definition _def1, Definition _def2) {
		return _def1.CompareTo(_def2) == -1;
	}
}