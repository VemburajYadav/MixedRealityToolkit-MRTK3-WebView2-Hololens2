using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Linq;

namespace DataUtils
{
    public class CSVDataSet
    {
        // Dictionary to store columns dynamically
        public Dictionary<string, List<float>> columns = new Dictionary<string, List<float>>();

        // Column headers
        public List<string> headers = new List<string>();

        // Add a method to get data for a specific column
        public List<float> GetColumnData(string columnName)
        {
            if (columns.TryGetValue(columnName, out List<float> columnData))
            {
                return columnData;
            }
            Debug.LogWarning($"Column {columnName} not found in the dataset.");
            return new List<float>();
        }

        // Clear the dataset
        public void Clear()
        {
            columns.Clear();
            headers.Clear();
        }
    }

    public class CSVDataLoader
    {
        // Multiple datasets to handle different CSV files
        public Dictionary<string, CSVDataSet> dataSets = new Dictionary<string, CSVDataSet>();


        // Load a specific CSV file
        public CSVDataSet LoadCSVDataFromFilePath(string filePath)
        {
            CSVDataSet dataSet = new CSVDataSet();
            string fileName = Path.GetFileName(filePath);

            try
            {
                string[] lines = File.ReadAllLines(filePath);

                // Parse headers
                string[] headers = lines[0].Split(',');
                dataSet.headers.AddRange(headers);

                // Initialize columns
                foreach (string header in headers)
                {
                    dataSet.columns[header] = new List<float>();
                }

                // Parse data rows
                for (int i = 1; i < lines.Length; i++)
                {
                    string[] values = lines[i].Split(',');

                    // Ensure we don't exceed available headers or values
                    for (int j = 0; j < Math.Min(headers.Length, values.Length); j++)
                    {
                        if (float.TryParse(values[j], NumberStyles.Float, CultureInfo.InvariantCulture, out float parsedValue))
                        {
                            dataSet.columns[headers[j]].Add(parsedValue);
                        }
                        else
                        {
                            Debug.LogWarning($"Could not parse value '{values[j]}' in column {headers[j]} at row {i}");
                            dataSet.columns[headers[j]].Add(0f);
                        }
                    }
                }

                // Store the dataset
                dataSets[fileName] = dataSet;

                Debug.Log($"Successfully loaded {dataSet.headers.Count} columns and {dataSet.columns[headers[0]].Count} rows from {fileName}");
                return dataSet;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading CSV file {fileName}: {e.Message}");
                return dataSet;
            }
        }

        // Load a specific CSV file
        public CSVDataSet LoadCSVFile(string fileName)
        {
            CSVDataSet dataSet = new CSVDataSet();
            string filePath = Path.Combine(Application.streamingAssetsPath, fileName);
            try
            {
                string[] lines = File.ReadAllLines(filePath);

                // Parse headers
                string[] headers = lines[0].Split(',');
                dataSet.headers.AddRange(headers);

                // Initialize columns
                foreach (string header in headers)
                {
                    dataSet.columns[header] = new List<float>();
                }

                // Parse data rows
                for (int i = 1; i < lines.Length; i++)
                {
                    string[] values = lines[i].Split(',');

                    // Ensure we don't exceed available headers or values
                    for (int j = 0; j < Math.Min(headers.Length, values.Length); j++)
                    {
                        if (float.TryParse(values[j], NumberStyles.Float, CultureInfo.InvariantCulture, out float parsedValue))
                        {
                            dataSet.columns[headers[j]].Add(parsedValue);
                        }
                        else
                        {
                            Debug.LogWarning($"Could not parse value '{values[j]}' in column {headers[j]} at row {i}");
                            dataSet.columns[headers[j]].Add(0f);
                        }
                    }
                }

                // Store the dataset
                dataSets[fileName] = dataSet;

                Debug.Log($"Successfully loaded {dataSet.headers.Count} columns and {dataSet.columns[headers[0]].Count} rows from {fileName}");
                return dataSet;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading CSV file {fileName}: {e.Message}");
                return dataSet;
            }
        }

