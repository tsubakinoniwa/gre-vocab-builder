using System;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace GREVocab {
    public partial class App : Application {
        public static VocabBuilderViewModel ViewModel = new VocabBuilderViewModel();
        public App() {
            InitializeComponent();
            MainPage = new HomePage();
            BindingContext = ViewModel;
        }

        protected override async void OnStart() {
            if (Preferences.Get("FirstTime", true)) {
                Preferences.Set("FirstTime", false);
                await ViewModel.ResetViewModel();
                Preferences.Set("LastCompleted", DateTime.Now.Date - new TimeSpan(24, 0, 0));
                Preferences.Set("TodayLoaded", false);
            }
        }

        protected override void OnSleep() {
        }

        protected override void OnResume() {
        }
    }
}
