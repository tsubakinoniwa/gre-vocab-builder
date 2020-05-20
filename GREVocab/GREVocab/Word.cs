using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace GREVocab {
    /*
     * Word class holding all the necessary attributes that 
     * the Json string in Record will expand into
     */
    public class Word : INotifyPropertyChanged {
        public int Id;

        [JsonProperty("word")]
        public string content;
        public string Content {
            get { return content; }
        }
        [JsonProperty("definitions")]
        public WordDefinition[] definitions;
        public WordDefinition[] Definitions {
            get { return definitions; }
        }
        [JsonProperty("gre_synonym")]
        public string[] greSynonyms;
        public string[] GRESynonyms {
            get { return greSynonyms; }
        }

        private int timesStudied;
        public int TimesStudied {
            get { return timesStudied; }
            set {
                timesStudied = value;
                OnPropertyChanged();
                OnPropertyChanged("Color1");
                OnPropertyChanged("Color2");
                OnPropertyChanged("Color3");
                OnPropertyChanged("Color4");
                OnPropertyChanged("Color5");
                OnPropertyChanged("Color6");
            }
        }

        // Some color properties for the display of progress
        public Color Color1 {
            get { return timesStudied > 0 ? Color.Green : Color.LightGray; }
        }
        public Color Color2 {
            get { return timesStudied > 1 ? Color.Green : Color.LightGray; }
        }
        public Color Color3 {
            get { return timesStudied > 2 ? Color.Green : Color.LightGray; }
        }
        public Color Color4 {
            get { return timesStudied > 3 ? Color.Green : Color.LightGray; }
        }
        public Color Color5 {
            get { return timesStudied > 4 ? Color.Green : Color.LightGray; }
        }
        public Color Color6 {
            get { return timesStudied > 5 ? Color.Green : Color.LightGray; }
        }

        // Short Definition for Words List Page
        public string ShortDefinition {
            get {
                string res = "";
                foreach (WordDefinition wd in definitions) {
                    string addition = $"; {wd.PartOfSpeech} {wd.DefinitionCN}";
                    if (res.Length + addition.Length - 2 >= 12 && !res.Equals("")) {
                        res += "...";
                        break;
                    }
                    else {
                        res += addition;
                    }
                }
                return res.Substring(2);  // Remove the starting "; "
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString() {
            string res = $"{Content}\n";
            foreach (var defn in Definitions) {
                res += $"\t{defn}\n";
            }
            foreach (var syn in GRESynonyms) {
                res += $"\t{syn}\n";
            }
            return res;
        }
    }

    public class WordDefinition {
        [JsonProperty("part_of_speech")]
        public string PartOfSpeech;
        [JsonProperty("definition_EN")]
        public string DefinitionEN;
        [JsonProperty("definition_CN")]
        public string DefinitionCN;
        [JsonProperty("synonym")]
        public string[] Synonyms;

        public override string ToString() {
            string res = $"{PartOfSpeech} {DefinitionEN} {DefinitionCN}";
            foreach (var syn in Synonyms) {
                res += $"\t{syn}\n";
            }

            return res;
        }
    }
}
