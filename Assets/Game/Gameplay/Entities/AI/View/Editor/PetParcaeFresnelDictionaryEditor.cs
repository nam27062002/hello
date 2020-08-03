// RewardTypeSetupDictionaryEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 26/07/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom editor for the MonoBehaviourTemplate class.
/// </summary>
[CustomPropertyDrawer(typeof(PetParcaeViewControl.PetParcaeFresnelDictionary), true)]
public class PetParcaeFresnelDictionaryEditor : SerializableDictionaryEditor { }