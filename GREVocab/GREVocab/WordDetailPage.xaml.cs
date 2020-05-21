using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms;

namespace GREVocab {
    public partial class WordDetailPage : ContentPage {
        public Record Record { get; set; }
        public Word Word {
            get {
                return Record.Word;
            }
        }

        public WordDetailPage(Record r, bool wordsListMode = false) {
            InitPage(r, wordsListMode);
        }

        public void InitPage(Record r, bool wordsListMode = false) {
            Record = r;
            BindingContext = App.ViewModel;

            InitializeComponent();

            // Choose the correct button layout to load
            if (wordsListMode) {
                RevealButtonStack.IsVisible = false;
                StudyButtonsStack.IsVisible = false;
                ControlButtonsStack.IsVisible = true;
                AddDefinition();
            }
            else {
                RevealButtonStack.IsVisible = true;
            }
        }

        /*
         * Helper method to add the grid of definitions, along with
         * the synonyms, to the page layout.
         */
        private void AddDefinition() {
            Word w = Record.Word;
            Grid g = new Grid {
                ColumnDefinitions = {
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) }
                },
                ColumnSpacing = 20
            };
            int rowsAdded = 0;
            foreach (WordDefinition d in w.Definitions) {
                // Add a new row for part of speech and Chinese definition
                g.RowDefinitions.Add(new RowDefinition {
                    Height = GridLength.Auto
                });
                Label partOfSpeechLabel = new Label {
                    Text = d.PartOfSpeech,
                    FontFamily = "Times New Roman",
                    //FontFamily = "Didot",
                    FontAttributes = FontAttributes.Bold,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.End,
                };
                g.Children.Add(new Label {
                    Text = d.DefinitionCN,
                    VerticalOptions = LayoutOptions.Center
                }, 1, rowsAdded);

                rowsAdded += 1;

                // Add a new row for English definition
                if (!d.DefinitionEN.Equals("")) {
                    g.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    g.Children.Add(new Label {
                        Text = d.DefinitionEN,
                        VerticalOptions = LayoutOptions.Center,
                        FontAttributes = FontAttributes.Italic,
                        FontFamily = "Times New Roman"
                        //FontFamily = "Didot",
                    }, 1, rowsAdded);

                    rowsAdded += 1;
                    g.Children.Add(partOfSpeechLabel, 0, 1, rowsAdded - 2, rowsAdded);
                }
                else {
                    g.Children.Add(partOfSpeechLabel, 0, 1, rowsAdded - 1, rowsAdded);
                }

                // Add a row for a list of synonyms if there is any
                if (d.Synonyms.Length != 0) {
                    g.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    g.Children.Add(new Label {
                        Text = d.Synonyms.Length == 1 ? "SYNONYMS" : "SYNONYMS",
                        TextColor = Color.Gray,
                        FontSize = Device.GetNamedSize(NamedSize.Small, new Label()),
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.End
                    }, 0, rowsAdded);

                    string text = "";
                    foreach (string syn in d.Synonyms) {
                        text += $", {syn}";
                    }
                    g.Children.Add(new Label {
                        Text = text.Substring(2),
                        VerticalOptions = LayoutOptions.Center
                    }, 1, rowsAdded);

                    rowsAdded += 1;
                }

                // Add a empty row for spacing
                g.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                g.Children.Add(new BoxView { HeightRequest = 10 }, 0, rowsAdded++);
            }

            // Add GRE synonyms if there is any
            if (w.GRESynonyms.Length != 0) {
                g.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                g.Children.Add(new Label {
                    Text = "GRE TESTED\n" + (w.GRESynonyms.Length == 1 ? "SYNONYM" : "SYNONYMS"),
                    TextColor = Color.Gray,
                    FontSize = Device.GetNamedSize(NamedSize.Small, new Label()),
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.End
                }, 0, rowsAdded);

                string text = "";
                foreach (string syn in w.GRESynonyms) {
                    text += $", {syn}";
                }
                g.Children.Add(new Label {
                    Text = text.Substring(2),
                    FontFamily = "Times New Roman",
                    VerticalOptions = LayoutOptions.Center
                }, 1, rowsAdded);
            }

            OuterGrid.Children.Add(g, 0, 2);
        }

        void RevealButton_Clicked(object sender, EventArgs e) {
            RevealButtonStack.IsVisible = false;
            StudyButtonsStack.IsVisible = true;
            ControlButtonsStack.IsVisible = false;
            AddDefinition();
        }

        async void UpdateWord(object sender, EventArgs e) {
            await Navigation.PopAsync();
        }
    }
}
