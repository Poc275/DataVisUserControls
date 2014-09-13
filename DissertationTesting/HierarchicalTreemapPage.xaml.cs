using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;


namespace DissertationTesting
{
    public sealed partial class HierarchicalTreemapPage : Page
    {
        public HierarchicalTreemapPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            switch (e.Parameter.ToString())
            {
                case "FTSE100.csv":
                    pageTitle.Text = "FTSE 100";
                    hierarchyTreemap.Folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                    hierarchyTreemap.FileName = "FTSE100.csv";
                    hierarchyTreemap.Id = "Code";
                    hierarchyTreemap.Description = "Name";
                    hierarchyTreemap.Area = "Price";
                    hierarchyTreemap.Colour = "Performance";
                    hierarchyTreemap.Hierarchy = "Sector";
                    break;

                case "FTSE250.csv":
                    pageTitle.Text = "FTSE 250";
                    hierarchyTreemap.Folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                    hierarchyTreemap.FileName = "FTSE250.csv";
                    hierarchyTreemap.Id = "Code";
                    hierarchyTreemap.Description = "Name";
                    hierarchyTreemap.Area = "Price";
                    hierarchyTreemap.Colour = "Performance";
                    hierarchyTreemap.Hierarchy = "Sector";
                    break;

                case "BBC-Spending-2013.csv":
                    pageTitle.Text = "BBC Spending 2013";
                    hierarchyTreemap.Folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                    hierarchyTreemap.FileName = "BBC-Spending-2013.csv";
                    hierarchyTreemap.Id = "label";
                    hierarchyTreemap.Description = "description";
                    hierarchyTreemap.Area = "value";
                    hierarchyTreemap.Colour = "change";
                    hierarchyTreemap.Hierarchy = "category";
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
