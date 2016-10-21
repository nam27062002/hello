using System;
using System.IO;
using UnityEngine;
using UnityEngine.GIF;

/// <summary>
/// Factory for creating Texture2D objects from GIF files 
/// </summary>
public static class GifTextureFactory
{
    /// <summary>
    /// Creates Texture2D object with loaded texture data from GIF stream
    /// </summary>
    /// <param name="data">Memory stream</param>
    /// <returns>Texture2D or null</returns>
    public static Texture2D CreateTexture(Stream data)
    {
        GifDecoder decoder = new GifDecoder();
        int status = decoder.Read(data);
        if (status == GifDecoder.STATUS_OK)
        {
            int frameCount = decoder.GetFrameCount();
            if (frameCount > 0)
            {
                GifDecoder.Size size = decoder.GetFrameSize();
                GifDecoder.Image image = decoder.GetImage();

                //Debug.Log(string.Format("GIF file opened. FrameCNT: {0} FrameW: {1} FrameH: {2}", frameCount, size.Width, size.Height));

                if (image != null)
                {
                    Color[] pixels = image.Pixels;

                    image = null;   // clear image as soon as possible
                    decoder = null; //  clear decoder as soon as possible

                    if (pixels != null)
                    {
                        //  Load Texture (TODO: Benchmark how fast this really is)
                        Texture2D texture = new Texture2D(size.Width, size.Height);
                        texture.SetPixels(pixels, 0);
                        texture.Apply();

                        return texture;
                    }
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Creates Texture2D object with loaded texture data from GIF byte array (usually loaded by WWW)
    /// </summary>
    /// <param name="data">Data</param>
    /// <returns>Texture2D or null</returns>
    public static Texture2D CreateTexture(byte[] data)
    {
        MemoryStream stream = new MemoryStream(data);
        return CreateTexture(stream);
    }

    /// <summary>
    /// Creates Texture2D object with loaded texture data from GIF file path
    /// </summary>
    /// <param name="filePath">Path to gif file</param>
    /// <returns>Texture2D or null</returns>
    public static Texture2D CreateTexture(string filePath)
    {
        if (File.Exists(filePath))
        {
            FileInfo info = new FileInfo(filePath);
            if (info != null)
            {
                Stream stream = info.OpenRead();
                if (stream != null)
                {
                    return CreateTexture(stream);
                }
            }
        }
        return null;
    }

}

