
using System.Data;
using System.Globalization;

namespace KoTblDbImporter.Encryption.V1
{
    public class EncryptionKOStandardV1 : IEncryption
    {
        public void Decode(ref byte[] data)
        {
            uint num = 0x816;
            for (int i = 0; i < data.Length; i++)
            {
                byte num3 = data[i];
                uint num4 = num;
                byte num5 = 0;
                num4 &= 0xff00;
                num4 = num4 >> 8;
                num5 = (byte)(num4 ^ num3);
                num4 = num3;
                num4 += num;
                num4 &= 0xffff;
                num4 *= 0x6081;
                num4 &= 0xffff;
                num4 += 0x1608;
                num4 &= 0xffff;
                num = num4;
                data[i] = num5;
            }
        }
        public void Encode(FileStream stream)
        {
            int num = stream.ReadByte();
            uint num2 = 0x816;
            while (num != -1)
            {
                stream.Seek(-1L, SeekOrigin.Current);
                byte num3 = (byte)(num & 0xff);
                byte num4 = 0;
                uint num5 = num2;
                num5 &= 0xff00;
                num5 = num5 >> 8;
                num4 = (byte)(num5 ^ num3);
                num5 = num4;
                num5 += num2;
                num5 &= 0xffff;
                num5 *= 0x6081;
                num5 &= 0xffff;
                num5 += 0x1608;
                num5 &= 0xffff;
                num2 = num5;
                stream.WriteByte(num4);
                num = stream.ReadByte();
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

            if (fileData.Length == 0)
            {
                Console.WriteLine("Not a standard tbl file.");
                return false;
            }

            int numColumns = BitConverter.ToInt32(fileData, startIndex);

            if ((numColumns < 0) || (((numColumns * 4) + (numColumns * 1)) > fileData.Length))
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

        /*
        public static void LoadByteDataIntoView(byte[] fileData, string tableName, DataSet tblDatabase)
        {
           if (fileData.Length == 0)
           {
               Console.WriteLine("Not a standard tbl file.");
               return;
           }

           using (var memoryStream = new MemoryStream(fileData))
           {
               using (var reader = new BinaryReader(memoryStream))
               {
                   int numColumns = reader.ReadInt32();
                   int[] columnTypes = new int[numColumns];
                   for (int i = 0; i < numColumns; i++)
                   {
                       columnTypes[i] = reader.ReadInt32();
                   }

                   int numRows = reader.ReadInt32();
                   DataTable table = new DataTable(tableName);
                   for (int i = 0; i < numColumns; i++)
                   {
                       DataColumn column;
                       string columnName = $"{i}";

                       switch (columnTypes[i])
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
                               column = new DataColumn(columnName, typeof(int)) { DefaultValue = 0 };
                               break;
                       }

                       table.Columns.Add(column);
                   }

                   for (int i = 0; i < numRows; i++)
                   {
                       DataRow row = table.NewRow();
                       for (int j = 0; j < numColumns; j++)
                       {
                           switch (columnTypes[j])
                           {
                               case 1:
                                   row[j] = reader.ReadSByte();
                                   break;
                               case 2:
                                   row[j] = reader.ReadByte();
                                   break;
                               case 3:
                                   row[j] = reader.ReadInt16();
                                   break;
                               case 5:
                                   row[j] = reader.ReadInt32();
                                   break;
                               case 6:
                                   row[j] = reader.ReadUInt32();
                                   break;
                               case 7:
                                   int stringLength = reader.ReadInt32();
                                   row[j] = new string(reader.ReadChars(stringLength));
                                   break;
                               case 8:
                                   row[j] = reader.ReadSingle();
                                   break;
                               default:
                                   row[j] = reader.ReadInt32();
                                   break;
                           }
                       }
                       table.Rows.Add(row);
                   }

                   tblDatabase.Tables.Add(table);
               }
           }
        }
        */
    }
}
