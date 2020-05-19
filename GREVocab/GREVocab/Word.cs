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
        [JsonProperty("part_of_speech")]
        public string PartOfSpeech;
        [JsonProperty("definition_EN")]
        public string DefinitionEN;
        [JsonProperty("definition_CN")]
        public string DefinitionCN;
        [JsonProperty("synonym")]
        public string[] Synonyms;
        [JsonProperty("gre_synonym")]
        public string[] GRESynonyms;
    }
}
