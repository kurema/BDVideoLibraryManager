using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel;
using System.Collections;

using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Specialized;

namespace VideoLibraryManagerCommon.Library
{
    public class Library
    {
        public DiskBD[] Contents { get; private set; }

        public Library(DiskBD[] contents)
        {
            this.Contents = contents;
        }

        public TimeSpan TimeSpan
        {
            get
            {
                var total = new TimeSpan(0);
                foreach (var item in Contents)
                {
                    total += item.TimeSpan;
                }
                return total;
            }
        }

        public string[] Genres { get
            {
                if (_Genres != null) return _Genres;
                var result = new List<string>();
                foreach(var disk in this.Contents)
                {
                    foreach(var video in disk.Contents)
                    {
                        foreach(var genre in video.ProgramGenre.Split('　'))
                        {
                            if (!result.Contains(genre)) result.Add(genre);
                            var mainGenre = genre.Split(' ')[0];
                            if (!result.Contains(mainGenre)) result.Add(mainGenre);
                        }
                    }
                }
                _Genres = result.ToArray();
                Array.Sort(_Genres);
                return _Genres;
            }
        }
        private string[] _Genres;
    }

    public class DiskVideoPairList : IList<DiskVideoPair>,INotifyPropertyChanged,System.Collections.Specialized.INotifyCollectionChanged
    {
        private List<DiskVideoPair> Contents = new List<DiskVideoPair>();

        public event PropertyChangedEventHandler PropertyChanged;
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private void OnPropertyChanged(string name) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        private void OnCollectionChanged(NotifyCollectionChangedAction Action) { CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(Action)); }

        public TimeSpan TimeSpan
        {
            get
            {
                var total = new TimeSpan(0);
                foreach (var item in Contents)
                {
                    total += item.Video.Length;
                }
                return total;
            }
        }

        public void SetContents(IEnumerable<DiskVideoPair> dp)
        {
            this.Contents = dp.ToList();
            OnCollectionChanged(NotifyCollectionChangedAction.Reset);
            OnPropertyChanged(nameof(TimeSpan));
        }

        #region IList interface
        public DiskVideoPair this[int index]
        {
            get
            {
                return ((IList<DiskVideoPair>)Contents)[index];
            }

            set
            {
                ((IList<DiskVideoPair>)Contents)[index] = value;
            }
        }

        public int Count
        {
            get
            {
                return ((IList<DiskVideoPair>)Contents).Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return ((IList<DiskVideoPair>)Contents).IsReadOnly;
            }
        }

        public void Add(DiskVideoPair item)
        {
            ((IList<DiskVideoPair>)Contents).Add(item);
        }

        public void Clear()
        {
            ((IList<DiskVideoPair>)Contents).Clear();
        }

        public bool Contains(DiskVideoPair item)
        {
            return ((IList<DiskVideoPair>)Contents).Contains(item);
        }

        public void CopyTo(DiskVideoPair[] array, int arrayIndex)
        {
            ((IList<DiskVideoPair>)Contents).CopyTo(array, arrayIndex);
        }

        public IEnumerator<DiskVideoPair> GetEnumerator()
        {
            return ((IList<DiskVideoPair>)Contents).GetEnumerator();
        }

        public int IndexOf(DiskVideoPair item)
        {
            return ((IList<DiskVideoPair>)Contents).IndexOf(item);
        }

        public void Insert(int index, DiskVideoPair item)
        {
            ((IList<DiskVideoPair>)Contents).Insert(index, item);
        }

        public bool Remove(DiskVideoPair item)
        {
            return ((IList<DiskVideoPair>)Contents).Remove(item);
        }

