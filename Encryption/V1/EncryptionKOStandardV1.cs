
using System.Data;
using System.Globalization;

namespace KoTblDbImporter.Encryption.V1
{
    public class EncryptionKOStandardV1 : IEncryption
    {
        public void Decode(ref byte[] data)
        {
            uint key = 0x816;
            for (int i = 0; i < data.Length; i++)
            {
                byte currentByte = data[i];
                uint upperPart = key >> 8;
                byte xored = (byte)(upperPart ^ currentByte);
                byte decodedByte = xored;
                key = (currentByte + key) * 0x6081 + 0x1608;
                key &= 0xffff;
                data[i] = decodedByte;
            }
        }
        public void Encode(FileStream stream)
        {
            int currentByte = stream.ReadByte();
            uint key = 0x816;
    
            while (currentByte != -1)
            {
                stream.Seek(-1L, SeekOrigin.Current);
                byte byteToEncode = (byte)(currentByte & 0xff);
                uint upperPart = key >> 8;
                byte encodedByte = (byte)(upperPart ^ byteToEncode);
        
                key = (byteToEncode + key) * 0x6081 + 0x1608;
                key &= 0xffff;
        
                stream.WriteByte(encodedByte);
                currentByte = stream.ReadByte();
            }
        }

        public byte[] ProcessFile(string fileName)
        {
            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                int offset = 0;
                var buffer = new byte[stream.Length];
                while (offset < stream.Length)
                {
                    offset += stream.Read(buffer, offset, (int)stream.Length - offset);
                }
                stream.Close();
                Decode(ref buffer);
                return buffer;
            }
        }

        public bool LoadByteDataIntoDataSet(byte[] fileData, string tableName, DataSet tblDatabase)
        {
            int startIndex = 0;

            int numColumns = BitConverter.ToInt32(fileData, startIndex);

            if (fileData.Length == 0 || numColumns < 0 || (numColumns * 5) > fileData.Length)
            {
                Console.WriteLine("Not a standard tbl file.");
                return false;
            }

            string table_name = tableName;
            startIndex += 4;
            var columnTypesArray = new int[numColumns];
            var table = new DataTable(table_name);
            for (int i = 0; i < numColumns; i++)
            {
                DataColumn column;
                int columnType = BitConverter.ToInt32(fileData, startIndex);
                columnTypesArray[i] = columnType;
                string columnName = i.ToString(CultureInfo.InvariantCulture);


                switch (columnType)
                {
                    case 1:
                        column = new DataColumn(columnName, typeof(sbyte)) { DefaultValue = (sbyte)0 };
                        break;
                    case 2:
                        column = new DataColumn(columnName, typeof(byte)) { DefaultValue = (byte)0 };
                        break;
                    case 3:
                        column = new DataColumn(columnName, typeof(short)) { DefaultValue = (short)0 };
                        break;
                    case 5:
                        column = new DataColumn(columnName, typeof(int)) { DefaultValue = 0 };
                        break;
                    case 6:
                        column = new DataColumn(columnName, typeof(uint)) { DefaultValue = 0 };
                        break;
                    case 7:
                        column = new DataColumn(columnName, typeof(string)) { DefaultValue = "" };
                        break;
                    case 8:
                        column = new DataColumn(columnName, typeof(float)) { DefaultValue = 0f };
                        break;

                    default:
                        column = new DataColumn(columnName, typeof(int));

                        break;
                }
                table.Columns.Add(column);
                startIndex += 4;
            }

            int numRows = BitConverter.ToInt32(fileData, startIndex);
            startIndex += 4;
            for (int j = 0; j < numRows && startIndex < fileData.Length; j++)
            {
                DataRow row = table.NewRow();
                for (int k = 0; k < numColumns && startIndex < fileData.Length; k++)
                {
                    int stringLength;
                    switch (columnTypesArray[k])
                    {
                        case 1:
                            {
                                row[k] = (sbyte)fileData[startIndex];
                                startIndex++;
                                continue;
                            }
                        case 2:
                            {
                                row[k] = fileData[startIndex];
                                startIndex++;
                                continue;
                            }
                        case 3:
                            {
                                row[k] = BitConverter.ToInt16(fileData, startIndex);
                                startIndex += 2;
                                continue;
                            }
                        case 5:
                            {
                                row[k] = BitConverter.ToInt32(fileData, startIndex);
                                startIndex += 4;
                                continue;
                            }
                        case 6:
                            {
                                row[k] = BitConverter.ToUInt32(fileData, startIndex);
                                startIndex += 4;
                                continue;
                            }
                        case 7:
                            {
                                stringLength = BitConverter.ToInt32(fileData, startIndex);
                                startIndex += 4;
                                if (stringLength > 0)
                                {
                                    break;
                                }
                                continue;
                            }
                        case 8:
                            {
                                row[k] = BitConverter.ToSingle(fileData, startIndex);
                                startIndex += 4;
                                continue;
                            }
                        default:
                            goto Label_03F5;
                    }

                    var charArray = new char[stringLength];
                    for (int m = 0; m < stringLength; m++)
                    {
                        charArray[m] = (char)fileData[startIndex];
                        startIndex++;
                    }
                    row[k] = new string(charArray);
                    continue;
                Label_03F5:
                    row[k] = BitConverter.ToInt32(fileData, startIndex);
                    startIndex += 4;
                }
                table.Rows.Add(row);
            }

            tblDatabase.Tables.Add(table);

            return true;
        }
    }
}
