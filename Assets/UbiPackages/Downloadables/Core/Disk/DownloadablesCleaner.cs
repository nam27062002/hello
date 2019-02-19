using System.Collections.Generic;
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
        private List<string> m_idsToKeep;
        private List<string> m_fileNamesToDelete;
        private Disk.EDirectoryId m_directoryIdToDelete;
        private float m_latestErrorTimestamp;
        private float m_timeToWaitAfterError;

        private enum EStep
        {
            Init,
            RetrievingManifests,
            RetrievingDownloads,
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
            m_idsToKeep = null;
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
        /// <param name="ids">List of downloadable ids to keep from both (manifests and downloads)</param>
        public void CleanAllExcept(List<string> idsToKeep)
        {
            Reset();

            m_idsToKeep = idsToKeep;

            if (idsToKeep == null || idsToKeep.Count == 0)
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
                            if (m_directoryIdToDelete == Disk.EDirectoryId.Manifests)
                            {
                                SetStep(EStep.RetrievingDownloads);
                            }
                            else
                            {
                                SetStep(EStep.Done);
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
                int count = fileNames.Count;
                string fileName;
                for (int i = 0; i < count; i++)
                {
                    // The id is the fileName without extension
                    fileName = Path.GetFileNameWithoutExtension(fileNames[i]);
                    if (!m_idsToKeep.Contains(fileName))
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