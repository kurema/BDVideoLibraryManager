
using System.ComponentModel;
using System.Collections.Generic;

using System.Linq;

using VideoLibraryManagerCommon.Library;
using System;
using System.Globalization;

namespace BDVideoLibraryManagerXF.ViewModels
{
    public class LibraryViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }

        public Library FullLibrary { get { return _FullLibrary; } set { _FullLibrary = value; OnPropertyChanged(nameof(Library)); OnPropertyChanged(nameof(FullLibrary)); } }
        private Library _FullLibrary = new Library(new DiskBD[0]);

        public Library Library { get { IsBusy = true;  var result= Search(FullLibrary, SearchWord,SearchGenre,TargetDisc);IsBusy = false;return result; } }

        public string SearchWord { get { return _SearchWord; } set { _SearchWord = value;OnPropertyChanged(nameof(SearchWord)); if (value is null or "") OnPropertyChanged(nameof(Library)); } }
        private string _SearchWord;

        public string SearchGenre { get { return _SearchGenre; } set { _SearchGenre = value; OnPropertyChanged(nameof(SearchGenre)); OnPropertyChanged(nameof(Library)); } }
        private string _SearchGenre="";

        public DiskBD TargetDisc { get { return _TargetDisc; }set { _TargetDisc = value; OnPropertyChanged(nameof(TargetDisc)); OnPropertyChanged(nameof(Library)); } }
        private DiskBD _TargetDisc;

        public System.Windows.Input.ICommand SearchCommand { get { return _SearchCommand ??= new DelegateCommand((o) => true, (o) => OnPropertyChanged(nameof(Library))); } }
        private System.Windows.Input.ICommand _SearchCommand;

        public bool IsBusy { get { return _IsBusy; }set { _IsBusy = value;OnPropertyChanged(nameof(IsBusy)); } }
        private bool _IsBusy;

        static private Library Search(Library lib, string word,string genre,DiskBD TargetDisk)
        {
            if (TargetDisk != null)
            {
                lib = new Library(new DiskBD[] { TargetDisk });
            }
            if (string.IsNullOrEmpty(word) && (string.IsNullOrEmpty(genre))) return lib;
            var result = new Queue<DiskBD>();
            foreach(var item in lib.Contents)
            {
                IEnumerable<VideoBD> videos = item.Contents;
                if (!string.IsNullOrEmpty(word))
                    videos = videos.Where(b => ContainsAmbiguous(b.ProgramTitleNormalized, VideoBD.NormalizeText(word)) || ContainsAmbiguous(b.ProgramDetailNormalized, VideoBD.NormalizeText(word)));
                if (!string.IsNullOrEmpty(genre))
                    videos = videos.Where(b => b.ProgramGenre.Contains(genre));

                if (videos.Count() > 0)
                {
                    var disk = new DiskBD(item.DiskTitle, item.DiskName, videos.ToArray());
                    result.Enqueue(disk);
                }
            }
            return new Library(result.ToArray());
        }

        public static bool ContainsAmbiguous(string text, string word)
        {
            if (word == null || word == "") { return true; }
            var ci = System.Globalization.CultureInfo.CurrentCulture.CompareInfo;
            return ci.IndexOf(text, word, System.Globalization.CompareOptions.IgnoreKanaType | System.Globalization.CompareOptions.IgnoreCase | System.Globalization.CompareOptions.IgnoreWidth
                | System.Globalization.CompareOptions.IgnoreNonSpace | System.Globalization.CompareOptions.IgnoreSymbols) != -1;
        }

        public class DelegateCommand : System.Windows.Input.ICommand
        {
            public event EventHandler CanExecuteChanged;

            public Func<object, bool> CanExecuteDelegate;
            public Action<object> ExecuteDelegate;

            public DelegateCommand(Func<object, bool> CanExecuteDelegate, Action<object> ExecuteDelegate)
            {
                this.CanExecuteDelegate = CanExecuteDelegate;
                this.ExecuteDelegate = ExecuteDelegate;
            }

            public bool CanExecute(object parameter)
            {
                return CanExecuteDelegate(parameter);
            }

            public void Execute(object parameter)
            {
                ExecuteDelegate(parameter);
            }
        }

    }
}
