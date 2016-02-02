using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.ServiceModel;
using Ubi.Tools.Oasis.Shared.Collections.Generic;
using Ubi.Tools.Oasis.Shared.Comparers;
using Ubi.Tools.Oasis.WebServices.XmlExtractor.OasisService;

namespace Ubi.Tools.Oasis.WebServices.XmlExtractor
{
    /// <summary>
    /// This class stores the data into a cache so that subsequent calls to the same
    /// data can be served more quickly (i.e. without contacting the server). It also
    /// provides services to arrange the data in a more structured way and for easy
    /// access.
    /// </summary>
    internal sealed class DataContext
    {
        private readonly OasisServiceClient _client;

        private IList<AIElement> _aiElements;
        private IList<AIElementSection> _aiElementSections;
        private IList<CustomData> _customDatas;
        private IList<CustomProperty> _customProperties;
        private IList<CustomValue> _customValues;
        private IList<Character> _characters;
        private IList<Scene> _dialogs;
        private IList<Language> _languages;
        private IList<Line> _lines;
        private IList<Control> _menuControls;
        private IList<Team> _teams;
        private IList<Section> _sections;
        private IList<CustomPreset> _customPresets;
        private IList<CustomPresetType> _customPresetTypes;
        private IList<LineCustomDataValue> _lineCustomDataValues;
        private IList<SceneCustomDataValue> _sceneCustomDataValues;
        private IList<LineTranslation> _lineTranslations;

        private IDictionary<int, AIElement> _aiElementsById;
        private IDictionary<int, IList<AIElementSection>> _aiElementSectionsByParentId;
        private IDictionary<int, bool> _isRecordingRequired;
        private IDictionary<int, IDictionary<int, bool>> _isTranslationRequired;
        private IDictionary<KeyValuePair<int, int>, LineTranslation> _lineTranslationByLineIdLanguageId;
        private IDictionary<int, string> _audioFileNameByLineId;
        private IDictionary<int, Character> _charactersById;
        private IDictionary<int, CustomData> _customDataById;
        private IDictionary<string, CustomData> _customDataByName;
        private IDictionary<int, CustomValue> _customValuesById;
        private IDictionary<int, IList<CustomValue>> _customValuesByCustomPropertyId;
        private IDictionary<int, IList<Scene>> _scenesBySectionId;
        private IDictionary<int, IList<Scene>> _scenesByAIElementSectionId;
        private IDictionary<int, IList<Control>> _menuControlsBySectionId;
        private IDictionary<int, IList<Line>> _linesByDialogId;
        private IDictionary<int, Team> _teamsById;
        private IDictionary<SectionType, IList<Section>> _rootSections;
        private IDictionary<int, IList<Section>> _sectionsByParentId;

        public DataContext(OasisServiceClient client)
        {
            if (client == null)
                throw new ArgumentNullException("client");
            if (client.State != CommunicationState.Opened)
                throw new ArgumentException("The client state is not supported: " + client.State, "client");

            _client = client;
        }

        public IList<AIElement> AIElements
        {
            get
            {
                if (_aiElements == null)
                {
                    GetAIElementsResponse result = _client.GetAIElements(new GetAIElementsRequest());
                    if (result.Success)
                        _aiElements = result.Result == null ?
                            ToReadOnly(new List<AIElement>()) :
                            ToReadOnly(result.Result.ToList());
                }

                return _aiElements;
            }
        }

        public IDictionary<int, AIElement> AIElementsById
        {
            get { return _aiElementsById ?? (_aiElementsById = ToReadOnly(AIElements.ToDictionary(aiElement => aiElement.AIElementId))); }
        }

        public IList<AIElementSection> AIElementSections
        {
            get
            {
                if (_aiElementSections == null)
                {
                    GetAIElementSectionsResponse result = _client.GetAIElementSections(new GetAIElementSectionsRequest());
                    if (result.Success)
                        _aiElementSections = result.Result == null ?
                            ToReadOnly(new List<AIElementSection>()) :
                            ToReadOnly(result.Result.ToList());
                }

                return _aiElementSections;
            }
        }

