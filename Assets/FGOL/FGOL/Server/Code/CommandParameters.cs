using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using FGOL.ThirdParty.MiniJSON;

namespace FGOL.Server
{
    public class CommandParameters
    {
        public enum ParameterType
        {
            Flat,
            Nested
        }

        private ParameterType m_type = ParameterType.Flat;
        private Dictionary<string, object> m_params = null;

        public CommandParameters(ParameterType type = ParameterType.Flat, Dictionary<string, object> rawParams = null)
        {
            m_type = type;

            if(rawParams != null)
            {
                m_params = rawParams;
            }
            else
            {
                m_params = new Dictionary<string, object>();
            }
        }

        public ParameterType Type
        {
            get { return m_type; }
            set { m_type = value; }
        }

        public object this[string key]
        {
            get { return m_params[key]; }
            set { m_params[key] = value; }
        }

		public int Count
		{
			get { return m_params.Count; }
		}

        public WWWForm GetForm()
        {
			WWWForm form = null;

			if (m_params.Count > 0)
			{
				form = new WWWForm();

	            if(m_type == ParameterType.Flat)
	            {
	                foreach(KeyValuePair<string, object> param in m_params)
	                {
	                    if(param.Value is string)
	                    {
	                        form.AddField(param.Key, (string)param.Value);
	                    }
	                    else if(param.Value is int)
	                    {
	                        form.AddField(param.Key, (int)param.Value);
	                    }
	                    else
	                    {
	                        Debug.LogWarning("Type not supported: " + param.Value.GetType());
	                    }
	                }
	            }
	            else
	            {
	                form.AddField("params", Json.Serialize(m_params));
	            }
			}

            return form;
        }
    }
}
