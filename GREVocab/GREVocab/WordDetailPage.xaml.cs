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

        private int progress1;
        private int progress2;
        private int progress3;
        private int total;

        public WordDetailPage(Record r, bool wordsListMode = false,
            int progress1 = -1, int progress2 = -1, int progress3 = -1,
            int total = -1) {

            this.progress1 = progress1;
            this.progress2 = progress2;
            this.progress3 = progress3;
            this.total = total;

            InitPage(r, wordsListMode);
        }

        public void InitPage(Record r, bool wordsListMode) {
            Record = r;
            BindingContext = App.ViewModel;

            InitializeComponent();

            // Choose the correct button layout to load
            if (wordsListMode) {
                RevealButtonStack.IsVisible = false;
                StudyButtonsStack.IsVisible = false;
                ControlButtonsStack.IsVisible = true;

                TopGrid.IsVisible = false;
                AddDefinition();
            }
            else {
                RevealButtonStack.IsVisible = true;
            }
        }

        protected override void OnAppearing() {
            base.OnAppearing();
            PlotProgress();
        }

        private void PlotProgress() {
            if (progress1 != -1) {
                double width = FullBar.Width;

                // We are done with reviews, so plot all three progress bars
                if (progress2 != -1 && progress3 != -1) {
                    int[] progressArray = new int[] { progress1, progress2, progress3 };
                    Color[] colors = new Color[] { Color.LightGreen, Color.Green, Color.Gold };

                    int runningTotal = progress1 + progress2 + progress3;
                    for (int i = 2; i >= 0; i--) {
                        double plotWidth = width * runningTotal / total;

                        TopGrid.Children.Add(new BoxView {
                            HeightRequest = 20,
                            WidthRequest = plotWidth,
                            BackgroundColor = colors[i],
                            HorizontalOptions = LayoutOptions.Start,
                        }, 0, 0);

                        runningTotal -= progressArray[i];
                    }
                }
                // We are reviewing, so only plot progress 1
                else {
                    double plotWidth = width * progress1 / total;
                    TopGrid.Children.Add(new BoxView {
                        HeightRequest = 20,
                        WidthRequest = plotWidth,
                        BackgroundColor = Color.Green,
                        HorizontalOptions = LayoutOptions.Start,
                    }, 0, 0);
                }
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
                //if (!d.DefinitionEN.Equals("")) {
                if (false) {
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
                        //Text = d.Synonyms.Length == 1 ? "SYNONYM" : "SYNONYMS",
                        Text = "同义词",
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
                g.Children.Add(new BoxView { HeightRequest = 5 }, 0, rowsAdded++);
            }

            // Add GRE synonyms if there is any
            if (w.GRESynonyms.Length != 0) {
                g.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                g.Children.Add(new Label {
                    //Text = "GRE TESTED\n" + (w.GRESynonyms.Length == 1 ? "SYNONYM" : "SYNONYMS"),
                    Text = "六选二\n同义词",
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

        async void QuitButton_Clicked(object sender, EventArgs e) {
            bool choice = await DisplayAlert("Be Careful", "If the app is closed " +
                "before you finish today's words, your progress will be lost.",
                "Quit", "Cancel");
            if (choice) {
                await Navigation.PopToRootAsync();
            }
        }
    }
}