        public void RemoveAt(int index)
        {
            ((IList<DiskVideoPair>)Contents).RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList<DiskVideoPair>)Contents).GetEnumerator();
        }
        #endregion
    }
    public class DiskVideoPair
    {
        public VideoBD Video { get; private set; }
        public DiskBD Disk { get; private set; }
        public DiskVideoPair(DiskBD disk,VideoBD video)
        {
            this.Video = video;
            this.Disk = disk;
        }
    }


    public class DiskBD:IEnumerable<VideoBD>
    {
        public string DiskTitle { get; private set; }
        public string DiskName { get; private set; }
        public VideoBD[] Contents { get; private set; }

        public TimeSpan TimeSpan { get
            {
                var total = new TimeSpan(0);
                foreach(var item in Contents)
                {
                    total += item.Length;
                }
                return total;
            }
        }

        public DiskBD()
        {
            DiskTitle = "";
            DiskName = "";
            Contents = new VideoBD[0];
        }

        public DiskBD(string title,string name,VideoBD[] contents)
        {
            DiskTitle = title;
            DiskName = name;
            Contents = contents;
        }

        public DiskBD(TextReader tr,string DiskName)
        {
            this.DiskName = DiskName;

            var parser = new CsvHelper.CsvParser(tr);
            parser.Configuration.HasHeaderRecord = false;
            DiskTitle = parser.Read()[0];
            var result = new Queue<VideoBD>();
            while (true)
            {
                var line = parser.Read();
                if (line==null) { Contents = result.ToArray(); return; }
                result.Enqueue(new VideoBD(line));
            }
        }

        public IEnumerator<VideoBD> GetEnumerator()
        {
            return ((IEnumerable<VideoBD>)Contents).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<VideoBD>)Contents).GetEnumerator();
        }
    }


    public class VideoBD
    {
        public DateTime RecordDateTime { get; set; }
        public TimeSpan Length { get; set; }
        public string ChannelName { get; set; }
        public int ChannelNumber { get; set; }
        public string BroadcastType { get; set; }
        public string ProgramTitle { get; set; }
        public string ProgramTitleNormalized { get { return _ProgramTitleNormalized ?? (_ProgramTitleNormalized = NormalizeText(ProgramTitle)); } }
        private string _ProgramTitleNormalized;
        public string ProgramDetail { get; set; }
        public string ProgramDetailNormalized { get { return _ProgramDetailNormalized ?? (_ProgramDetailNormalized = NormalizeText(ProgramDetail)); } }
        private string _ProgramDetailNormalized;
        public string ProgramGenre { get; set; }

        public static string NormalizeText(string s)
        {
            s= s.ToLower().Replace('―', '-').Replace('ー', '-').Replace("・", "").Replace('　', ' ').Replace("：",":");
            s= Regex.Replace(s, "[０-９]", p => ((char)(p.Value[0] - '０' + '0')).ToString());
            s= Regex.Replace(s, "[ａ-ｚ]", p => ((char)(p.Value[0] - 'ａ' + 'a')).ToString());
            s= Regex.Replace(s, "[Ａ-Ｚ]", p => ((char)(p.Value[0] - 'Ａ' + 'A')).ToString());
            return s;
        }

        public VideoBD()
        {
            RecordDateTime = DateTime.MaxValue;
            ChannelName = "";
            ChannelNumber = -1;
            BroadcastType = "";
            ProgramTitle = "";
            ProgramDetail = "";
            ProgramGenre = "";
        }

        public VideoBD(string[] CsvEntry) {
            var date = DateTime.Parse(CsvEntry[3]);
            var time = DateTime.Parse(CsvEntry[4]);
            this.RecordDateTime = date.Date + time.TimeOfDay;
            this.Length = TimeSpan.Parse(CsvEntry[5]);
            this.ChannelName = CsvEntry[9];
            this.ChannelNumber = int.Parse(CsvEntry[10].Substring(0, CsvEntry[10].Length - 2));
            this.BroadcastType = CsvEntry[11];
            this.ProgramTitle = CsvEntry[12];
            this.ProgramDetail = CsvEntry[13];
            this.ProgramGenre = CsvEntry[15];
        }

    }
}
