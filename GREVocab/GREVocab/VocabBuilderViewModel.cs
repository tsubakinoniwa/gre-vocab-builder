using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Newtonsoft.Json;
using SQLite;
using Xamarin.Essentials;

namespace GREVocab {
    public class VocabBuilderViewModel : INotifyPropertyChanged {
        private SQLiteConnection Conn;
        public List<Record> ReviewRecords;
        public List<Record> NewRecords;
        private bool Reviewing;

        public int NewWordsPerDay {
            get {
                return Preferences.Get("NewWordsPerDay", 50);
            }
            set {
                Preferences.Set("NewWordsPerDay", value);
                OnPropertyChanged();
            }
        }

        public Record CurrentRecord {
            get {
                if (ReviewRecords.Count != 0) {
                    return ReviewRecords[0];
                }
                else if (NewRecords.Count != 0) {
                    return NewRecords[0];
                }
                else {
                    return null;
                }
            }
        }

        public Word DisplayWord {
            get {
                return CurrentRecord.GetWord();
            }
        }

        public ICommand RecognizedCommand;
        public ICommand NotRecognizedCommand;

        public event PropertyChangedEventHandler PropertyChanged;

        public VocabBuilderViewModel() {
            // Connect to database
            string libFolder = FileSystem.AppDataDirectory;
            string fName = Path.Combine(libFolder, "record.db");
            Conn = new SQLiteConnection(fName);
            Conn.CreateTable<Record>();

            // Initializing values
            Reviewing = true;
            ReviewRecords = new List<Record>();
            NewRecords = new List<Record>();
        }

        /*
         * Loads from database all the words scheduled to be reviewed
         * today. This is calculated based on NextSchedule.
         */
        public void LoadReviewWords() {
            DateTime today = DateTime.Now.Date;
            ReviewRecords = Conn.Table<Record>().ToList().Where(
                x => x.NextSchedule.Date.CompareTo(today) == 0).ToList();
        }

        /*
         * Loads from database NewWordsPerDay number of new words, if
         * there are still enough words to load.
         */
        public void LoadNewWords() {
            // All the unencountered words will have next scheduled date
            // earlier than any date since the last call to InitDatabase().
            DateTime today = DateTime.Now.Date;
            var allNewRecords = Conn.Table<Record>().ToList().Where(
                x => x.NextSchedule.Date.CompareTo(today) < 0).ToList();

            if (allNewRecords.Count < NewWordsPerDay) {
                NewRecords = allNewRecords;
            }
            else {
                if (Preferences.Get("Shuffle", true)) {
                    // Use the Fisher-Yates shuffle algorithm to shuffle
                    // the indices, and keep the first NewWordsPerDay number
                    // of them.
                    int[] inds = new int[allNewRecords.Count];
                    for (int i = 0; i < inds.Length; i++) {
                        inds[i] = i;
                    }

                    for (int i = 0; i < NewWordsPerDay; i++) {
                        // Swap inds[i] with inds[k] for some random k >= i
                        // Upperbound is not inclusive.
                        int k = new Random().Next(i, allNewRecords.Count);
                        inds[k] = i;

                        // We will no longer modify inds[i] = k, so might
                        // as well just add the element here
                        NewRecords.Add(allNewRecords[k]);
                    }
                }
                else {
                    NewRecords = allNewRecords.GetRange(0, NewWordsPerDay);
                }
            }
        }


        /* 
         * Initializes the database with data populated from data.json.
         * Completely wipes all exisiting records.
         */
        public async void InitDatabase() {
            Conn.DropTable<Record>();
            Conn.CreateTable<Record>();

            // Load data.json
            var assembly = IntrospectionExtensions.GetTypeInfo(typeof(VocabBuilderViewModel)).Assembly;
            Stream stream = assembly.GetManifestResourceStream("GREVocab.data.json");

            string data = "";
            using (var reader = new StreamReader(stream)) {
                data = await reader.ReadToEndAsync();
            }

            // Deserialize and re-serialize to insert into DB
            Word[] words = JsonConvert.DeserializeObject<Word[]>(data);
            foreach (var word in words) {
                Conn.Insert(new Record {
                    Json = JsonConvert.SerializeObject(word),
                    NextSchedule = DateTime.Now - new TimeSpan(1, 0, 0, 0, 0),
                    TimesMemorized = 0
                });
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
