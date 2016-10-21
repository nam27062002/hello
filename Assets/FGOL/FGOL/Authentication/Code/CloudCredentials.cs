using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FGOL.Authentication
{
    public class CloudCredentials
    {
        private Dictionary<string, string> m_values = new Dictionary<string, string>();
        private int m_expiry = -1;

        public bool isValid
        {
            get
            {
                // [DGR] SERVER: To receive credentials from server
                //return m_values != null && m_expiry != -1 && m_values.ContainsKey("Expiration") && m_values.ContainsKey("SecretAccessKey") && m_values.ContainsKey("AccessKeyId") && m_values.ContainsKey("BaseURL") && m_values.ContainsKey("S3Region");
                return true;
            }
        }

        public Dictionary<string, string> values
        {
            get { return m_values; }
        }

        public int expiry
        {
            get { return m_expiry; }
            set { m_expiry = value; }
        }
    }
}
