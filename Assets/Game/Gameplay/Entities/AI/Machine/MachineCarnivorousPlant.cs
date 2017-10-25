using UnityEngine;
using System.Collections.Generic;

namespace AI {
	public class MachineCarnivorousPlant : MonoBehaviour, IMachine, ISpawnable {
		[SerializeField] protected MachineSensor m_sensor = new MachineSensor();
		[SerializeField] protected MachineInflammable m_inflammable = new MachineInflammable();

		UnityEngine.Events.UnityAction m_deactivateCallback;

		private Pilot m_pilot = null;
		private ViewControlCarnivorousPlant m_viewControl = null;
		private IEntity m_entity = null;
		private Transform m_transform;
		private Transform m_eye; // for aiming purpose

		private Vector3 m_upVector;

		private Signals m_signals;

		public Vector3 eye		{ get { return m_eye.position; } }
		public Vector3 target	{ get { return Vector3.zero; } }
		public Vector3 upVector { get { return m_upVector; } set {} }

		public Transform enemy { 
			get {
				if (m_sensor != null && (GetSignal(Signals.Type.Warning) || GetSignal(Signals.Type.Danger))) {
					return m_sensor.enemy;
				} else {
					return null;
				}
			}
		}

		public bool isPetTarget 		{ get { return false; } set {} }
		public float lastFallDistance 	{ get { return 0f; } }
		public bool isKinematic 		{ get { return false; } set {} }

		public Quaternion orientation 	{ get { return m_transform.rotation; } set { m_transform.rotation = value; } }
		public Vector3 position			{ get { return m_transform.position; } set { m_transform.position = value; } }
		public Vector3 direction 		{ get { return Vector3.zero; } }
		public Vector3 groundDirection	{ get { return Vector3.right; } }
		public Vector3 velocity			{ get { return Vector3.zero; } }
		public Vector3 angularVelocity	{ get { return Vector3.zero; } }

		public float biteResistance { get { return 0; } }
		public HoldPreyPoint[] holdPreyPoints { get{ return null; } }


		protected void Awake() {
			m_transform = transform;
			m_eye = m_transform.Find("eye");
			m_pilot = GetComponent<Pilot>();
			m_viewControl = GetComponent<ViewControlCarnivorousPlant>();
			m_entity = GetComponent<IEntity>();	

			m_sensor.Attach(this, m_entity, m_pilot);
			m_inflammable.Attach(this, m_entity, m_pilot);

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
			m_inflammable.Init();

			if (InstanceManager.player != null)	{
				DragonPlayer player = InstanceManager.player;
				m_sensor.SetupEnemy(player.dragonEatBehaviour.mouth, player.dragonEatBehaviour.eatDistanceSqr, player.dragonMotion.hitBounds);
			}

			m_upVector = Vector3.up;
			FindUpVector();
			m_transform.rotation = Quaternion.LookRotation(Vector3.forward, m_upVector);
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

		public void OnTrigger(string _trigger, object[] _param = null) {
			if (_trigger == SignalTriggers.OnDestroyed) {
				m_entity.Disable(true);
			} 
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

		public bool Burn(Transform _transform, bool instant = false) {
			if (m_entity.allowBurnable && m_inflammable != null && !IsDead()) {
				if (!GetSignal(Signals.Type.Burning)) {
					ReceiveDamage(9999f);
					m_inflammable.Burn(_transform, instant);
				}
				return true;
			}
			return false;
		}

		public bool IsDead() {
			return m_entity.health <= 0 || m_signals.GetValue(Signals.Type.Destroyed);
		}

		public bool IsDying() {
			return GetSignal(AI.Signals.Type.Chewing) || GetSignal(AI.Signals.Type.Burning);
		}

		public void CustomUpdate() {
			m_inflammable.Update();

			if (IsDying() || IsDead()) {
				m_viewControl.Attack(false);
			} else {
				m_sensor.Update();

				if (m_pilot.IsActionPressed(Pilot.Action.Attack)) {
					UpdateAim();
					m_viewControl.Attack(true);
				} else {
					m_viewControl.Attack(false);
				}
			}
		}

		private void UpdateAim() {			
			Transform target = m_sensor.enemy;
			if (target != null) {
				Vector3 targetDir = target.position - m_eye.position;
				targetDir.z = 0f;

				targetDir.Normalize();
				Vector3 cross = Vector3.Cross(targetDir, m_upVector);
				float aim = cross.z;

				// blend between attack directions
				m_viewControl.Aim(aim);
			}
		}

		private void FindUpVector() {
			RaycastHit[] hit = new RaycastHit[4];
			bool[] hasHit = new bool[4];
			int groundMask = LayerMask.GetMask("Ground", "GroundVisible", "Obstacle", "PreyOnlyCollisions");

			hasHit[0] = Physics.Raycast(position, Vector3.down,  out hit[0], 10f, groundMask);
			hasHit[1] = Physics.Raycast(position, Vector3.up,	 out hit[1], 10f, groundMask);
			hasHit[2] = Physics.Raycast(position, Vector3.right, out hit[2], 10f, groundMask);
			hasHit[3] = Physics.Raycast(position, Vector3.left,  out hit[3], 10f, groundMask);

			float d = 99999f;
			for (int i = 0; i < 4; i++) {
				if (hasHit[i]) {
					if (hit[i].distance < d) {
						d = hit[i].distance;

						m_upVector = hit[i].normal;
						position = hit[i].point;
					}
				}
			}
		}


		/**************************************************************************************************************/

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