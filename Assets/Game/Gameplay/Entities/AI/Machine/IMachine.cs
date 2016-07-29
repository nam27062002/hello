﻿using UnityEngine;
using System.Collections.Generic;

namespace AI {
	public interface IMachine {
		//
		Vector3 position 	{ get; }
		Vector3 target		{ get; }
		Vector3 direction 	{ get; }

		// Internal connections
		void SetSignal(Signals.Type _signal, bool _activated);
		bool GetSignal(Signals.Type _signal);

		// Group membership -> for collective behaviours
		void	EnterGroup(ref Group _group);
		Group 	GetGroup();
		void	LeaveGroup();

		// External interactions
		void Bite();
		void BeingSwallowed(Transform _transform);
		void BiteAndHold();
		bool Burn(float _damage, Transform _transform);
	}
}