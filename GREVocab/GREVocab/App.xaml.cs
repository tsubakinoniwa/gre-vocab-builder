using System;
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

        protected override void OnStart() {
        }

        protected override void OnSleep() {
        }

        protected override void OnResume() {
        }
    }
}
