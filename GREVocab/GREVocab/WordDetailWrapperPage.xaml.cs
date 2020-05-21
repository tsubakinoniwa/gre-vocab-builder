using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Essentials;

namespace GREVocab {
    public partial class WordDetailWrapperPage : ContentPage {
        private VocabBuilderViewModel ViewModel;
        public WordDetailWrapperPage() {
            InitializeComponent();
            ViewModel = App.ViewModel;
        }

        protected override async void OnAppearing() {
            base.OnAppearing();

            // Loop to keep pushing new words
            if (ViewModel.DisplayRecord != null) {
                await Navigation.PushAsync(new WordDetailPage(ViewModel.DisplayRecord, false));
            }
            else {
                Result.IsVisible = true;
                Preferences.Set("LastCompleted", DateTime.Now);
                Preferences.Set("TodayLoaded", false);
            }
        }
    }
}
