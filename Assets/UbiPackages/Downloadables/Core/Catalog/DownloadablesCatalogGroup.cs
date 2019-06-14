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

        private static int MIN_PRIORITY = 100;

        private static Disk sm_disk;
        public static void StaticSetup(Disk disk)
        {
            sm_disk = disk;
        }

        public string Id { get; private set; }

        public List<string> EntryIds;

        private bool NeedsToSave { get; set; }

        /// <summary>
        /// Whether or not the user has been notified about the download. According to Apple Store compliance guidelines the user needs to be notified for both over carrier and wifi downloads
        /// </summary>
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

                if (GroupsLinked != null)
                {
                    int count = GroupsLinked.Count;
                    for (int i = 0; i < count; i++)
                    {
                        GroupsLinked[i].PermissionRequested = value;
                    }
                }

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

                if (GroupsLinked != null)
                {
                    int count = GroupsLinked.Count;
                    for (int i = 0; i < count; i++)
                    {
                        GroupsLinked[i].PermissionOverCarrierGranted = value;
                    }
                }

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

        /// <summary>
        /// List of groups linked to this group. This is typically used to make this list of groups permissions share permissions with this group.
        /// </summary>
        private List<CatalogGroup> GroupsLinked { get; set; }

        public void Reset()
        {
            EntryIds = null;
            ResetPermissions();            
            NeedsToSave = false;
            Priority = MIN_PRIORITY;
            m_latestSaveAt = -1;
            Index = -1;
            GroupsLinked = null;
        }

        public void ResetPermissions()
        {
            PermissionRequested = false;
            PermissionOverCarrierGranted = false;
        }

        public void Setup(string id, List<string> entryIds, List<CatalogGroup> groupsLinked = null)
        {
            Reset();

            Id = id;
            EntryIds = entryIds;
            GroupsLinked = groupsLinked;

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
                PermissionRequested = GetAttAsBool (data, ATT_PERMISSION_REQUESTED);
                PermissionOverCarrierGranted = GetAttAsBool(data, ATT_PERMISSION_GRANTED); 
            }
        }

        private JSONNode ToJSON()
        {
            JSONClass data = new JSONClass();

            AddAttAsInt(data, ATT_PERMISSION_REQUESTED, PermissionRequested);
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
