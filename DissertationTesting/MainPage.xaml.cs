using Windows.UI.Xaml.Controls;

namespace DissertationTesting
{
    public sealed partial class MainPage : Page
    {
        string flatTreemapDataset;
        string hierarchicalTreemapDataset;
        string parallelCoordinatesDataset;
        string graduatedSymbolMapDataset;
        string heatmapDataset;
        
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void treemapDatasets_Checked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            flatTreemapDataset = rb.Content.ToString();
        }

        private void flatTreemapButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(FlatTreemapPage), flatTreemapDataset);
        }


        private void hierarchicalTreemapDatasets_Checked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            hierarchicalTreemapDataset = rb.Content.ToString();
        }

        private void hierarchicalTreemapButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(HierarchicalTreemapPage), hierarchicalTreemapDataset);
        }


        private void parallelCoordinatesDatasets_Checked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            parallelCoordinatesDataset = rb.Content.ToString();
        }

        private void parallelCoordinatesButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(ParallelCoordinatesPage), parallelCoordinatesDataset);
        }


        private void graduatedSymbolMapDatasets_Checked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            graduatedSymbolMapDataset = rb.Content.ToString();
        }

        private void graduatedSymbolMapButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(GraduatedSymbolMapPage), graduatedSymbolMapDataset);
        }


        private void heatmapDatasets_Checked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            heatmapDataset = rb.Content.ToString();
        }

        private void heatmapButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(HeatmapPage), heatmapDataset);
        }

    }
}
