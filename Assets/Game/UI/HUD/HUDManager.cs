// HUDManager.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 13/08/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Centralized class to manage updates of all in-game HUD widgets for better performance.
/// </summary>
public class HUDManager {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private class WidgetCollection : HashSet<IHUDWidget> {
		public float timer = 0f;
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Internal
	private Dictionary<float, WidgetCollection> m_widgets = new Dictionary<float, WidgetCollection>();  // Sorting the widgets by update frequency. Key is the frequency.
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Called every frame.
	/// </summary>
	public void Update() {
		// Update all widgets needing it
		foreach(KeyValuePair<float, WidgetCollection> collection in m_widgets) {
			// Update collection timer
			collection.Value.timer += Time.unscaledDeltaTime;	// Don't rely on time scale! Doesn't make sense for UI refreshing

			// Does this collection need to be updated?
			if(collection.Value.timer >= collection.Key) {	// Key is the frequency for this type of widgets
				// Reset timer
				collection.Value.timer = 0f;

				// Iterate through all widgets in this collection and update them
				foreach(IHUDWidget widget in collection.Value) {
					widget.PeriodicUpdate();
				}
			}
		}
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	public void OnDestroy() {
		// Clear widget's list
		m_widgets.Clear();
		m_widgets = null;
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Register a widget to the manager.
	/// </summary>
	/// <param name="_widget"></param>
	public void AddWidget(IHUDWidget _widget) {
		// Find the collection it belongs to (based on its update interval)
		WidgetCollection targetCollection = null;
		if(!m_widgets.TryGetValue(_widget.UPDATE_INTERVAL, out targetCollection)) {
			// No widget with this interval has ever been registered, create the collection
			targetCollection = new WidgetCollection();
			m_widgets[_widget.UPDATE_INTERVAL] = targetCollection;
		}

		// Add the widget to its collection
		targetCollection.Add(_widget);

		// Perform a first update
		_widget.PeriodicUpdate();
	}

	/// <summary>
	/// Unregister a widget from the manager.
	/// </summary>
	/// <param name="_widget"></param>
	public void RemoveWidget(IHUDWidget _widget) {
		// Find widget's collection
		WidgetCollection targetCollection = null;
		if(m_widgets.TryGetValue(_widget.UPDATE_INTERVAL, out targetCollection)) {
			targetCollection.Remove(_widget);
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}