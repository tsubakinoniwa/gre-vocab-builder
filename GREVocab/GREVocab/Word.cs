using System;
using Newtonsoft.Json;

namespace GREVocab {
    /*
     * Word class holding all the necessary attributes that 
     * the Json string in Record will expand into
     */
    public class Word {
        [JsonProperty("word")]
        public string Content;
        [JsonProperty("definitions")]
        public WordDefinition[] Definitions;
        [JsonProperty("gre_synonym")]
        public string[] GRESynonyms;

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
