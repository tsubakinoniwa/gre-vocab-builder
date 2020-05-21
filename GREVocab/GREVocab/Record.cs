using System;
using SQLite;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace GREVocab {
    /*
     * Class representing a row of record in the database.
     * Holds an Json string that will expand to a Word object.
     */
    [Table("record")]
    public class Record : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }
        public string Json { get; set; }
        public DateTime NextSchedule { get; set; }

        private int timesStudied;
        public int TimesStudied {
            get {
                return timesStudied;
            }
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

        private Word word;
        public Word Word {
            get {
                if (word == null) {
                    word = JsonConvert.DeserializeObject<Word>(Json);
                }
                return word;
            }
        }

        // Some color properties for the display of progress
        public Color Color1 {
            get { return TimesStudied > 0 ? Color.Green : Color.LightGray; }
        }
        public Color Color2 {
            get { return TimesStudied > 1 ? Color.Green : Color.LightGray; }
        }
        public Color Color3 {
            get { return TimesStudied > 2 ? Color.Green : Color.LightGray; }
        }
        public Color Color4 {
            get { return TimesStudied > 3 ? Color.Green : Color.LightGray; }
        }
        public Color Color5 {
            get { return TimesStudied > 4 ? Color.Green : Color.LightGray; }
        }
        public Color Color6 {
            get { return TimesStudied > 5 ? Color.Green : Color.LightGray; }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString() {
            return Word.Content;
        }
    }
}
