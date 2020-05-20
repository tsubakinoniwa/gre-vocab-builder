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
        public event PropertyChangedEventHandler PropertyChanged;

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
