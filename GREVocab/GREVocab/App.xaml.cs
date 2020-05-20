using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace GREVocab {
    public partial class App : Application {
        public App() {
            InitializeComponent();

            MainPage = new MainPage();
            VocabBuilderViewModel vm = new VocabBuilderViewModel();
            //vm.InitDatabase();
            vm.LoadNewWords();
            vm.LoadReviewWords();

            Console.WriteLine(vm.ReviewRecords.Count);
            Console.WriteLine(vm.NewRecords.Count);

            //foreach (var r in vm.NewRecords) {
            //    Console.WriteLine(r.GetWord().Content);
            //}
        }

        protected override void OnStart() {
        }

        protected override void OnSleep() {
        }

        protected override void OnResume() {
        }
    }
}
