using SimpleJSON;
using System.Collections.Generic;
using UnityEngine;

namespace Downloadables
{
    public class CatalogGroup
    {
        public static float TIME_TO_WAIT_BETWEEN_SAVES = 3f;

        private static string ATT_PERMISSION_REQUESTED = "pr";
        private static string ATT_PERMISSION_GRANTED = "pg";

        private static Disk sm_disk;
        public static void StaticSetup(Disk disk)
        {
            sm_disk = disk;
        }

        private string Id { get; set; }

        public List<string> EntryIds;

        private bool NeedsToSave { get; set; }

        private bool m_permissionOverCarrierRequested;
        public bool PermissionOverCarrierRequested
        {
            get
            {
                return m_permissionOverCarrierRequested;
            }

            set
            {
                m_permissionOverCarrierRequested = value;
                NeedsToSave = true;
            }
        }

        private bool m_permissionOverCarrierGranted;
        public bool PermissionOverCarrierGranted
        {
            get
            {
                return m_permissionOverCarrierGranted;
            }

            set
            {
                m_permissionOverCarrierGranted = value;
                NeedsToSave = true;
            }
        }

        private float m_latestSaveAt;

        /// <summary>
        /// Download priority. Highest priority: 1. The higher this number the lower priority when downloading
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Internal stuff used only to make List.Sort stable
        /// </summary>
        public int Index { get; set; }

        public void Reset()
        {
            EntryIds = null;
            PermissionOverCarrierRequested = false;
            PermissionOverCarrierGranted = false;
            NeedsToSave = false;
            Priority = int.MaxValue;
            m_latestSaveAt = -1;
            Index = -1;
        }

        public void Setup(string id, List<string> entryIds)
        {
            Reset();

            Id = id;
            EntryIds = entryIds;

            Load();
        }

        private void Load()
        {
            Error error;
            bool exists = sm_disk.File_Exists(Disk.EDirectoryId.Groups, Id, out error);            
            if (error == null && exists)
            {
                JSONNode json = sm_disk.File_ReadJSON(Disk.EDirectoryId.Groups, Id, out error);                
                if (error == null)
                {
                    LoadFromJSON(json);
                }
            }
        }

        private void LoadFromJSON(JSONNode data)
        {
            if (data != null)
            {
                m_permissionOverCarrierRequested = GetAttAsBool (data, ATT_PERMISSION_REQUESTED);
                m_permissionOverCarrierGranted = GetAttAsBool(data, ATT_PERMISSION_GRANTED); 
            }
        }

        private JSONNode ToJSON()
        {
            JSONClass data = new JSONClass();

            AddAttAsInt(data, ATT_PERMISSION_REQUESTED, PermissionOverCarrierRequested);
            AddAttAsInt(data, ATT_PERMISSION_GRANTED, PermissionOverCarrierGranted);
            
            return data;            
        }

        private bool GetAttAsBool(JSONNode data, string attName)
        {
            return data != null && data[attName].AsInt > 0;
        }

        private void AddAttAsInt(JSONNode data, string attName, bool value)
        {
            if (data != null)
            {
                data[attName] = (value) ? 1 : 0;
            }
        }

        public void Save()
        {             
            Error error;
            sm_disk.File_WriteJSON(Disk.EDirectoryId.Groups, Id, ToJSON(), out error);
            if (error == null)
            {
                NeedsToSave = false;
            }    
            else
            {
                m_latestSaveAt = Time.realtimeSinceStartup;
            }                        
        }

        public void Update()
        {
            if (NeedsToSave)
            {
                bool canSave = true;

                if (m_latestSaveAt >= 0f)
                {
                    float timeSinceLatestSave = Time.realtimeSinceStartup - m_latestSaveAt;
                    canSave = timeSinceLatestSave >= TIME_TO_WAIT_BETWEEN_SAVES;
                }

                if (canSave)
                {
                    Save();
                }
            }
        }
    }
}
