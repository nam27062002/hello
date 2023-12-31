// CoroutineManager.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 12/06/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Helper class to allow launching coroutines and async tasks from non-MonoBehaviour classes.
/// </summary>
namespace UbiBCN {
	public class CoroutineManager : UbiBCN.SingletonMonoBehaviour<CoroutineManager> {
		//------------------------------------------------------------------------//
		// CONSTANTS															  //
		//------------------------------------------------------------------------//
		
		//------------------------------------------------------------------------//
		// MEMBERS AND PROPERTIES												  //
		//------------------------------------------------------------------------//
		
		//------------------------------------------------------------------------//
		// INTERNAL METHODS														  //
		//------------------------------------------------------------------------//
		/// <summary>
		/// Creates a simple coroutine that waits some time before triggering an action.
		/// </summary>
		/// <returns>The coroutine.</returns>
		/// <param name="_action">Action to be triggered.</param>
		/// <param name="_delay">Delay before triggereing the action.</param>
		/// <param name="_ignoreTimescale">Whether to take timescale in account when delaying the action.</param>
		private IEnumerator DelayedCoroutine(Action _action, float _delay, bool _ignoreTimescale) {
			// If delay is 0, invoke the action immediately
			if(_delay > 0) {
				// Wait the target time
				// Ignore time scale?
				if(_ignoreTimescale) {
					yield return new WaitForSecondsRealtime(_delay);
				} else {
					yield return new WaitForSeconds(_delay);
				}
			}

			// Trigger the action
			_action.Invoke();
		}

		/// <summary>
		/// Creates a simple coroutine that waits some frames before triggering an action.
		/// </summary>
		/// <returns>The coroutine.</returns>
		/// <param name="_action">Action to be triggered.</param>
		/// <param name="_delay">Frames to wait before triggereing the action.</param>
		private IEnumerator DelayedCoroutineByFrames(Action _action, int _delayFrames) {
			// If delay is 0, invoke the action immediately
			while(_delayFrames > 0) {
				_delayFrames--;
				yield return new WaitForEndOfFrame();
			}

			// Trigger the action
			_action.Invoke();
		}

		//------------------------------------------------------------------------//
		// SINGLETON METHODS													  //
		//------------------------------------------------------------------------//
		/// <summary>
		/// Trigger an action after some delay.
		/// </summary>
		/// <param name="_action">Action to be triggered.</param>
		/// <param name="_delaySecs">Delay in seconds.</param>
		/// <param name="_ignoreTimescale">Whether to take timescale in account when delaying the action.</param>
        public static Coroutine DelayedCall(Action _action, float _delaySecs = 0f, bool _ignoreTimescale = true) {
			// Launch the coroutine
            return instance.StartCoroutine(instance.DelayedCoroutine(_action, _delaySecs, _ignoreTimescale));
		}

		/// <summary>
		/// Trigger an action after some frames.
		/// </summary>
		/// <param name="_action">Action to be triggered.</param>
		/// <param name="_delay">Frames to wait before triggereing the action.</param>
        public static Coroutine DelayedCallByFrames(Action _action, int _delayFrames) {
			// Launch the coroutine
            return instance.StartCoroutine(instance.DelayedCoroutineByFrames(_action, _delayFrames));
		}

		/// <summary>
		/// Static wrapper for the StartCoroutine method, allowing any class to launch 
		/// a coroutine from this manager.
		/// </summary>
		/// <returns>The started coroutine.</returns>
		/// <param name="_coroutine">The coroutine to be started.</param>
		public static Coroutine StartExternalCoroutine(IEnumerator _coroutine) {
			// Just use the instance method
			return instance.StartCoroutine(_coroutine);
		}
	}
}