using System;
using SQLite;
using Newtonsoft.Json;
using System.ComponentModel;

namespace GREVocab {
    /*
     * Class representing a row of record in the database.
     * Holds an Json string that will expand to a Word object.
     */
    [Table("record")]
    public class Record : INotifyPropertyChanged {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }
        public string Json { get; set; }
        public DateTime NextSchedule { get; set; }
        private int timesStudied;
        public int TimesStudied {
            get { return timesStudied; }
            set {
                timesStudied = value;
                if (Word == null) {
                    Word = GetWord();
                }
                Word.TimesStudied = timesStudied;
            }
        }

        private Word Word;

        public Word GetWord() {
            if (Word == null) {
                Word = JsonConvert.DeserializeObject<Word>(Json);
                Word.TimesStudied = TimesStudied;
                Word.Id = Id;
            }
            return Word;
        }
    }
}
