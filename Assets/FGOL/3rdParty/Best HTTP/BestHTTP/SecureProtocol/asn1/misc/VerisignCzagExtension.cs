#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
using Org.BouncyCastle.Asn1;

namespace Org.BouncyCastle.Asn1.Misc
{
    public class VerisignCzagExtension
        : DerIA5String
    {
        public VerisignCzagExtension(DerIA5String str)
			: base(str.GetString())
        {
        }

        public override string ToString()
        {
            return "VerisignCzagExtension: " + this.GetString();
        }
    }
}

#endif
