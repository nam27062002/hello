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

        private List<string> EntryIds;

        private bool NeedsToSave { get; set; }

        private bool m_permissionRequested;
        public bool PermissionRequested
        {
            get
            {
                return m_permissionRequested;
            }

            set
            {
                m_permissionRequested = value;
                NeedsToSave = true;
            }
        }

        private bool m_permissionGranted;
        public bool PermissionGranted
        {
            get
            {
                return m_permissionGranted;
            }

            set
            {
                m_permissionGranted = value;
                NeedsToSave = true;
            }
        }

        private float m_latestSaveAt;

        public void Reset()
        {
            EntryIds = null;
            PermissionRequested = false;
            PermissionGranted = false;
            NeedsToSave = false;
            m_latestSaveAt = -1;
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

            bool canAdvance = error == null;
            if (error == null && exists)
            {
                JSONNode json = sm_disk.File_ReadJSON(Disk.EDirectoryId.Groups, Id, out error);
                canAdvance = (error == null);
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
                m_permissionRequested = GetAttAsBool (data, ATT_PERMISSION_REQUESTED);
                m_permissionGranted = GetAttAsBool(data, ATT_PERMISSION_GRANTED); 
            }
        }

        private JSONNode ToJSON()
        {
            JSONClass data = new JSONClass();

            AddAttAsInt(data, ATT_PERMISSION_REQUESTED, PermissionRequested);
            AddAttAsInt(data, ATT_PERMISSION_GRANTED, PermissionGranted);
            
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
