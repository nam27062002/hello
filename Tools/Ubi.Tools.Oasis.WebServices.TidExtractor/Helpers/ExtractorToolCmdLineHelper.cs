using Ubi.Tools.Oasis.Shared.Helpers;

namespace Ubi.Tools.Oasis.WebServices.XmlExtractor.Helpers
{
    sealed class ExtractorToolCmdLineHelper : CommandLineHelper
    {
        private static readonly Command[] _possibleCommands = new[]
        {
            new Command("host", 1, "The host address to connect to the Oasis database.", true),
            new Command("directory", 1, "The directory path used to drop exported files.", false),
            new Command("verbose", 0, "Displays error details.", false),
            new Command("?", 0, "Displays help information for this tool.", false),
        };

        public override string ToolName
        {
            get { return "Extractor Tool"; }
        }

        public ExtractorToolCmdLineHelper()
            : base(_possibleCommands)
        {
        }

        public override string GetOasisVersion()
        {
            return string.Empty;
        }
    }
}
