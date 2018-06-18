using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;


public class ScrollRectItemData<D> {
	public D data;
	public int pillType;
}

public abstract class ScrollRectItem<D> : MonoBehaviour {	
	public Vector2 size;
	private RectTransform m_rt;

	public abstract void InitWithData(D _data);

	public void ComputeSize() {
		if (m_rt == null) {
			m_rt = gameObject.GetComponent<RectTransform>();
		}
		size = m_rt.sizeDelta;
	}

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

public class OptimizedScrollRect<T, D> : ScrollRect where T : ScrollRectItem<D> {	
	//------------------------------------------------------------------------//
	// SERIALIZED    														  //
	//------------------------------------------------------------------------//

	[SerializeField] private float m_autoScrollTime = 0.75f;
	[SerializeField] private Padding m_padding;



	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	private List<GameObject> m_pillPrefabs;
	private List<ScrollRectItemData<D>> m_itemData;
	private Vector2[] m_pillPosition;
	private List<List<T>> m_pills;
	private List<int> m_usedPills;
 
	protected Vector2 m_visibleAreaSize = GameConstants.Vector2.zero;
	protected Vector2 m_containerSize = GameConstants.Vector2.zero;

	private int m_itemCount = 0;
	private int m_firstVisibleItemIndex = 0;

	private Vector2 m_lastPosition;

	private bool m_isAutoScrolling;
	private Vector2 m_autoScrollVelocity;
	private Vector2 m_targetPosition;


	//------------------------------------------------------------------------//
	// QUERIES      														  //
	//------------------------------------------------------------------------//
	protected float Top() 		{ return (m_containerSize.y * 0.5f) - (m_padding.top); }
	protected float Bottom() 	{ return (m_containerSize.y * 0.5f) - (m_visibleAreaSize.y - m_padding.bottom); }
	protected float Left() 		{ return (m_containerSize.x * 0.5f) - (m_padding.left);	}
	protected float Right() 	{ return (m_containerSize.x * 0.5f) - (m_visibleAreaSize.x - m_padding.right); }

	protected Vector2 GetPillPosition(int _index) { return (m_containerSize * 0.5f) - m_pillPosition[_index];  }



	//------------------------------------------------------------------------//
	// METHODS      														  //
	//------------------------------------------------------------------------//
	protected override void OnEnable() {
		base.OnEnable();
		onValueChanged.AddListener(OnValueChanged);
	}

	protected override void OnDisable() {
		base.OnDisable();
		onValueChanged.RemoveListener(OnValueChanged);
	}

	/// <summary>
	/// Empty and destroy everything.
	/// </summary>
	public void Clear() {
		m_itemData.Clear();
		m_pillPrefabs.Clear();
		m_usedPills.Clear();

		for (int i = 0; i < m_pills.Count; ++i) {
			for (int j = 0; j < m_pills.Count; ++j) {
				GameObject.Destroy(m_pills[i][j].gameObject);
			}
			m_pills[i].Clear();
		}
		m_pills.Clear();

		content.sizeDelta = GameConstants.Vector2.zero;
	}


	/// <summary>
	/// Configure this scroll rect. 
	/// </summary>
	/// <param name="_pillPrefabs">All the prefabs used in this scroll rect.</param>
	/// <param name="_itemData">All the items inside the scroll. Each item specifies which pill type it needs.</param>
	public void Setup(List<GameObject> _pillPrefabs, List<ScrollRectItemData<D>> _itemData) {
		m_pillPrefabs = _pillPrefabs;
		m_itemData = _itemData;

		m_visibleAreaSize.x = viewRect.rect.width;
		m_visibleAreaSize.y = viewRect.rect.height;

		//
		// Create the smallest 
		if (m_pills == null) {
			m_pills = new List<List<T>>();
			m_usedPills = new List<int>();

			for (int i = 0; i < m_pillPrefabs.Count; ++i) {				
				m_pills.Add(new List<T>());
				m_usedPills.Add(0);

				CreatePillOfType(i);
			}
		}

		for (int i = 0; i < m_usedPills.Count; ++i) {
			m_usedPills[i] = 0;
		}

		//
		// Resize container
		m_itemCount = m_itemData.Count;
		m_pillPosition = new Vector2[m_itemCount];
		m_containerSize = GameConstants.Vector2.zero;

		if (vertical) {
			m_containerSize.y = m_padding.top;
			for (int i = 0; i < m_itemCount; ++i) {
				T pill = m_pills[m_itemData[i].pillType][0];
				m_containerSize.y += pill.size.y * 0.5f;
				m_pillPosition[i] = m_containerSize;
				m_containerSize.y += pill.size.y * 0.5f + m_padding.spacing;
			}
			m_containerSize.y += m_padding.bottom;
		}

		if (horizontal) {
			m_containerSize.x = m_padding.left;
			for (int i = 0; i < m_itemCount; ++i) {
				T pill = m_pills[m_itemData[i].pillType][0];
				m_containerSize.x += pill.size.x * 0.5f;
				m_pillPosition[i] = m_containerSize;
				m_containerSize.x += pill.size.x * 0.5f + m_padding.spacing;
			}
			m_containerSize.x += m_padding.right;
		}

		content.sizeDelta = m_containerSize;


		//
		m_lastPosition = content.anchoredPosition;

		m_isAutoScrolling = false;
		m_autoScrollVelocity = GameConstants.Vector2.zero;
		m_firstVisibleItemIndex = 0;
		ShowPillsFrom(m_firstVisibleItemIndex);
	}

