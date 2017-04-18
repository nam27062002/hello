using System.Collections.Generic;
using System.IO;
using System.Linq;

public enum AssetMemoryPrinterSortType
{
	AsIs,
	HighToLow,
	LowToHigh,    
}

public enum AssetMemoryPrinterFormatType
{
	Tree,
	CSV,
	CSVSimplified,
	XML,
	XMLSimplified,
	CSVMegaSimplified
}

public class AssetMemoryProfilerPrinter
{
    public static void Print(string path, List<AssetMemoryGlobals.AssetMemoryRawData> data, AssetMemoryPrinterSortType sort)
    {
        long total = 0;
        string output = "";
        if (data != null)
        {
            data = GetSortedRawData(sort, data);
            int count = data.Count;
            for (int i = 0; i < count; i++)
            {
                if (i > 0)
                {
                    output += "\n";
                }
                total += data[i].Bytes;
                output += data[i].Name + " : " + BytesToString(data[i].Bytes);
            }

            output += "\n";
            output += "Total (" + data.Count + "): " + BytesToString(total);
        }

        File.WriteAllText(path, output);
    }

    public static string BytesToString(long bytes)
    {
        int halfMega = 512 * 1024;
        return (bytes >= halfMega) ? (bytes / (1024f * 1024f)) + "MB" : (bytes / 1024f) + "KB";        
    }

	AssetMemoryProfiler m_profiler = null;

	public AssetMemoryProfilerPrinter(AssetMemoryProfiler profiler)
	{
		m_profiler = profiler;
	}

	public string Print(AssetMemoryPrinterFormatType format, AssetMemoryPrinterSortType sort, bool sortByLabel)
	{
		List<AssetMemoryGlobals.GoExtended> prefabs = GetSortedPrefabs(sort, m_profiler.Gos);
		string output = "";

		if (prefabs != null)
		{
			int index = 0;

			if (format == AssetMemoryPrinterFormatType.XML || format == AssetMemoryPrinterFormatType.XMLSimplified)
				output += "<Items>\n";

			if (format == AssetMemoryPrinterFormatType.CSVMegaSimplified)
				output += "Name,Path,TextureSize,MeshSize,AnimationSize,TotalSize\n";

            if (sortByLabel &&
                (format == AssetMemoryPrinterFormatType.XML || format == AssetMemoryPrinterFormatType.XMLSimplified))
            {
                List<string> labels = m_profiler.Labels;
                int count = labels.Count;
                for (int i = 0; i < count; i++)
                {
                    output += "<" + labels[i] + ">\n";
                    prefabs = GetSortedPrefabs(sort, m_profiler.GetGosPerLabel(labels[i]));
                    foreach (AssetMemoryGlobals.GoExtended go in prefabs)
                    {
                        ProcessInfo(go.Info, index, ref output, format);
                    }

                    output += "</" + labels[i] + ">\n";
                }
            }
            else
            {
                foreach (AssetMemoryGlobals.GoExtended go in prefabs)
                {
                    ProcessInfo(go.Info, index, ref output, format);
                }
            }

			if (format == AssetMemoryPrinterFormatType.XML || format == AssetMemoryPrinterFormatType.XMLSimplified)
				output += "</Items>\n";
		}

		return output;
	}
    
	public void Print(string path, AssetMemoryPrinterFormatType format = AssetMemoryPrinterFormatType.Tree, AssetMemoryPrinterSortType sort = AssetMemoryPrinterSortType.HighToLow, bool sortByLabel=true)
	{
		string output = Print(format, sort, sortByLabel);
		File.WriteAllText(path, output);
	}   

	static List<AssetMemoryGlobals.GoExtended> GetSortedPrefabs(AssetMemoryPrinterSortType type, List<AssetMemoryGlobals.GoExtended> prefabs)
	{
		switch (type)
		{
			case AssetMemoryPrinterSortType.AsIs:
				return prefabs;
			case AssetMemoryPrinterSortType.HighToLow:
				return prefabs.OrderByDescending(item => item.Info.GetSize()).ToList();
			case AssetMemoryPrinterSortType.LowToHigh:
				return prefabs.OrderBy(item => item.Info.GetSize()).ToList();
		}
		return null;
	}

    static List<AssetMemoryGlobals.AssetMemoryRawData> GetSortedRawData(AssetMemoryPrinterSortType type, List<AssetMemoryGlobals.AssetMemoryRawData> data)
    {
        switch (type)
        {
            case AssetMemoryPrinterSortType.AsIs:
                return data;
            case AssetMemoryPrinterSortType.HighToLow:
                return data.OrderByDescending(item => item.Bytes).ToList();
            case AssetMemoryPrinterSortType.LowToHigh:
                return data.OrderBy(item => item.Bytes).ToList();
        }
        return null;
    }

    float L2F(long l)
	{
		const float ff = 1024.0f * 1024.0f;
		return l / ff;
	}    

