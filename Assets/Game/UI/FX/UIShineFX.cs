using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIShineFX : MonoBehaviour {
	//-------------------------------------------------------------//

	private class ShineElement {
		public int spriteIdx;

		public int spawnPointIdx;

		public float duration;
		public float timer;

		public float scaleStart;
		public float scaleEnd;

		public float rotationStart;
		public float rotationEnd;
	}

	[System.Serializable]
	private class SpriteAndSize {
		public Sprite sprite;
		public float size;
	}


	//-------------------------------------------------------------//
	[SeparatorAttribute]
	[SerializeField] private Transform m_spawnPointsRoot;

	[SeparatorAttribute]
	[SerializeField] private SpriteAndSize[] m_sprites;

	[SeparatorAttribute]
	[SerializeField] private int m_maxAlive;

	[SeparatorAttribute]
	[SerializeField] private Range m_duration;
	[SerializeField] private Range m_delay;
	[SerializeField] private Range m_scale;
	[SerializeField] private Range m_rotation;
	[SerializeField] private Color[] m_colors;


	//-------------------------------------------------------------//
	private List<Transform> m_spawnPoints;
	private bool[] m_freeSpawnPoints;

	private ShineElement[] m_elements;
	private Transform[] m_transforms;
	private Image[] m_images;
	private bool[] m_free;

	private float m_spawnTimer;

	private int m_spawnMax;
	private int m_spawnCount;


	//-------------------------------------------------------------//
	// Use this for initialization
	void Start () {		
		m_spawnPoints = new List<Transform>(m_spawnPointsRoot.GetComponentsInChildren<Transform>());
		m_spawnPoints.Remove(this.transform);	// Remove ourselves!
		m_freeSpawnPoints = new bool[m_spawnPoints.Count];

		m_spawnMax = Mathf.Min(m_maxAlive, m_spawnPoints.Count);

		m_elements = new ShineElement[m_spawnMax];
		m_transforms = new Transform[m_spawnMax];
		m_images = new Image[m_spawnMax];
		m_free = new bool[m_spawnMax];

		for (int i = 0; i < m_spawnMax; ++i) {
			GameObject go = new GameObject();
			m_images[i] = go.AddComponent<Image>();
			m_images[i].raycastTarget = false;

			m_elements[i] = new ShineElement();

			go.name = "shine";
			go.transform.SetParent (transform, false);
			m_transforms[i] = go.transform;

			go.layer = transform.gameObject.layer;
			go.SetActive(false);

			m_free[i] = true;
		}

		m_spawnCount = 0;

		m_spawnTimer = m_delay.GetRandom();
	}
	
	// Update is called once per frame
	void Update () {
		// Spawn a shine
		m_spawnTimer -= Time.deltaTime;
		if (m_spawnTimer <= 0) {
			if (m_spawnCount < m_spawnMax) {
				Spawn();
			}

			m_spawnTimer = m_delay.GetRandom();
		}

		// update alive shines
		for (int i = 0; i < m_spawnMax; ++i) {
			if (!m_free[i]) {
				ShineElement se = m_elements[i];

				se.timer -= Time.deltaTime;
				if (se.timer <= 0) {
					Return(i);
				} else {
					UpdateShine(i);
				}
			}
		}
	}

	private void Spawn() {
		int freeIndex = -1;

		for (int i = 0; i < m_spawnMax; ++i) {
			if (m_free[i]) {
				freeIndex = i;
			}
		}

		if (freeIndex >= 0) {
			ShineElement se = m_elements[freeIndex];

			se.spriteIdx = Random.Range(0, m_sprites.Length);

			if (freeIndex % 2 == 0) {
				se.spawnPointIdx = Random.Range(0, Mathf.FloorToInt(m_freeSpawnPoints.Length * 0.5f));
			} else {
				se.spawnPointIdx = Random.Range(Mathf.FloorToInt(m_freeSpawnPoints.Length * 0.5f), m_freeSpawnPoints.Length);
			}
			m_freeSpawnPoints[se.spawnPointIdx] = false;

			se.duration = m_duration.GetRandom();
			se.timer = se.duration;

			se.scaleStart = m_scale.GetRandom();
			se.scaleEnd = m_scale.GetRandom();

			se.rotationStart = m_rotation.GetRandom();
			se.rotationEnd = m_rotation.GetRandom();

			m_transforms[freeIndex].position = m_spawnPoints[se.spawnPointIdx].position;

			UpdateShine(freeIndex);

			m_images[freeIndex].sprite = m_sprites[se.spriteIdx].sprite;
			m_images[freeIndex].rectTransform.sizeDelta = Vector2.one * m_sprites[se.spriteIdx].size;
			m_images[freeIndex].gameObject.SetActive(true);

			if (m_colors.Length > 0) {
				m_images[freeIndex].color = m_colors[Random.Range (0, m_colors.Length)];
			}

			m_spawnCount++;

			m_free[freeIndex] = false;
		}
	}

	private void Return(int _index) {
		m_freeSpawnPoints[m_elements[_index].spawnPointIdx] = true;
		m_images[_index].gameObject.SetActive(false);
		m_free[_index] = true;

		m_spawnCount--;
	}

	private void UpdateShine(int _index) {
		ShineElement se = m_elements[_index];
		float dt = 1f - (se.timer / se.duration);

		float scale 	= Mathf.Lerp(se.scaleStart, se.scaleEnd, dt);
		float rotation 	= Mathf.Lerp(se.rotationStart, se.rotationEnd, dt);

		Color c = m_images[_index].color;
		c.a = Mathf.Sin(Mathf.Lerp(0f, Mathf.PI, dt));
		m_images[_index].color = c;

		Transform t = m_transforms[_index];

		Vector3 s = t.localScale;
		s.x = scale; s.y = scale;
		t.localScale = s;

		t.localRotation = Quaternion.Euler(GameConstants.Vector3.forward * rotation);
	}
}
