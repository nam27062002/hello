using UnityEngine;
using System.Collections;

public class CatapultAmmo : MonoBehaviour {

	[SerializeField] private float m_damage;
	[SerializeField] private float m_knockback = 0f;
	[SerializeField] private string m_hitParticle = "";

	[SeparatorAttribute]
	[SerializeField] private float m_speedFactor = 1f;
	[SerializeField] private float m_rotationSpeedFactor = 1f;

	[SeparatorAttribute("Audio")]
	[SerializeField] private string m_onExplodeAudio = "";

	private PoolHandler m_poolHandler;
	private ParticleHandler m_hitParticleHandler;

	private Transform m_oldParent = null;

	private bool m_hasBeenTossed = false;

	private Vector3 m_scale;
	private Vector3 m_po;
	private float m_vo;
	private float m_vAngle;
	private float m_hAngle;

	private Vector3 m_rotAxis;

	private float m_elapsedTime;

	void Start() {
		m_scale = transform.localScale;
		m_poolHandler = PoolManager.GetHandler(gameObject.name);
		m_hitParticleHandler = ParticleManager.CreatePool(m_hitParticle);
	}

	public void AttachTo(Transform _parent, Vector3 _localPosition) {
		m_oldParent = transform.parent;
		transform.parent = _parent;
		transform.forward = _parent.forward;
		transform.localPosition = _localPosition;
		transform.localScale = Vector3.zero;

		m_hasBeenTossed = false;
	}

	public void Toss(float _vo, float _vAngle, float _hAngle) {
		m_vo = _vo;
		m_vAngle = _vAngle;
		m_hAngle = _hAngle;

		m_elapsedTime = 0;

		m_rotAxis = transform.parent.right;
		transform.parent = m_oldParent;

		m_po = transform.position;

		m_hasBeenTossed = true;
	}

	// Update is called once per frame
	void Update() {
		transform.localScale = Vector3.Lerp(transform.localScale, m_scale, Time.smoothDeltaTime * 4f);

		if (m_hasBeenTossed) {
			m_elapsedTime += Time.deltaTime * m_vo * 0.125f * m_speedFactor;
			transform.position = m_po + UpdateMovement(m_elapsedTime);
			transform.rotation = Quaternion.AngleAxis(m_elapsedTime * 240f * m_rotationSpeedFactor, m_rotAxis);

			if (m_elapsedTime > 30f) {
				m_hasBeenTossed = false;
				gameObject.SetActive(false);
				m_poolHandler.ReturnInstance(gameObject);
			}
		}
	}

	private Vector3 UpdateMovement(float _t) {
		Vector3 pos = Vector3.zero;
		pos.x = m_vo * Mathf.Cos(m_vAngle * Mathf.Deg2Rad) * Mathf.Sin(m_hAngle * Mathf.Deg2Rad) * _t;
		pos.y = m_vo * Mathf.Sin(m_vAngle * Mathf.Deg2Rad) * _t - (9.8f * _t * _t * 0.5f);
		pos.z = m_vo * Mathf.Cos(m_vAngle * Mathf.Deg2Rad) * Mathf.Cos(m_hAngle * Mathf.Deg2Rad) * _t;

		return pos;
	}

	private void OnTriggerEnter(Collider _other) {
		if (m_hasBeenTossed) {
			if (_other.CompareTag("Player"))  {
				Explode(true);
			} else if ((((1 << _other.gameObject.layer) & LayerMask.GetMask("Ground", "GroundVisible")) > 0)) {
				Explode(false);
			}
		}
	}

	public void Explode(bool _hitDragon) {		
		m_hitParticleHandler.Spawn(null, transform.position);

		if (_hitDragon) {
			if (m_knockback > 0) {
				DragonMotion dragonMotion = InstanceManager.player.dragonMotion;

				Vector3 knockBackDirection = dragonMotion.transform.position - transform.position;
				knockBackDirection.z = 0f;
				knockBackDirection.Normalize();

				dragonMotion.AddForce(knockBackDirection * m_knockback);
			}

			InstanceManager.player.GetComponent<DragonHealthBehaviour>().ReceiveDamage(m_damage, DamageType.NORMAL, transform);
		}

		if (!string.IsNullOrEmpty(m_onExplodeAudio))
			AudioController.Play(m_onExplodeAudio, transform.position);

		m_hasBeenTossed = false;
		gameObject.SetActive(false);
		m_poolHandler.ReturnInstance(gameObject);
	}
}
