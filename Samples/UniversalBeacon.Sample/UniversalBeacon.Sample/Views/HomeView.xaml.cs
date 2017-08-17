using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalBeacon.Sample.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace UniversalBeacon.Sample.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HomeView : ContentPage
    {
        public HomeView()
        {
            InitializeComponent();

            this.BindingContext = new HomeViewModel();
        }
    }
}