        public string GetAIElementSectionName(AIElementSection aiElementSection)
        {
            switch (aiElementSection.Type)
            {
                case AIElementType.AIElement:
                    return AIElementsById[aiElementSection.AIElementId].Name;
                case AIElementType.Character:
                    return CharactersById[aiElementSection.AIElementId].Name;
                case AIElementType.Team:
                    return TeamsById[aiElementSection.AIElementId].Name;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public string GetAIElementSectionTag(AIElementSection aiElementSection)
        {
            switch (aiElementSection.Type)
            {
                case AIElementType.AIElement:
                    return AIElementsById[aiElementSection.AIElementId].Tag;
                case AIElementType.Character:
                    return CharactersById[aiElementSection.AIElementId].Tag;
                case AIElementType.Team:
                    return TeamsById[aiElementSection.AIElementId].Tag;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public IDictionary<int, IList<AIElementSection>> AIElementSectionsByParentId
        {
            get { return _aiElementSectionsByParentId ?? (_aiElementSectionsByParentId = ToReadOnlyMultiDictionary(AIElementSections, s => s.ParentId, new AIElementSectionComparer(GetAIElementSectionName))); }
        }

        public IList<CustomData> CustomDatas
        {
            get
            {
                if (_customDatas == null)
                {
                    GetCustomDatasResponse result = _client.GetCustomDatas(new GetCustomDatasRequest());

                    if (result.Success)
                        _customDatas = result.Result == null ?
                            ToReadOnly(new List<CustomData>()) :
                            ToReadOnly(result.Result.OrderBy(r => r.Name, AlphaNumericStringComparer.Default).ToList());
                }

                return _customDatas;
            }
        }

        public IList<CustomProperty> CustomProperties
        {
            get
            {
                if (_customProperties == null)
                {
                    GetCustomPropertiesResponse result = _client.GetCustomProperties(new GetCustomPropertiesRequest());

                    if (result.Success)
                        _customProperties = result.Result == null ?
                            ToReadOnly(new List<CustomProperty>()) :
                            ToReadOnly(result.Result.OrderBy(r => r.Name, AlphaNumericStringComparer.Default).ToList());
                }

                return _customProperties;
            }
        }

        public IList<CustomValue> CustomValues
        {
            get
            {
                if (_customValues == null)
                {
                    GetCustomValuesResponse result = _client.GetCustomValues(new GetCustomValuesRequest());

                    if (result.Success)
                        _customValues = result.Result == null ?
                            ToReadOnly(new List<CustomValue>()) :
                            ToReadOnly(result.Result.ToList());
                }

                return _customValues;
            }
        }

        public IList<Control> MenuControls
        {
            get
            {
                if (_menuControls == null)
                {
                    GetMenuControlsResponse result = _client.GetMenuControls(new GetMenuControlsRequest());

                    if (result.Success)
                        _menuControls = result.Result == null ?
                            ToReadOnly(new List<Control>()) :
                            ToReadOnly(result.Result.ToList());
                }

                return _menuControls;
            }
        }

        public IDictionary<int, CustomData> CustomDataById
        {
            get { return _customDataById ?? (_customDataById = ToReadOnly(CustomDatas.ToDictionary(c => c.CustomDataId))); }
        }

        public IDictionary<string, CustomData> CustomDataByName
        {
            get { return _customDataByName ?? (_customDataByName = ToReadOnly(CustomDatas.ToDictionary(c => c.Name))); }
        }

        public IDictionary<int, CustomValue> CustomValuesById
        {
            get { return _customValuesById ?? (_customValuesById = ToReadOnly(CustomValues.ToDictionary(cv => cv.CustomValueId))); }
        }

        public IDictionary<int, IList<CustomValue>> CustomValuesByCustomPropertyId
        {
            get { return _customValuesByCustomPropertyId ?? (_customValuesByCustomPropertyId = ToReadOnlyMultiDictionary(CustomValues, cv => cv.CustomPropertyId, CustomValueComparer.Default)); }
        }

        public IList<Character> Characters
        {
            get
            {
                if (_characters == null)
                {
                    GetCharactersResponse result = _client.GetCharacters(new GetCharactersRequest());

                    if (result.Success)
                        _characters = result.Result == null ?
                            ToReadOnly(new List<Character>()) :
                            ToReadOnly(result.Result.OrderBy(c => c.Name, AlphaNumericStringComparer.Default).ToList());
                }

                return _characters;
            }
        }

        public IDictionary<int, Character> CharactersById
        {
            get { return _charactersById ?? (_charactersById = ToReadOnly(Characters.ToDictionary(c => c.CharacterId))); }
        }

        public IList<Scene> Scenes
        {
            get
            {
                if (_dialogs == null)
                {
                    GetScenesResponse result = _client.GetScenes(new GetScenesRequest());
                    if (result.Success)
                        _dialogs = result.Result == null ?
                            ToReadOnly(new List<Scene>()) :
                            ToReadOnly(result.Result.ToList());
                }

                return _dialogs;
            }
        }

        public IList<Language> Languages
        {
            get
            {
                if (_languages == null)
                {
                    GetLanguagesResponse result = _client.GetLanguages(new GetLanguagesRequest());
                    if (result.Success)
                        _languages = result.Result == null ?
                            ToReadOnly(new List<Language>()) :
                            ToReadOnly(result.Result.OrderBy(l => l.Name, AlphaNumericStringComparer.Default).ToList());
                }

                return _languages;
            }
        }

        public IList<Line> Lines
        {
            get
            {
                if (_lines == null)
                {
                    GetLinesResponse result = _client.GetLines(new GetLinesRequest());
                    if (result.Success)
                        _lines = result.Result == null ?
                            ToReadOnly(new List<Line>()) :
                            ToReadOnly(result.Result.ToList());
                }

                return _lines;
            }
        }

        public IList<LineTranslation> LineTranslations
        {
            get
            {
                if (_lineTranslations == null)
                {
                    GetLineTranslationsResponse result = _client.GetLineTranslations(new GetLineTranslationsRequest());
                    if (result.Success)
                        _lineTranslations = result.Result == null ?
                            ToReadOnly(new List<LineTranslation>()) :
                            ToReadOnly(result.Result.ToList());
                }

                return _lineTranslations;
            }
        }

        public LineTranslation GetLineTranslation(int lineId, int languageId)
        {
            if (_lineTranslationByLineIdLanguageId == null)
                _lineTranslationByLineIdLanguageId = ToReadOnly(LineTranslations.ToDictionary(t => new KeyValuePair<int, int>(t.LineId, t.LanguageId)));

            LineTranslation lineTranslation;
            return _lineTranslationByLineIdLanguageId.TryGetValue(new KeyValuePair<int, int>(lineId, languageId), out lineTranslation) ? lineTranslation : null;
        }

        public IList<Team> Teams
        {
            get
            {
                if (_teams == null)
                {
                    GetTeamsResponse result = _client.GetTeams(new GetTeamsRequest());
                    if (result.Success)
                        _teams = result.Result == null ?
                            ToReadOnly(new List<Team>()) :
                            ToReadOnly(result.Result.OrderBy(t => t.Name, AlphaNumericStringComparer.Default).ToList());
                }

                return _teams;
            }
        }

        public IDictionary<int, Team> TeamsById
        {
            get { return _teamsById ?? (_teamsById = ToReadOnly(Teams.ToDictionary(t => t.TeamId))); }
        }

        /// <summary>
        /// Verifies if the specified line needs to be recorded in at least one language.
        /// </summary>
        /// <remarks>
        /// Be careful, it does NOT indicate whether or not the line has been recorded.
        /// </remarks>
        /// <param name="lineId">The line ID.</param>
        /// <returns>
        /// True if the specified line needs to be recorded in at least one
        /// language. Otherwise, false.
        /// </returns>
        public bool IsRecordingRequired(int lineId)
        {
            if (_isRecordingRequired == null)
            {
                IsRecordingRequiredResponse result = _client.IsRecordingRequired(new IsRecordingRequiredRequest());
                if (result.Success)
                    _isRecordingRequired = result.IsRecordingRequired == null ?
                        ToReadOnly(new System.Collections.Generic.Dictionary<int, bool>()) :
                        ToReadOnly(result.IsRecordingRequired);
            }

            bool isRecordingRequired;
            if (_isRecordingRequired == null || !_isRecordingRequired.TryGetValue(lineId, out isRecordingRequired))
                return false;

            return isRecordingRequired;
        }

        /// <summary>
        /// Verifies if the specified line needs to be translated in the specified language.
        /// </summary>
        /// <remarks>
        /// Be careful, it does NOT indicate whether or not the line has been translated.
        /// </remarks>
        /// <param name="lineId">The line ID.</param>
        /// <param name="languageId">The language ID.</param>
        /// <returns>
        /// True if the specified line needs to be translated in the specified 
        /// language. Otherwise, false.
        /// </returns>
        public bool IsTranslationRequired(int lineId, int languageId)
        {
            IDictionary<int, bool> translationRequired;

            if (_isTranslationRequired == null)
                _isTranslationRequired = new System.Collections.Generic.Dictionary<int, IDictionary<int, bool>>();

            if (!_isTranslationRequired.TryGetValue(languageId, out translationRequired))
            {
                IsTranslationRequiredResponse result = _client.IsTranslationRequired(new IsTranslationRequiredRequest { LanguageId = languageId });
                if (result.Success)
                {
                    translationRequired = ToReadOnly(result.IsTranslationRequired);
                    _isTranslationRequired.Add(languageId, translationRequired);
                }
            }

            bool isTranslationRequired;
            if (translationRequired == null || !translationRequired.TryGetValue(lineId, out isTranslationRequired))
                return false;

            return isTranslationRequired;
        }

        public IDictionary<int, string> AudioFileNameByLineId
        {
            get
            {
                if (_audioFileNameByLineId == null)
                {
                    GetAudioFilenamesResponse result = _client.GetAudioFilenames(new GetAudioFilenamesRequest());
                    if (result.Success)
                        _audioFileNameByLineId = result.AudioFileNames == null ?
                            ToReadOnly(new System.Collections.Generic.Dictionary<int, string>()) :
                            ToReadOnly(result.AudioFileNames);
                }

                return _audioFileNameByLineId;
            }
        }

        public IDictionary<int, IList<Scene>> ScenesBySectionId
        {
            get { return _scenesBySectionId ?? (_scenesBySectionId = ToReadOnlyMultiDictionary(Scenes.Where(s => s.SectionId > 0), s => s.SectionId, SceneComparer.Default)); }
        }

        public IDictionary<int, IList<Scene>> ScenesByAIElementSectionId
        {
            get { return _scenesByAIElementSectionId ?? (_scenesByAIElementSectionId = ToReadOnlyMultiDictionary(Scenes.Where(s => s.AISectionId > 0), s => s.AISectionId, SceneComparer.Default)); }
        }

        public IDictionary<int, IList<Control>> MenuControlsBySectionId
        {
            get { return _menuControlsBySectionId ?? (_menuControlsBySectionId = ToReadOnlyMultiDictionary(MenuControls, control => control.SectionId, ControlComparer.Default)); }
        }

        public IDictionary<int, IList<Line>> LinesByDialogId
        {
            get { return _linesByDialogId ?? (_linesByDialogId = ToReadOnlyMultiDictionary(Lines, line => line.SceneId, LineComparer.Default)); }
        }

        public IList<Section> Sections
        {
            get
            {
                if (_sections == null)
                {
                    GetSectionsResponse result = _client.GetSections(new GetSectionsRequest());
                    if (result.Success)
                        _sections = result.Result == null ? ToReadOnly(new List<Section>()) : ToReadOnly(result.Result.ToList());
                }

                return _sections;
            }
        }

        public IDictionary<int, IList<Section>> SectionsByParentId
        {
            get { return _sectionsByParentId ?? (_sectionsByParentId = ToReadOnlyMultiDictionary(Sections, section => section.ParentId, SectionComparer.Default)); }
        }

        public IList<Section> GetRootSections(SectionType sectionType)
        {
            if (_rootSections == null)
            {
                IList<Section> subSections;
                SectionsByParentId.TryGetValue(-1, out subSections);
                _rootSections = ToReadOnlyMultiDictionary(subSections, section => section.Type, SectionComparer.Default);
            }

            IList<Section> sections;
            return _rootSections.TryGetValue(sectionType, out sections) ? sections : new List<Section>();
        }

        public IList<CustomPreset> CustomPresets
        {
            get
            {
                if (_customPresets == null)
                {
                    GetCustomPresetsResponse result = _client.GetCustomPresets(new GetCustomPresetsRequest());
                    if (result.Success)
                    {
                        _customPresets = result.Result == null ?
                            ToReadOnly(new List<CustomPreset>()) :
                            ToReadOnly(result.Result.OrderBy(r => r.Name, AlphaNumericStringComparer.Default).ToList());
                    }
                }

                return _customPresets;
            }
        }

        public IList<CustomPresetType> CustomPresetTypes
        {
            get
            {
                if (_customPresetTypes == null)
                {
                    GetCustomPresetTypesResponse result = _client.GetCustomPresetTypes(new GetCustomPresetTypesRequest());
                    if (result.Success)
                    {
                        _customPresetTypes = result.Result == null ?
                            ToReadOnly(new List<CustomPresetType>()) :
                            ToReadOnly(result.Result.OrderBy(r => r.Name, AlphaNumericStringComparer.Default).ToList());
                    }
                }

                return _customPresetTypes;
            }
        }

        public IList<LineCustomDataValue> LineCustomDataValues
        {
            get
            {
                if (_lineCustomDataValues == null)
                {
                    GetLineCustomDataValuesResponse result = _client.GetLineCustomDataValues(new GetLineCustomDataValuesRequest());
                    if (result.Success)
                    {
                        _lineCustomDataValues = result.Result == null ?
                            ToReadOnly(new List<LineCustomDataValue>()) :
                            ToReadOnly(result.Result.OrderBy(r => r, LineCustomDataValueComparer.Default).ToList());
                    }
                }

                return _lineCustomDataValues;
            }
        }

        public IList<SceneCustomDataValue> SceneCustomDataValues
        {
            get
            {
                if (_sceneCustomDataValues == null)
                {
                    GetSceneCustomDataValuesResponse result = _client.GetSceneCustomDataValues(new GetSceneCustomDataValuesRequest());
                    if (result.Success)
                    {
                        _sceneCustomDataValues = result.Result == null ?
                            ToReadOnly(new SceneCustomDataValue[] { }) :
                            ToReadOnly(result.Result.OrderBy(r => r, SceneCustomDataValueComparer.Default).ToList());
                    }
                }

                return _sceneCustomDataValues;
            }
        }

        private static IList<T> ToReadOnly<T>(IList<T> source)
        {
            return new ReadOnlyCollection<T>(source);
        }

        private static IDictionary<TKey, TValue> ToReadOnly<TKey, TValue>(System.Collections.Generic.Dictionary<TKey, TValue> source)
        {
            return new ReadOnlyDictionary<TKey, TValue>(source);
        }

        private static IDictionary<TKey, IList<TSource>> ToReadOnlyMultiDictionary<TSource, TKey>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TSource> elementComparer)
        {
            return ToReadOnly(source.GroupBy(keySelector).ToDictionary<IGrouping<TKey, TSource>, TKey, IList<TSource>>(group => group.Key, group =>
            {
                List<TSource> elements = group.ToList();
                elements.Sort(elementComparer);
                return new ReadOnlyCollection<TSource>(elements);
            }));
        }

        #region Comparers

        private sealed class SceneCustomDataValueComparer : IComparer<SceneCustomDataValue>
        {
            public readonly static SceneCustomDataValueComparer Default = new SceneCustomDataValueComparer();

            private SceneCustomDataValueComparer()
            {
            }

            #region IComparer<SceneCustomDataValue> Members

            int IComparer<SceneCustomDataValue>.Compare(SceneCustomDataValue x, SceneCustomDataValue y)
            {
                int compare = x.SceneId.CompareTo(y.SceneId);
                return compare == 0 ? x.CustomDataId.CompareTo(y.CustomDataId) : compare;
            }

            #endregion
        }

        private sealed class LineCustomDataValueComparer : IComparer<LineCustomDataValue>
        {
            public readonly static LineCustomDataValueComparer Default = new LineCustomDataValueComparer();

            private LineCustomDataValueComparer()
            {
            }

            #region IComparer<LineCustomDataValue> Members

            int IComparer<LineCustomDataValue>.Compare(LineCustomDataValue x, LineCustomDataValue y)
            {
                int compare = x.LineId.CompareTo(y.LineId);
                return compare == 0 ? x.CustomDataId.CompareTo(y.CustomDataId) : compare;
            }

            #endregion
        }

        private sealed class SectionComparer : IComparer<Section>
        {
            public readonly static SectionComparer Default = new SectionComparer();

            private SectionComparer()
            {
            }

            #region IComparer<Section> Members

            int IComparer<Section>.Compare(Section x, Section y)
            {
                return x.OrderIndex.CompareTo(y.OrderIndex);
            }

            #endregion
        }

        private sealed class LineComparer : IComparer<Line>
        {
            public readonly static LineComparer Default = new LineComparer();

            private LineComparer()
            {
            }

            #region IComparer<Scene> Members

            int IComparer<Line>.Compare(Line x, Line y)
            {
                return x.OrderIndex.CompareTo(y.OrderIndex);
            }

            #endregion
        }

        private sealed class ControlComparer : IComparer<Control>
        {
            public readonly static ControlComparer Default = new ControlComparer();

            private ControlComparer()
            {
            }

            #region IComparer<Scene> Members

            int IComparer<Control>.Compare(Control x, Control y)
            {
                return x.OrderIndex.CompareTo(y.OrderIndex);
            }

            #endregion
        }

        private sealed class SceneComparer : IComparer<Scene>
        {
            public readonly static SceneComparer Default = new SceneComparer();

            private SceneComparer()
            {
            }

            #region IComparer<Scene> Members

            int IComparer<Scene>.Compare(Scene x, Scene y)
            {
                return x.OrderIndex.CompareTo(y.OrderIndex);
            }

            #endregion
        }

        private sealed class AIElementSectionComparer : IComparer<AIElementSection>
        {
            private readonly Func<AIElementSection, string> _getAIElementSectionName;

            public AIElementSectionComparer(Func<AIElementSection, string> getAIElementSectionName)
            {
                if (getAIElementSectionName == null)
                    throw new ArgumentNullException("getAIElementSectionName");

                _getAIElementSectionName = getAIElementSectionName;
            }

            #region IComparer<AIElementSectionInfo> Members

            public int Compare(AIElementSection x, AIElementSection y)
            {
                return AlphaNumericStringComparer.Compare(_getAIElementSectionName(x), _getAIElementSectionName(y));
            }

            #endregion
        }

        private sealed class CustomValueComparer : IComparer<CustomValue>
        {
            public readonly static CustomValueComparer Default = new CustomValueComparer();

            private CustomValueComparer()
            {
            }

            #region IComparer<CustomValue> Members

            int IComparer<CustomValue>.Compare(CustomValue x, CustomValue y)
            {
                return AlphaNumericStringComparer.Compare(x.Name, y.Name);
            }

            #endregion
        }

        #endregion
    }
}
