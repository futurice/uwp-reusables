using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
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
    }
}
