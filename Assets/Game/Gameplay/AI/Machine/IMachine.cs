namespace AI {
	public interface IMachine {
		// Internal connections
		void SetSignal(Machine.Signal _signal, bool _activated);
		bool GetSignal(Machine.Signal _signal);
			
		// External interactions
		void Bite();
		void BiteAndHold();
		void Burn();

	}
}