using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;

namespace DissertationControls
{
    public class ColourScheme
    {
        static ColourScheme instance = null;
        static JsonObject colourData;

        private ColourScheme()
        {
        }

        public static ColourScheme Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ColourScheme();
                    Initialise();
                }
                return instance;
            }
        }

        private static async void Initialise()
        {
            await LoadColourData();
        }

        private static async Task LoadColourData()
        {
            var folder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("DissertationControls");
            var folder2 = await folder.GetFolderAsync("ColourData");
            var file = await folder2.GetFileAsync("ColourBrewerData.txt");
            var result = await FileIO.ReadTextAsync(file);
            colourData = JsonObject.Parse(result);
        }


        // Method that returns the possible colour schemes based on user selections
        public List<string> GetColourSchemes(int numOfClasses, string type)
        {
            List<string> colours = new List<string>();
            
            switch (type)
            {
                case "Sequential single hue":
                    colours.Add("Blue");
                    colours.Add("Green");
                    colours.Add("Grey");
                    colours.Add("Orange");
                    colours.Add("Purple");
                    colours.Add("Red");
                    break;

                case "Sequential multi hue":
                    colours.Add("Blue-Green");
                    colours.Add("Blue-Purple");
                    colours.Add("Green-Blue");
                    colours.Add("Orange-Red");
                    colours.Add("Purple-Blue");
                    colours.Add("Purple-Red");
                    colours.Add("Red-Purple");
                    colours.Add("Yellow-Green");
                    colours.Add("Purple-Blue-Green");
                    colours.Add("Yellow-Green-Blue");
                    colours.Add("Yellow-Orange-Brown");
                    colours.Add("Yellow-Orange-Red");
                    break;

                case "Diverging":
                    colours.Add("Brown-Blue-Green");
                    colours.Add("Pink-Yellow-Green");
                    colours.Add("Purple-Red-Green");
                    colours.Add("Purple-Orange");
                    colours.Add("Red-Blue");
                    colours.Add("Red-Grey");
                    colours.Add("Red-Yellow-Blue");

                    if (numOfClasses < 6)
                    {
                        colours.Add("Red-Yellow-Green");
                        colours.Add("Spectral");
                    }
                    break;

                case "Qualitative":
                    if (numOfClasses <= 12)
                    {
                        colours.Add("Paired");
                        colours.Add("Set3");
                    }
                    if (numOfClasses <= 9)
                    {
                        colours.Add("Set1");
                        colours.Add("Pastel1");
                    }
                    if (numOfClasses <= 8)
                    {
                        colours.Add("Accent");
                        colours.Add("Dark2");
                        colours.Add("Pastel2");
                        colours.Add("Set2");
                    }
                    break;

                default:
                    break;
            }

            return colours;
        }

        public byte[][] GetRGBColours(string scheme, int nClasses)
        {
            byte[][] rgbArray = new byte[nClasses][];
            JsonObject colourSchemes = colourData.GetNamedObject(scheme);
            JsonArray colourScheme = colourSchemes.GetNamedArray(nClasses.ToString());
            for (uint i = 0; i < colourScheme.Count; i++)
            {
                JsonArray rgbValues = colourScheme.GetArrayAt(i);
                byte[] parsedRgbValues = new byte[3];

                for (uint j = 0; j < rgbValues.Count; j++)
                {
                    parsedRgbValues[j] = (byte)rgbValues.GetNumberAt(j);
                }

                rgbArray[i] = parsedRgbValues;
            }

            return rgbArray;
        }
    }
}