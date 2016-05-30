using System.Collections.Generic;

namespace AI {
	public interface IMachine {
		// Internal connections
		void SetSignal(Machine.Signal _signal, bool _activated);
		bool GetSignal(Machine.Signal _signal);

		// Flocking -> move to another interface?
		void			SetFlock(List<IMachine> _flock);
		List<IMachine> 	GetFlock();

		// External interactions
		void Bite();
		void BiteAndHold();
		void Burn();
	}
}