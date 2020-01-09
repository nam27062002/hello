// BaseIcon.cs
// Hungry Dragon
// 
// Created by J.M.Olea on 20/05/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// This component loads an icon tha can be a 3d model, or a image.
/// In case of an image, it just enables the image entity. In case
/// of a 3d icon it enables the AddressableLoader entity that will retrieve
/// asynchronously the 3d model from the asset bundles catalog.
/// The type of the icon is defined in the iconDefinitions.xml file.
/// </summary>
public class BaseIcon : MonoBehaviour {
    //------------------------------------------------------------------------//
    // CONSTANTS															  //
    //------------------------------------------------------------------------//

    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//
    [Tooltip("The image shown in case of 2d icon")]
    [SerializeField] private Image m_2dIcon;

    [Tooltip("The model shown in case of 3d icon")]
    [SerializeField] private UI3DAddressablesLoader m_3dIcon;

    [Space(10)]

    [Tooltip("Image shown when no icon 2d/3d is found")]
    [SerializeField] private Image m_defaultImage;

    [Space(10)]

    [SerializeField] private string m_iconSKU;

    [Space(10)]
    [Comment("If render queue is zero, ignore it")]
    [SerializeField] private int m_renderQueue = 0;

    // Keep a record of the icon type
    private bool is3DIcon;
    public bool Is3DIcon
    { get { return is3DIcon; } }

    // Keep a record of the load state
    private bool isLoadFinished;
    public bool IsLoadFinished
    { get { return isLoadFinished; } }

	// Events
	public UnityEvent OnLoadFinished = new UnityEvent();

    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Initialization
    /// </summary>
    private void Awake() {

        // Just in case
        if (m_2dIcon == null && m_3dIcon == null)
        {

            Debug.LogError("The BaseIcon has no 2d or 3d icon field defined");
            
        }
    }

    public void LoadIcon (string _iconSku)
    {
        // Keep a record for debugging purposes in the inspector
        m_iconSKU = _iconSku;

        // Get the icon definition.  The prefab name is the icon definicion sku
        DefinitionNode iconDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.ICONS, _iconSku);

        if (iconDef == null)
        {
            // SKU not found in IconDefinitions.xml
            Debug.LogError("Icon definition not found for sku " + _iconSku + " in IconDefinitions.xml.");

            // Not showing anything
            if (m_2dIcon != null) { m_2dIcon.gameObject.SetActive(false); }
            if (m_3dIcon != null) { m_3dIcon.gameObject.SetActive(false); }
            if (m_defaultImage != null) { m_defaultImage.gameObject.SetActive(false); }

            return;
        }

        is3DIcon = iconDef.GetAsBool("icon3d");

        // Check if the icon is an image or a 3d model
        if ( ! is3DIcon )
        {

            // Icon is a sprite
            if (m_3dIcon != null) { m_3dIcon.gameObject.SetActive(false); }
            if (m_defaultImage != null) { m_defaultImage.gameObject.SetActive(false); }

            if (m_2dIcon != null)
            {
                m_2dIcon.gameObject.SetActive(true);

                // Load the sprite. The sprite name is the icon definition sku
                m_2dIcon.sprite = Resources.Load<Sprite>(UIConstants.MISSION_ICONS_PATH + iconDef.GetAsString("asset"));

				isLoadFinished = true;

                ApplyRenderQueueSettings();

				OnLoadFinished.Invoke();
            }
            else
            {
                Debug.LogError("The 2d icon is needed here, but it wasn't defined. ");
                return;
            }

        }
        else
        {
            // Would be nice to use nested prefabs instead
            // but until Unity v.2018 this is what we have

            // Icon is a 3d Model
            if (m_3dIcon == null)
            {
                Debug.LogError("The 3d icon is needed here, but it wasn't defined. ");
                return;
            }
            m_3dIcon.gameObject.SetActive(true);

            if (m_2dIcon != null) { m_2dIcon.gameObject.SetActive(false); }
            if (m_defaultImage != null) { m_defaultImage.gameObject.SetActive(false); }
                                 
            
            string assetId = iconDef.GetAsString("asset");

            // Check if the asset is available
            if (HDAddressablesManager.Instance.IsResourceAvailable(assetId) )
            {

				// Load the 3d model asynchronously
				isLoadFinished = false;
                AddressablesOp op = m_3dIcon.LoadAsync(iconDef.GetAsString("asset"));
                op.OnDone = OnAddressablesLoadFinished;

            }
            else
            {

                // Asset not available. Show the default icon
                if (m_defaultImage == null)
                {
                    Debug.LogError("The default icon is needed here, but it wasn't defined. Not showing any icon then.");
                }
                else
                {
                    ForceShowDefaultIcon();
                }

            }
                       
        }

    }


    /// <summary>
    /// Hide 2d and 3d elements, and just enable the default image
    /// We can use this function if we reach a timeout, but the icon is not 
    /// ready and we need to show something in the screen.
    /// </summary>
    public void ForceShowDefaultIcon ()
    {

        if (m_defaultImage == null)
        {
            Debug.LogError("Trying to show the default icon, but it is not defined");
            return;
        }

        m_defaultImage.gameObject.SetActive(true);

        // Hide everything else
        if (m_2dIcon != null) { m_2dIcon.gameObject.SetActive(false); }
        if (m_3dIcon != null) { m_3dIcon.gameObject.SetActive(false); }
        
    }

    // Asynchronous load callback when the asset is successfully loaded
    private void OnAddressablesLoadFinished(AddressablesOp op)
    {

        isLoadFinished = true;

        ApplyRenderQueueSettings();

        OnLoadFinished.Invoke();
	}


    /// <summary>
    /// Look for all the children renderers, and modify their render queue
    /// </summary>
    private void ApplyRenderQueueSettings()
    {
        if (m_renderQueue != 0)
        {
            if (Is3DIcon)
            {
                // Modify the render queue of the 3d children elements
                RenderQueueSetter.Apply(m_3dIcon.transform, m_renderQueue, true, false);
            }
            else
            {
                // Modify the render queue of the 2d children elements
                RenderQueueSetter.Apply(m_2dIcon.transform, m_renderQueue, false,true);
            }
        }
    }

}