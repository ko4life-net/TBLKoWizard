using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBLKoWizard.Encryption
{
    public interface IEncryption
    {
        void Decode(ref byte[] data);
        void Encode(FileStream stream);
        byte[] ProcessFile(string fileName);
        bool LoadByteDataIntoDataSet(byte[] fileData, string tableName, DataSet tblDatabase);
    }

}
