// Version.cs
// 
// Created by Alger Ortín Castellví on 29/10/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System;
using System.Globalization;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Simple class to store and format a version number.
/// Given a version number MAJOR.MINOR.PATCH, increment the:
/// MAJOR version when you make incompatible API changes,
/// MINOR version when you add functionality in a backwards-compatible manner, and
/// PATCH version when you make backwards-compatible bug fixes.
/// Additional labels for pre-release and build metadata are available as extensions to the MAJOR.MINOR.PATCH format.
/// </summary>
[Serializable]
public class Version : IComparableWithOperators<Version> {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public enum Format {
		FULL,
		NO_PATCH,
		NO_PATCH_IF_0,
		TEXT
	}

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	[SerializeField] private int m_major = 0;
	public int major {
		get { return m_major; } 
		set { m_major = value; }
	}

	[SerializeField] private int m_minor = 0;
	public int minor {
		get { return m_minor; } 
		set { m_minor = value; }
	}

	[SerializeField] private int m_patch = 0;
	public int patch {
		get { return m_patch; } 
		set { m_patch = value; }
	}

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// Parametrized constructor.
	/// </summary>
	/// <param name="_major">Initial major number.</param>
	/// <param name="_minor">Initial minor number.</param>
	/// <param name="_patch">Initial patch number.</param>
	public Version(int _major = 0, int _minor = 0, int _patch = 0) {
		Set(_major, _minor, _patch);
	}

	/// <summary>
	/// Set patch version in a single call
	/// </summary>
	/// <param name="_major">Major number.</param>
	/// <param name="_minor">Minor number.</param>
	/// <param name="_patch">Patch number.</param>
	public void Set(int _major, int _minor, int _patch) {
		m_major = _major;
		m_minor = _minor;
		m_patch = _patch;
	}

	/// <summary>
	/// Get the string representation of the version.
	/// </summary>
	/// <returns>A string that represents the current <see cref="Version"/>.</returns>
	public override string ToString() {
		return ToString(Format.FULL);
	}

	/// <summary>
	/// Get the string representation of the version.
	/// </summary>
	/// <returns>A string that represents the current <see cref="Version"/>.</returns>
	/// <param name="_format">Format of the version string.</param>
	public string ToString(Format _format) {
		switch(_format) {
			case Format.FULL: {
				return String.Format("{0}.{1}.{2}", major, minor, patch);
			}

			case Format.NO_PATCH: {
				return String.Format("{0}.{1}", major, minor);
			}

			case Format.NO_PATCH_IF_0: {
				if(patch == 0) {
					return ToString(Format.NO_PATCH);
				} else {
					return ToString(Format.FULL);
				}
			}

			case Format.TEXT: {
				return String.Format("major: {0}, minor: {1}, patch: {2}", major, minor, patch);
			}
		}

		return "";
	}

	/// <summary>
	/// Parse a version from string.
	/// </summary>
	/// <param name="_str">String, in the format major.minor.patch. Optionally no patch.</param>
	public static Version Parse(string _str) {
		// Split
		string[] tokens = _str.Split('.');

		// Parse tokens and store them in a new Version object
		Version v = new Version();
		if(tokens.Length > 0) int.TryParse(tokens[0], NumberStyles.Any, CultureInfo.InvariantCulture, out v.m_major);
		if(tokens.Length > 1) int.TryParse(tokens[1], NumberStyles.Any, CultureInfo.InvariantCulture, out v.m_minor);
		if(tokens.Length > 2) int.TryParse(tokens[2], NumberStyles.Any, CultureInfo.InvariantCulture, out v.m_patch);

		return v;
	}

	//------------------------------------------------------------------//
	// IComparableWithOperators IMPLEMENTATION							//
	//------------------------------------------------------------------//
	/// <summary>
	/// Compare this instance with another one.
	/// </summary>
	/// <returns>The result of the comparison (-1, 0, 1).</returns>
	/// <param name="_other">Instance to be compared to.</param>
	override protected int CompareToImpl(Version _other) {
		// Compare from major to patch
		// Equal major?
		int res = m_major.CompareTo(_other.m_major);
		if(res != 0) return res;

		// Equal minor?
		res = m_minor.CompareTo(_other.m_minor);
		if(res != 0) return res;

		// Same major and minor: Result based on patch
		return m_patch.CompareTo(_other.m_patch);
	}

	/// <summary>
	/// Get the hash code corresponding to this object. Used in hashable classes such as Dictionary.
	/// </summary>
	/// <returns>The hash code corresponding to this object.</returns>
	override protected int GetHashCodeImpl() {
		// Generate a unique int from the string representation of the version
		return ToString().GetHashCode();
	}
}