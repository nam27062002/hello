using SimpleJSON;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Downloadables
{
    /// <summary>
    /// This class is responsible for handling Disk accesses and to report errors
    /// </summary>
    public class Disk
    {
        public enum EDirectoryId
        {
            Manifests,
            Downloads,
            Groups,
            Dump
        };

        private string[] m_rootPaths;
        public DiskDriver DiskDriver;

        /// <summary>
        /// Time in seconds to wait between two issues are notified
        /// </summary>
        private float m_issueNotifPeriod;
        
        public Disk(DiskDriver diskDriver, string manifestsRootPath, string downloadsRootPath, string groupsRootPath, string dumpRootPath, float issuesNofitPeriod, OnIssue onIssueCallbak)
        {
            Reset();

            DiskDriver = diskDriver;

            m_rootPaths = new string[4];
            m_rootPaths[(int)EDirectoryId.Manifests] = manifestsRootPath;
            m_rootPaths[(int)EDirectoryId.Downloads] = downloadsRootPath;
            m_rootPaths[(int)EDirectoryId.Groups] = groupsRootPath;
            m_rootPaths[(int)EDirectoryId.Dump] = dumpRootPath;

            m_onIssueCallback = onIssueCallbak;
            m_issueNotifPeriod = issuesNofitPeriod;
        }

        public void Reset()
        {            
            IssueTypes_Reset();
        }        

        private string GetRootPath(EDirectoryId id)
        {
            return m_rootPaths[(int)id];            
        }

        private string GetFullPath(EDirectoryId id, string fileName)
        {
            return GetRootPath(id) + "/" + fileName;            
        }                

        private void ProcessError(Error error, bool writeOp)
        {
            if (error == null)
            {
                IssueType_NeedsToNotify(EIssueType.UnauthorizedAccess, false);                

                if (writeOp)
                {
                    IssueType_NeedsToNotify(EIssueType.OutOfSpace, false);                    
                }
            }
            else if (error.Type == Error.EType.Disk_UnauthorizedAccess)
            {
                IssueType_NeedsToNotify(EIssueType.UnauthorizedAccess, true);                
            }
            else if (writeOp)
            {
                IssueType_NeedsToNotify(EIssueType.OutOfSpace, true);
            }
        }

        public bool Directory_Exists(EDirectoryId id, out Error error)
        {
            error = null;

            try
            {
                return DiskDriver.Directory_Exists(GetRootPath(id));                
            }
            catch (Exception e)
            {
                error = new Error(e);
                return false;
            }
            finally
            {
                ProcessError(error, false);
            }
        }

        public DirectoryInfo Directory_CreateDirectory(EDirectoryId id, out Error error)
        {
            error = null;

            try
            {
                return DiskDriver.Directory_CreateDirectory(GetRootPath(id));
            }
            catch (Exception e)
            {
                error = new Error(e);
                return null;
            }
            finally
            {
                ProcessError(error, false);
            }
        }

        public List<string> Directory_GetFiles(EDirectoryId id, out Error error)
        {
            error = null;

            try
            {
                string rootPath = GetRootPath(id);                
                if (rootPath != null && DiskDriver.Directory_Exists(rootPath))
                {
                    return DiskDriver.Directory_GetFiles(rootPath);                    
                }
                else
                {
                    return new List<string>();
                }
            }
            catch (Exception e)
            {
                error = new Error(e);
                return null;
            }
            finally
            {
                ProcessError(error, false);
            }
        }

        public string File_ReadAllText(EDirectoryId id, string fileName, out Error error)
        {
            if (File_Exists(id, fileName, out error))
            {
                try
                {
                    error = null;
                    return DiskDriver.File_ReadAllText(GetFullPath(id, fileName));
                }
                catch (Exception e)
                {
                    error = new Error(e);
                    return null;
                }
                finally
                {
                    ProcessError(error, false);
                }
            }
            else
            {
                return null;
            }
        }

        public byte[] File_ReadAllBytes(EDirectoryId id, string fileName, out Error error)
        {
            if (File_Exists(id, fileName, out error))
            {
                try
                {
                    error = null;
                    return DiskDriver.File_ReadAllBytes(GetFullPath(id, fileName));
                }
                catch (Exception e)
                {
                    error = new Error(e);
                    return null;
                }
                finally
                {
                    ProcessError(error, false);
                }
            }
            else
            {
                return null;
            }
        }

        public void File_WriteAllText(EDirectoryId id, string fileName, string content, out Error error)
        {
            error = null;

            try
            {
                if (!Directory_Exists(id, out error))
                {
                    if (error == null)
                    {
                        Directory_CreateDirectory(id, out error);
                    }
                }

                if (error == null)
                {
                    DiskDriver.File_WriteAllText(GetFullPath(id, fileName), content);
                }
            }
            catch (Exception e)
            {
                error = new Error(e);
            }
            finally
            {
                ProcessError(error, true);
            }
        }

        public JSONNode File_ReadJSON(EDirectoryId id, string fileName, out Error error)
        {
            JSONNode returnValue = null;

            string content = File_ReadAllText(id, fileName, out error);
            if (error == null && !string.IsNullOrEmpty(content))
            {
                returnValue = JSON.Parse(content);
            }

            return returnValue;
        }

        public void File_WriteJSON(EDirectoryId id, string fileName, JSONNode json, out Error error)
        {
            if (json == null)
            {
                error = null;
            }
            else
            {
                File_WriteAllText(id, fileName, json.ToString(), out error);
            }
        }

        public bool File_Exists(EDirectoryId id, string fileName, out Error error)
        {
            error = null;

            try
            {                
                return DiskDriver.File_Exists(GetFullPath(id, fileName));
            }
            catch (Exception e)
            {
                error = new Error(e);
                return false;
            }
            finally
            {
                ProcessError(error, false);
            }
        }

        public void File_Delete(EDirectoryId id, string fileName, out Error error)
        {
            error = null;

            if (File_Exists(id, fileName, out error))
            {
                try
                {
                    DiskDriver.File_Delete(GetFullPath(id, fileName));
                }
                catch (Exception e)
                {
                    error = new Error(e);
                }
                finally
                {
                    ProcessError(error, true);
                }
            }            
        }

        public FileInfo File_GetInfo(EDirectoryId id, string fileName, out Error error)
        {
            error = null;

            try
            {                
                return DiskDriver.File_GetInfo(GetFullPath(id, fileName));
            }
            catch (Exception e)
            {
                error = new Error(e);
                return null;
            }
            finally
            {
                ProcessError(error, false);
            }
        }    

        public FileStream File_Open(FileInfo info, FileMode mode, FileAccess access, FileShare share, out Error error)
        {
            error = null;

            try
            {
                return info.Open(mode, access, share);
            }
            catch (Exception e)
            {
                error = new Error(e);
                return null;
            }
            finally
            {
                ProcessError(error, false);
            }
        }
    
        public void Update()
        {
            DiskDriver.Update();

            float timePassed;
            float now = Time.realtimeSinceStartup;
            for (int i = 0; i < IssueTypesCount; i++)
            {
                if (m_issueTypesNeedsToNotify[i])
                {
                    timePassed = now - m_issueTypesLatestNotificationTimestamp[i];
                    if (timePassed >= m_issueNotifPeriod)
                    {
                        IssueTypes_Notify((EIssueType)i);
                    }
                }                
            }
        }

        #region issues
        private enum EIssueType
        {            
            UnauthorizedAccess,
            OutOfSpace,
            Other
        };

        private static int IssueTypesCount = Enum.GetValues(typeof(EIssueType)).Length;

        private float[] m_issueTypesLatestNotificationTimestamp = new float[IssueTypesCount];
        private bool[] m_issueTypesNeedsToNotify = new bool[IssueTypesCount];

        public delegate void OnIssue(Error.EType errorType);
        private OnIssue m_onIssueCallback;

        private void IssueTypes_Reset()
        {
            for (int i = 0; i < IssueTypesCount; i++)
            {
                m_issueTypesLatestNotificationTimestamp[i] = -1f;
                m_issueTypesNeedsToNotify[i] = false;
            }
        }

        private Error.EType IssueTypeToErrorType(EIssueType issueType)
        {
            switch (issueType)
            {
                case EIssueType.UnauthorizedAccess:
                    return Error.EType.Disk_UnauthorizedAccess;

                case EIssueType.OutOfSpace:
                    return Error.EType.Disk_IOException;

                default:
                    return Error.EType.Disk_Other;
            }
        }

        private void IssueType_NeedsToNotify(EIssueType issueType, bool value)
        {
            m_issueTypesNeedsToNotify[(int)issueType] = value;
        }

        private void IssueTypes_Notify(EIssueType issueType)
        {
            m_issueTypesLatestNotificationTimestamp[(int)issueType] = Time.realtimeSinceStartup;

            if (m_onIssueCallback != null)
            {                
                Error.EType errorType = IssueTypeToErrorType(issueType);
                m_onIssueCallback(errorType);
            }
        }
        #endregion
    }
}
