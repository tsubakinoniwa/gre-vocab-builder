using System;
using System.Collections.Generic;

using Xamarin.Essentials;
using Xamarin.Forms;

namespace GREVocab {
    public partial class OverviewPage : ContentPage {
        private VocabBuilderViewModel ViewModel;
        public OverviewPage() {
            InitializeComponent();
            ViewModel = App.ViewModel;
            BindingContext = ViewModel;
        }

        protected override void OnAppearing() {
            LoadNewWords();
            WelcomeLabel.Text = "Good " + (DateTime.Now.Hour < 12 ? "Morning." :
                DateTime.Now.Hour < 18 ? "Afternoon." : "Evening.");
            SummaryLabel.Text = $"You have {ViewModel.NewRecords.Count} " +
                $"new words to study and {ViewModel.ReviewRecords.Count} " +
                $"words to review.";
            InsertProgress();
        }

        private void LoadNewWords() {
            DateTime today = DateTime.Now;
            if (!Preferences.Get("TodayLoaded", false)) {
                // If we are ready to learn new words, load new words.
                Console.WriteLine(Preferences.Get("LastCompleted", today.Date - new TimeSpan(24, 0, 0)));
                if (Preferences.Get("LastCompleted", today.Date - new TimeSpan(
                    24, 0, 0)).CompareTo(today.Date) < 0) {
                    ViewModel.LoadNewWords();
                }
                // No matter what, we always need to load review words.
                ViewModel.LoadReviewWords();
                Preferences.Set("TodayLoaded", true);
            }
        }

        private void InsertProgress() {
            double fullWidth = FullBar.Width;
            double studiedWidth = fullWidth * ViewModel.StudiedCount / ViewModel.TotalCount;
            double completedWidth = fullWidth * ViewModel.CompletedCount / ViewModel.TotalCount;

            int n = MainGrid.Children.Count;
            MainGrid.Children.RemoveAt(n - 1);
            MainGrid.Children.RemoveAt(n - 2);

            MainGrid.Children.Add(new BoxView {
                BackgroundColor = Color.LightGreen,
                WidthRequest = studiedWidth,
                HeightRequest = 20,
                HorizontalOptions = LayoutOptions.Start,
            }, 0, 3, 5, 6);

            MainGrid.Children.Add(new BoxView {
                BackgroundColor = Color.Green,
                WidthRequest = completedWidth,
                HeightRequest = 20,
                HorizontalOptions = LayoutOptions.Start,
            }, 0, 3, 5, 6);
        }

        async void StartButton_Clicked(object sender, EventArgs e) {
            await Navigation.PushAsync(new WordDetailWrapperPage());
        }
    }
}
