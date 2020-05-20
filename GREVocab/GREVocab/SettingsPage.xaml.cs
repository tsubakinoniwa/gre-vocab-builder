using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Essentials;

namespace GREVocab {
    public partial class SettingsPage : ContentPage {
        VocabBuilderViewModel ViewModel;
        public SettingsPage() {
            InitializeComponent();

            ShuffleDescription.Text = "Randomly choose new words to study. " +
                "Encountered words will not be affected.";
            ResetDescription.Text = "Clear all study progress. This operation " +
                "is irreversible.";
        }

        protected override void OnAppearing() {
            // Set the view model here since we can guarantee the binding
            // context has been initialized
            ViewModel = (VocabBuilderViewModel)BindingContext;

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
                await ViewModel.InitDatabase();
            }
        }
    }
}
