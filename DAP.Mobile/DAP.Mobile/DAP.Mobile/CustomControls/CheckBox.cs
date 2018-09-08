using Prism.Commands;
using Xamarin.Forms;

namespace DAP.Mobile.CustomControls
{
    public class CheckBox : StackLayout
    {
        private readonly BoxView boxBackground = new BoxView { HeightRequest = 25, WidthRequest = 25, BackgroundColor = Color.LightGray, VerticalOptions = LayoutOptions.CenterAndExpand };
        private readonly BoxView boxSelected = new BoxView { IsVisible = false, HeightRequest = 18, WidthRequest = 18, BackgroundColor = Color.Accent, VerticalOptions = LayoutOptions.CenterAndExpand, HorizontalOptions = LayoutOptions.Center };
        private readonly Label lblSelected = new Label { Text = "✓", FontAttributes = FontAttributes.Bold, IsVisible = false, TextColor = Color.Accent, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.CenterAndExpand };
        private readonly Label lblOption = new Label { VerticalOptions = LayoutOptions.CenterAndExpand };
        private CheckType _type = CheckType.Box;

        public CheckBox()
        {
            this.Orientation = StackOrientation.Horizontal;
            //this.Margin = new Thickness(10, 0);
            //this.Padding = new Thickness(10);
            //this.Spacing = 10;
            this.Children.Add(new Grid { Children = { boxBackground, boxSelected } });
            this.Children.Add(lblOption);
            this.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new DelegateCommand(() => { IsChecked = !IsChecked; }),
            });
        }

        public CheckBox(string optionName, int key) : this()
        {
            Key = key;
            lblOption.Text = optionName;
        }

        public int Key { get; set; }
        public string Text { get => lblOption.Text; set => lblOption.Text = value; }

        public bool IsChecked
        {
            get => ((this.Children[0] as Grid).Children[1]).IsVisible;
            set
            {
                (this.Children[0] as Grid).Children[1].IsVisible = value;
                SetValue(IsCheckedProperty, value);
            }
        }

        public Color Color { get => boxSelected.BackgroundColor; set { boxSelected.BackgroundColor = value; lblSelected.TextColor = value; } }
        public Color TextColor { get => lblOption.TextColor; set => lblOption.TextColor = value; }
        public CheckType Type { get => _type; set { _type = value; UpdateType(value); } }

        public static readonly BindableProperty ColorProperty = BindableProperty.Create(nameof(Color), typeof(Color), typeof(CheckBox), Color.Accent, propertyChanged: (bo, ov, nv) => (bo as CheckBox).Color = (Color)nv);
        public static readonly BindableProperty TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(CheckBox), Color.Gray, propertyChanged: (bo, ov, nv) => (bo as CheckBox).TextColor = (Color)nv);
        public static readonly BindableProperty IsCheckedProperty = BindableProperty.Create(nameof(IsChecked), typeof(bool), typeof(CheckBox), false, BindingMode.TwoWay, propertyChanged: (bo, ov, nv) => (bo as CheckBox).IsChecked = (bool)nv);
        public static readonly BindableProperty KeyProperty = BindableProperty.Create(nameof(Key), typeof(int), typeof(CheckBox), 0, propertyChanged: (bo, ov, nv) => (bo as CheckBox).Key = (int)nv);
        public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(CheckBox), "", propertyChanged: (bo, ov, nv) => (bo as CheckBox).Text = (string)nv);

        private void UpdateType(CheckType _Type)
        {
            switch (_Type)
            {
                case CheckType.Box:
                    (this.Children[0] as Grid).Children.Remove(lblSelected);
                    (this.Children[0] as Grid).Children.Add(boxSelected);
                    break;

                case CheckType.Check:
                    lblSelected.Text = "✓";
                    (this.Children[0] as Grid).Children.Remove(boxSelected);
                    (this.Children[0] as Grid).Children.Add(lblSelected);
                    break;

                case CheckType.Cross:
                    lblSelected.Text = "✕";
                    (this.Children[0] as Grid).Children.Remove(boxSelected);
                    (this.Children[0] as Grid).Children.Add(lblSelected);
                    break;

                case CheckType.Star:
                    lblSelected.Text = "★";
                    (this.Children[0] as Grid).Children.Remove(boxSelected);
                    (this.Children[0] as Grid).Children.Add(lblSelected);
                    break;
            }
        }

        public enum CheckType
        {
            Box,
            Check,
            Cross,
            Star
        }
    }
}