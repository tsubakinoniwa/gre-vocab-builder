using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using SQLite;
using Xamarin.Forms;

namespace GREVocab {
    public partial class AllWordsPage : ContentPage {
        private VocabBuilderViewModel ViewModel;
        public AllWordsPage() {
            InitializeComponent();
            ViewModel = App.ViewModel;
        }

        protected override void OnAppearing() {
            base.OnAppearing();
            AllWords.ItemsSource = ViewModel.AllRecords;
        }

        async void AllWords_ItemTapped(object sender, ItemTappedEventArgs e) {
            await Navigation.PushAsync(new WordDetailPage((Record)AllWords.SelectedItem, true));
        }
    }
}
