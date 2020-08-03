using System;
using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class GridLayoutCustom : MonoBehaviour {

	[Serializable]
	private struct Padding {
		public float top;
		public float right;
		public float bottom;
		public float left;
	};

	[SerializeField] private Padding m_padding;
	[SerializeField] private Vector2 m_itemSize;

	private RectTransform[] m_items;

	// Use this for initialization
	void Start () {
		SetupLayout();
	}
	
	// Update is called once per frame
	void Update () {
		if (transform.hasChanged) {
			SetupLayout();
		}
	}

	private void SetupLayout() {
		int childCount = transform.childCount;

		if (m_items == null || m_items.Length != childCount) {
			m_items = new RectTransform[childCount];
			for (int i = 0; i < childCount; i++) {
				m_items[i] = transform.GetChild(i) as RectTransform;
			}
		}

		if (childCount > 0) {
			RectTransform parentTransform = transform as RectTransform;
			float width = parentTransform.rect.width;
			float height = parentTransform.rect.height;

			//how many elements can we put between columns and rows?
			float availableWidth = width - m_padding.left - m_padding.right;
			float availableHeight = height - m_padding.top - m_padding.bottom;

			int columns = Mathf.FloorToInt(availableWidth / m_itemSize.x);
			int rows = Mathf.FloorToInt(availableHeight / m_itemSize.y);

			float offsetX = 0;
			if (columns > 1) offsetX = (availableWidth - (m_itemSize.x * columns)) / (columns - 1);

			float offsetY = 0;
			if (rows > 1) offsetY = (availableHeight - (m_itemSize.y * rows)) / (rows - 1);

			int r = 0;
			int c = 0;
			for (int i = 0; i < childCount; i++) {
				RectTransform itemTransform = m_items[i];
				Vector3 pos = Vector3.zero;

				// compute local position
				pos.x = (m_padding.left + c * (offsetX + m_itemSize.x));
				pos.y = (m_padding.top - r * (offsetY + m_itemSize.y));

				// offset based on item pivot
				pos.x += (itemTransform.pivot.x * m_itemSize.x);
				pos.y -= ((1f - itemTransform.pivot.y) * m_itemSize.y);

				// offest based on parent pivot
				pos.x -= (parentTransform.pivot.x * parentTransform.rect.width);
				pos.y += (parentTransform.pivot.y * parentTransform.rect.height);

				// update item position
				itemTransform.localPosition = pos;

				c++;
				if (c == columns) {
					c = 0;
					r++;
				}
			}
		}
	}
}
