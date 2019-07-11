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
	public int pillType;
	private RectTransform m_rt;

	public abstract void InitWithData(D _data);
	public abstract void Animate(int _index);

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

	private List<T> m_pillsReference; // query for sizes
	private List<T> m_slotsOfPills;
	private List<Stack<T>> m_poolsOfPills;
 
	protected Vector2 m_visibleAreaSize = GameConstants.Vector2.zero;
	protected Vector2 m_containerSize = GameConstants.Vector2.zero;

	private int m_itemCount = 0;
	private int m_firstVisibleItemIndex = 0;

	private Vector2 m_lastPosition;

	private int m_focusToPillIndex;
	private bool m_isAutoScrolling;
	private Vector2 m_autoScrollVelocity;
	private Vector2 m_targetPosition;


	//------------------------------------------------------------------------//
	// QUERIES      														  //
	//------------------------------------------------------------------------//
	protected float Top() 		{ return (m_containerSize.y * 0.5f) - (m_padding.top); }
	protected float Bottom() 	{ return (m_containerSize.y * 0.5f) - (m_visibleAreaSize.y - m_padding.bottom); }
	protected float Left() 		{ return (m_padding.left) - (m_containerSize.x * 0.5f);	}
	protected float Right() 	{ return (m_visibleAreaSize.x - m_padding.right) - (m_containerSize.x * 0.5f); }

	protected Vector2 GetPillPosition(int _index) { 
		Vector2 ret = GameConstants.Vector2.zero;
		ret.y = (m_containerSize.y * 0.5f) - m_pillPosition[_index].y;
		ret.x = m_pillPosition[_index].x - (m_containerSize.x * 0.5f);
		return ret;
	}



	//------------------------------------------------------------------------//
	// METHODS      														  //
	//------------------------------------------------------------------------//
	protected override void OnEnable() {
		base.OnEnable();
		onValueChanged.AddListener(OnValueChanged);
	}

	protected override void OnDisable() {
		base.OnDisable();

		// return all used pills
		if (m_slotsOfPills != null) {
			for (int i = 0; i < m_slotsOfPills.Count; ++i) {
				ReturnPillToPool(i);
			}
		}

		onValueChanged.RemoveListener(OnValueChanged);
	}

	/// <summary>
	/// Empty and destroy everything.
	/// </summary>
	public void Clear() {
		m_itemData.Clear();
		m_pillPrefabs.Clear();
		m_slotsOfPills.Clear();

		for (int i = 0; i < m_poolsOfPills.Count; ++i) {
			while (m_poolsOfPills[i].Count > 0) {
				GameObject.Destroy(m_poolsOfPills[i].Pop().gameObject);
			}
		}
		m_poolsOfPills.Clear();

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
		m_itemCount = m_itemData.Count;

		m_visibleAreaSize.x = viewRect.rect.width;
		m_visibleAreaSize.y = viewRect.rect.height;

		//
		// Create the smallest 
		if (m_poolsOfPills == null || m_poolsOfPills.Count == 0) {
			m_poolsOfPills = new List<Stack<T>>();
			m_pillsReference = new List<T>();

			for (int i = 0; i < m_pillPrefabs.Count; ++i) {				
				m_poolsOfPills.Add(new Stack<T>());
				CreatePillOfType(i);
				m_pillsReference.Add(m_poolsOfPills[i].Peek());
			}

			m_slotsOfPills = new List<T>();
			for (int i = 0; i < m_itemCount; ++i) {
				m_slotsOfPills.Add(null);
			}
		} else  {
			// return all used pills
			for (int i = 0; i < m_slotsOfPills.Count; ++i) {
				ReturnPillToPool(i);
			}

			// maybe we have more items now
			for (int i = m_slotsOfPills.Count; i < m_itemCount; ++i) {
				m_slotsOfPills.Add(null);
			}
		}

		//
		// Resize container
		m_pillPosition = new Vector2[m_itemCount];
		m_containerSize = GameConstants.Vector2.zero;

		if (vertical) {
			m_containerSize.y = m_padding.top;
			for (int i = 0; i < m_itemCount; ++i) {
				T pill = m_pillsReference[m_itemData[i].pillType];
				m_containerSize.y += pill.size.y * 0.5f;
				m_pillPosition[i] = m_containerSize;
				m_containerSize.y += pill.size.y * 0.5f + m_padding.spacing;
			}
			m_containerSize.y += m_padding.bottom;
		}

		if (horizontal) {
			m_containerSize.x = m_padding.left;
			for (int i = 0; i < m_itemCount; ++i) {
				T pill = m_pillsReference[m_itemData[i].pillType];
				m_containerSize.x += pill.size.x * 0.5f;
				m_pillPosition[i] = m_containerSize;
				m_containerSize.x += pill.size.x * 0.5f + m_padding.spacing;
			}
			m_containerSize.x += m_padding.right;
		}

		content.anchoredPosition = GameConstants.Vector2.zero;
		content.sizeDelta = m_containerSize;

		//
		m_lastPosition = content.anchoredPosition;

		m_isAutoScrolling = false;
		m_focusToPillIndex = -1;
		m_autoScrollVelocity = GameConstants.Vector2.zero;
		m_firstVisibleItemIndex = 0;

		ShowPillsFrom(m_firstVisibleItemIndex);
	}

	private void CreatePillOfType(int _type) {
		GameObject prefab = m_pillPrefabs[_type];
		GameObject instance = GameObject.Instantiate<GameObject>(prefab, content, false);
		instance.SetActive(false);
		T pill = instance.GetComponent<T>();
		pill.pillType = _type;
		pill.ComputeSize();
		instance.SetUniqueName(prefab.name);
		m_poolsOfPills[_type].Push(pill);

		OnPillCreated(pill);
	}

	private void ReturnPillToPool(int _itemIndex) {
		T pill = m_slotsOfPills[_itemIndex];
		if (pill != null) {
			pill.gameObject.SetActive(false);
			m_poolsOfPills[pill.pillType].Push(pill);
			m_slotsOfPills[_itemIndex] = null;
		}
	}

	/// <summary>
	/// Draw all visible items.
	/// </summary>
	/// <param name="_index">First visible item index.</param>
	private void ShowPillsFrom(int _firstItemIndex) {	
		// Ignore if there is no data
		if(m_itemData == null) return;
		if(m_itemData.Count == 0) return;
		_firstItemIndex = Mathf.Clamp(_firstItemIndex, 0, m_itemData.Count - 1);

		float screenPos = 0;
		float screenLimit = 0;
		float pillSize = 0;
		int pillType = m_itemData[_firstItemIndex].pillType;
		int lastItemIndex = _firstItemIndex;

		if (vertical) {
			screenPos = m_pillPosition[lastItemIndex].y - content.anchoredPosition.y;
			screenLimit = m_visibleAreaSize.y - m_padding.bottom;
		}

		if (horizontal) {
			screenPos = content.anchoredPosition.x + m_pillPosition[lastItemIndex].x;
			screenLimit = m_visibleAreaSize.x - m_padding.right;
		}

		//draw all the pills visible inside the viewport
		do {
			//Next pill. We will check if it fits inside the viewport.
			lastItemIndex++;
			if (lastItemIndex >= m_itemCount) {
				break;
			} else {
				pillType = m_itemData[lastItemIndex].pillType;

				if (vertical) {
					screenPos = m_pillPosition[lastItemIndex].y - content.anchoredPosition.y;
					pillSize = m_pillsReference[pillType].size.y;
				}
				if (horizontal) {
					screenPos = m_pillPosition[lastItemIndex].x + content.anchoredPosition.x;
					pillSize = m_pillsReference[pillType].size.x;
				}
			}
		} while (screenPos < screenLimit + pillSize);


		// we have to show pills from _firstIndex to lastItemIndex,
		// and return all the other pills to the pool

		// first, return all the pills not visible
		// left side
		int i = _firstItemIndex - 1;
		while (i >= 0) {			
			ReturnPillToPool(i);
			i--;
		}

		// right side
		i = lastItemIndex + 1;
		while (i < m_itemCount) {
			ReturnPillToPool(i);
			i++;
		}

		// setup new items
		for (i = _firstItemIndex; i <= lastItemIndex && i < m_itemCount; ++i) {
			if (m_slotsOfPills[i] == null) {
				D data = m_itemData[i].data;
				pillType = m_itemData[i].pillType;

				//if we don't have enough pills of one type, create them
				if (m_poolsOfPills[pillType].Count == 0) {
					CreatePillOfType(pillType);
				}

				T pill = m_poolsOfPills[pillType].Pop();

				pill.InitWithData(data);
				pill.SetPosition(GetPillPosition(i));
				pill.gameObject.SetActive(true);

				m_slotsOfPills[i] = pill;
			}
		}

		m_firstVisibleItemIndex = _firstItemIndex;
		OnShowPillsFrom(_firstItemIndex);
	}

	//
	protected override void LateUpdate() {
		base.LateUpdate();
		if (m_isAutoScrolling) {			
			content.anchoredPosition = Vector2.SmoothDamp(content.anchoredPosition, m_targetPosition, ref m_autoScrollVelocity, m_autoScrollTime, 10000f, Time.deltaTime);

			// Stop moving at some point!
            if (m_autoScrollVelocity.sqrMagnitude < 1f) {
				OnFocusFinished(m_slotsOfPills[m_focusToPillIndex]);
				m_focusToPillIndex = -1;
				m_isAutoScrolling = false;
			}
		}
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	public override void OnBeginDrag(PointerEventData _eventData) {
		base.OnBeginDrag(_eventData);
		OnFocusCanceled();
		m_focusToPillIndex = -1;
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
			int startIndex = 0;

			for (int i = 0; i < m_itemCount; ++i) {
				T pill = m_pillsReference[m_itemData[i].pillType];
				if (vertical) {
					float relativeY = m_pillPosition[i].y - content.anchoredPosition.y;
					if (relativeY >= -pill.size.y) {
						startIndex = i;
						break;
					}
				}
				if (horizontal) {
					float relativeX = m_pillPosition[i].x + content.anchoredPosition.x;
					if (relativeX >= -pill.size.x) {
						startIndex = i;
						break;
					}
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
		T pill = m_pillsReference[m_itemData[_index].pillType];

		m_targetPosition = content.anchoredPosition;
		if (vertical) {
			if (m_containerSize.y > m_visibleAreaSize.y) {
				m_targetPosition.y = m_pillPosition[_index].y - m_visibleAreaSize.y/2f;
				if (m_targetPosition.y < 0) m_targetPosition.y = 0;
				if (m_targetPosition.y > (m_containerSize.y - m_visibleAreaSize.y)) m_targetPosition.y = (m_containerSize.y - m_visibleAreaSize.y);
			}
		}
		if (horizontal)	{
			if (m_containerSize.x > m_visibleAreaSize.x) {
				m_targetPosition.x = m_visibleAreaSize.x/2f - m_pillPosition[_index].x;
				if (m_targetPosition.x > 0) m_targetPosition.x = 0;
				if (m_targetPosition.x < -(m_containerSize.x - m_visibleAreaSize.x)) m_targetPosition.x = -(m_containerSize.x - m_visibleAreaSize.x);
			}
		}

		m_focusToPillIndex = _index;

		if (_animate) {
			m_isAutoScrolling = true;
		} else {
			content.anchoredPosition = m_targetPosition;
			OnFocusFinished(m_slotsOfPills[m_focusToPillIndex]);
			m_focusToPillIndex = -1;
			m_isAutoScrolling = false;
		}
	}

	public void AnimateVisiblePills() {
		int i = m_firstVisibleItemIndex;
		while (m_slotsOfPills[i] != null && i >= 0 && i < m_slotsOfPills.Count) {
			m_slotsOfPills[i].Animate(i - m_firstVisibleItemIndex);
			i++;
		}
	}

	protected virtual void OnShowPillsFrom(int _index) {}
	protected virtual void OnScrollMoved() {}
	protected virtual void OnPillCreated(T _pill) {}
	protected virtual void OnFocusFinished(T _pill) {}
	protected virtual void OnFocusCanceled() {}
}
