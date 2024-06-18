using System;
using TBLKoWizard.Encryption.V1;

namespace TBLKoWizard.Encryption
{
    public static class EncryptionFactory
    {
        public static IEncryption CreateEncryptor(int version)
        {
            if (version <= 1400)
            {
                return new EncryptionKOStandardV1();
            }
            else if (version <= 1800)
            {
                throw new ArgumentException("The implementation of this encryption has not been developed yet.");
            }
            else
            {
                throw new ArgumentException("The implementation of this encryption has not been developed yet.");
            }
        }
    }
}
