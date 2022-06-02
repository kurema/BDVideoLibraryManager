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
            BindableProperty.Create(nameof(TextColor), typeof(Color?), typeof(LinksView), null, propertyChanged: (b, _, _) =>
            {
                (b as LinksView)?.UpdateLabelDesign();
            });

        public Thickness Spacing
        {
            get { return (Thickness)GetValue(SpacingProperty); }
            set { SetValue(SpacingProperty, value); }
        }

        public static readonly BindableProperty SpacingProperty =
            BindableProperty.Create(nameof(Spacing), typeof(Thickness), typeof(LinksView), null, propertyChanged: (b, _, _) =>
            {
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
            if (ItemsSource is null)
            {
                MainStack.Children.Clear();
                return;
            }
            var fontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label));
            foreach (var item in ItemsSource)
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
                //label.GestureRecognizers.Add( )
                string address = item.TextFull;

                switch (item.Type)
                {
                    case VideoLibraryManagerCommon.Library.LinkedTextType.PhoneNumber:
                        if (!IsDialEnabled) continue;

                        {
                            tapr.Tapped += (_, _) =>
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
                            tapr.Tapped += async (_, _) =>
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
                label.GestureRecognizers.Add(tapr);
                MainStack.Children.Add(label);
            }
        }
    }
}