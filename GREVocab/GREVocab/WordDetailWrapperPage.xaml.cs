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
                if (ViewModel.State == VocabBuilderViewModelState.Review) {
                    await Navigation.PushAsync(new WordDetailPage(r, false,
                        ViewModel.NumReviewRecords - ViewModel.ReviewRecords.Count,
                        -1, -1, ViewModel.NumReviewRecords));
                }
                else {
                    await Navigation.PushAsync(new WordDetailPage(r, false,
                        ViewModel.Review10Records.Count, ViewModel.Review60Records.Count,
                        ViewModel.NumNewRecords - ViewModel.NewRecords.Count -
                        ViewModel.Review10Records.Count - ViewModel.Review60Records.Count,
                        ViewModel.NumNewRecords));
                }
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
