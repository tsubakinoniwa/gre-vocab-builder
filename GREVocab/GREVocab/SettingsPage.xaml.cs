using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Essentials;

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
    }
}
