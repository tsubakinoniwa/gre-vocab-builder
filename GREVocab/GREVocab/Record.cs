using System;
using SQLite;
using Newtonsoft.Json;

namespace GREVocab {
    /*
     * Class representing a row of record in the database.
     * Holds an Json string that will expand to a Word object.
     */
    [Table("record")]
    public class Record {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }
        public string Json { get; set; }
        public DateTime NextSchedule { get; set; }
        public int TimesStudied { get; set; }

        private Word Word = null;

        public Word GetWord() {
            if (Word == null) {
                Word = JsonConvert.DeserializeObject<Word>(Json);
                Word.timesStudied = TimesStudied;
            }
            return Word;
        }
    }
}
