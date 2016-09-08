using FGOL.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;

namespace FGOL.Server
{
    public class Commander : AutoGeneratedSingleton<Commander>
    {
        public delegate void BeforeCommandComplete(Error error);
        public delegate void BeforeCommand(Command command, Dictionary<string, string> parameters, BeforeCommandComplete callback);
        public delegate void AfterCommand(Command command, Dictionary<string, string> parameters, Error error, Dictionary<string, object> result, Action<Error, Dictionary<string, object>> callback, int retries);

        private Dictionary<string, Command> m_commands = new Dictionary<string, Command>();
        private BeforeCommand m_beforeCommand = null;
        private AfterCommand m_afterCommand = null;
        private string m_sslValidationDomain = null;

        private static Hashtable ms_globalHeaders = new Hashtable();

        public static Hashtable GlobalHeaders
        {
            get { return ms_globalHeaders; }
        }

        public void Init(List<Command> commands, BeforeCommand beforeCallback, AfterCommand afterCallback, string sslValidationDomain = null)
        {
            commands.ForEach(delegate(Command command)
            {
                if(!m_commands.ContainsKey(command.Name))
                {
                    m_commands.Add(command.Name, command);
                }
                else
                {
                    throw new Exception("Commander already contains command with name: " + command.Name);
                }
            });

            m_beforeCommand = beforeCallback;
            m_afterCommand = afterCallback;
            m_sslValidationDomain = sslValidationDomain;
        }

        public void RunCommand(string command, Dictionary<string, string> parameters, Action<Error, Dictionary<string, object>> callback, int retries = 0)
        {
            Command currentCommand = null;
            m_commands.TryGetValue(command, out currentCommand);

            if(currentCommand != null)
            {
                RunCommand(currentCommand, parameters, callback, retries);
            }
            else
            {
                throw new ArgumentException("Unable to run unknown command: " + command);
            }
        }

        public void RunCommand(Command command, Dictionary<string, string> parameters, Action<Error, Dictionary<string, object>> callback, int retries = 0)
        {
            //Make sure we have a valid parameters object as before or after command callbacks may modify it
            if(parameters == null)
            {
                parameters = new Dictionary<string, string>();
            }

            BeforeCommandComplete runCommand = delegate(Error beforeError)
            {
                if(beforeError == null)
                {
                    command.Run(parameters, ms_globalHeaders, delegate(Error error, Dictionary<string, object> result)
                    {
                        if(m_afterCommand != null)
                        {
                            m_afterCommand(command, parameters, error, result, callback, retries);
                        }
                        else
                        {
                            callback(error, result);
                        }
                    }, m_sslValidationDomain);
                }
                else
                {
                    callback(beforeError, null);
                }
            };

            if(m_beforeCommand != null)
            {
                m_beforeCommand(command, parameters, runCommand);
            }
            else
            {
                runCommand(null);
            }
        }

        public static bool IsValidResponse(Dictionary<string, object> response, string[] parameters)
        {
            bool validResponse = true;

            foreach(string parameter in parameters)
            {
                validResponse &= response.ContainsKey(parameter);
            }

            return validResponse;
        }
    }
}
