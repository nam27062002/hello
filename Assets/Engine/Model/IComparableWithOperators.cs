// IComparableWithOperators.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 20/02/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using System;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom implementation of the IComparable<T> and IEquatable<T> .NET class to simplify their usage.
/// Inherit from this one instead and just implement the abstract methods!
/// From https://damieng.com/blog/2005/10/11/automaticcomparisonoperatoroverloadingincsharp.
/// </summary>
public abstract class IComparableWithOperators<T> : IComparer<T>, IComparable<T>, IEquatable<T> where T : IComparableWithOperators<T> {
	//------------------------------------------------------------------------//
	// ABSTRACT METHODS														  //
	// To be implemented by heirs.											  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Compare this instance with another one.
	/// </summary>
	/// <returns>The result of the comparison (-1, 0, 1).</returns>
	/// <param name="_other">Instance to be compared to.</param>
	protected abstract int CompareToImpl(T _other);

	/// <summary>
	/// Get the hash code corresponding to this object. Used in hashable classes such as Dictionary.
	/// </summary>
	/// <returns>The hash code corresponding to this object.</returns>
	protected abstract int GetHashCodeImpl();

    //------------------------------------------------------------------------//
    // IComparer<T> IMPLEMENTATION                                          //
    //------------------------------------------------------------------------//

    public int Compare(T x, T y) { 
        return InternalCompare(x, y);
    }

    //------------------------------------------------------------------------//
    // IComparable<T> IMPLEMENTATION										  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Compare this instance with another one.
    /// </summary>
    /// <returns>The result of the comparison (-1, 0, 1).</returns>
    /// <param name="_other">Instance to be compared to.</param>
    public int CompareTo(T _other) {
		if(_other == null) return 1;
		return CompareToImpl(_other);
	}

	//------------------------------------------------------------------------//
	// IEquatable<T> IMPLEMENTATION											  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Determines whether the specified <see cref="System.Object"/> is equal to this one.
	/// </summary>
	/// <param name="_other">The <see cref="System.Object"/> to compare to.</param>
	/// <returns><c>true</c> if both objects are equal, <c>false</c> otherwise.</returns>
	public override bool Equals(object _other) {
		// Validate that the object is of this type
		IComparableWithOperators<T> other = _other as IComparableWithOperators<T>;

		// Use typified method
		//return (other == null) ? false : (this == other);
		return (other == null) ? false : this.Equals(other);
	}

	/// <summary>
	/// Determines whether the specified object is equal to this one.
	/// </summary>
	/// <param name="_other">The object to compare to.</param>
	/// <returns><c>true</c> if both objects are equal, <c>false</c> otherwise.</returns>
	public bool Equals(T _other) {
		return this == _other;    // T is of type IComparableWithOperators<T> so it should be ok to use the == operator :)
	}

    /// <summary>
    /// Get the hash code corresponding to this object. Used in hashable classes such as Dictionary.
    /// </summary>
    /// <returns>The hash code corresponding to this object.</returns>
    public override int GetHashCode() {
		return GetHashCodeImpl();
	}

	//------------------------------------------------------------------------//
	// OPERATORS															  //
	//------------------------------------------------------------------------//
	public static bool operator < (IComparableWithOperators<T> _obj1, IComparableWithOperators<T> _obj2) {
		return InternalCompare(_obj1, _obj2) < 0;
	}

	public static bool operator > (IComparableWithOperators<T> _obj1, IComparableWithOperators<T> _obj2) {
		return InternalCompare(_obj1, _obj2) > 0;
	}

	public static bool operator == (IComparableWithOperators<T> _obj1, IComparableWithOperators<T> _obj2) {
		return InternalCompare(_obj1, _obj2) == 0;
	}

	public static bool operator != (IComparableWithOperators<T> _obj1, IComparableWithOperators<T> _obj2) {
		return InternalCompare(_obj1, _obj2) != 0;
	}

	public static bool operator <= (IComparableWithOperators<T> _obj1, IComparableWithOperators<T> _obj2) {
		return InternalCompare(_obj1, _obj2) <= 0;
	}

	public static bool operator >= (IComparableWithOperators<T> _obj1, IComparableWithOperators<T> _obj2) {
		return InternalCompare(_obj1, _obj2) >= 0;
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Internal aux method to simplify operators implementation.
	/// </summary>
	/// <returns>The comparison result (-1, 0, 1).</returns>
	/// <param name="_obj1">First object to compare.</param>
	/// <param name="_obj2">Second object to compare.</param>
	private static int InternalCompare(IComparableWithOperators<T> _obj1, IComparableWithOperators<T> _obj2) {
		// Can't use == operator to check vs null, since we would end up in this function again -> stack overflow!! Use Object.ReferenceEquals() instead :)
		if(Object.ReferenceEquals(_obj1, _obj2)) return 0;
		if(Object.ReferenceEquals(_obj1, null)) return -1;
		if(Object.ReferenceEquals(_obj2, null)) return 1;
		return _obj1.CompareToImpl(_obj2 as T);    // Assuming T : IComparableWithOperators<T>, which should be true
	}
}