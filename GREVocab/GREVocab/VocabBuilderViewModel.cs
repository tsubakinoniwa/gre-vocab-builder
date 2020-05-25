using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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
        public VocabBuilderViewModelState State = VocabBuilderViewModelState.Review;

        public List<Record> ReviewRecords { get; set; } = new List<Record>();
        public List<Record> NewRecords { get; set; } = new List<Record>();
        public List<Record> Review10Records { get; set; } = new List<Record>();
        public List<Record> Review60Records { get; set; } = new List<Record>();

        // Variables to keep track of the total amount of work scheduled today
        public int NumReviewRecords { get; private set; }
        public int NumNewRecords { get; private set; }

        private ObservableCollection<Record> allRecords = null;
        public ObservableCollection<Record> AllRecords {
            get {
                if (allRecords == null) {
                    GetAllRecords();
                }
                return allRecords;
            }
        }

        public int NewWordsPerDay {
            get {
                return Preferences.Get("NewWordsPerDay", 120);
            }
            set {
                if (value != NewWordsPerDay) {
                    Preferences.Set("NewWordsPerDay", value);
                    Preferences.Set("TodayLoaded", false);
                    OnPropertyChanged();
                }
            }
        }

        private Record CurrentRecord {
            get {
                // We start by going through all the review words.
                if (ReviewRecords.Count != 0) {
                    State = VocabBuilderViewModelState.Review;
                    return ReviewRecords[0];
                }
                // Whenever the bucket for most recent 10 words is filled,
                // start reviewing those until they are exhausted.
                else if ((Review10Records.Count == 10 || NewRecords.Count == 0 ||
                    State == VocabBuilderViewModelState.Review10) &&
                    Review10Records.Count != 0) {

                    State = VocabBuilderViewModelState.Review10;
                    return Review10Records[0];
                }
                // Whenever the bucket for the most recent 60 words is filled,
                // (note that we can overfill, unlike the most recent 10 words),
                // start reviewing those until they are exhausted.
                else if ((Review60Records.Count >= 60 || (NewRecords.Count == 0 &&
                    Review10Records.Count == 0) || State == VocabBuilderViewModelState.Review60)
                    && Review60Records.Count != 0) {

                    State = VocabBuilderViewModelState.Review60;
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

        public Record DisplayRecord {
            get {
                return CurrentRecord;
            }
        }

        public Word DisplayWord {
            get {
                return CurrentRecord.Word;
            }
        }

        public bool Shuffle {
            get {
                return Preferences.Get("Shuffle", true);
            }
            set {
                Preferences.Set("Shuffle", value);
                Preferences.Set("TodayLoaded", false);  // Invalidate loaded
                OnPropertyChanged();
            }
        }

        public int StudiedCount {
            get {
                return AllRecords.Where(x => x.TimesStudied >= 1).Count();
            }
        }

        public int CompletedCount {
            get {
                return AllRecords.Where(x => x.TimesStudied >= 6).Count();
            }
        }

        public int TotalCount {
            get {
                return AllRecords.Count;
            }
        }

        public ICommand RecognizedCommand { get; set; }
        public ICommand UnrecognizedCommand { get; set; }
        public ICommand TooEasyCommand { get; set; }
        public ICommand ResetWordCommand { get; set; }
        public ICommand ReloadNewWordsCommand { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public VocabBuilderViewModel() {
            // Connect to database
            string libFolder = FileSystem.AppDataDirectory;
            string fName = Path.Combine(libFolder, "record.db");
            Conn = new SQLiteConnection(fName);
            Conn.CreateTable<Record>();

            // Initializing values
            State = VocabBuilderViewModelState.Review;

            // Attaching handler to commands
            RecognizedCommand = new Command(execute: () => RecognizedCommandHandler());
            UnrecognizedCommand = new Command(execute: () => UnrecognizedCommandHandler());
            TooEasyCommand = new Command(execute: (r) => TooEasyCommandHandler((Record)r));
            ResetWordCommand = new Command(execute: (r) => ResetWordCommandHandler((Record)r));
            ReloadNewWordsCommand = new Command(execute: () => ReloadNewWordsCommandHandler());

            // Demo
            //Demo();
        }

        private void TooEasyCommandHandler(Record r) {
            r.TimesStudied = 6;
            r.NextSchedule = DateTime.Now + new TimeSpan(365 * 100, 0, 0, 0, 0);
            Conn.Update(r);
        }

        private void ResetWordCommandHandler(Record r) {
            r.TimesStudied = 0;
            r.NextSchedule = DateTime.Now + new TimeSpan(365 * 100, 0, 0, 0, 0);
            Conn.Update(r);
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

            //Console.WriteLine(State);
            //Console.WriteLine($"{NewRecords.Count} {ReviewRecords.Count} {Review10Records.Count} {Review60Records.Count}");
        }

        /*
         * Helper function to insert an item (source) into a list
         * (target) to a random position.
         */
        private void RandomInsert<T>(T source, List<T> target) {
            if (target.Count == 0) {
                target.Add(source);
            }
            else {
                int ind = new Random().Next(0, target.Count);
                target.Add(target[ind]);
                target[ind] = source;
            }
            //Console.WriteLine(IsListUnique(target));
        }

        private void UnrecognizedCommandHandler() {
            // No matter which State, insert this record into NewRecords
            // at a random position
            Record r = CurrentRecord;
            PopCurrentHead();
            RandomInsert(r, NewRecords);
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
                    r.NextSchedule = DateTime.Now + new TimeSpan(1, 0, 0, 0, 0);
                    break;
                case 3:  // 4 days
                    r.NextSchedule = DateTime.Now + new TimeSpan(2, 0, 0, 0, 0);
                    break;
                case 4:  // 7 days
                    r.NextSchedule = DateTime.Now + new TimeSpan(3, 0, 0, 0, 0);
                    break;
                case 5:  // 15 days
                    r.NextSchedule = DateTime.Now + new TimeSpan(8, 0, 0, 0, 0);
                    break;
                default:  // Mark done with 100 years difference
                    r.NextSchedule = DateTime.Now + new TimeSpan(365 * 100, 0, 0, 0, 0);
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
            DateTime today = DateTime.Now;
            ReviewRecords = new List<Record>();
            ReviewRecords = AllRecords.Where(
                x => x.NextSchedule.CompareTo(today) < 0).ToList();

            NumReviewRecords = ReviewRecords.Count;
        }

        /*
         * Loads from database NewWordsPerDay number of new words, if
         * there are still enough words to load.
         */
        public void LoadNewWords() {
            //DateTime today = DateTime.Now.Date;
            NewRecords = new List<Record>();
            var allNewRecords = AllRecords.Where(x => x.TimesStudied == 0).ToList();

            if (allNewRecords.Count < NewWordsPerDay) {
                NewRecords = allNewRecords;
            }
            else {
                if (Shuffle) {
                    // Use the Fisher-Yates shuffle algorithm to shuffle
                    // the indices, and keep the first NewWordsPerDay number
                    // of them.
                    int[] inds = new int[allNewRecords.Count];
                    for (int i = 0; i < inds.Length; i++) {
                        inds[i] = i;
                    }

                    for (int i = 0; i < Math.Min(NewWordsPerDay, allNewRecords.Count); i++) {
                        // Swap inds[i] with inds[k] for some random k >= i
                        // Upperbound is not inclusive.
                        int k = new Random().Next(i, allNewRecords.Count);
                        int tmp = inds[k];
                        inds[k] = inds[i];

                        // We will no longer modify inds[i] = inds[k], so might
                        // as well just add the element here
                        NewRecords.Add(allNewRecords[tmp]);
                    }
                }
                else {
                    NewRecords = allNewRecords.GetRange(0, NewWordsPerDay);
                }
            }

            NumNewRecords = NewRecords.Count;
            //Console.WriteLine(IsListUnique(NewRecords));
        }

        private void ReloadNewWordsCommandHandler() {
            LoadNewWords();
        }


        /* 
         * Initializes the database with data populated from data.json.
         * Completely wipes all exisiting records.
         */
        public async Task ResetViewModel() {
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
                    NextSchedule = DateTime.Now + new TimeSpan(100 * 365, 0, 0, 0, 0),
                    TimesStudied = 0
                });
            }

            // Refresh the saved records
            GetAllRecords();
            OnPropertyChanged("AllRecords");
        }

        private void Demo() {
            Conn.DropTable<Record>();
            Conn.CreateTable<Record>();

            // Loads data.json
            var assembly = IntrospectionExtensions.GetTypeInfo(typeof(VocabBuilderViewModel)).Assembly;
            Stream stream = assembly.GetManifestResourceStream("GREVocab.data.json");

            string data = "";
            using (var reader = new StreamReader(stream)) {
                data = reader.ReadToEndAsync().Result;
            }

            // Deserialize and re-serialize to insert into DB
            Word[] words = JsonConvert.DeserializeObject<Word[]>(data);
            foreach (var word in words) {
                int timesStudied = new Random().Next(0, 7);
                Conn.Insert(new Record {
                    Json = JsonConvert.SerializeObject(word),
                    TimesStudied = timesStudied,
                    NextSchedule = timesStudied == 0 ? DateTime.Now + new TimeSpan(100 * 365, 0, 0, 0, 0)
                    : DateTime.Now - new TimeSpan(24, 0, 0),
                });
            }

            // Refresh the saved records
            GetAllRecords();
            OnPropertyChanged("AllRecords");
        }

        private void GetAllRecords() {
            var allRecordsList = Conn.Table<Record>().ToList();
            allRecords = new ObservableCollection<Record>();
            foreach (var r in allRecordsList) {
                allRecords.Add(r);
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Debug method to check if a list is unique
        private bool IsListUnique<T>(List<T> list) {
            HashSet<T> seen = new HashSet<T>();
            foreach (T t in list) {
                if (seen.Contains(t))
                    return false;
                else
                    seen.Add(t);
            }

            return true;
        }

        public void SetToOne(Record r) {
            DateTime today = DateTime.Now.Date;
            r.TimesStudied = 1;
            r.NextSchedule = new DateTime(today.Year, today.Month, today.Day,
                20, 0, 0);
            Conn.Update(r);
        }
    }
}
