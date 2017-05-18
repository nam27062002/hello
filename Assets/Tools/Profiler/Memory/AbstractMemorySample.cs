using System.Collections.Generic;
using System.Xml;

public abstract class AbstractMemorySample
{
    protected const string XML_PARAM_SAMPLE = "Sample";
    protected const string XML_PARAM_NAME = "name";
    protected const string XML_PARAM_ID = "id";
    protected const string XML_PARAM_SIZE = "size";
    protected const string XML_PARAM_SIZE_RAW = "sizeRaw";
    protected const string XML_PARAM_AMOUNT = "amount";
    protected const string XML_PARAM_SIZE_STRATEGY = "sizeStrategy";

    public string Name { get; set; }
   
    public abstract void Clear();   

    protected string FormatSizeString(long bytes)
    {
        float memSizeKB = bytes / 1024.0f;
        if (memSizeKB < 1024f) return "" + memSizeKB + "k";
        else
        {
            float memSizeMB = ((float)memSizeKB) / 1024.0f;
            return memSizeMB.ToString("0.00") + "Mb";
        }
    }    

    public abstract long GetTotalMemorySize();

    #region size   
    public enum ESizeStrategy
    {
        Profiler,
        DeviceQuarter,
        DeviceHalf,
        DeviceFull
    }

    private ESizeStrategy mSizeStrategy;
    public ESizeStrategy SizeStrategy
    {
        get
        {
            return mSizeStrategy;
        }

        set
        {
            if (mSizeStrategy != value)
            {
                mSizeStrategy = value;
                Size_Recalculate();               
            }
        }
    }

    protected abstract void Size_Recalculate();
    #endregion

    #region xml
    public abstract XmlNode ToXML(XmlDocument xmlDoc = null, XmlNode rootNode = null, Dictionary<string, List<string>> typeGroups = null);
    public abstract void FromXML(XmlNode xml);
    #endregion
}
