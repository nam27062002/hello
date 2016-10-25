﻿using UnityEngine;
using System.Collections;

public class CatapultAmmo : MonoBehaviour {

	[SerializeField] private float m_damage;
	[SerializeField] private string m_hitParticle = "";

	private Transform m_oldParent;

	private bool m_hasBeenTossed;

	private Vector3 m_po;
	private float m_vo;
	private float m_vAngle;
	private float m_hAngle;

	private Vector3 m_rotAxis;

	private float m_elapsedTime;

	// Use this for initialization
	void Start () {
		m_oldParent = null;
		m_hasBeenTossed = false;
	}

	public void AttachTo(Transform _parent) {
		m_oldParent = transform.parent;
		transform.parent = _parent;
		transform.forward = _parent.forward;
		transform.localPosition = Vector3.zero;

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
		if (m_hasBeenTossed) {
			m_elapsedTime += Time.deltaTime * m_vo * 0.125f;
			transform.position = m_po + UpdateMovement(m_elapsedTime);
			transform.rotation = Quaternion.AngleAxis(m_elapsedTime * 240f, m_rotAxis);

			//TODO: check out of camera/visible	
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
			if (_other.tag == "Player")  {
				Explode(true);
			} else if ((((1 << _other.gameObject.layer) & LayerMask.GetMask("Ground", "GroundVisible")) > 0)) {
				Explode(false);
			}
		}
	}

	private void Explode(bool _hitDragon) {		
		ParticleManager.Spawn(m_hitParticle, transform.position);

		gameObject.SetActive(false);
		PoolManager.ReturnInstance(gameObject);
	}
}
