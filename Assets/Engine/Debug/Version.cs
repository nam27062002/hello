// Version.cs
// 
// Created by Alger Ortín Castellví on 29/10/2015.
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
/// Simple class to store and format a version number.
/// Given a version number MAJOR.MINOR.PATCH, increment the:
/// MAJOR version when you make incompatible API changes,
/// MINOR version when you add functionality in a backwards-compatible manner, and
/// PATCH version when you make backwards-compatible bug fixes.
/// Additional labels for pre-release and build metadata are available as extensions to the MAJOR.MINOR.PATCH format.
/// </summary>
[Serializable]
public class Version {
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
			} break;

			case Format.NO_PATCH: {
				return String.Format("{0}.{1}", major, minor);
			} break;

			case Format.NO_PATCH_IF_0: {
				if(patch == 0) {
					return ToString(Format.NO_PATCH);
				} else {
					return ToString(Format.FULL);
				}
			} break;

			case Format.TEXT: {
				return String.Format("major: {0}, minor: {1}, patch: {2}", major, minor, patch);
			} break;
		}

		return "";
	}
}