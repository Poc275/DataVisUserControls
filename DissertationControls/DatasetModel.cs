using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Popups;

namespace DissertationControls
{
    public class DatasetModel
    {
        string[] _columnHeadings;
        List<string[]> _dataset;
        bool _isColourQualitative;
        double[] _colourMinMax;
        Dictionary<string, double[]> _axesMinMax;

        public DatasetModel()
        {
            _dataset = new List<string[]>();
            _isColourQualitative = false;
        }

        public string[] ColumnHeadings
        {
            get { return _columnHeadings; }
            set { _columnHeadings = value; }
        }

        public List<string[]> Dataset
        {
            get { return _dataset; }
        }

        public bool IsColourQualitative
        {
            get { return _isColourQualitative; }
            set { _isColourQualitative = value; }
        }

        public double[] ColourMinMax
        {
            get { return _colourMinMax; }
        }


        public async Task ReadCsvFile(StorageFolder folder, string filename)
        {
            // assuming a folder named Data exists
            var folderContents = await folder.GetFolderAsync("Data");

            var file = await folderContents.GetFileAsync(filename);
            IList<string> content = await Windows.Storage.FileIO.ReadLinesAsync(file);

            string headings = content[0];
            _columnHeadings = headings.Split(',');

            for (int i = 1; i < content.Count; i++)
            {
                string line = content[i];
                string[] csvLine = line.Split(',');
                _dataset.Add(csvLine);
            }
        }

        public bool ColourValueCheck(int colourIndex)
        {
            double colourValue = 0.0;
            _isColourQualitative = double.TryParse(_dataset.ElementAt(0)[colourIndex], out colourValue) ? false : true;
            
            return _isColourQualitative;
        }

        public async Task<bool> GetColourMaxMin(int colourIndex)
        {
            bool success = true;
            double parsedValue = 0;
            _colourMinMax = new double[2];
            
            for (int i = 0; i < _dataset.Count; i++)
            {
                if (!double.TryParse(_dataset[i][colourIndex], out parsedValue))
                {
                    success = false;
                    // add 2 to the current iteration to account for the zero index and the column headings line
                    await new MessageDialog("string to double parse error at line " + (i + 2).ToString(), "Error parsing the dataset").ShowAsync();
                }
                else
                {
                    // initialise the values during the 1st loop
                    if (i == 0)
                    {
                        _colourMinMax[0] = parsedValue;
                        _colourMinMax[1] = parsedValue;
                    }
                    else
                    {
                        if (parsedValue > _colourMinMax[1])
                        {
                            _colourMinMax[1] = parsedValue;
                        }
                        if (parsedValue < _colourMinMax[0])
                        {
                            _colourMinMax[0] = parsedValue;
                        }
                    }
                }
            }

            return success;
        }

        public async Task<bool> GetAxesMaxMin(string[] axes, int[] axesIndexes)
        {
            bool success = true;
            _axesMinMax = new Dictionary<string, double[]>();

            // generate the dictionary keys
            for (int i = 0; i < axes.Length; i++)
            {
                _axesMinMax.Add(_columnHeadings[axesIndexes[i]], new double[2]);
            }

            // max/min checks
            for (int i = 0; i < _dataset.Count; i++)
            {
                for (int j = 0; j < axes.Length; j++)
                {
                    string columnTitle = _columnHeadings[axesIndexes[j]];
                    double[] minMax = _axesMinMax[columnTitle];
                    double parsedValue = 0;

                    if (!double.TryParse(_dataset.ElementAt(i)[axesIndexes[j]], out parsedValue))
                    {
                        success = false;
                        // add 2 to the current iteration to account for the zero index and the column headings line
                        await new MessageDialog("string to double parse error at line " + (i + 2).ToString(), "Error parsing the dataset").ShowAsync();
                    }
                    else
                    {
                        // initialise the values during the 1st loop
                        if (i == 0)
                        {
                            minMax[0] = parsedValue;
                            minMax[1] = parsedValue;
                        }
                        else
                        {
                            if (parsedValue > minMax[1])
                            {
                                minMax[1] = parsedValue;
                            }
                            if (parsedValue < minMax[0])
                            {
                                minMax[0] = parsedValue;
                            }
                        }
                    }
                }
            }

            return success;
        }

        public double[] GetAxisMinMaxValues(string axisName)
        {
            return _axesMinMax[axisName];
        }

        public void SwapDataEntries(int oldIndex, int newIndex)
        {
            string[] row1 = _dataset[oldIndex];
            string[] row2 = _dataset[newIndex];

            _dataset[oldIndex] = row2;
            _dataset[newIndex] = row1;
        }

        public void ReorderDatasetByDescending(int column)
        {
            _dataset = _dataset.OrderByDescending(o => double.Parse(o[column])).ToList();
        }
    }
}