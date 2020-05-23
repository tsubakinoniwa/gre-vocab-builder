using System;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace GREVocab {
    public partial class App : Application {
        public static VocabBuilderViewModel ViewModel;
        public App() {
            ViewModel = new VocabBuilderViewModel();
            InitializeComponent();
            MainPage = new HomePage();
            BindingContext = ViewModel;
        }

        protected override async void OnStart() {
            if (Preferences.Get("FirstTime", true)) {
                await ViewModel.ResetViewModel();

                Preferences.Set("FirstTime", false);
                Preferences.Set("LastCompleted", DateTime.Now - new TimeSpan(24, 0, 0));
                Preferences.Set("TodayLoaded", false);
            }
        }

        protected override void OnSleep() {
            Preferences.Set("TodayLoaded", false);
        }

        protected override void OnResume() {
        }
    }
}
