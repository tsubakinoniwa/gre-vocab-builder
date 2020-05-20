using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SQLite;
using Xamarin.Forms;

namespace GREVocab {
    public partial class AllWordsPage : ContentPage {
        VocabBuilderViewModel ViewModel;
        public AllWordsPage() {
            InitializeComponent();
        }

        protected override void OnAppearing() {
            base.OnAppearing();
            ViewModel = (VocabBuilderViewModel)BindingContext;

            List<Record> records = ViewModel.LoadAllRecords();
            List<Word> words = new List<Word>();
            foreach (Record r in records) {
                words.Add(r.GetWord());
            }
            Console.WriteLine(words.Count);

            AllWords.ItemsSource = words;
        }
    }
}
