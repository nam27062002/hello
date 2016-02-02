using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;
using Ubi.Tools.Oasis.Shared.Helpers;
using Ubi.Tools.Oasis.Shared.PowerCollections;
using Ubi.Tools.Oasis.WebServices.XmlExtractor.Extractor;
using Ubi.Tools.Oasis.WebServices.XmlExtractor.Helpers;
using Ubi.Tools.Oasis.WebServices.XmlExtractor.OasisService;

namespace Ubi.Tools.Oasis.WebServices.XmlExtractor
{
    class Program
    {
        private static readonly ExtractorToolCmdLineHelper _cmdLineHelper = new ExtractorToolCmdLineHelper();
        private static OasisServiceClient _client;
        private static string _version;

        public static string GetOasisVersion
        {
            get
            {
                if (string.IsNullOrEmpty(_version))
                    _version = _client.GetDatabaseVersion().ToString();

                return _version;
            }
        }

        public static string GetToolVersion
        {
            get { return "2.0.0"; }
        }

        static int Main(string[] args)
        {
            // Extract commands from the configuration file
            IList<Command> commands = ReadAppConfig();

            // Skip the command line if configuration file contains commands
            if (commands == null || commands.Count == 0)
            {
                int returnValue = _cmdLineHelper.ExtractCommands(args, out commands);

                if (returnValue >= 0)
                    return returnValue;
            }

            int returnCode = _cmdLineHelper.ValidateCommands(commands);

            if (returnCode >= 0)
                return returnCode;

            // assume now the rest failed, and set proper result on success
            returnCode = 1;

            // Retrieve host address
            Command hostCommand = commands.First(command => command.Name == "host");
            string host = hostCommand.Args[0];

            //Build Oasis Service Endpoint Address
            string endpointAddress = string.Concat(host.TrimEnd('/'), "/WebServices/OasisService.svc");

            // Retrieve export directory
            Command directoryCommand = commands.FirstOrDefault(command => command.Name == "directory");
            string directory = directoryCommand == null ? null : directoryCommand.Args[0];

            try
            {
                _client = new OasisServiceClient(CreateOasisBinding(), new EndpointAddress(endpointAddress));
                _client.Open();

                // Start the extraction of data
                
                // IExtractor extractor = new Extractor.XmlExtractor(_client, directory);
                IExtractor extractor = new Extractor.TidExtractor(_client, directory);

                if (extractor.Extract())
                    returnCode = 0;
                
            }
            catch (FaultException<ServiceFault> ex)
            {
                // Only if a fault contract was specified 
                ServiceFault fault = ex.Detail;

                Console.WriteLine(fault.FaultMessage);
                Console.WriteLine(fault.Operation);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (_client != null)
                    _client.Abort();
            }

            return returnCode;
        }

        public static void DisplayException(Exception exception)
        {
            _cmdLineHelper.DisplayException(exception);
        }

        private static IList<Command> ReadAppConfig()
        {
            List<Command> commands = new List<Command>();
            Set<string> keys = new Set<string>(ConfigurationManager.AppSettings.AllKeys, StringComparer.InvariantCultureIgnoreCase);

            foreach (Command command in _cmdLineHelper.PossibleCommands.Where(command => keys.Contains(command.Name)))
            {
                if (command.Name == "?")
                    return new[] { command };

                if (command.Argc > 0)
                {
                    string arg = ConfigurationManager.AppSettings[command.Name];

                    if (string.IsNullOrEmpty(arg))
                        continue;

                    command.Args.Add(arg);
                }

                commands.Add(command);
            }

            return commands;
        }

        private static Binding CreateOasisBinding()
        {
            TimeSpan timeoutSpan = new TimeSpan(0, 0, 3, 0);

            return new BasicHttpBinding
            {
                SendTimeout = timeoutSpan,
                ReceiveTimeout = timeoutSpan,
                OpenTimeout = timeoutSpan,
                CloseTimeout = timeoutSpan,

                MaxBufferPoolSize = int.MaxValue,
                MaxBufferSize = int.MaxValue,
                MaxReceivedMessageSize = int.MaxValue,

                Security =
                {
                    Mode = BasicHttpSecurityMode.TransportCredentialOnly,
                    Transport = new HttpTransportSecurity
                    {
                        ClientCredentialType = HttpClientCredentialType.Ntlm
                    }
                },

                ReaderQuotas = new XmlDictionaryReaderQuotas
                {
                    MaxArrayLength = int.MaxValue,
                    MaxBytesPerRead = int.MaxValue,
                    MaxDepth = int.MaxValue,
                    MaxNameTableCharCount = int.MaxValue,
                    MaxStringContentLength = int.MaxValue,
                },
            };
        }
    }
}
