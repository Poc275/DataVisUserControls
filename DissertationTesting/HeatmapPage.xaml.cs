using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace DissertationTesting
{
    public sealed partial class HeatmapPage : Page
    {
        public HeatmapPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            switch (e.Parameter.ToString())
            {
                case "UK-HE-Stats.csv":
                    pageTitle.Text = "Higher Education in the UK";
                    heatmap.Folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                    heatmap.FileName = "UK-HE-Stats.csv";
                    heatmap.Id = "Institute of Education";
                    heatmap.Columns = new string[] { "Total students", "Total FE students", "FE Full-time", "FE Part-time", "Total PG students", "PG Full-time",
                                                    "PG Part-time", "PG UK", "PG EU", "PG Non-EU", "Total UG students", "UG Full-time", "UG Part-time",
                                                    "UG UK", "UG EU", "UG Non-EU" };
                    break;

                case "Basketball-Data.csv":
                    pageTitle.Text = "Basketball Data";
                    heatmap.Folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                    heatmap.FileName = "Basketball-Data.csv";
                    heatmap.Id = "Name";
                    heatmap.Columns = new string[] {"G", "MIN", "PTS", "FGM", "FGA", "FGP", "FTM", "FTA", "FTP", "3PM", "3PA", "3PP", "ORB", "DRB", "TRB",
                                                   "AST", "STL", "BLK", "TO", "PF" };
                    break;

                case "Historical-UK-Elections.csv":
                    pageTitle.Text = "Share of seats in UK general elections";
                    heatmap.Folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                    heatmap.FileName = "Historical-UK-Elections.csv";
                    heatmap.Id = "party";
                    heatmap.Columns = new string[] { "1900", "1906", "1910 (Jan)", "1910 (Dec)", "1918", "1922", "1923", "1924", "1929", "1931", "1935", "1945",
                                                     "1950", "1951", "1955", "1959", "1964", "1966", "1970", "1974 (Feb)", "1974 (Oct)", "1979", "1983",
                                                     "1987", "1992", "1997", "2001", "2005", "2010" };
                    break;

                case "National-Student-Survey-2012.csv":
                    pageTitle.Text = "National Student Survey 2012";
                    heatmap.Folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                    heatmap.FileName = "National-Student-Survey-2012.csv";
                    heatmap.Id = "Institution";
                    heatmap.Columns = new string[] { "2009/10 full-time first degree students", "2008/09 drop-out rate", "Average Teaching Score", "NSS Teaching (%)",
                                                    "NSS Overall (%)", "Student:staff ratio", "Career prospects (%)", "Entry Tariff", "NSS Feedback (%)" };
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
