namespace Ubi.Tools.Oasis.WebServices.XmlExtractor.Extractor
{
    /// <summary>
    /// This interface provides services to extract data from a provider to another.
    /// </summary>
    public interface IExtractor
    {
        /// <summary>
        /// Performs the data extraction.
        /// </summary>
        /// <returns>True if successful, otherwise false.</returns>
        bool Extract();
    }
}
