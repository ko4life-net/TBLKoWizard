using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoTblDbImporter.Encryption
{
    public static class EncryptionFactory
    {
        public static IEncryption CreateEncryptor(int version)
        {
            if (version <= 1400)
            {
                throw new ArgumentException("The implementation of this encryption has not been developed yet.");
                //return new EncryptionKOStandardV1();
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
