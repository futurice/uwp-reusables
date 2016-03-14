using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Windows.System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using UwpReusables.Controls.Standard.Formatting;
using UwpReusables.Controls.Standard.Validation;

namespace UwpReusables.TestBed
{
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        //Validation functions take a string as input, and return a bool: True for "valid input", false for "invalid input".
        public Func<string, bool> NoDotsFunction { get; set; } = NoDotsFunctionImpl;
        public Func<string, bool> NoExclamationsFunction { get; set; } = NoExclamationsImpl;
        public Func<string, bool> NotEmptyValidator { get; set; } = NotEmptyImpl;
        public IList<Func<string, string>> TopBoxValidationFunctions => new List<Func<string, string>>
        {
            input => input.Contains("!") ? "No shouting! Exclamations aren't allowed." : null,
            input => input.Contains(".") ? "Can't be having none of them dots, either." : null
        };

        private bool _isResetDirtyStateBoxDirty;

        public bool IsResetDirtyStateBoxDirty
        {
            get { return _isResetDirtyStateBoxDirty; }
            set
            {
                if (_isResetDirtyStateBoxDirty != value)
                {
                    _isResetDirtyStateBoxDirty = value;
                    OnPropertyChanged(nameof(IsResetDirtyStateBoxDirty));
                }
            }
        }

        public MainPage()
        {
            this.InitializeComponent();

            //Validation can also be defined in code-behind as well.
            string locallyScopedString = "closures work just fine";
            CodeBehindBox.ValidationPairs.Add(new ValidationPair
            {
                ValidationFunction = s => s.Contains("@"),
                ErrorMessage = $"Gotta have at least one @ here. And {locallyScopedString}!",
            });
        }

        private static bool NoDotsFunctionImpl(string input)
        {
            return !input.Contains(".");
        }

        private static bool NoExclamationsImpl(string arg)
        {
            return !arg.Contains("!");
        }

        private static bool NotEmptyImpl(string arg)
        {
            return !String.IsNullOrWhiteSpace(arg);
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ResetDirtyButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            IsResetDirtyStateBoxDirty = false;
        }

        /// <summary>
        /// Sample formatting text using <see cref="InlineFormatter.DefaultTransformer"/>.
        /// </summary>
        public string SimpleFormattingText { get; private set; } 
            = "<format>This <bold>text</bold> <underline>has</underline> <italic>formatting</italic>!</format>";

        /// <summary>
        /// Sample formatting text using <see cref="ColorTransformer"/>.
        /// </summary>
        public string ColorFormattingText { get; private set; } 
            = "<format>Color <blue>up</blue> <red>your</red> <green>life</green>!</format>";

        /// <summary>
        /// Sample formatting text using <see cref="ColorTransformer"/>.
        /// </summary>
        public string LinkFormattingText { get; private set; } 
            = @"<format>Click <close>here</close> to close the app, or <a href=""http://futurice.com"">here</a> to go to our home page.</format>";

        /// <summary>
        /// The XML => <see cref="Span"/> transformer for colored text. See <see cref="InlineFormatter"/>.
        /// </summary>
        public Func<XElement, Span> ColorTransformer { get; private set; } = ColorTransformImpl;

        /// <summary>
        /// Implementation for <see cref="ColorTransformer"/>.
        /// </summary>
        /// <param name="el">Formatting input XML element.</param>
        /// <returns><see cref="Span"/> with properties corresponding to <paramref name="el"/>.</returns>
        private static Span ColorTransformImpl(XElement el)
        {
            switch (el.Name.LocalName.ToLowerInvariant())
            {
                case "red":
                    return new Span() { Foreground = new SolidColorBrush(Colors.Red) };
                case "green":
                    return new Span() { Foreground = new SolidColorBrush(Colors.Green) };
                case "blue":
                    return new Span() { Foreground = new SolidColorBrush(Colors.Blue) };
                default:
                    return null;
            }
        }

        /// <summary>
        /// The XML => <see cref="Span"/> transformer for link text. See <see cref="InlineFormatter"/>.
        /// </summary>
        public Func<XElement, Span> LinkTransformer { get; private set; } = LinkTransformerImpl;

        /// <summary>
        /// Implementation for <see cref="LinkTransformer"/>.
        /// </summary>
        /// <param name="el">Formatting input XML element.</param>
        /// <returns><see cref="Span"/> with properties corresponding to <paramref name="el"/>.</returns>
        private static Span LinkTransformerImpl(XElement el)
        {
            Hyperlink link;
            switch (el.Name.LocalName.ToLowerInvariant())
            {
                case "close":
                    link = new Hyperlink() { Foreground = new SolidColorBrush(Colors.Blue) };
                    link.Click += (_, __) => Application.Current.Exit();
                    return link;
                case "a":
                    link = new Hyperlink() { Foreground = new SolidColorBrush(Colors.Blue) };
                    link.Click += async (_, __) => await Launcher.LaunchUriAsync(new Uri(el.Attribute("href")?.Value));
                    return link;
                default:
                    return null;
            }
        }
    }
}
