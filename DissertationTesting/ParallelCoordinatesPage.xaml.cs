using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace DissertationTesting
{
    public sealed partial class ParallelCoordinatesPage : Page
    {
        public ParallelCoordinatesPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            switch (e.Parameter.ToString())
            {
                case "US-Education-Stats.csv":
                    pageTitle.Text = "US Education";
                    pc.Folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                    pc.FileName = "US-Education-Stats.csv";
                    pc.Id = "state";
                    pc.Axes = new string[] { "writing", "reading", "math", "dropout_rate", "pupil_staff_ratio", "percent_graduates_sat" };
                    //pc.ColourValue = "pupil_staff_ratio";
                    pc.ColourValue = "region";
                    break;

                case "Food-macronutrient-data.csv":
                    pageTitle.Text = "What are you eating?";
                    pc.Folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                    pc.FileName = "Food-macronutrient-data.csv";
                    pc.Id = "Food";
                    pc.Axes = new string[] { "Water g/100g", "Protein g/100g", "Total fat g/100g", "Carbohydrate g/100g", "Energy (kcal)/100g", 
                                                "Englyst fibre g/100g", "Total sugars g/100g", "Saturated fatty acids g/100g",
                                                "Cis-monounsaturated fatty acids g/100g", "Cis-polyunsaturated fatty acids g/100g",
                                                "Trans fatty acids g/100g", "Cholesterol mg/100g" };
                    pc.ColourValue = "Category";
                    break;

                case "Car-fuel-and-emissions.csv":
                    pageTitle.Text = "Car fuel efficiency and emissions data";
                    pc.Folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                    pc.FileName = "Car-fuel-and-emissions.csv";
                    pc.Id = "Car";
                    pc.Axes = new string[] { "Engine Capacity", "Metric Urban (Cold)", "Metric Extra-Urban", "Metric Combined", "CO2 g/km", 
                                                "Fuel Cost 12000 Miles", "Noise Level dB(A)", "Emissions CO [mg/km]" };
                    pc.ColourValue = "Fuel Type";
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
