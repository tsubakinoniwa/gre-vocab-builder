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
using Xamarin.Forms;

namespace GREVocab {
    public enum VocabBuilderViewModelState {
        Review,     // Reviewing old words from previous days
        Review10,   // Reviewing the most recent 10 words learned
        Review60,   // Reviewing the most recent 60 words learned
        New         // Studying new words
    }
    public class VocabBuilderViewModel : INotifyPropertyChanged {
        private SQLiteConnection Conn;
        private VocabBuilderViewModelState State;

        public List<Record> ReviewRecords;
        public List<Record> NewRecords;
        public List<Record> Review10Records;
        public List<Record> Review60Records;

        public int NewWordsPerDay {
            get {
                return Preferences.Get("NewWordsPerDay", 120);
            }
            set {
                Preferences.Set("NewWordsPerDay", value);
                OnPropertyChanged();
            }
        }

        private Record CurrentRecord {
            get {
                // We start by going through all the review words.
                if (State == VocabBuilderViewModelState.Review && ReviewRecords.Count != 0) {
                    if (ReviewRecords.Count != 1) State = VocabBuilderViewModelState.Review;
                    else State = VocabBuilderViewModelState.New;

                    return ReviewRecords[0];
                }
                // Whenever the bucket for most recent 10 words is filled,
                // start reviewing those until they are exhausted.
                else if (Review10Records.Count == 10 || NewRecords.Count == 0 ||
                    State == VocabBuilderViewModelState.Review10) {

                    if (Review10Records.Count != 1) State = VocabBuilderViewModelState.Review10;
                    else State = VocabBuilderViewModelState.New;

                    return Review10Records[0];
                }
                // Whenever the bucket for the most recent 60 words is filled,
                // (note that we can overfill, unlike the most recent 10 words),
                // start reviewing those until they are exhausted.
                else if (Review60Records.Count >= 60 || (NewRecords.Count == 0 &&
                    Review10Records.Count == 0) || State == VocabBuilderViewModelState.Review60) {

                    if (Review60Records.Count != 1) State = VocabBuilderViewModelState.Review60;
                    else State = VocabBuilderViewModelState.New;

                    return Review60Records[0];
                }
                else if (NewRecords.Count != 0) {
                    State = VocabBuilderViewModelState.New;
                    return NewRecords[0];
                }
                // Done for today!
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
        public ICommand UnrecognizedCommand;

        public event PropertyChangedEventHandler PropertyChanged;

        public VocabBuilderViewModel() {
            // Connect to database
            string libFolder = FileSystem.AppDataDirectory;
            string fName = Path.Combine(libFolder, "record.db");
            Conn = new SQLiteConnection(fName);
            Conn.CreateTable<Record>();

            // Initializing values
            State = VocabBuilderViewModelState.Review;
            ReviewRecords = new List<Record>();
            LoadReviewWords();
            NewRecords = new List<Record>();
            LoadNewWords();

            // Attaching handler to commands
            RecognizedCommand = new Command(execute: RecognizedCommandHandler);
            UnrecognizedCommand = new Command(execute: UnrecognizedCommandHandler);
        }

        private void RecognizedCommandHandler() {
            Record r = CurrentRecord;
            PopCurrentHead();

            // Depending on the current State, different behavior ensues.
            switch (State) {
                case VocabBuilderViewModelState.New:
                    RandomInsert(r, Review10Records);
                    break;
                case VocabBuilderViewModelState.Review:
                    // We are done with this word for today. Schedule this word
                    // for further review if necessary.
                    ScheduleNextReview(r);
                    break;
                case VocabBuilderViewModelState.Review10:
                    // Put this in the 60 review bin.
                    RandomInsert(r, Review60Records);
                    break;
                case VocabBuilderViewModelState.Review60:
                    // Done with this word for today.
                    ScheduleNextReview(r);
                    break;
            }
        }

        /*
         * Helper function to insert an item (source) into a list
         * (target) to a random position.
         */
        private void RandomInsert<T>(T source, List<T> target) {
            int ind = new Random().Next(0, target.Count);
            target.Add(target[ind]);
            target[ind] = source;
        }

        private void UnrecognizedCommandHandler() {
            // No matter which State, insert this record into NewRecords
            // at a random position
            RandomInsert(CurrentRecord, NewRecords);
            PopCurrentHead();
        }

        /*
         * Helper method to pop the head of the List<Record> object from
         * which CurrentRecord is obtained. In effect, CurrentRecord is popped.
         */
        private void PopCurrentHead() {
            switch (State) {
                case VocabBuilderViewModelState.New:
                    NewRecords.RemoveAt(0);
                    break;
                case VocabBuilderViewModelState.Review:
                    ReviewRecords.RemoveAt(0);
                    break;
                case VocabBuilderViewModelState.Review10:
                    Review10Records.RemoveAt(0);
                    break;
                case VocabBuilderViewModelState.Review60:
                    Review60Records.RemoveAt(0);
                    break;
            }
        }

        /*
         * Helper method to update the database to set the next review
         * date based on TimesStudied. Also updates TimesStudied
         */
        private void ScheduleNextReview(Record r) {
            switch (r.TimesStudied) {
                case 0:  // 12 hours
                    r.NextSchedule = DateTime.Now + new TimeSpan(12, 0, 0);
                    break;
                case 1:  // 1 day
                    r.NextSchedule = DateTime.Now + new TimeSpan(1, 0, 0, 0, 0);
                    break;
                case 2:  // 2 days
                    r.NextSchedule = DateTime.Now + new TimeSpan(2, 0, 0, 0, 0);
                    break;
                case 3:  // 4 days
                    r.NextSchedule = DateTime.Now + new TimeSpan(4, 0, 0, 0, 0);
                    break;
                case 4:  // 7 days
                    r.NextSchedule = DateTime.Now + new TimeSpan(7, 0, 0, 0, 0);
                    break;
                case 5:  // 15 days
                    r.NextSchedule = DateTime.Now + new TimeSpan(15, 0, 0, 0, 0);
                    break;
                default:  // Mark done with 100 years difference
                    r.NextSchedule = DateTime.Now + new TimeSpan(365 * 10, 0, 0, 0, 0);
                    break;
            }

            r.TimesStudied += 1;
            Conn.Update(r);
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

            // Loads data.json
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
                    TimesStudied = 0
                });
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
