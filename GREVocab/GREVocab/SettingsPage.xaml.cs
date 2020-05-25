using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Essentials;
using System.Linq;

namespace GREVocab {
    public partial class SettingsPage : ContentPage {
        VocabBuilderViewModel ViewModel;
        public SettingsPage() {
            InitializeComponent();
            ViewModel = App.ViewModel;

            ShuffleDescription.Text = "Randomly chooses new words to study. " +
                "Encountered words will not be affected.";
            ResetDescription.Text = "Clears all study progress. This operation " +
                "is irreversible.";
        }

        protected override void OnAppearing() {
            base.OnAppearing();

            // Initialize controls to previously set values
            NewWordsPerDay.Value = ViewModel.NewWordsPerDay;
        }

        void NewWordsPerDay_ValueChanged(object sender, ValueChangedEventArgs e) {
            int selection = (int)Math.Round(NewWordsPerDay.Value);
            NewWordsPerDay.Value = selection;
            ViewModel.NewWordsPerDay = selection;
        }

        async void ResetButton_Clicked(object sender, EventArgs e) {
            string action = await DisplayActionSheet(
                "Confirm Reset? This operation is irreversible.",
                "Cancel", "Confirm Reset");
            if (action.Equals("Confirm Reset")) {
                await ViewModel.ResetViewModel();
                Preferences.Set("LastCompleted", DateTime.Now.Date - new TimeSpan(24, 0, 0));
                Preferences.Set("TodayLoaded", false);
            }
        }

        async void FixButton_Clicked(object sender, EventArgs e) {
            var res = ViewModel.AllRecords.Where(x => x.TimesStudied != 0).ToList();
            int repCounts = res.Where(x => x.TimesStudied > 1).Count();

            bool choice = await DisplayAlert("Review", $"There are {res.Count} " +
                $"encountered words, {repCounts} of which are studied more than once. " +
                $"Continue setting all repeats to seen once?", "Yes", "No");
            if (choice) {
                foreach (var r in res) {
                    if (r.TimesStudied > 1)
                        ViewModel.SetToOne(r);
                }
            }
        }
    }
}
