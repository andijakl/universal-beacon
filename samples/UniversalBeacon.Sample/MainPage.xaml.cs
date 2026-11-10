using UniversalBeacon.Sample.ViewModels;

namespace UniversalBeacon.Sample;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
