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
            Record r = ViewModel.DisplayRecord;
            if (r != null) {
                await Navigation.PushAsync(new WordDetailPage(r, false));
                await TextToSpeech.SpeakAsync(r.Word.Content);
            }
            else {
                Result.IsVisible = true;
                if (Preferences.Get("LastCompleted", DateTime.Now).Date.CompareTo(
                    DateTime.Now.Date) != 0) {
                    // Only set this flag when we are not reviewing today's words
                    Preferences.Set("LastCompleted", DateTime.Now);
                }
                Preferences.Set("TodayLoaded", false);
            }
        }
    }
}