        // Load all CSV files in the StreamingAssets folder
        public void LoadAllCSVFiles()
        {
            try
            {
                string[] csvFiles = Directory.GetFiles(Application.streamingAssetsPath, "*.csv");
                foreach (string filePath in csvFiles)
                {
                    string fileName = Path.GetFileName(filePath);
                    LoadCSVFile(fileName);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading CSV files: {e.Message}");
            }
        }

        public void MetricToCSVDataset(string metricName, Dictionary<string, List<float>> metricData)
        {
            CSVDataSet dataSet = new CSVDataSet();
            List<string> workflowNames = metricData.Keys.ToList();
            dataSet.headers.Add("Epoch");

            int numValues = metricData[workflowNames[0]].Count;

            foreach (string workflowName in workflowNames)
            {
                dataSet.headers.Add(workflowName);
                dataSet.columns[workflowName] = metricData[workflowName];
            }

            List<float> epochValues = new List<float>(numValues);
            for (int i = 0; i < numValues; i++)
            {
                epochValues.Insert(i, i);
            }

            dataSet.columns["Epoch"] = epochValues;
            dataSets[metricName] = dataSet;
        }

        public void LoadFromMetricsData(Dictionary<string, Dictionary<string, List<float>>> metricsData)
        {
            List<string> metricNames = metricsData.Keys.ToList();
            foreach (string metricName in metricNames)
            {
                Debug.Log($"File: {metricName}");
                MetricToCSVDataset(metricName, metricsData[metricName]);

            }
        }

        // Demonstrate how to access data
        public void DemonstrateDataAccess()
        {
            foreach (var dataSetEntry in dataSets)
            {
                string fileName = dataSetEntry.Key;
                CSVDataSet dataSet = dataSetEntry.Value;

                Debug.Log($"Dataset: {fileName}");
                Debug.Log($"Columns: {string.Join(", ", dataSet.headers)}");

                // Print first few rows for each column
                foreach (string header in dataSet.headers)
                {
                    var columnData = dataSet.GetColumnData(header);
                    string firstFewValues = string.Join(", ",
                        columnData.Take(5).Select(v => v.ToString("F2")));
                    Debug.Log($"{header}: {firstFewValues}");
                }
            }
        }

        // Method to get data for a specific column in a specific file
        public List<float> GetColumnData(string fileName, string columnName)
        {
            if (dataSets.TryGetValue(fileName, out CSVDataSet dataSet))
            {
                return dataSet.GetColumnData(columnName);
            }

            Debug.LogWarning($"Dataset {fileName} not found.");
            return new List<float>();
        }

        // Optional: Method to find the range of values in a column
        public (float min, float max) GetColumnRange(string fileName, string columnName)
        {
            var columnData = GetColumnData(fileName, columnName);

            if (columnData.Count > 0)
            {
                return (columnData.Min(), columnData.Max());
            }

            return (0f, 0f);
        }

        public (float, float, float, float) GetDatasetMinMax(string fileName)
        {
            List<string> headers = dataSets[fileName].headers;

            float minY = float.MaxValue;
            float maxY = float.MinValue;

            var columnDataX = GetColumnData(fileName, headers[0]);
            float minX = columnDataX.Min();
            float maxX = columnDataX.Max();

            for (int i = 1; i < headers.Count; i++)
            {
                var columnData = GetColumnData(fileName, headers[i]);
                minY = Math.Min(minY, columnData.Min());
                maxY = Math.Max(maxY, columnData.Max());
            }

            return (minX, maxX, minY, maxY);
        }

        public (float min, float max) RoundDynamically(float min, float max)
        {
            float range = max - min;
            float precision;
            float roundedMin;
            float roundedMax;
            // Determine precision based on the range
            if (range > 100f)
            {
                precision = 10f;
                roundedMin = (float)Math.Floor(min / precision) * precision;
                roundedMax = (float)Math.Ceiling(max / precision) * precision;
            }
            else if (range > 10f)
            {
                precision = 5f;
                roundedMin = (float)Math.Floor(min / precision) * precision;
                roundedMax = (float)Math.Ceiling(max / precision) * precision;
            }

            else if (range > 1f)
            {
                precision = 1f;
                roundedMin = (float)Math.Floor(min / precision) * precision;
                roundedMax = (float)Math.Ceiling(max / precision) * precision;
            }

            else if (range > 0.1f)
            {
                precision = 0.1f;
                roundedMin = (float)Math.Floor(min / precision) * precision;
                roundedMax = (float)Math.Ceiling(max / precision) * precision;
            }

            else if (range > 0.01f)
            {
                precision = 0.01f;
                roundedMin = (float)Math.Floor(min / precision) * precision;
                roundedMax = (float)Math.Ceiling(max / precision) * precision;
            }

            else if (range > 0.001f)
            {
                precision = 0.001f;
                roundedMin = (float)Math.Floor(min / precision) * precision;
                roundedMax = (float)Math.Ceiling(max / precision) * precision;
            }
            else
            {
                precision = 0.0001f;
                roundedMin = (float)Math.Floor(min / precision) * precision;
                roundedMax = (float)Math.Ceiling(max / precision) * precision;
            }


            return (roundedMin, roundedMax);
        }

    }
}
