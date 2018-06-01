using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public abstract class ScrollRectItem<D> : MonoBehaviour {	
	private RectTransform m_rt;


	public abstract void InitWithData(D _data);

	public void SetPosition(Vector2 _pos) {
		if (m_rt == null) {
			m_rt = gameObject.GetComponent<RectTransform>();
		}
		m_rt.anchoredPosition = _pos;
	}
}

[System.Serializable]
public class Padding {
	public float left;
	public float right;
	public float top;
	public float bottom;
	public float spacing;
}

public class OptimizedScrollRect<T,D> : ScrollRect where T : ScrollRectItem<D> {	
	//------------------------------------------------------------------------//
	// SERIALIZED    														  //
	//------------------------------------------------------------------------//

	[SerializeField] private RectTransform m_mask;
	[SerializeField] private Padding m_padding;



	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	private List<D> m_itemData;


	private List<T> m_pills;

	protected Vector2 m_pillSize = GameConstants.Vector2.zero;
	protected Vector2 m_visibleAreaSize = GameConstants.Vector2.zero;
	protected Vector2 m_containerSize = GameConstants.Vector2.zero;

	private int m_itemCount = 0;
	private int m_firstVisibleItemIndex = 0;



	//------------------------------------------------------------------------//
	// QUERIES      														  //
	//------------------------------------------------------------------------//

	protected float Top() 		{ return (m_containerSize.y * 0.5f) - (m_padding.top); }
	protected float Bottom() 	{ return (m_containerSize.y * 0.5f) - (m_visibleAreaSize.y - m_padding.bottom); }
	protected float Left() 		{ return (m_containerSize.x * 0.5f) - (m_padding.left);	}
	protected float Right() 	{ return (m_containerSize.x * 0.5f) - (m_visibleAreaSize.x - m_padding.right); }

	protected float GetPillPositionY(int _index) { return (m_containerSize.y * 0.5f) - (m_padding.top + (m_pillSize.y * 0.5f) + ((m_pillSize.y + m_padding.spacing) * _index));  }
	protected float GetPillPositionX(int _index) { return (m_containerSize.x * 0.5f) - (m_padding.left + (m_pillSize.x * 0.5f) + ((m_pillSize.x + m_padding.spacing) * _index)); }



	//------------------------------------------------------------------------//
	// METHODS      														  //
	//------------------------------------------------------------------------//

	protected override void OnEnable() {
		base.OnEnable();
		onValueChanged.AddListener(OnScrollMoved);
	}

	protected override void OnDisable() {
		base.OnDisable();
		onValueChanged.RemoveListener(OnScrollMoved);
	}

	public void Setup(GameObject _pillPrefab, List<D> _itemData) {
		m_itemData = _itemData;

		m_visibleAreaSize.x = m_mask.rect.width;
		m_visibleAreaSize.y = m_mask.rect.height;

		//
		// Create the smallest 
		if (m_pills == null) {
			m_pills = new List<T>();
			m_pills.Add(GameObject.Instantiate<GameObject>(_pillPrefab, content, false).GetComponent<T>());
		}
		GameObject go = m_pills[0].gameObject;

		RectTransform rt = go.GetComponent<RectTransform>();
		m_pillSize = rt.sizeDelta;

		int pillCount = 0;
		if (vertical) {
			float h = m_padding.spacing + m_pillSize.y;
			float areaH = m_visibleAreaSize.y - m_padding.top - m_padding.bottom;
			pillCount = Mathf.CeilToInt(areaH / h);
		}

		if (horizontal) {
			float w = m_padding.spacing + m_pillSize.x;
			float areaW = m_visibleAreaSize.x - m_padding.left - m_padding.right;
			pillCount = Mathf.CeilToInt(areaW / w);
		}

		if (pillCount > m_pills.Count) {
			for (int i = m_pills.Count; i < pillCount; ++i) {
				go = GameObject.Instantiate<GameObject>(_pillPrefab, content, false);
				m_pills.Add(go.GetComponent<T>());
			}
		} else {
			while (pillCount < m_pills.Count) {
				T last = m_pills.Last();
				m_pills.RemoveAt(m_pills.Count - 1);
				GameObject.Destroy(last.gameObject);
			}		
		}

		//
		// Resize container
		m_itemCount = _itemData.Count;
		if (vertical) {
			m_containerSize.y = m_padding.top + (m_pillSize.y * m_itemCount) + (m_padding.spacing * (m_itemCount - 1)) + m_padding.bottom;
		}
		if (horizontal) {
			m_containerSize.x = m_padding.left + (m_pillSize.x * m_itemCount) + (m_padding.spacing * (m_itemCount - 1)) + m_padding.right;
		}
		content.sizeDelta = m_containerSize;


		//
		m_firstVisibleItemIndex = 0;
		ShowPillsFrom(m_firstVisibleItemIndex);
	}

	private void ShowPillsFrom(int _index) {
		int pillCount = m_pills.Count;
		for (int i = 0; i < pillCount; ++i) {
			int itemIndex = i + _index; 

			T pill = m_pills[itemIndex % pillCount];

			if (itemIndex < m_itemCount) {
				pill.InitWithData(m_itemData[itemIndex]);
				pill.gameObject.SetActive(true);

				Vector2 pillPos = GameConstants.Vector2.zero;
				if (vertical) {
					pillPos.y = GetPillPositionY(itemIndex);
				}
				if (horizontal) {
					pillPos.x = GetPillPositionX(itemIndex);
				}
				pill.SetPosition(pillPos);
			} else {
				pill.gameObject.SetActive(false);
			}
		}

		m_firstVisibleItemIndex = _index;

		OnShowPillsFrom(_index);
	}

	private void OnScrollMoved(Vector2 _position) {
		Vector2 anchoredPos = content.anchoredPosition;

		float startPosition = 0f;

		if (vertical) {
			startPosition = anchoredPos.y - m_padding.top;
			startPosition /= (m_pillSize.y + m_padding.spacing);
		}
		if (horizontal) {
			startPosition = anchoredPos.x - m_padding.left;
			startPosition /= (m_pillSize.x + m_padding.spacing);
		}

		int startIndex = (int)startPosition;
		if (startIndex < 0) {
			startIndex = 0;
		}

		if (startIndex != m_firstVisibleItemIndex) {
			ShowPillsFrom(startIndex);
		}

		OnScrollMoved();
	}


	protected virtual void OnShowPillsFrom(int _index) {}
	protected virtual void OnScrollMoved() {}
}