	private void CreatePillOfType(int _type) {
		GameObject instance = GameObject.Instantiate<GameObject>(m_pillPrefabs[_type], content, false);
		T pill = instance.GetComponent<T>();
		pill.ComputeSize();
		m_pills[_type].Add(pill);

		OnPillCreated();
	}

	/// <summary>
	/// Draw all visible items.
	/// </summary>
	/// <param name="_index">First visible item index.</param>
	private void ShowPillsFrom(int _index) {	
		// Ignore if there is no data
		if(m_itemData == null) return;
		if(m_itemData.Count == 0) return;
		_index = Mathf.Clamp(_index, 0, m_itemData.Count - 1);

		float screenPos = 0;
		float screenLimit = 0;
		float pillSize = 0;
		int pillType = m_itemData[_index].pillType;
		int itemIndex = _index;

		if (vertical) {
			screenPos = m_pillPosition[itemIndex].y - content.anchoredPosition.y;
			screenLimit = m_visibleAreaSize.y - m_padding.bottom;
		}

		if (horizontal) {
			screenPos = m_pillPosition[itemIndex].x - content.anchoredPosition.x;
			screenLimit = m_visibleAreaSize.x - m_padding.right;
		}

		//reset used pills counter
		for (int i = 0; i < m_usedPills.Count; ++i) {
			m_usedPills[i] = 0;
		}

		//draw all the pills visible inside the viewport
		do {
			//if we don't have enough pills of one type, create them
			if (m_usedPills[pillType] >= m_pills[pillType].Count) {
				CreatePillOfType(pillType);
			}

			T pill = m_pills[pillType][m_usedPills[pillType]];
			pill.InitWithData(m_itemData[itemIndex].data);
			pill.SetPosition(GetPillPosition(itemIndex));
			pill.gameObject.SetActive(true);

			m_usedPills[pillType]++;

			//Next pill. We will check if it fits inside the viewport.
			itemIndex++;
			if (itemIndex >= m_itemCount) {
				break;
			} else {
				pillType = m_itemData[itemIndex].pillType;

				if (vertical) {
					screenPos = m_pillPosition[itemIndex].y - content.anchoredPosition.y;
					pillSize = m_pills[pillType][0].size.y;
				}
				if (horizontal) {
					screenPos = m_pillPosition[itemIndex].x - content.anchoredPosition.x;
					pillSize = m_pills[pillType][0].size.x;
				}
			}
		} while (screenPos < screenLimit + pillSize);

		// Disable unused pills 
		for (int i = 0; i < m_usedPills.Count; ++i) {
			for (int j = m_usedPills[i]; j < m_pills[i].Count; ++j) {
				m_pills[i][j].gameObject.SetActive(false);
			}
		}

		m_firstVisibleItemIndex = _index;
		OnShowPillsFrom(_index);
	}

	//
	protected override void LateUpdate() {
		base.LateUpdate();
		if (m_isAutoScrolling) {			
			content.anchoredPosition = Vector2.SmoothDamp(content.anchoredPosition, m_targetPosition, ref m_autoScrollVelocity, m_autoScrollTime, 10000f, Time.deltaTime);

			// Stop moving at some point!
			if(m_autoScrollVelocity.sqrMagnitude < 0.01f) {
				m_isAutoScrolling = false;
			}
		}
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	public override void OnBeginDrag(PointerEventData _eventData) {
		base.OnBeginDrag(_eventData);
		m_isAutoScrolling = false;
	}

	/// <summary>
	/// The scroll position has changed, lets see which is the first item visible.
	/// </summary>
	/// <param name="_position">Scroll position.</param>
	private void OnValueChanged(Vector2 _position) {
		m_lastPosition = _position;

		Vector2 deltaMove = _position - m_lastPosition;
		deltaMove.Normalize();

		if (deltaMove.magnitude < 10f) {
			Vector2 anchoredPos = content.anchoredPosition; //it is more accurate to use the anchor pos than the transform pos.
			Vector2 relativePos = GameConstants.Vector2.zero;
			int startIndex = 0;

			for (int i = 0; i < m_itemCount; ++i) {
				T pill = m_pills[m_itemData[i].pillType][0];
				relativePos = m_pillPosition[i] - content.anchoredPosition;
				if (vertical && relativePos.y >= -pill.size.y) {
					startIndex = i;
					break;
				}
				if (horizontal && relativePos.x >= -pill.size.x) {
					startIndex = i;
					break;
				}
			}

			ShowPillsFrom(startIndex);
		}

		OnScrollMoved();
	}

	/// <summary>
	/// Auto scroll to selected item. This item will be placed at the top of the viewport.
	/// </summary>
	/// <param name="_index">Item index.</param>
	public void FocusOn(int _index, bool _animate) {
		T pill = m_pills[m_itemData[_index].pillType][0];

		m_targetPosition = content.anchoredPosition;
		m_targetPosition.y = m_pillPosition[_index].y - m_padding.top - m_visibleAreaSize.y/2f;

		if (_animate) {
			m_isAutoScrolling = true;
		} else {
			content.anchoredPosition = m_targetPosition;
			m_isAutoScrolling = false;
		}
	}


	protected virtual void OnShowPillsFrom(int _index) {}
	protected virtual void OnScrollMoved() {}
	protected virtual void OnPillCreated() {}
}
