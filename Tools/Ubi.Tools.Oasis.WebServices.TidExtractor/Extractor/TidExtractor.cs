using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using Ubi.Tools.Oasis.Shared;
using Ubi.Tools.Oasis.WebServices.XmlExtractor.OasisService;

namespace Ubi.Tools.Oasis.WebServices.XmlExtractor.Extractor
{
    /// <summary>
    /// This class extracts data from an Oasis database and output it into XML files.
    /// </summary>
    sealed class TidExtractor : Extractor
    {
        private const string NAMESPACE = "http://schemas.ubisoft.com/oasis/2011/extractor";
        private const string GLOBAL_FILE_NAME = "oasis__global";
        private const string LOC_FILE_NAME = "oasis_loc";
        private readonly XmlWriterSettings _settings;
        private readonly string _directory;
        private List<string> _extractedTids;

        public TidExtractor(OasisServiceClient client, string directory)
            : base(client)
        {
            _directory = directory ?? string.Empty;

            _settings = new XmlWriterSettings { Encoding = Encoding.Unicode, Indent = true, IndentChars = ("    ") };

            _extractedTids = new List<string>();
        }

        protected override bool ExtractCore()
        {
            if (_directory.Length > 0 && !Directory.Exists(_directory))
                Directory.CreateDirectory(_directory);
            /*
            // Create the XML Schema file in the destination directory
            using (Stream resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(string.Format("Ubi.Tools.Oasis.WebServices.XmlExtractor.Resources.{0}.xsd", GLOBAL_FILE_NAME)))
                StreamToFile(Path.Combine(_directory, string.Format("{0}.xsd", GLOBAL_FILE_NAME)), resourceStream);

            // Create the global XML file
            using (XmlWriter writer = XmlWriter.Create(Path.Combine(_directory, string.Format("{0}.xml", GLOBAL_FILE_NAME)), _settings))
                WriteGlobal(writer);

            // Create the XML Schema file in the destination directory
            using (Stream resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(string.Format("Ubi.Tools.Oasis.WebServices.XmlExtractor.Resources.{0}.xsd", LOC_FILE_NAME)))
                StreamToFile(Path.Combine(_directory, string.Format("{0}.xsd", LOC_FILE_NAME)), resourceStream);
            */
            // Create a TIDs file for each language
            foreach (Language language in DataContext.Languages)
            {
                _extractedTids.Clear();
                bool ret = true;
                string filePath = Path.Combine(_directory, string.Format("{0}.txt", language.Name.ToLower()));
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    if (!language.IsMaster)
                        ret = WriteTranslations(writer, language);
                    else
                        ret = WriteMasterLines(writer, language);   
                }
                if (!ret)
                {
                    // Something went wrong. We dont have to publish texts to force correct the problem
                    Console.WriteLine("Something went wrong with this language: " + language.Name);
                    Console.WriteLine("Deleting File and aborting extraction");
                    File.Delete(filePath);
                    return false;
                }
            }
            return true;
        }

        private void WriteGlobal(XmlWriter writer)
        {
            writer.WriteStartElement("oasis", NAMESPACE);

            WriteOasis(writer, GLOBAL_FILE_NAME);
            WriteLanguages(writer);
            WriteCharacters(writer);
            WriteTeams(writer);
            WriteRootSections(writer);
            WriteRootAIs(writer);
            WriteCustomData(writer);

            writer.WriteEndElement();
        }

        private static void WriteOasis(XmlWriter writer, string fileName)
        {
            writer.WriteAttributeString("oasisVersion", Program.GetOasisVersion);
            writer.WriteAttributeString("toolVersion", Program.GetToolVersion);
            writer.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
            writer.WriteAttributeString("xsi", "schemaLocation", null, string.Format("{0} {1}.xsd", NAMESPACE, fileName));
        }

