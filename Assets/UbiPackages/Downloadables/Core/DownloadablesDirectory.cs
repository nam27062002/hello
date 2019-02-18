using SimpleJSON;
using System;
using System.Collections.Generic;
using System.IO;

namespace Downloadables
{
    /// <summary>
    /// Helper class to manage files in a directory.
    /// </summary>
    public class Directory
    {        
        private string m_path;
        private DiskDriver m_diskDriver;

        public Directory(string path, DiskDriver diskDriver)
        {
            m_path = path;            
            m_diskDriver = diskDriver;
        }
        
        public string GetFullPath(string fileName)
        {
            return m_path + "/" + fileName;
        }

        public List<string> Directory_GetFiles(out Error error)
        {
            try
            {
                error = null;
                if (m_diskDriver.Directory_Exists(m_path))
                {
                    return m_diskDriver.Directory_GetFiles(m_path);
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
        }        

        public string File_ReadAllText(string fileName, out Error error)
        {            
            if (File_Exists(fileName, out error))
            {
                try
                {
                    error = null;
                    return m_diskDriver.File_ReadAllText(GetFullPath(fileName));
                }
                catch (Exception e)
                {
                    error = new Error(e);
                    return null;
                }
            }
            else
            {
                return null;
            }            
        }

        public void File_WriteAllText(string fileName, string content, out Error error)
        {
            try
            {
                error = null;
                m_diskDriver.File_WriteAllText(GetFullPath(fileName), content);                
            }
            catch (Exception e)
            {
                error = new Error(e);
            }                              
        }

        public JSONNode File_ReadJSON(string fileName, out Error error)
        {
            JSONNode returnValue = null;

            string content = File_ReadAllText(fileName, out error);
            if (error == null && !string.IsNullOrEmpty(content))
            {
                returnValue = JSON.Parse(content);
            }

            return returnValue;
        }

        public void File_WriteJSON(string fileName, JSONNode json, out Error error)
        {
            if (json == null)
            {
                error = null;
            }
            else
            {
                File_WriteAllText(fileName, json.ToString(), out error);
            }                                    
        }

        public bool File_Exists(string fileName, out Error error)
        {
            try
            {
                error = null;
                return m_diskDriver.File_Exists(GetFullPath(fileName));                
            }
            catch (Exception e)
            {
                error = new Error(e);
                return false;
            }            
        }

        public void File_Delete(string fileName, out Error error)
        {            
            try
            {
                error = null;
                m_diskDriver.File_Delete(GetFullPath(fileName));
            }
            catch (Exception e)
            {
                error = new Error(e);                
            }
        }

        public FileInfo File_GetInfo(string fileName, out Error error)
        {
            try
            {
                error = null;
                return m_diskDriver.File_GetInfo(GetFullPath(fileName));
            }
            catch (Exception e)
            {
                error = new Error(e);
                return null;
            }
        }
    }
}