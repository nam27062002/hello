using UnityEngine;
using System.Collections.Generic;

namespace AI {
	public interface IMachine {
		//
		Vector3 position { get; }
		Vector3 direction { get; }

		// Internal connections
		void SetSignal(Machine.Signal _signal, bool _activated);
		bool GetSignal(Machine.Signal _signal);

		// Group membership -> for collective behaviours
		void	EnterGroup(ref Group _group);
		Group 	GetGroup();
		void	LeaveGroup();

		// External interactions
		void Bite();
		void BiteAndHold();
		void Burn();
	}
}