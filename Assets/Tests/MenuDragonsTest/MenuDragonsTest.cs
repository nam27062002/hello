// MenuDragonsTest.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 17/07/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
public class MenuDragonsTest : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	public PathFollower m_cameraPathFollower = null;
	public MenuDragonPreview[] m_dragons = new MenuDragonPreview[0];

	[Space]
	public Range m_scaleRange = new Range(1f, 3f);
	[Comment("X: [0..1] representing dragons sequence delta" + "\n" +
			 "Y: [0..1] representing value to be applied to the Scale Range property")]
	public AnimationCurve m_scaleCurve = new AnimationCurve();

	[Space]
	[Comment("Ref size should be the size in screen of the smallest dragon at scale 1")]
	public Vector2 m_gridRefSize = Vector2.zero;
	public Gradient m_gridGradient = new Gradient();
	public RectTransform[] m_grids = new RectTransform[0];

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// First update call.
	/// </summary>
	public void Start() {
		// Make sure content is initialized
		if(!ContentManager.ready) {
			ContentManager.InitContent(true, false);
		}
		HDAddressablesManager.Instance.Initialize();

		// Reload dragons using a dragon loader to see them textured and animated
		for(int i = 0; i < m_dragons.Length; ++i) {
			// Grab parent
			MenuDragonPreview oldDragon = m_dragons[i];
			Transform parent = oldDragon.transform.parent;

			// Add MenuDragonLoader component to parent
			MenuDragonLoader loader = parent.gameObject.AddComponent<MenuDragonLoader>();
			loader.Setup(MenuDragonLoader.Mode.MANUAL, MenuDragonPreview.Anim.IDLE, true);
			loader.keepLayers = true;

			// Load dragon - this will destroy old dragon instance, so store any needed values
			Vector3 targetScale = oldDragon.transform.localScale;
			loader.LoadDragon(oldDragon.sku);
			MenuDragonPreview newDragon = loader.dragonInstance;

			// Apply placeholder's scale (might have not yet been saved to the dragon's prefab)
			newDragon.transform.localScale = targetScale;

			// Replace dragon instance in the array
			m_dragons[i] = newDragon;
		}
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Scroll camera to previous dragon.
	/// </summary>
	public void FocusPreviousDragon() {
		if(m_cameraPathFollower.snapPoint == 0) {
			m_cameraPathFollower.snapPoint = m_cameraPathFollower.path.pointCount - 1;
		} else {
			m_cameraPathFollower.snapPoint = m_cameraPathFollower.snapPoint - 1;
		}
	}

	/// <summary>
	/// Scroll camera to next dragon.
	/// </summary>
	public void FocusNextDragon() {
		if(m_cameraPathFollower.snapPoint == m_cameraPathFollower.path.pointCount - 1) {
			m_cameraPathFollower.snapPoint = 0;
		} else {
			m_cameraPathFollower.snapPoint = m_cameraPathFollower.snapPoint + 1;
		}
	}

	/// <summary>
	/// Apply scale curve to all dragons.
	/// </summary>
	public void ApplyCurve() {
		// Apply to dragons
		int numDragons = m_dragons.Length;
		for(int i = 0; i < numDragons; ++i) {
			// Do the math
			float delta = (float)i/(float)(numDragons - 1);
			float scaleDelta = m_scaleCurve.Evaluate(delta);
			float scale = m_scaleRange.Lerp(scaleDelta);

			// Apply to dragon
			m_dragons[i].transform.SetLocalScale(scale);

			// Update matching grid
			if(i < m_grids.Length) {
				m_grids[i].sizeDelta = m_gridRefSize * scale;
				m_grids[i].GetComponent<UIColorFX>().colorMultiply = m_gridGradient.Evaluate(scaleDelta);	// Use scale delta to follow the curve
			}
		}
	}

	/// <summary>
	/// Reset scales of both slots and dragon instances to 1.
	/// </summary>
	public void ResetScales() {
		for(int i = 0; i < m_dragons.Length; ++i) {
			// Apply to dragon
			m_dragons[i].transform.SetLocalScale(1f);

			// Update matching grid
			if(i < m_grids.Length) {
				m_grids[i].sizeDelta = m_gridRefSize;
				m_grids[i].GetComponent<UIColorFX>().colorMultiply = m_gridGradient.Evaluate(0f);   // Use scale delta to follow the curve
			}
		}
	}
}