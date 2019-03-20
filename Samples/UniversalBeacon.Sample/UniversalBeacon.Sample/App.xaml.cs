// Copyright 2015 - 2019 Andreas Jakl, Chris Tacke and Contributors. All rights reserved. 
// https://github.com/andijakl/universal-beacon 
// 
// This code is licensed under the MIT License.
// See the LICENSE file in the project root for more information.

using UniversalBeacon.Sample.Views;
using Xamarin.Forms;

namespace UniversalBeacon.Sample
{
    public partial class App : Application
    {
        private readonly HomeView _viewInstance;

        public App()
        {
            InitializeComponent();
           
            _viewInstance = new HomeView();
            MainPage = _viewInstance;
        }
       

        protected override async void OnStart()
        {
            // Handle when your app starts
            await _viewInstance.Init();
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
