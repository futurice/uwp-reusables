using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml.Controls;
using ValidatingTextBox;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TestBed
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        public Func<string, bool> ValidateFunction { get; set; } = ValidateFunctionImpl;
        public Func<string, bool> NoExclamationsFunction { get; set; } = NoExclamationsImpl;

        public MainPage()
        {
            this.InitializeComponent();

            string localScopedString = "local-scope-land";
            CodeBehindBox.ValidationPairs.Add(new ValidationPair
            {
                ErrorMessage = $"Hey! You can't do that here in {localScopedString}!",
                ValidationFunction = s => s.Contains("@")
            });
        }

        private static bool ValidateFunctionImpl(string input)
        {
            if (input.Contains("."))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private static bool NoExclamationsImpl(string arg)
        {
            if (arg.Contains("!"))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
