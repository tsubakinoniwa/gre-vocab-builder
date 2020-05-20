using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using SQLite;
using Xamarin.Essentials;

namespace GREVocab {
    public class VocabBuilderViewModel : INotifyPropertyChanged {
        private SQLiteConnection conn;
        public event PropertyChangedEventHandler PropertyChanged;

        public VocabBuilderViewModel() {
            // Connect to database
            string libFolder = FileSystem.AppDataDirectory;
            string fName = Path.Combine(libFolder, "record.db");
            conn = new SQLiteConnection(fName);
            conn.CreateTable<Record>();
        }

        public async void InitDatabase() {
            conn.DropTable<Record>();
            conn.CreateTable<Record>();

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
                conn.Insert(new Record {
                    Json = JsonConvert.SerializeObject(word),
                    LastMemorized = DateTime.Now,
                    TimesMemorized = 0
                });
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
