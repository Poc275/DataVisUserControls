using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace DissertationTesting
{
    public sealed partial class GraduatedSymbolMapPage : Page
    {
        public GraduatedSymbolMapPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            switch (e.Parameter.ToString())
            {
                case "World-adolescent-fertility-rate.csv":
                    pageTitle.Text = "World adolescent fertility rate";
                    gsm.Folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                    gsm.FileName = "World-adolescent-fertility-rate.csv";
                    gsm.Latitude = "latitude";
                    gsm.Longitude = "longitude";
                    gsm.PlaceName = "country";
                    gsm.SymbolSize = "ad_fert_rate";
                    gsm.SymbolColour = "hdi_rank";
                    //gsm.SymbolColour = "continent";
                    gsm.LargestBubbleSize = 30;
                    break;

                case "UK-Train-Station-Usage.csv":
                    pageTitle.Text = "Train station usage in the UK";
                    gsm.Folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                    gsm.FileName = "UK-Train-Station-Usage.csv";
                    gsm.PlaceName = "Location for map";
                    gsm.Description = "Station Name";
                    gsm.SymbolSize = "Total usage";
                    //gsm.SymbolColour = "Percentage change";
                    gsm.SymbolColour = "Government Office Region (GOR)";
                    gsm.LargestBubbleSize = 30;
                    break;

                case "World-alcohol-consumption.csv":
                    pageTitle.Text = "World alcohol consumption per capita (litres)";
                    gsm.Folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                    gsm.FileName = "World-alcohol-consumption.csv";
                    gsm.PlaceName = "country";
                    gsm.Latitude = "latitude";
                    gsm.Longitude = "longitude";
                    gsm.SymbolSize = "alcohol per capita";
                    gsm.SymbolColour = "alcohol per capita";
                    gsm.LargestBubbleSize = 30;
                    break;

                case "Worlds-road-accidents.csv":
                    pageTitle.Text = "Dangerous driving around the world";
                    gsm.Folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                    gsm.FileName = "Worlds-road-accidents.csv";
                    gsm.PlaceName = "Country";
                    gsm.SymbolSize = "Road deaths per 100000 (reported)";
                    gsm.SymbolColour = "Number of registered vehicles";
                    gsm.LargestBubbleSize = 50;
                    break;

                default:
                    break;
            }
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }
    }
}
