using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BDVideoLibraryManagerXF.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LinksView : ContentView
    {
        //電話を書けるには権限が必要。実際は開くだけでも。
        //https://docs.microsoft.com/ja-jp/xamarin/essentials/phone-dialer?tabs=android
        public bool IsDialEnabled { get; set; } = false;

        public LinksView()
        {
            InitializeComponent();
        }

        public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable<VideoLibraryManagerCommon.Library.LinkedText>)
            , typeof(LinksView), new VideoLibraryManagerCommon.Library.LinkedText[0],
            propertyChanged: (b, o, n) =>
            {
                (b as LinksView)?.UpdateStacks();
            });

        public IEnumerable<VideoLibraryManagerCommon.Library.LinkedText> ItemsSource
        {
            get => (IEnumerable<VideoLibraryManagerCommon.Library.LinkedText>)GetValue(ItemsSourceProperty);
            set
            {
                SetValue(ItemsSourceProperty, value);
            }
        }

        public Color? TextColor
        {
            get { return (Color?)GetValue(TextColorProperty); }
            set { SetValue(TextColorProperty, value); }
        }

        public static readonly BindableProperty TextColorProperty =
            BindableProperty.Create(nameof(TextColor), typeof(Color?), typeof(LinksView), null, propertyChanged: (b, o, n) =>
            {
                if (o == n) return;
                (b as LinksView)?.UpdateLabelDesign();
            });

        public bool EnableWebArchive
        {
            get { return (bool)GetValue(EnableWebArchiveProperty); }
            set { SetValue(EnableWebArchiveProperty, value); }
        }

        public static readonly BindableProperty EnableWebArchiveProperty =
            BindableProperty.Create(nameof(EnableWebArchive), typeof(bool), typeof(LinksView),false, propertyChanged: (b, o, n) =>
            {
                if (o == n) return;
                (b as LinksView)?.UpdateLabelDesign();
            });

        public string WebArchiveTitle
        {
            get { return (string)GetValue(WebArchiveTitleProperty); }
            set { SetValue(WebArchiveTitleProperty, value); }
        }

        public static readonly BindableProperty WebArchiveTitleProperty =
            BindableProperty.Create(nameof(WebArchiveTitle), typeof(string), typeof(LinksView), "archive.org", propertyChanged: (b, o, n) =>
            {
                if (o == n) return;
                (b as LinksView)?.UpdateLabelDesign();
            });

        public DateTime TargetDate
        {
            get { return (DateTime)GetValue(TargetDateProperty); }
            set { SetValue(TargetDateProperty, value); }
        }

        public static readonly BindableProperty TargetDateProperty =
            BindableProperty.Create(nameof(TargetDate), typeof(DateTime), typeof(LinksView), DateTime.Now, propertyChanged: (b, o, n) =>
            {
                if (o == n) return;
                (b as LinksView)?.UpdateLabelDesign();
            });

        public Thickness Spacing
        {
            get { return (Thickness)GetValue(SpacingProperty); }
            set { SetValue(SpacingProperty, value); }
        }

        public static readonly BindableProperty SpacingProperty =
            BindableProperty.Create(nameof(Spacing), typeof(Thickness), typeof(LinksView), null, propertyChanged: (b, o, n) =>
            {
                if (o == n) return;
                (b as LinksView)?.UpdateLabelDesign();
            });

        public void UpdateLabelDesign()
        {
            ActionLabels(l =>
            {
                l.TextColor = TextColor ?? l.TextColor;
                l.Margin = Spacing;
            });
        }

        private void ActionLabels(Action<Label> action)
        {
            if (action is null) return;
            foreach (var item in MainStack.Children)
            {
                if (item is not Label l) continue;
                action?.Invoke(l);
            }
        }

        public void UpdateStacks()
        {
            MainStack.Children.Clear();
            if (ItemsSource is null) return;
            var fontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label));
            foreach (var item in ItemsSource)
            {
                EventHandler action = null;
                EventHandler actionOldLink = null;
                string address = item.TextFull;

                switch (item.Type)
                {
                    case VideoLibraryManagerCommon.Library.LinkedTextType.PhoneNumber:
                        if (!IsDialEnabled) continue;

                        {
                            action= (_, _) =>
                            {
                                try
                                {
                                    Xamarin.Essentials.PhoneDialer.Open(address);
                                }
                                catch
                                {
                                }
                            };
                        }
                        break;
                    case VideoLibraryManagerCommon.Library.LinkedTextType.Http:
                    case VideoLibraryManagerCommon.Library.LinkedTextType.HttpAssumption:
                        {
                            action = async (_, _) =>
                            {
                                try
                                {
                                    await Xamarin.Essentials.Browser.OpenAsync(address);
                                }
                                catch { }
                            };
                            actionOldLink = async (_, _) =>
                            {
                                try
                                {
                                    //厳密には色々考えてUTC変換しないといけないが、Xamarinでタイムゾーン系のクラス触るのは危ういので単純減算。
                                    await Xamarin.Essentials.Browser.OpenAsync($"https://web.archive.org/web/{TargetDate.AddHours(-9):yyyyMMddHHmmss}/{address}");
                                }
                                catch { }
                            };
                        }
                        break;
                    case VideoLibraryManagerCommon.Library.LinkedTextType.Search:
                        {
                            action = async (_, _) =>
                            {
                                try
                                {
                                    await Xamarin.Essentials.Browser.OpenAsync(address);
                                }
                                catch { }
                            };
                        }
                        break;
                }

                if (actionOldLink is null || !EnableWebArchive)
                {
                    var label = new Label()
                    {
                        Text = item.Text,
                        TextDecorations = TextDecorations.Underline,
                        FontSize = fontSize,
                    };
                    label.TextColor = TextColor ?? label.TextColor;
                    label.Margin = Spacing;
                    var tapr = new TapGestureRecognizer();
                    tapr.Tapped += action;
                    label.GestureRecognizers.Add(tapr);
                    MainStack.Children.Add(label);
                }
                else
                {
                    var label = new Label()
                    {
                        FontSize = fontSize,
                    };
                    label.TextColor = TextColor ?? label.TextColor;
                    label.Margin = Spacing;
                    label.FormattedText = new FormattedString();
                    {
                        var span = new Span()
                        {
                            Text = item.Text,
                            TextDecorations = TextDecorations.Underline,
                        };
                        var tapr = new TapGestureRecognizer();
                        tapr.Tapped += action;
                        span.GestureRecognizers.Add(tapr);
                        label.FormattedText.Spans.Add(span);
                    }
                    {
                        var span = new Span()
                        {
                            Text = " (",
                        };
                        label.FormattedText.Spans.Add(span);
                    }
                    {
                        var span = new Span()
                        {
                            Text = WebArchiveTitle,
                            TextDecorations = TextDecorations.Underline,
                        };
                        var tapr = new TapGestureRecognizer();
                        tapr.Tapped += actionOldLink;
                        span.GestureRecognizers.Add(tapr);
                        label.FormattedText.Spans.Add(span);
                    }
                    {
                        var span = new Span()
                        {
                            Text = ")",
                        };
                        label.FormattedText.Spans.Add(span);
                    }

                    MainStack.Children.Add(label);
                }
            }
        }
    }
}