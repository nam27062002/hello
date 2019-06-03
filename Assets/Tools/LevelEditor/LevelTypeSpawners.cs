// LevelTypeSpawners.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 21/12/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
namespace LevelEditor {
	/// <summary>
	/// Specialization of a level.
	/// </summary>
	[ExecuteInEditMode]
	public class LevelTypeSpawners : Level {
		//------------------------------------------------------------------//
		// CONSTANTS														//
		//------------------------------------------------------------------//
		public static readonly string DRAGON_SPAWN_POINT_NAME = "DragonSpawn";	// Concatenate DragonId for specific spawn points
		public static readonly string LEVEL_EDITOR_SPAWN_POINT_NAME = "LevelEditor";
		private static readonly string DRAGON_SPAWN_POINTS_CONTAINER_NAME = "DragonSpawnPoints";

		//------------------------------------------------------------------//
		// GENERIC METHODS													//
		//------------------------------------------------------------------//
		[Serializable]
		public class SpawnData
		{
			public string m_dragonSku;
			public string m_prefabName;	

			public SpawnData( string dragon, string prefab ){
				m_dragonSku = dragon;
				m_prefabName = prefab;
			}
		}
		public List<SpawnData> m_spawnsData = new List<SpawnData>();
        private GameObject m_spawnerRoot;
        private int m_spawnersCount;

		/// <summary>
		/// Initialization.
		/// </summary>
		override protected void Awake() {
			if (Application.isPlaying) {
				// Call parent
				base.Awake();

				// TODO(miguel): improve this
				// only if loading for the first time
				GameSceneController gameController = InstanceManager.gameSceneController;
				if ( gameController != null && gameController.state <= GameSceneController.EStates.COUNTDOWN )
				{
					// Create default point if not already done
					if ( InstanceManager.player != null ){
						string sku = InstanceManager.player.data.def.sku;
						IntroSpawn( sku );
					}else{
						GetDragonSpawnPoint("", true);
					}
				}
			}
		}

		public void IntroSpawn( string sku )
		{
			GameObject go = GetDragonSpawnPoint(sku, false);
			// Spawn here eating entity
			/*
			for( int i = 0; i<m_spawnsData.Count; i++ ){
				if ( m_spawnsData[i].m_dragonSku == sku ){
					if ( !string.IsNullOrEmpty( m_spawnsData[i].m_prefabName ) ){
						InstantiateSpawner( go, m_spawnsData[i].m_prefabName );
					}
				}
			}
			*/
			InstantiateSpawner( go, "IntroSpawners/SP_Intro" );            
		}


		void InstantiateSpawner( GameObject root, string spawner){
			GameObject sp = (GameObject)Resources.Load( spawner );
            m_spawnerRoot = GameObject.Instantiate(sp, root.transform) as GameObject;
            if (m_spawnerRoot != null) {
                List<ISpawner> spawnersToSpawn = SpawnerManager.instance.m_spawning;

                Spawner[] spawners = m_spawnerRoot.GetComponentsInChildren<Spawner>();
                int i;
                m_spawnersCount = spawners.Length;
                for (i = 0; i < m_spawnersCount; i++) {
                    spawners[i].OnDone = OnSpawnerDone;
                    
                    if (spawnersToSpawn != null && !spawnersToSpawn.Contains(spawners[i]))
                    {
                        //add item into respawn stack and begin the respawn process
                        spawnersToSpawn.Add(spawners[i]);
                        spawners[i].Respawn();
                    }
                }

                m_spawnerRoot.transform.localPosition = Vector3.zero;
            }
		}

        private void OnSpawnerDone(AbstractSpawner spawner) {            
            m_spawnersCount--;
            if (m_spawnersCount == 0 && m_spawnerRoot != null) {
                Destroy(m_spawnerRoot);
                m_spawnerRoot = null;
            }
        }

		//------------------------------------------------------------------//
		// OTHER METHODS													//
		//------------------------------------------------------------------//
		/// <summary>
		/// Finds and returns the spawn point linked to a specific dragon in this level.
		/// </summary>
		/// <returns>The dragon spawn point.</returns>
		/// <param name="_sku">Sku of the dragon whose spawn point we're looking for.</param>
		/// <param name="_createItIfNotFound">If set to <c>true</c> _create it if not found.</param>
		public GameObject GetDragonSpawnPoint(string _sku = "", bool _createItIfNotFound = false, bool _returnDefaultIfNotFound = true) {
			// Generate game object name for this dragon
			string name = DRAGON_SPAWN_POINT_NAME;
			if(!string.IsNullOrEmpty(_sku)) name += "_" + _sku.ToString();

			// Does the level have a spawn point for this dragon?
			GameObject spawnPointObj = gameObject.FindObjectRecursive(name);
			if(spawnPointObj == null) {
				if (_createItIfNotFound)
				{
					// No! Create one!
					// Get the spawners container object (or create it if not found)
					GameObject spawnContainerObj = gameObject.FindObjectRecursive(DRAGON_SPAWN_POINTS_CONTAINER_NAME);
					if(spawnContainerObj == null) {
						spawnContainerObj = new GameObject(DRAGON_SPAWN_POINTS_CONTAINER_NAME);
						spawnContainerObj.transform.SetParent(this.gameObject.transform, true);
					}

					// Now we can create the spawn point for that dragon
					spawnPointObj = new GameObject(name);
					spawnPointObj.transform.position = new Vector3(0, 20, 0);
					spawnPointObj.transform.SetParent(spawnContainerObj.transform, true);
				}
				else if (_returnDefaultIfNotFound)
				{
					spawnPointObj = gameObject.FindObjectRecursive(DRAGON_SPAWN_POINT_NAME);
				}
			}

			return spawnPointObj;
		}
	}
}

