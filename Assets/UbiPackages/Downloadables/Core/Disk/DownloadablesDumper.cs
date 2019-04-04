#if USE_DUMPER
using System;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// This class is responsible for handling trash that is stored to test how the system performs when the disk is full. This class should be used only on debug builds.
/// </summary>

namespace Downloadables
{
    public class Dumper
    {
        private Disk m_disk;
        private int m_latestIndex;
        private byte[] m_stuffBuffer;
        private Logger m_logger;

        public void Initialize(Disk disk, Logger logger)
        {   
            m_disk = disk;
            m_logger = logger;
            UpdateLatestIndexAccordingToFiles();
            IsFillingUp = false;
        }

        private bool IsInitialized()
        {
            return m_disk != null;
        }

        private void UpdateLatestIndexAccordingToFiles()
        {
            m_latestIndex = 0;

            Error error;
            List<string> fileNames = m_disk.Directory_GetFiles(Disk.EDirectoryId.Dump, out error);
            if (fileNames != null)
            {
                int number;
                int count = fileNames.Count;
                string fileName;
                for (int i = 0; i < count; i++)
                {
                    fileName = Path.GetFileName(fileNames[i]);
                    if (int.TryParse(fileName, out number))
                    {
                        if (number > m_latestIndex)
                        {
                            m_latestIndex = number;
                        }
                    }
                }
            }            
        }

        public void CleanUp()
        {
            if (IsInitialized())
            {
                Error error;
                if (m_disk.Directory_Exists(Disk.EDirectoryId.Dump, out error))
                {
                    m_disk.Directory_Delete(Disk.EDirectoryId.Dump, out error);
                }
            }
        }

        public bool IsFillingUp { get; set; }        

        private void StoreChunk()
        {
            FileStream saveFileStream = null;

            try
            {
                Error error;
                if (!m_disk.Directory_Exists(Disk.EDirectoryId.Dump, out error))
                {
                    m_disk.Directory_CreateDirectory(Disk.EDirectoryId.Dump, out error);
                }

                if (error == null)
                {
                    string fileName = (m_latestIndex + 1) + "";
                    FileInfo fileInfo = m_disk.File_GetInfo(Disk.EDirectoryId.Dump, fileName, out error);
                    if (error == null)
                    {
                        using (saveFileStream = m_disk.DiskDriver.File_Open(fileInfo, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                        {
                            if (m_stuffBuffer == null)
                            {
                                m_stuffBuffer = new byte[40960000];
                            }

                            m_disk.DiskDriver.File_Write(saveFileStream, m_stuffBuffer, 0, m_stuffBuffer.Length);
                        }

                        m_latestIndex++;
                    }
                }
            }
            catch (Exception e)
            {
                IsFillingUp = false;

                if (m_logger != null && m_logger.CanLog())
                {
                    m_logger.LogError(e.ToString());
                }
            }
            finally
            {
                if (saveFileStream != null)
                {
                    saveFileStream.Close();
                    saveFileStream = null;
                }
            }
        }

        public void Update()
        {
            if (IsFillingUp && IsInitialized())
            {
                StoreChunk();
            }
        }        
    }
}
#endif