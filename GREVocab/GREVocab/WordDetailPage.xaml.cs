using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms;

namespace GREVocab {
    public partial class WordDetailPage : ContentPage {
        public Word Word { get; }

        public WordDetailPage(Word w, bool wordsListMode = false) {
            Word = w;
            BindingContext = App.ViewModel;

            InitializeComponent();

            if (wordsListMode) {
                StudyButtonsStack.IsVisible = false;
                ControlButtonsStack.IsVisible = true;
            }
        }
    }
}
