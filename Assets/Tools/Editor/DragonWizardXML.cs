﻿// DragonWizardXML.cs
// Hungry Dragon
// 
// Created by Jordi Riambau on 22/07/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Xml;

public class DragonWizardXML : EditorWindow
{
    const string BASE_DEFINITIONS_PATH = "Assets/Resources/Rules/";
    const string DRAGON_DEFINITIONS_PATH = BASE_DEFINITIONS_PATH + "dragonDefinitions.xml";
    const string DRAGON_PROGRESSION_DEFINITIONS_PATH = BASE_DEFINITIONS_PATH + "dragonProgressionDefinitions.xml";
    const string DISGUISES_DEFINITIONS_PATH = BASE_DEFINITIONS_PATH + "disguisesDefinitions.xml";
    const string DAILY_REWARDS_DRAGON_MODIFIERS_DEFINITIONS_PATH = BASE_DEFINITIONS_PATH + "dailyRewardsDragonModifiersDefinitions.xml";
    const string MISSION_DRAGON_MODIFIERS_DEFINITIONS_PATH = BASE_DEFINITIONS_PATH + "missionDragonModifiersDefinitions.xml";

    string sku;
    readonly List<string> skin = new List<string>();

    public void Init(string newSku)
    {
        if (string.IsNullOrEmpty(newSku))
            newSku = "dragon_sku";

        sku = newSku;
        skin.Add(newSku + "_0");
    }

    void OnGUI()
    {
        EditorGUILayout.HelpBox("Prepare the required XML files when creating a new dragon", MessageType.Info, true);

        // Dragon sku
        EditorGUILayout.LabelField("Configuration", EditorStyles.boldLabel);
        sku = EditorGUILayout.TextField("Dragon sku:", sku);

        // Skins
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Skins", EditorStyles.boldLabel);
        for (int i = 0; i < skin.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            skin[i] = EditorGUILayout.TextField("Skin " + i + " sku", skin[i]);
            if (GUILayout.Button(" X "))
            {
                skin.RemoveAt(i);
            }
            EditorGUILayout.EndHorizontal();
        }

        // Add new skin
        if (GUILayout.Button("+ Add new skin", GUILayout.Height(20)))
        {
            skin.Add(sku + "_" + skin.Count);
        }

        // Add dragon to XMLs
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(sku) || skin.Count == 0);
        if (GUILayout.Button("Add new dragon to XMLs", GUILayout.Height(40)))
        {
            ProcessXMLs();
        }
        EditorGUI.EndDisabledGroup();
    }

    void ProcessXMLs()
    {
        bool result = ProcessXML(DRAGON_DEFINITIONS_PATH, sku, new KeyValuePair<string, string>("trackingSku", sku));
        if (result)
        {
            ProcessXML(DRAGON_PROGRESSION_DEFINITIONS_PATH, sku + "_progression", new KeyValuePair<string, string>("dragonSku", sku));
            for (int i = 0; i < skin.Count; i++)
            {
                ProcessXML(DISGUISES_DEFINITIONS_PATH, skin[i], new KeyValuePair<string, string>("dragonSku", sku), new KeyValuePair<string, string>("unlockLevel", "0"), new KeyValuePair<string, string>("trackingSku", sku));
            }

            ProcessXML(DAILY_REWARDS_DRAGON_MODIFIERS_DEFINITIONS_PATH, sku + "_reward", new KeyValuePair<string, string>("dragonSku", sku));
            ProcessXML(MISSION_DRAGON_MODIFIERS_DEFINITIONS_PATH, sku + "_mission", new KeyValuePair<string, string>("dragonSku", sku));

            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Process completed", "Dragon " + sku + " added to XML files", "Close");
        }
        else
        {
            EditorUtility.DisplayDialog("Process failed", "Dragon " + sku + " was already added to the XML files", "Close");
        }
    }

    bool ProcessXML(string xmlPath, string newSku, params KeyValuePair<string, string>[] extraAttributes)
    {
        // Load XML file
        XmlDocument doc = new XmlDocument();
        doc.Load(xmlPath);
        
        // Select XML node
        XmlNodeList nodeList = doc.SelectNodes("Definitions");
        XmlNode lastChildNode = nodeList[nodeList.Count - 1].LastChild;

        // Check if the new sku already exists
        XmlNode duplicatedElement = doc.SelectSingleNode("/Definitions/Definition[@sku='" + newSku + "']");
        if (duplicatedElement != null)
        {
            Debug.LogError("Duplicated element " + newSku + " found on: " + xmlPath);
            return false;
        }

        // Create new XML element
        XmlElement xmlElement = doc.CreateElement("Definition");

        // Copy attributes from last XML element
        XmlAttributeCollection attributes = lastChildNode.Attributes;
        foreach (XmlAttribute attribute in attributes)
        {
            xmlElement.SetAttribute(attribute.Name, attribute.Value);
        }

        // Override new attributes
        xmlElement.SetAttribute("sku", newSku);
        if (extraAttributes.Length > 0)
        {
            foreach (KeyValuePair<string, string> attr in extraAttributes)
            {
                xmlElement.SetAttribute(attr.Key, attr.Value);
            }
        }

        // Append new element and save
        doc.DocumentElement.AppendChild(xmlElement);

        // Create XML writer settings
        XmlWriterSettings settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "\t",
            OmitXmlDeclaration = true
        };

        // Save the changes to disk using the XML writer settings
        using (XmlWriter writer = XmlWriter.Create(xmlPath, settings))
        {
            doc.Save(writer);
        }

        return true;
    }
}
