// BoHFontTestDouble.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 09/02/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
public class BoHFontTestDouble : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	private static readonly string TEST_STRING = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!\"·$%&/()=?¿*+-_{}<>|@#€\\";	// Almost every standard char (no language customs)
	
	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	public GameObject m_textsContainer1 = null;
	public GameObject m_textsContainer2 = null;
	public Text m_outputText = null;

	private Text[] m_textsFont1 = null;
	private Text[] m_textsFont2 = null;
	private System.DateTime m_startTime = System.DateTime.UtcNow;
	private int m_idx = -1;
	
	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		// Check fields
		Debug.Assert(m_textsContainer1 != null, "Required field not initialized!");
		Debug.Assert(m_textsContainer2 != null, "Required field not initialized!");
		Debug.Assert(m_outputText != null, "Required field not initialized!");

		// Get references to all relevant elements
		m_textsFont1 = m_textsContainer1.GetComponentsInChildren<Text>();
		m_textsFont2 = m_textsContainer2.GetComponentsInChildren<Text>();
	}

	/// <summary>
	/// Called every frame
	/// </summary>
	private void Update() {
		if(m_idx >= 0) {
			if(m_idx < m_textsFont1.Length) {
				m_textsFont1[m_idx].text = TEST_STRING;
				m_outputText.text += ElapsedTime() + ": " + m_textsFont1[m_idx].font.name + m_textsFont1[m_idx].fontSize + "\n";
			}

			if(m_idx < m_textsFont2.Length) {
				m_textsFont2[m_idx].text = TEST_STRING;
				m_outputText.text += ElapsedTime() + ": " + m_textsFont2[m_idx].font.name + m_textsFont2[m_idx].fontSize + "\n";
			}

			m_idx++;
			if(m_idx >= m_textsFont1.Length && m_idx >= m_textsFont2.Length) {
				m_outputText.text += ElapsedTime() + ": " + "Done! ";
				m_idx = -1;
			}
		}
	}

	/// <summary>
	/// Elapsed time since the test started
	/// </summary>
	/// <returns>Seconds since the test started.</returns>
	private float ElapsedTime() {
		return (float)(System.DateTime.UtcNow - m_startTime).TotalMilliseconds;
	}

	/// <summary>
	/// 
	/// </summary>
	private void ResetTests() {
		m_outputText.text = "";
		for(int i = 0; i < m_textsFont1.Length; i++) {
			m_textsFont1[i].text = "";
		}
		for(int i = 0; i < m_textsFont2.Length; i++) {
			m_textsFont2[i].text = "";
		}
		m_startTime = System.DateTime.UtcNow;
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// 
	/// </summary>
	public void OnTestSync() {
		// Reset
		ResetTests();
		m_idx = -1;

		m_outputText.text += ElapsedTime() + ": " + "Starting test...\n";

		for(int i = 0; i < m_textsFont1.Length || i < m_textsFont2.Length; i++) {
			if(i < m_textsFont1.Length) {
				m_textsFont1[i].text = TEST_STRING;
				m_outputText.text += ElapsedTime() + ": " + m_textsFont1[i].font.name + m_textsFont1[i].fontSize + "\n";
			}

			if(i < m_textsFont2.Length) {
				m_textsFont2[i].text = TEST_STRING;
				m_outputText.text += ElapsedTime() + ": " + m_textsFont2[i].font.name + m_textsFont2[i].fontSize + "\n";
			}
		}

		m_outputText.text += ElapsedTime() + ": " + "Done! ";
	}

	/// <summary>
	/// 
	/// </summary>
	public void OnTestAsync() {
		// Reset
		ResetTests();
		m_idx = 0;

		m_outputText.text += ElapsedTime() + ": " + "Starting test...\n";
	}
}