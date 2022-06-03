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

        public string[] Genres
        {
            get
            {
                if (_Genres != null) return _Genres;
                var result = new List<string>();
                foreach (var disk in this.Contents)
                {
                    foreach (var video in disk.Contents)
                    {
                        if (string.IsNullOrWhiteSpace(video.ProgramGenre)) continue;
                        foreach (var genre in video.ProgramGenre.Split('　'))
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

    public class DiskVideoPairList : IList<DiskVideoPair>, INotifyPropertyChanged, System.Collections.Specialized.INotifyCollectionChanged
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
        public DiskVideoPair(DiskBD disk, VideoBD video)
        {
            this.Video = video;
            this.Disk = disk;
        }
    }


    public class DiskBD : IEnumerable<VideoBD>
    {
        public string DiskTitle { get; private set; }
        public string DiskName { get; private set; }
        public VideoBD[] Contents { get; private set; }

        public TimeSpan TimeSpan
        {
            get
            {
                var total = new TimeSpan(0);
                foreach (var item in Contents)
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

        public DiskBD(string title, string name, VideoBD[] contents)
        {
            DiskTitle = title;
            DiskName = name;
            Contents = contents;
        }

        public DiskBD(TextReader tr, string DiskName)
        {
            this.DiskName = DiskName;

            var parser = new CsvHelper.CsvParser(tr);
            parser.Configuration.HasHeaderRecord = false;
            DiskTitle = parser.Read()[0];
            var result = new Queue<VideoBD>();
            while (true)
            {
                var line = parser.Read();
                if (line == null) { Contents = result.ToArray(); return; }
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

        private LinkedText[] _Links = null;
        public LinkedText[] Links => _Links ?? LinkedText.GetFromText(ProgramDetailNormalized);

        public static string NormalizeText(string s)
        {
            s = s.ToLower().Replace('―', '-').Replace('ー', '-').Replace("・", "").Replace('　', ' ').Replace("：", ":");
            s = Regex.Replace(s, "[０-９]", p => ((char)(p.Value[0] - '０' + '0')).ToString());
            s = Regex.Replace(s, "[ａ-ｚ]", p => ((char)(p.Value[0] - 'ａ' + 'a')).ToString());
            s = Regex.Replace(s, "[Ａ-Ｚ]", p => ((char)(p.Value[0] - 'Ａ' + 'A')).ToString());
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

        public VideoBD(string[] CsvEntry)
        {
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

    public class LinkedText
    {
        private const string PatternHttpCharsInside = @"\w!?/+\-_~;.,*&@#$%()'[\]";
        public const string PatternHttpChars = "[" + PatternHttpCharsInside + "]";
        public const string PatternHttpCharsNot = "[^" + PatternHttpCharsInside + "]";

        private static Regex _RegexPhone = null;
        public static Regex RegexPhone => _RegexPhone = _RegexPhone ?? new Regex(@"(0\d{1,4})[\-\(](\d{1,4})[\-\)](\d{3,4})", RegexOptions.Compiled);
        //下だと末尾にマッチしなくて対策も面倒なので上にしました。
        //public static Regex RegexPhone => _RegexPhone = _RegexPhone ?? new Regex(@"(?<=[^\d])(0\d{1,4})[\-\(](\d{1,4})[\-\)](\d{3,4})(?=[^\d])", RegexOptions.Compiled);

        private static Regex _RegexHttp1 = null;
        public static Regex RegexHttp1 => _RegexHttp1 = _RegexHttp1 ?? new Regex($@"https?://{PatternHttpChars}+", RegexOptions.Compiled);

        private static Regex _RegexHttp2 = null;
        public static Regex RegexHttp2 => _RegexHttp2 = _RegexHttp2 ?? new Regex($@"www\.{PatternHttpChars}+", RegexOptions.Compiled);

        //private static Regex _RegexHttp3 = null;
        //public static Regex RegexHttp3 => _RegexHttp3 = _RegexHttp3 ?? new Regex($@"{RegexHttpChars}+\.(?:jp|com|gov|net|co|org)(?:/{RegexHttpChars}+|)(?=[^\w!?/+\-_~;.,*&@#$%()'[\]])", RegexOptions.Compiled);

        //バックトラックで処理速度が遅くなるのを避けるために二段階に分けました。
        private static Regex _RegexHttp3 = null;
        public static Regex RegexHttp3 => _RegexHttp3 = _RegexHttp3 ?? new Regex($@"\.(?:jp|com|gov|net|co|org)(?:/{PatternHttpChars}+|)(?={PatternHttpCharsNot})", RegexOptions.Compiled);


        //private static Regex _RegexHttp3Pre = null;
        //public static Regex RegexHttp3Pre => _RegexHttp3Pre = _RegexHttp3Pre ?? new Regex($@"\.(?:jp|com|gov|net|co|org)", RegexOptions.Compiled);

        public LinkedText(string text, string textFull, LinkedTextType type)
        {
            Type = type;
            Text = text ?? throw new ArgumentNullException(nameof(text));
            TextFull = textFull ?? throw new ArgumentNullException(nameof(textFull));
        }

        public LinkedTextType Type { get; set; }

        public string Text { get; set; }

        public string TextFull { get; set; }

        public static LinkedText[] GetFromText(string text)
        {
            var result = new List<(LinkedText, int)>();

            //System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            //System.Diagnostics.Debug.WriteLine("_____");
            //sw.Start();
            //sw.Stop();
            //System.Diagnostics.Debug.WriteLine(sw.ElapsedMilliseconds);
            //sw.Restart();
            {
                //var matches = Regex.Matches(text, @"(?:[^\d])(0\d{1,4})\-(\d{1,4})\-(\d{4})(?:[^\d])");
                var matches = RegexPhone.Matches(text);
                foreach (Match item in matches)
                {
                    if ((item.Groups[1].Value.Length + item.Groups[2].Value.Length) == 5 && item.Groups[3].Length == 4)
                    {
                        //固定電話
                        //https://www.soumu.go.jp/main_sosiki/joho_tsusin/top/tel_number/q_and_a.html#q2
                    }
                    else if (Regex.IsMatch(item.Groups[1].Value, @"0[2-9]0") && item.Groups[2].Value.Length == 4 && item.Groups[3].Length == 4)
                    {
                        //携帯電話その他
                        //010は国際電話に使うようだが、番組情報で出て来る可能性はないだろう。
                    }
                    else
                    {
                        //他に0120とか色々あるので結局素通し。
                    }

                    result.Add((new LinkedText(item.Value, item.Value, LinkedTextType.PhoneNumber), item.Index));
                }
            }
            {
                var matches = RegexHttp1.Matches(text);
                foreach (Match item in matches)
                {
                    if (!item.Value.Contains('.')) continue;
                    result.Add((new LinkedText(item.Value, item.Value, LinkedTextType.Http), item.Index));
                }
            }
            {
                var matches = RegexHttp2.Matches(text);
                foreach (Match item in matches)
                {
                    if (!item.Value.Contains('.')) continue;
                    if (result.Any(a => a.Item1.Text.Contains(item.Value))) continue;
                    result.Add((new LinkedText(item.Value, $"https://{item.Value}", LinkedTextType.HttpAssumption), item.Index));
                }
            }
            {
                var matches = RegexHttp3.Matches(text);
                foreach (Match item in matches)
                {
                    var matches2 = Regex.Matches(text,$@"{PatternHttpChars}+{Regex.Escape(item.Value)}");
                    foreach (Match item2 in matches2)
                    {
                        if (result.Any(a => a.Item1.Text.Contains(item2.Value))) continue;
                        result.Add((new LinkedText(item2.Value, $"https://{item2.Value}", LinkedTextType.HttpAssumption), item2.Index));
                    }
                }

            }

            return result.OrderBy(a => a.Item2).Select(a => a.Item1).ToArray();
        }
    }

    public enum LinkedTextType
    {
        PhoneNumber, Http, HttpAssumption
    }
}
