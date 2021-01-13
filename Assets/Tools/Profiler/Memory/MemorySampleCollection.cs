using System.Collections.Generic;
using System.Xml;

public class MemorySampleCollection : AbstractMemorySample
{
    public Dictionary<string, AbstractMemorySample> Samples { get; set; }

    public MemorySampleCollection(string name, MemorySample.ESizeStrategy sizeStrategoy)
    {
        Name = name;
        SizeStrategy = sizeStrategoy;
    }

    public override void Clear()
    {
        Name = null;
        Samples = null;
    }

    public void AddSample(string key, AbstractMemorySample sample)
    {
        if (Samples == null)
        {
            Samples = new Dictionary<string, AbstractMemorySample>();
        }

        if (Samples.ContainsKey(key))
        {
            Samples[key] = sample;
        }
        else
        {
            Samples.Add(key, sample);
        }
    }   

    public AbstractMemorySample GetSample(string key)
    {
        AbstractMemorySample returnValue = null;
        if (Samples != null && Samples.ContainsKey(key))
        {
            returnValue = Samples[key];
        }

        return returnValue;
    }

    public override long GetTotalMemorySize()
    {
        long returnValue = 0;
        if (Samples != null)
        {
            foreach (KeyValuePair<string, AbstractMemorySample> pair in Samples)
            {
                returnValue += pair.Value.GetTotalMemorySize();
            }
        }

        return returnValue;
    }

    #region size    
    protected override void Size_Recalculate()
    {
        if (Samples != null)
        {
            foreach (KeyValuePair<string, AbstractMemorySample> pair in Samples)
            {
                pair.Value.SizeStrategy = SizeStrategy;
            }
        }
    }
    #endregion

    #region xml
    public override XmlNode ToXML(XmlDocument xmlDoc = null, XmlNode rootNode = null, Dictionary<string, List<string>> categories = null)
    {
        // Header
        if (xmlDoc == null)
        {
            xmlDoc = new XmlDocument();
        }

        if (rootNode == null)
        {
            rootNode = xmlDoc.CreateElement("Samples");
        }

        XmlAttribute attribute = xmlDoc.CreateAttribute(XML_PARAM_NAME);
        attribute.Value = Name;
        rootNode.Attributes.Append(attribute);
        xmlDoc.AppendChild(rootNode);

        if (Samples != null)
        {
            foreach (KeyValuePair<string, AbstractMemorySample> pair in Samples)
            {
                pair.Value.ToXML(xmlDoc, rootNode, categories);
            }
        }

        // Total memory taken up by all assets is set as an attribute of the header
        attribute = xmlDoc.CreateAttribute(XML_PARAM_SIZE);
        attribute.Value = FormatSizeString(GetTotalMemorySize());
        rootNode.Attributes.Append(attribute);

        attribute = xmlDoc.CreateAttribute(XML_PARAM_SIZE_RAW);
        attribute.Value = "" + GetTotalMemorySize();
        rootNode.Attributes.Append(attribute);

        return xmlDoc;
    }

    public override void FromXML(XmlNode xml)
    {
    }
    #endregion
}
