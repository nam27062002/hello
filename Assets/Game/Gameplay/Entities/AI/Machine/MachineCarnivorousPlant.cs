using UnityEngine;
using System.Collections.Generic;

namespace AI {
	public class MachineCarnivorousPlant : MonoBehaviour, IMachine, ISpawnable {
		[SerializeField] protected MachineSensor m_sensor = new MachineSensor();


		UnityEngine.Events.UnityAction m_deactivateCallback;

		private Pilot m_pilot = null;
		private ViewControlCarnivorousPlant m_viewControl = null;
		private IEntity m_entity = null;
		private Transform m_transform;
		private Transform m_eye; // for aiming purpose

		private Signals m_signals;

		public Vector3 eye						{ get { return m_eye.position; } }
		public Vector3 target					{ get { return Vector3.zero; } }
		public virtual Vector3 upVector 		{ get { return Vector3.up; } set {} }

		public Transform enemy { 
			get {
				if (m_sensor != null && (GetSignal(Signals.Type.Warning) || GetSignal(Signals.Type.Danger))) {
					return m_sensor.enemy;
				} else {
					return null;
				}
			}
		}

		public bool isPetTarget 				{ get { return false; } set {} }
		public virtual float lastFallDistance 	{ get { return 0f; } }
		public virtual bool isKinematic 		{ get { return false; } set {} }

		public virtual Quaternion orientation 	{ get { return m_transform.rotation; } set { m_transform.rotation = value; } }
		public virtual Vector3 position			{ get { return m_transform.position; } set { m_transform.position = value; } }
		public virtual Vector3 direction 		{ get { return Vector3.zero; } }
		public virtual Vector3 groundDirection	{ get { return Vector3.right; } }
		public virtual Vector3 velocity			{ get { return Vector3.zero; } }
		public virtual Vector3 angularVelocity	{ get { return Vector3.zero; } }

		public float biteResistance { get { return 0; } }
		public HoldPreyPoint[] holdPreyPoints { get{ return null; } }


		protected void Awake() {
			m_transform = transform;
			m_eye = m_transform.Find("eye");
			m_pilot = GetComponent<Pilot>();
			m_viewControl = GetComponent<ViewControlCarnivorousPlant>();
			m_entity = GetComponent<IEntity>();	

			m_sensor.Attach(this, m_entity, m_pilot);

			m_signals = new Signals(this);
			m_signals.Init();

			m_signals.SetOnEnableTrigger(Signals.Type.Alert, SignalTriggers.OnAlert);
			m_signals.SetOnDisableTrigger(Signals.Type.Alert, SignalTriggers.OnIgnoreAll);

			m_signals.SetOnEnableTrigger(Signals.Type.Warning, SignalTriggers.OnWarning);	
			m_signals.SetOnDisableTrigger(Signals.Type.Warning, SignalTriggers.OnCalm);		

			m_signals.SetOnEnableTrigger(Signals.Type.Danger, SignalTriggers.OnDanger);
			m_signals.SetOnDisableTrigger(Signals.Type.Danger, SignalTriggers.OnSafe);

			m_signals.SetOnEnableTrigger(Signals.Type.Critical, SignalTriggers.OnCritical);

			m_signals.SetOnEnableTrigger(Signals.Type.Panic, SignalTriggers.OnPanic);
			m_signals.SetOnDisableTrigger(Signals.Type.Panic, SignalTriggers.OnRecoverFromPanic);

			m_signals.SetOnEnableTrigger(Signals.Type.Burning, SignalTriggers.OnBurning);

			m_signals.SetOnEnableTrigger(Signals.Type.Destroyed, SignalTriggers.OnDestroyed);
		}

		public void Spawn(ISpawner _spawner) {
			m_signals.Init();
			m_sensor.Init();
			if (InstanceManager.player != null)	{
				DragonPlayer player = InstanceManager.player;
				m_sensor.SetupEnemy(player.dragonEatBehaviour.mouth, player.dragonEatBehaviour.eatDistanceSqr, player.dragonMotion.hitBounds);
			}
		}

		public void Activate() {
			gameObject.SetActive(true);
			if (m_deactivateCallback != null)
				m_deactivateCallback();
		}

		public void Deactivate( float duration, UnityEngine.Events.UnityAction _action) {
			gameObject.SetActive(false);
			m_deactivateCallback = _action;
			Invoke("Activate", duration);
		}

		public void SetSignal(Signals.Type _signal, bool _activated, object[] _params = null) {
			m_signals.SetValue(_signal, _activated, _params);
		}

		public bool GetSignal(Signals.Type _signal) {
			if (m_signals != null)
				return m_signals.GetValue(_signal);

			return false;
		}

		public object[] GetSignalParams(Signals.Type _signal) {
			return m_signals.GetParams(_signal);
		}

		public void DisableSensor(float _seconds) {
			m_sensor.Disable(_seconds);
		}

		public virtual bool Burn(Transform _transform) {
			//TODO
			return false; 
		}

		public bool IsDead() {
			//TODO
			return false; 
		}

		public bool IsDying() {
			//TODO
			return false; 
		}

		public void CustomUpdate() {
			m_sensor.Update();

			if (m_pilot.IsActionPressed(Pilot.Action.Attack)) {
				UpdateAim();
				m_viewControl.Attack(true);
			} else {
				m_viewControl.Attack(false);
			}
		}

		private void UpdateAim() {			
			Transform target = m_sensor.enemy;
			if (target != null) {
				Vector3 targetDir = target.position - m_eye.position;
				targetDir.z = 0f;

				targetDir.Normalize();
				Vector3 cross = Vector3.Cross(targetDir, Vector3.up);
				float aim = cross.z;

				// blend between attack directions
				m_viewControl.Aim(aim);
			}
		}


		public void OnTrigger(string _trigger, object[] _param = null) {}
		public virtual void CheckCollisions(bool _value) {}
		public virtual void FaceDirection(bool _value) {}
		public bool HasCorpse() { return false; }
		public void ReceiveDamage(float _damage) {}

		public virtual void UseGravity(bool _value) { }
		public virtual bool IsFacingDirection() { return false; }
		public bool IsFreezing(){ return false; }
		public void CustomFixedUpdate(){}

		public void AddExternalForce(Vector3 force) {}
		public Quaternion GetDyingFixRot() { return Quaternion.identity; }
		public void SetVelocity(Vector3 _v) {}
		public void BiteAndHold() {}
		public void ReleaseHold() {}
		public void EndSwallowed(Transform _transform){}
		public void Bite() {}
		public void Drown() {}
		public void BeginSwallowed(Transform _transform, bool _rewardsPlayer, bool _isPlayer) {}

		public void	EnterGroup(ref Group _group) {}
		public Group GetGroup() {return null;}
		public void LeaveGroup() {}

		public void EnterDevice(bool _isCage) {}
		public void LeaveDevice(bool _isCage) {}
		public virtual bool CanBeBitten() {return false;}
	}
}