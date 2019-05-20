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
    [SerializeField] private Image m_image;

    [Tooltip("The model shown in case of 3d icon")]
    [SerializeField] private UI3DAddressablesLoader m_3dModel;

    [SerializeField] private string m_iconSKU;



    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Initialization
    /// </summary>
    private void Awake() {

        // Just in case
        if (m_image == null)
        {
            Debug.LogWarning("2d icon entity not defined. Trying to find it in children.");
            m_image = GetComponentInChildren<Image>();
        }

        if (m_3dModel == null)
        {
            Debug.LogWarning("3d icon entity not defined explicitely. Trying to find it in children.");
            m_3dModel = GetComponentInChildren<UI3DAddressablesLoader>();
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
            Debug.LogError("Icon definition not found for sku " + _iconSku + " in IconDefinitions.xml");

            m_image.gameObject.SetActive(false);
            m_3dModel.gameObject.SetActive(false);

            return;
        }

        // Check if the icon is an image or a 3d model
        if (iconDef.GetAsBool("icon3d"))
        {
            // Would be nice to use nested prefabs instead
            // but until Unity v.2018 this is what we have

            // Icon is a 3d Model
            m_image.gameObject.SetActive(false);
            m_3dModel.gameObject.SetActive(true);

            // Load the 3d model asynchronously
            m_3dModel.LoadAsync(iconDef.GetAsString("asset"));

        }
        else
        {

            // Icon is a sprite
            m_image.gameObject.SetActive(true);
            m_3dModel.gameObject.SetActive(false);

            // Load the sprite. The sprite name is the icon definicion sku
            m_image.sprite = Resources.Load<Sprite>(UIConstants.MISSION_ICONS_PATH + iconDef.GetAsString("asset"));

        }
    }


}