        private void WriteLanguages(XmlWriter writer)
        {
            writer.WriteStartElement("languages");

            foreach (Language language in DataContext.Languages)
            {
                writer.WriteStartElement("l");
                writer.WriteAttributeString("name", language.Name);
                writer.WriteStartAttribute("master");
                writer.WriteValue(language.IsMaster);
                writer.WriteEndAttribute();
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        private void WriteCharacters(XmlWriter writer)
        {
            writer.WriteStartElement("characters");

            foreach (Character character in DataContext.Characters)
            {
                writer.WriteStartElement("c");
                writer.WriteAttributeString("id", character.CharacterId.ToString());
                writer.WriteAttributeString("tag", character.Tag);
                writer.WriteAttributeString("name", character.Name);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        private void WriteTeams(XmlWriter writer)
        {
            writer.WriteStartElement("teams");

            foreach (Team team in DataContext.Teams)
            {
                writer.WriteStartElement("t");
                writer.WriteAttributeString("id", team.TeamId.ToString());
                writer.WriteAttributeString("tag", team.Tag);
                writer.WriteAttributeString("name", team.Name);

                writer.WriteStartElement("characters");

                foreach (int characterId in team.CharacterIds)
                {
                    writer.WriteStartElement("c");
                    writer.WriteAttributeString("id", characterId.ToString());
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        private void WriteRootSections(XmlWriter writer)
        {
            SectionType[] sectionTypes = new[] { SectionType.Script, SectionType.Cinematic, SectionType.Menu };

            foreach (SectionType sectionType in sectionTypes)
            {
                writer.WriteStartElement("sections");
                writer.WriteAttributeString("type", sectionType.ToString());

                WriteSections(writer, DataContext.GetRootSections(sectionType));

                
                writer.WriteEndElement();
            }
        }

        private void WriteRootAIs(XmlWriter writer)
        {
            writer.WriteStartElement("sections");
            writer.WriteAttributeString("type", "AI");

            IList<AIElementSection> aiElementSections;
            if (DataContext.AIElementSectionsByParentId.TryGetValue(-1, out aiElementSections))
                foreach (AIElementSection root in aiElementSections)
                    WriteAIElementSection(writer, root);

            writer.WriteEndElement();
        }

        private void WriteSections(XmlWriter writer, IEnumerable<Section> childSections)
        {
            foreach (Section section in childSections)
            {
                writer.WriteStartElement("s");
                writer.WriteAttributeString("id", section.SectionId.ToString());
                writer.WriteAttributeString("name", section.Name);
                writer.WriteAttributeString("tag", section.Tag);
                writer.WriteAttributeString("orderIndex", section.OrderIndex.ToString());

                IList<Section> sections;
                if (DataContext.SectionsByParentId.TryGetValue(section.SectionId, out sections))
                    WriteSections(writer, sections);

                if (section.Type == SectionType.Menu)
                    WriteMenuDialogs(writer, section);
                else
                    WriteDialogs(writer, section);

                writer.WriteEndElement();
            }
        }

        private void WriteDialogs(XmlWriter writer, Section section)
        {
            IList<Scene> scenes;
            if (DataContext.ScenesBySectionId.TryGetValue(section.SectionId, out scenes))
                WriteDialogs(writer, scenes);
        }

        private void WriteAIDialogs(XmlWriter writer, AIElementSection aiElementSection)
        {
            IList<Scene> scenes;
            if (DataContext.ScenesByAIElementSectionId.TryGetValue(aiElementSection.AIElementSectionId, out scenes))
                WriteDialogs(writer, scenes);
        }

        private void WriteMenuDialogs(XmlWriter writer, Section section)
        {
            IList<Control> menuControls;
            if (DataContext.MenuControlsBySectionId.TryGetValue(section.SectionId, out menuControls))
                WriteDialogs(writer, menuControls);
        }

        private void WriteDialogs(XmlWriter writer, IEnumerable<Scene> scenes)
        {
            foreach (Scene scene in scenes)
            {
                writer.WriteStartElement("d");
                writer.WriteAttributeString("id", scene.SceneId.ToString());

                writer.WriteAttributeString("name", scene.Name);
                writer.WriteAttributeString("tag", scene.Tag);
                writer.WriteAttributeString("orderIndex", scene.OrderIndex.ToString());

                WriteLines(writer, scene);

                writer.WriteEndElement();
            }
        }

        private static void WriteDialogs(XmlWriter writer, IEnumerable<Control> menuControls)
        {
            foreach (Control menuControl in menuControls)
            {
                writer.WriteStartElement("d");
                writer.WriteAttributeString("name", menuControl.Name);
                writer.WriteAttributeString("orderIndex", menuControl.OrderIndex.ToString());

                WriteLines(writer, menuControl);

                writer.WriteEndElement();
            }
        }

        private void WriteLines(XmlWriter writer, Scene scene)
        {
            IList<Line> lines;
            if (!DataContext.LinesByDialogId.TryGetValue(scene.SceneId, out lines))
                return;

            foreach (Line line in lines)
            {
                writer.WriteStartElement("l");
                writer.WriteAttributeString("id", line.LineId.ToString());
                writer.WriteAttributeString("orderIndex", line.OrderIndex.ToString());
                WriteAttributeString(writer, "text", line.Text);

                if (!string.IsNullOrEmpty(line.Parenthetical))
                    WriteAttributeString(writer, "parenthetical", line.Parenthetical);

                if (line.CharacterId > 0)
                    writer.WriteAttributeString("characterId", line.CharacterId.ToString());

                if (DataContext.IsRecordingRequired(line.LineId))
                    writer.WriteAttributeString("wav", DataContext.AudioFileNameByLineId[line.LineId]);

                if (!string.IsNullOrEmpty(line.Extension))
                    WriteAttributeString(writer, "extension", line.Extension);

                writer.WriteEndElement();
            }
        }

        private static void WriteLines(XmlWriter writer, Control menuControl)
        {
            writer.WriteStartElement("l");
            writer.WriteAttributeString("id", menuControl.LineId.ToString());
            writer.WriteAttributeString("orderIndex", (1).ToString());
            WriteAttributeString(writer, "text", menuControl.Text);

            if (!string.IsNullOrEmpty(menuControl.Comment))
                WriteAttributeString(writer, "parenthetical", menuControl.Comment);

            writer.WriteEndElement();
        }

        private void WriteAIElementSection(XmlWriter writer, AIElementSection parentAIElementSection)
        {
            writer.WriteStartElement("s");
            writer.WriteAttributeString("id", parentAIElementSection.AIElementSectionId.ToString());
            writer.WriteAttributeString("name", DataContext.GetAIElementSectionName(parentAIElementSection));
            writer.WriteAttributeString("tag", DataContext.GetAIElementSectionTag(parentAIElementSection));

            IList<AIElementSection> subSections;
            if (DataContext.AIElementSectionsByParentId.TryGetValue(parentAIElementSection.AIElementSectionId, out subSections))
                foreach (AIElementSection aiElementSection in subSections)
                    WriteAIElementSection(writer, aiElementSection);

            WriteAIDialogs(writer, parentAIElementSection);

            writer.WriteEndElement();
        }

        private bool WriteTranslations(StreamWriter writer, Language language)
        {
            /*
            foreach (Line line in DataContext.Lines)
            {
                if (DataContext.IsTranslationRequired(line.LineId, language.LanguageId))
                { 
                    WriteTranslation(writer, line.LineId, language.LanguageId);
                }
            }
             * */
            //TONI START
            IList<Section> AllSections = DataContext.GetRootSections(SectionType.Menu);
            string tagNameNotToSave = "NIGT";
            int tagIdNotToSave = -1;
            string tagName = "";
            foreach (Section s in AllSections)
            {
                tagName = s.Tag;
                if (tagName == tagNameNotToSave) tagIdNotToSave = s.SectionId;
            }
            //TONI END
            foreach ( Control c in DataContext.MenuControls)
            {
                if (DataContext.IsTranslationRequired(c.LineId, language.LanguageId))
                {
                    if ( _extractedTids.Contains( c.Name) )
                    {
                        // Print something to say what happened
                        Console.WriteLine("Tid " + c.Name +" already exists");
                        return false;
                    }
                    else
                    {
                        //TONI START
                        //string str = ReplaceCharacters(c.Text);
                        if (tagIdNotToSave != c.SectionId)
                        {
                            _extractedTids.Add(c.Name);
                            WriteTranslation(writer, c.LineId, language.LanguageId, c.Name);
                        }
                        //TONI END
                    }
                }
            }
            return true;
        }

        private void WriteTranslation(StreamWriter writer, int lineId, int languageId, string name)
        {
            LineTranslation lineTranslation = DataContext.GetLineTranslation(lineId, languageId);

            string text = "";

            if (lineTranslation != null && !string.IsNullOrEmpty(lineTranslation.Text))
            {
                text = (string)lineTranslation.Text;
                text = ReplaceCharacters( text );
            }
            writer.WriteLine( name + "=" + text);
        }


        private bool WriteMasterLines(StreamWriter writer, Language language)
        {
            /*
            foreach (Line line in DataContext.Lines)
            {
                writer.WriteLine(SearchLineCustomDataValue(line.LineId, "TID") + "=" + line.Text);
            }
            */
            //TONI START
            IList<Section> AllSections = DataContext.GetRootSections(SectionType.Menu);
            string tagNameNotToSave = "NIGT";
            string str = "";
            int tagIdNotToSave = -1;
            string tagName = "";
            foreach (Section s in AllSections)
            {
                tagName = s.Tag;
                if (tagName == tagNameNotToSave) tagIdNotToSave = s.SectionId;
            }
            //TONI END
            foreach (Control c in DataContext.MenuControls)
            {
                if ( _extractedTids.Contains(c.Name) )
                {
                    Console.WriteLine("Tid " + c.Name + " already exists");
                    return false;
                }
                else
                {
                    //TONI START
                    //string str = ReplaceCharacters(c.Text);
                    if (tagIdNotToSave != c.SectionId)
                    {
                        str = ReplaceCharacters(c.Text);
                        _extractedTids.Add(c.Name);
                        writer.WriteLine(c.Name + "=" + str);
                    }
                    //TONI END
                }
            }
            return true;
        }

        /// <summary>
        /// Replaces all characters we need to work with our system
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private string ReplaceCharacters(string str)
        {
            string ret = str;
            if ( !string.IsNullOrEmpty(ret) )
            { 
                ret = ret.Replace("\n", "\\n");
            }
           
            return ret;
        }

        private string SearchLineCustomDataValue(int lineId, string customValueName)
        {
            CustomData customData;
            if (DataContext.CustomDataByName.TryGetValue( customValueName, out customData ))
            {
                foreach (LineCustomDataValue lineCustomDataValue in DataContext.LineCustomDataValues)
                {
                    if (lineCustomDataValue.LineId == lineId)
                    {
                        switch (customData.CustomDataType)
                        {
                            case CustomDataType.Preset:
                                CustomPreset customPreset = lineCustomDataValue.Value as CustomPreset;
                                if (customPreset != null)
                                {
                                    return customPreset.CustomPresetId.ToString();
                                }
                                break;
                            case CustomDataType.Property:
                                if (lineCustomDataValue.Value != null)
                                {
                                    if (lineCustomDataValue.Value is CustomValue)
                                        return ((CustomValue)lineCustomDataValue.Value).Name;
                                    else
                                        return Convert.ToString(lineCustomDataValue.Value, CultureInfo.InvariantCulture);
                                }
                                break;
                            default:
                                throw new NotSupportedException("Not supported custom data type: " + customData.CustomDataType);
                        }
                    }
                }

                switch (customData.CustomDataType)
                {
                    case CustomDataType.Preset:
                        CustomPreset customPreset = customData.DefaultValue as CustomPreset;
                        if (customPreset != null)
                        {
                            return customPreset.CustomPresetId.ToString();
                        }
                        break;
                    case CustomDataType.Property:
                        if ( customData.DefaultValue != null)
                        {
                            if (customData.DefaultValue is CustomValue)
                                return ((CustomValue)customData.DefaultValue).Name;
                            else
                                return Convert.ToString(customData.DefaultValue, CultureInfo.InvariantCulture);
                        }
                        break;
                    default:
                        throw new NotSupportedException("Not supported custom data type: " + customData.CustomDataType);
                }
            }
            return "";
        }


        private void WriteCustomData(XmlWriter writer)
        {
            writer.WriteStartElement("customData");

            WriteCustomColumns(writer);
            WriteLineCustomValues(writer);
            WriteSceneCustomValues(writer);
            WriteProperties(writer);
            WritePresetTypes(writer);
            WritePresets(writer);

            writer.WriteEndElement();
        }

        private void WritePresets(XmlWriter writer)
        {
            writer.WriteStartElement("presets");
            foreach (CustomPreset customPreset in DataContext.CustomPresets)
            {
                writer.WriteStartElement("p");
                writer.WriteAttributeString("id", customPreset.CustomPresetId.ToString());
                writer.WriteAttributeString("name", customPreset.Name);
                writer.WriteAttributeString("presetTypeId", customPreset.CustomPresetTypeId.ToString());

                if (customPreset.Values != null)
                {
                    foreach (CustomPresetProperty customPresetProperty in customPreset.Values)
                    {
                        writer.WriteStartElement("value");
                        writer.WriteAttributeString("name", customPresetProperty.Name);
                        writer.WriteAttributeString("propertyId", customPresetProperty.CustomPropertyId.ToString());

                        if (customPresetProperty.Value != null)
                        {
                            CustomValue customValue = customPresetProperty.Value as CustomValue;
                            if (customValue == null)
                                writer.WriteValue(customPresetProperty.Value);
                            else
                                writer.WriteValue(customValue.Name);
                        }

                        writer.WriteEndElement();
                    }
                }

                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        private void WritePresetTypes(XmlWriter writer)
        {
            writer.WriteStartElement("presetTypes");
            foreach (CustomPresetType customPresetType in DataContext.CustomPresetTypes)
            {
                writer.WriteStartElement("pt");
                writer.WriteAttributeString("id", customPresetType.CustomPresetTypeId.ToString());
                writer.WriteAttributeString("name", customPresetType.Name);

                if (customPresetType.Fields != null)
                {
                    foreach (CustomPresetTypeProperty customPresetTypeProperty in customPresetType.Fields)
                    {
                        writer.WriteStartElement("value");
                        writer.WriteAttributeString("name", customPresetTypeProperty.Name);
                        writer.WriteAttributeString("propertyId", customPresetTypeProperty.CustomPropertyId.ToString());
                        writer.WriteEndElement();
                    }
                }

                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        private void WriteProperties(XmlWriter writer)
        {
            writer.WriteStartElement("properties");
            foreach (CustomProperty customProperty in DataContext.CustomProperties)
            {
                writer.WriteStartElement("pr");
                writer.WriteAttributeString("id", customProperty.CustomPropertyId.ToString());
                writer.WriteAttributeString("name", customProperty.Name);

                if (customProperty.CustomPropertyType == CustomPropertyType.Enum)
                {
                    IList<CustomValue> customValues;
                    if (DataContext.CustomValuesByCustomPropertyId.TryGetValue(customProperty.CustomPropertyId, out customValues))
                    {
                        foreach (CustomValue customValue in customValues)
                        {
                            writer.WriteStartElement("value");
                            writer.WriteAttributeString("name", customValue.Name);
                            writer.WriteEndElement();
                        }
                    }
                }

                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        private void WriteLineCustomValues(XmlWriter writer)
        {
            writer.WriteStartElement("lineCustomValues");
            foreach (LineCustomDataValue lineCustomDataValue in DataContext.LineCustomDataValues)
            {
                CustomData customData;
                if (!DataContext.CustomDataById.TryGetValue(lineCustomDataValue.CustomDataId, out customData))
                    continue;

                switch (customData.CustomDataType)
                {
                    case CustomDataType.Preset:
                        CustomPreset customPreset = lineCustomDataValue.Value as CustomPreset;
                        if (customPreset != null)
                        {
                            writer.WriteStartElement("lcv");
                            writer.WriteAttributeString("lineId", lineCustomDataValue.LineId.ToString());
                            writer.WriteAttributeString("columnId", lineCustomDataValue.CustomDataId.ToString());
                            writer.WriteAttributeString("presetId", customPreset.CustomPresetId.ToString());
                            writer.WriteEndElement();
                        }
                        break;
                    case CustomDataType.Property:
                        if (lineCustomDataValue.Value != null)
                        {
                            writer.WriteStartElement("lcv");
                            writer.WriteAttributeString("lineId", lineCustomDataValue.LineId.ToString());
                            writer.WriteAttributeString("columnId", lineCustomDataValue.CustomDataId.ToString());

                            if (lineCustomDataValue.Value is CustomValue)
                                writer.WriteAttributeString("propertyValue", ((CustomValue)lineCustomDataValue.Value).Name);
                            else
                                writer.WriteAttributeString("propertyValue", Convert.ToString(lineCustomDataValue.Value, CultureInfo.InvariantCulture));

                            writer.WriteEndElement();
                        }
                        break;
                    default:
                        throw new NotSupportedException("Not supported custom data type: " + customData.CustomDataType);
                }
            }
            writer.WriteEndElement();
        }

        private void WriteSceneCustomValues(XmlWriter writer)
        {
            writer.WriteStartElement("dialogCustomValues");
            foreach (SceneCustomDataValue sceneCustomDataValue in DataContext.SceneCustomDataValues)
            {
                CustomData customData;
                if (!DataContext.CustomDataById.TryGetValue(sceneCustomDataValue.CustomDataId, out customData))
                    continue;

                switch (customData.CustomDataType)
                {
                    case CustomDataType.Preset:
                        CustomPreset customPreset = sceneCustomDataValue.Value as CustomPreset;
                        if (customPreset != null)
                        {
                            writer.WriteStartElement("dcv");
                            writer.WriteAttributeString("dialogId", sceneCustomDataValue.SceneId.ToString());
                            writer.WriteAttributeString("columnId", sceneCustomDataValue.CustomDataId.ToString());
                            writer.WriteAttributeString("presetId", customPreset.CustomPresetId.ToString());
                            writer.WriteEndElement();
                        }
                        break;
                    case CustomDataType.Property:
                        if (sceneCustomDataValue.Value != null)
                        {
                            writer.WriteStartElement("dcv");
                            writer.WriteAttributeString("dialogId", sceneCustomDataValue.SceneId.ToString());
                            writer.WriteAttributeString("columnId", sceneCustomDataValue.CustomDataId.ToString());

                            if (sceneCustomDataValue.Value is CustomValue)
                                writer.WriteAttributeString("propertyValue", ((CustomValue)sceneCustomDataValue.Value).Name);
                            else
                                writer.WriteAttributeString("propertyValue", Convert.ToString(sceneCustomDataValue.Value, CultureInfo.InvariantCulture));

                            writer.WriteEndElement();
                        }
                        break;
                    default:
                        throw new NotSupportedException("Not supported custom data type: " + customData.CustomDataType);
                }
            }
            writer.WriteEndElement();
        }

        private void WriteCustomColumns(XmlWriter writer)
        {
            writer.WriteStartElement("customColumns");
            foreach (CustomData customColumn in DataContext.CustomDatas)
            {
                writer.WriteStartElement("cc");
                writer.WriteAttributeString("id", customColumn.CustomDataId.ToString());
                writer.WriteAttributeString("name", customColumn.Name);

                switch (customColumn.CustomDataType)
                {
                    case CustomDataType.Preset:
                        writer.WriteAttributeString("presetTypeId", customColumn.CustomPresetType.CustomPresetTypeId.ToString());
                        if (customColumn.DefaultValue != null && customColumn.DefaultValue is CustomPreset)
                        {
                            CustomPreset customPreset = (CustomPreset)customColumn.DefaultValue;
                            writer.WriteAttributeString("defaultPresetId", customPreset.CustomPresetId.ToString());
                        }
                        break;
                    case CustomDataType.Property:
                        writer.WriteAttributeString("propertyId", customColumn.CustomProperty.CustomPropertyId.ToString());
                        if (customColumn.DefaultValue != null)
                        {
                            CustomValue customValue = customColumn.DefaultValue as CustomValue;
                            writer.WriteAttributeString("defaultPropertyValue", customValue == null ?
                                customColumn.DefaultValue.ToString() : customValue.Name);
                        }
                        break;
                    default:
                        throw new NotSupportedException(string.Format("Not supported custom data type: {0}", customColumn.CustomDataType));
                }

                writer.WriteStartAttribute("readOnly");
                writer.WriteValue(customColumn.IsReadonly);
                writer.WriteEndAttribute();
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        private static void StreamToFile(string path, Stream stream)
        {
            byte[] buffer;

            using (BinaryReader binaryReader = new BinaryReader(stream))
                buffer = binaryReader.ReadBytes((int)stream.Length);

            File.WriteAllBytes(path, buffer);
        }

        private static void WriteAttributeString(XmlWriter xmlWriter, string localName, string value)
        {
            xmlWriter.WriteAttributeString(localName, Validation.ReplaceInvalidXmlChars(value));
        }
    }
}
