﻿using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Downloadables
{
    /// <summary>
    /// This class is responsible for deleting stuff (both manifests and downloads) that are obsolete, typically 
    /// downloadables that are not part of the game anymore.
    /// </summary>
    public class Cleaner
    {        
        private Disk m_disk;        
        private List<string> m_entryIdsToKeep;
        private List<string> m_groupIdsToKeep;
        private List<string> m_fileNamesToDelete;
        private Disk.EDirectoryId m_directoryIdToDelete;
        private float m_latestErrorTimestamp;
        private float m_timeToWaitAfterError;

        private enum EStep
        {
            Init,
            RetrievingManifests,
            RetrievingDownloads,
            RetrievingGroups,
            DeletingFiles,
            Done
        };

        private EStep m_step;

        public Cleaner(Disk disk, float timeToWaitAfterError)
        {
            m_disk = disk;
            m_fileNamesToDelete = new List<string>();
            m_timeToWaitAfterError = timeToWaitAfterError;
        }

        public void Reset()
        {
            m_entryIdsToKeep = null;
            m_groupIdsToKeep = null;
            m_fileNamesToDelete.Clear();
            m_latestErrorTimestamp = -1f;
            m_step = EStep.Init;
        }

        private void UpdateRetrievingStep(Disk.EDirectoryId id)
        {
            if (RetrieveFilesToDelete(id))
            {
                m_directoryIdToDelete = id;
                SetStep(EStep.DeletingFiles);
            }
        }

        /// <summary>
        /// Requests to delete all files except the ones in the list of downloadable ids passed as a parameter
        /// </summary>
        /// <param name="entryIdsToKeep">List of downloadable ids to keep from both folders(manifests and downloads)</param>
        /// <param name="groupIdsToKeep">List of downloadable group ids to keep from Groups folder</param>
        public void CleanAllExcept(List<string> entryIdsToKeep, List<string> groupIdsToKeep)
        {
            Reset();

            m_entryIdsToKeep = entryIdsToKeep;
            m_groupIdsToKeep = groupIdsToKeep;

            int count = (entryIdsToKeep == null || entryIdsToKeep.Count == 0) ? 0 : entryIdsToKeep.Count;
            count += (groupIdsToKeep == null || groupIdsToKeep.Count == 0) ? 0 : groupIdsToKeep.Count;
            if (count == 0)
            {
                SetStep(EStep.Done);
            }
            else
            {
                SetStep(EStep.RetrievingManifests);
            }
        }
        
        public void Update()
        {
            bool canUpdate = !IsDone();

            if (canUpdate && m_latestErrorTimestamp >= 0f)
            {
                float diff = Time.realtimeSinceStartup - m_latestErrorTimestamp;
                canUpdate = (diff > m_timeToWaitAfterError);
            }

            if (canUpdate)
            {
                switch (m_step)
                {
                    case EStep.RetrievingManifests:
                        UpdateRetrievingStep(Disk.EDirectoryId.Manifests);
                        break;

                    case EStep.RetrievingDownloads:
                        UpdateRetrievingStep(Disk.EDirectoryId.Downloads);
                        break;

                    case EStep.RetrievingGroups:
                        UpdateRetrievingStep(Disk.EDirectoryId.Groups);
                        break;

                    case EStep.DeletingFiles:
                        Error error = null;
                        while (m_fileNamesToDelete.Count > 0 && error == null)
                        {
                            m_disk.File_Delete(m_directoryIdToDelete, m_fileNamesToDelete[0], out error);
                            if (error == null)
                            {
                                m_fileNamesToDelete.RemoveAt(0);
                            }
                        }

                        if (m_fileNamesToDelete.Count == 0)
                        {
                            switch (m_directoryIdToDelete)
                            {
                                case Disk.EDirectoryId.Manifests:
                                    SetStep(EStep.RetrievingDownloads);
                                    break;

                                case Disk.EDirectoryId.Downloads:
                                    SetStep(EStep.RetrievingGroups);
                                    break;

                                default:
                                    SetStep(EStep.Done);
                                    break;
                            }                            
                        }
                        break;
                }
            }
        }

        public bool IsDone()
        {
            return m_step == EStep.Done;
        }

        private bool RetrieveFilesToDelete(Disk.EDirectoryId id)
        {
            m_fileNamesToDelete.Clear();

            Error error;
            
            List<string> fileNames = m_disk.Directory_GetFiles(id, out error);
            if (error == null)
            {
                List<string> sourceIds = (id == Disk.EDirectoryId.Groups) ? m_groupIdsToKeep : m_entryIdsToKeep;

                int count = fileNames.Count;
                string fileName;
                for (int i = 0; i < count; i++)
                {
                    // The id is the fileName without extension
                    fileName = Path.GetFileNameWithoutExtension(fileNames[i]);
                    if (!sourceIds.Contains(fileName))
                    {
                        // We need to include the extension because m_disk requires it to be able to delete it
                        m_fileNamesToDelete.Add(Path.GetFileName(fileNames[i]));
                    }
                }
            }
            else
            {
                m_latestErrorTimestamp = Time.realtimeSinceStartup;
            }

            return error == null;
        }

        private void SetStep(EStep step)
        {
            m_step = step;            
        }
    }
}