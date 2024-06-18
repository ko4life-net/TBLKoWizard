using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Newtonsoft.Json;

namespace TBLKoWizard.Utlis
{
    public class ColumnMapping
    {
        public Dictionary<string, Dictionary<string, Dictionary<string, string>>> Mappings { get; set; }

        public ColumnMapping()
        {
            Mappings = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
        }
    }

    public static class ColumnMappingHelper
    {
        public static ColumnMapping LoadMappings(string configFilePath)
        {
            string jsonConfig = File.ReadAllText(configFilePath);
            var mappings = JsonConvert.DeserializeObject<ColumnMapping>(jsonConfig);
            return mappings ?? new ColumnMapping();
        }

        public static DataSet ApplyColumnMappings(DataSet dataSet, Dictionary<string, Dictionary<string, Dictionary<string, string>>> mappings, int version, EventLogger _logger)
        {
            foreach (DataTable table in dataSet.Tables)
            {
                ApplyColumnMappings(table, mappings, version, _logger);
            }

            return dataSet;
        }

        private static void ApplyColumnMappings(DataTable table, Dictionary<string, Dictionary<string, Dictionary<string, string>>> mappings, int version, EventLogger _logger)
        {
            string versionString = version.ToString();

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("The column mapping process has started.");
            Console.ResetColor();

            _logger.LogEvent("The column mapping process has started.", LogLevel.Info);

            if (mappings.TryGetValue(versionString, out var versionMappings))
            {
                _logger.LogEvent($"The column mapping for version {versionString} has been found.", LogLevel.Info);

                //for item_ext_
                if (table.TableName.StartsWith("Item_ext_", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var tableMapping in versionMappings)
                    {
                        if (tableMapping.Key.StartsWith("item_ext_", StringComparison.OrdinalIgnoreCase))
                        {
                            _logger.LogEvent($"The column mapping for '{table.TableName}' has been found.", LogLevel.Info);

                            foreach (var columnMapping in tableMapping.Value)
                            {
                                if (table.Columns.Contains(columnMapping.Key))
                                {
                                    table.Columns[columnMapping.Key].ColumnName = columnMapping.Value;
                                }
                            }
                        }
                    }
                } 
                //Other Tbl
                else if (versionMappings.TryGetValue(table.TableName, out var tableMappings))
                {
                    _logger.LogEvent($"The column mapping for '{table.TableName}' has been found.", LogLevel.Info);

                    foreach (var columnMapping in tableMappings)
                    {
                        if (table.Columns.Contains(columnMapping.Key))
                        {
                            table.Columns[columnMapping.Key].ColumnName = columnMapping.Value;
                        }
                    }
                }
                //Mapping for table not found
                else 
                {
                    _logger.LogEvent($"The column mapping for '{table.TableName}' was not found. Default values have been set.", LogLevel.Warning);

                    for (int i = 0; i < table.Columns.Count; i++)
                    {
                        table.Columns[i].ColumnName = $"col_{i + 1}";
                    }
                }
            }
            else 
            {
                _logger.LogEvent($"The column mapping for version {versionString} was not found. Default values have been set.", LogLevel.Warning);

                for (int i = 0; i < table.Columns.Count; i++)
                {
                    table.Columns[i].ColumnName = $"col_{i + 1}";
                }
            }
        }
    }
}