	void ProcessInfo(AssetInformationStruct info, int index, ref string output, AssetMemoryPrinterFormatType type)
	{
		string starter = "";
		for (int i = 0; i < index; i++) starter += "\t";

		if (type == AssetMemoryPrinterFormatType.Tree)
		{
			output += starter + "Name: " + info.Name + "\n";
			output += starter + "Path: " + info.Path + "\n";
			output += starter + "Type: " + info.Type + "\n";
			output += starter + "Size: " + info.Size + "\n";
			output += starter + "Total: " + info.GetSize() + "\n";

            if (info.Children != null)
            {
                foreach (AssetInformationStruct child in info.Children)
                {
                    ProcessInfo(child, index + 1, ref output, type);
                }
            }

			//	Cleaner view
			if (index == 0)
				output += "\n";
		}
		else if (type == AssetMemoryPrinterFormatType.CSV)
		{
			output += string.Format("{0}{1},{2},{3},{4},{5:0.##}\n", starter, info.Name, info.Path, info.Type, info.Size, L2F(info.GetSize()));

            if (info.Children != null)
            {
                foreach (AssetInformationStruct child in info.Children)
                {
                    ProcessInfo(child, index + 1, ref output, type);
                }
            }
		}
		else if (type == AssetMemoryPrinterFormatType.CSVSimplified)
		{
			output += string.Format("{0}{1},{2},{3:0.##}\n", starter, info.Name, info.Path, L2F(info.GetSize()));

			Dictionary<string, SimplifiedInfoStruct> assets = new Dictionary<string, SimplifiedInfoStruct>();
			ProcessSimplifedInfo(info, ref assets);

			starter += "\t";
			foreach (var tmp in assets)
			{
				if (tmp.Key.Equals(info.Path))
					continue;

				output += string.Format("{0}{1},{2},{3:0.##}\n", starter, tmp.Key, tmp.Value.Type, L2F(tmp.Value.Size));
			}

		}
		else if (type == AssetMemoryPrinterFormatType.XML)
		{
			output += starter + "<Item>\n";
			output += starter + "<Name>" + info.Name + "</Name>\n";
			output += starter + "<Path>" + info.Path + "</Path>\n";
			output += starter + "<Type>" + info.Type + "</Type>\n";
			output += starter + "<Size>" + info.Size + "</Size>\n";
			output += starter + "<Total>" + info.GetSize() + "</Total>\n";

			if (info.Children != null && info.Children.Count > 0)
			{
				output += starter + "<Children>\n";
				foreach (AssetInformationStruct child in info.Children)
				{
					ProcessInfo(child, index + 1, ref output, type);
				}
				output += starter + "</Children>\n";
			}

			output += starter + "</Item>\n";
		}
		else if (type == AssetMemoryPrinterFormatType.XMLSimplified)
		{
			output += starter + "<Item>\n";
			output += starter + "<Name>" + info.Name + "</Name>\n";
			output += starter + "<Path>" + info.Path + "</Path>\n";
			output += starter + "<Type>" + info.Type + "</Type>\n";
			output += starter + "<Size>" + info.Size + "</Size>\n";
			output += starter + "<Total>" + info.GetSize() + "</Total>\n";

			Dictionary<string, SimplifiedInfoStruct> assets = new Dictionary<string, SimplifiedInfoStruct>();
			ProcessSimplifedInfo(info, ref assets);

			if (assets.Count > 0)
			{
				output += starter + "<Children>\n";
				string starter2 = starter + "\t";

				foreach (var tmp in assets)
				{
					if (tmp.Key.Equals(info.Path))
						continue;

					output += starter2 + "<Asset>\n";
					output += starter2 + "<Path> " + tmp.Key + "</Path>\n";
					output += starter2 + "<Size> " + tmp.Value.Size + "</Size>\n";
					output += starter2 + "<Type> " + tmp.Value.Type + "</Type>\n";
					output += starter2 + "</Asset>\n";
				}

				output += starter + "</Children>\n";
			}

			output += starter + "</Item>\n";
		}
		else if (type == AssetMemoryPrinterFormatType.CSVMegaSimplified)
		{
			long textureSize = 0;
			long animSize = 0;
			long meshSize = 0;
			long totalSize = 0;

			Dictionary<string, SimplifiedInfoStruct> assets = new Dictionary<string, SimplifiedInfoStruct>();
			ProcessSimplifedInfo(info, ref assets);

			starter += "\t";
			foreach (var tmp in assets)
			{
				totalSize += tmp.Value.Size;

				if (tmp.Value.Type.Equals("Animation"))
				{
					animSize += tmp.Value.Size;
				}
				if (tmp.Value.Type.Equals("Mesh"))
				{
					meshSize += tmp.Value.Size;
				}
				if (tmp.Value.Type.Equals("Diffuse") || tmp.Value.Type.Equals("Normal") || tmp.Value.Type.Equals("TextureOther"))
				{
					textureSize += tmp.Value.Size;
				}
			}

			//name,path,textures,meshes,anims,total							
			output += string.Format("{0},{1},{2:0.##},{3:0.##},{4:0.##},{5:0.##}\n", info.Name, info.Path, L2F(textureSize), L2F(meshSize), L2F(animSize), L2F(info.GetSize()));
		}
	}

	struct SimplifiedInfoStruct
	{
		public long Size;
		public AssetMemoryGlobals.EAssetType Type;
	}

    void ProcessSimplifedInfo(AssetInformationStruct info, ref Dictionary<string, SimplifiedInfoStruct> assets)
    {
        if (!string.IsNullOrEmpty(info.Path) && !assets.ContainsKey(info.Path))
        {
            SimplifiedInfoStruct si = new SimplifiedInfoStruct();
            si.Size = info.GetSize();
            si.Type = info.Type;
            assets[info.Path] = si;
        }

        if (info.Children != null)
        {
            foreach (AssetInformationStruct child in info.Children)
            {
                ProcessSimplifedInfo(child, ref assets);
            }
        }
    }
}
