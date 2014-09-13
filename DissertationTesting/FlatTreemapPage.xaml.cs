using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;


namespace DissertationTesting
{
    public sealed partial class FlatTreemapPage : Page
    {
        public FlatTreemapPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            switch (e.Parameter.ToString())
            {
                case "Billions-2013.csv":
                    pageTitle.Text = "Billions spent around the world in 2013";
                    treemap.Folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                    treemap.FileName = "Billions-2013.csv";
                    treemap.Id = "label";
                    treemap.Description = "description";
                    treemap.Area = "value";
                    treemap.Colour = "type";
                    break;

                case "FTSE100.csv":
                    pageTitle.Text = "FTSE 100";
                    treemap.Folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                    treemap.FileName = "FTSE100.csv";
                    treemap.Id = "Code";
                    treemap.Description = "Name";
                    treemap.Area = "Price";
                    treemap.Colour = "Performance";
                    break;

                case "FTSE250.csv":
                    pageTitle.Text = "FTSE 250";
                    treemap.Folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                    treemap.FileName = "FTSE250.csv";
                    treemap.Id = "Code";
                    treemap.Description = "Name";
                    treemap.Area = "Price";
                    treemap.Colour = "Performance";
                    break;

                case "AIG-Bailout-Cost.csv":
                    pageTitle.Text = "The Cost of A.I.G.'s rescue in 2008 compared to Federal Spending";
                    treemap.Folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                    treemap.FileName = "AIG-Bailout-Cost.csv";
                    treemap.Id = "Federal Spending";
                    treemap.Area = "Millions";
                    treemap.Colour = "Millions";
                    break;

                case "Lines-of-code.csv":
                    pageTitle.Text = "Millions of lines of code";
                    treemap.Folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                    treemap.FileName = "Lines-of-code.csv";
                    treemap.Id = "title";
                    treemap.Description = "notes";
                    treemap.Area = "million lines of code";
                    treemap.Colour = "detail";
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
