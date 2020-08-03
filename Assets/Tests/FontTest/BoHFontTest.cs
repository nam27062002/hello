// BoHFontTest.cs
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
public class BoHFontTest : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	private static readonly string TEST_STRING = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!\"·$%&/()=?¿*+-_{}<>|@#€\\";	// Almost every standard char (no language customs)
	
	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	public GameObject m_textsContainer = null;
	public Text m_outputText = null;

	private Text[] m_texts = null;
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
		Debug.Assert(m_textsContainer != null, "Required field not initialized!");
		Debug.Assert(m_outputText != null, "Required field not initialized!");

		// Get references to all relevant elements
		m_texts = m_textsContainer.GetComponentsInChildren<Text>();
	}

	/// <summary>
	/// Called every frame
	/// </summary>
	private void Update() {
		if(m_idx >= 0) {
			if(m_idx < m_texts.Length) {
				m_texts[m_idx].text = TEST_STRING;
				m_outputText.text += ElapsedTime() + ": " + m_texts[m_idx].fontSize + "\n";

				m_idx++;
				if(m_idx >= m_texts.Length) {
					m_outputText.text += ElapsedTime() + ": " + "Done! ";
					m_idx = -1;
				}
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
		for(int i = 0; i < m_texts.Length; i++) {
			m_texts[i].text = "";
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

		for(int i = 0; i < m_texts.Length; i++) {
			m_texts[i].text = TEST_STRING;
			m_outputText.text += ElapsedTime() + ": " + m_texts[i].fontSize + "\n";
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