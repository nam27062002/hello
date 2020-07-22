// DragonWizardXML.cs
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
    readonly List<string> skin = new List<string>() { string.Empty };

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
            skin.Add(string.Empty);
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
        ProcessXML(DRAGON_DEFINITIONS_PATH, sku);
        ProcessXML(DRAGON_PROGRESSION_DEFINITIONS_PATH, sku + "_progression", new KeyValuePair<string, string>("dragonSku", sku));
        for (int i = 0; i < skin.Count; i++)
        {
            ProcessXML(DISGUISES_DEFINITIONS_PATH, skin[i], new KeyValuePair<string, string>("dragonSku", sku));
        }

        ProcessXML(DAILY_REWARDS_DRAGON_MODIFIERS_DEFINITIONS_PATH, sku + "_reward", new KeyValuePair<string, string>("dragonSku", sku));
        ProcessXML(MISSION_DRAGON_MODIFIERS_DEFINITIONS_PATH, sku + "_mission", new KeyValuePair<string, string>("dragonSku", sku));

        EditorUtility.DisplayDialog("Process finished", "Dragon " + sku + " added to XML files", "Close");
    }

    void ProcessXML(string xmlPath, string newSku, params KeyValuePair<string, string>[] extraAttributes)
    {
        // Load XML file
        XmlDocument doc = new XmlDocument();
        doc.Load(xmlPath);

        // Select XML node
        XmlNodeList nodeList = doc.SelectNodes("Definitions");
        XmlNode lastChildNode = nodeList[nodeList.Count - 1].LastChild;

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
        doc.Save(xmlPath);
    }
}
