#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;

namespace Org.BouncyCastle.Crypto
{
    /**
     * base interface for general purpose byte derivation functions.
     */
    public interface IDerivationFunction
    {
        void Init(IDerivationParameters parameters);

        /**
         * return the message digest used as the basis for the function
         */
        IDigest Digest
        {
            get;
        }

        int GenerateBytes(byte[] output, int outOff, int length);
        //throws DataLengthException, ArgumentException;
    }

}

#endif
