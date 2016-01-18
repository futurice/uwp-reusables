using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;

namespace ValidatingTextBox
{
    [ContentProperty(Name = "ValidationPairs")]    
    public class ValidatingTextBox : TextBox
    {
        private readonly List<string> _errorList = new List<string>();

        private Border _errorFlyoutHost;
        private TextBlock _errorFlyoutTextBlock;
        private Flyout _errorFlyout;

        private Button _errorHint;
        private Button ErrorHint
        {
            get { return _errorHint; }
            set
            {
                if (_errorHint != null)
                {
                    _errorHint.Tapped -= ErrorHint_Tapped;
                }
                _errorHint = value;
                if (_errorHint != null)
                {
                    _errorHint.Tapped += ErrorHint_Tapped;
                }
            }
        }        

        public ValidatingTextBox()
        {
            this.DefaultStyleKey = typeof(ValidatingTextBox);
            this.TextChanged += ValidatingTextBox_TextChanged;                 
        }

        protected override void OnApplyTemplate()
        {
            _errorFlyoutHost = GetTemplateChild("BorderElement") as Border;
            _errorFlyout = GetTemplateChild("ErrorFlyout") as Flyout;
            _errorFlyoutTextBlock = GetTemplateChild("ErrorFlyoutTextBlock") as TextBlock;
            ErrorHint = GetTemplateChild("ErrorHint") as Button;            

            base.OnApplyTemplate();
        }

        public static readonly DependencyProperty IsDirtyProperty = 
            DependencyProperty.Register("IsDirty", typeof(bool), typeof(ValidatingTextBox), new PropertyMetadata(false));
        public bool IsDirty
        {
            get { return (bool)GetValue(IsDirtyProperty); }
            set { SetValue(IsDirtyProperty, value); }
        }

        public static readonly DependencyProperty IsValidProperty = 
            DependencyProperty.Register("IsValid", typeof(bool), typeof(ValidatingTextBox), new PropertyMetadata(false));
        public bool IsValid
        {
            get { return (bool)GetValue(IsValidProperty); }
            set { SetValue(IsValidProperty, value); }
        }

        public static readonly DependencyProperty ValidationPairsProperty =
            DependencyProperty.Register("ValidationPairs", typeof(List<ValidationPair>), typeof(ValidatingTextBox), new PropertyMetadata(new List<ValidationPair>()) );        
        public List<ValidationPair> ValidationPairs
        {
            get { return (List<ValidationPair>)GetValue(ValidationPairsProperty); }
            set { SetValue(ValidationPairsProperty, value);}
        }

        public static readonly DependencyProperty ErrorHintColorProperty = 
            DependencyProperty.Register("ErrorHintColor", typeof (Brush), typeof (ValidatingTextBox), new PropertyMetadata(new SolidColorBrush(Colors.Red)));
        /// <summary>
        /// Gets or sets a brush that defines the color of the error hint circle. Defaults to a Red SolidColorBrush.
        /// </summary>
        public Brush ErrorHintColor
        {
            get { return (Brush) GetValue(ErrorHintColorProperty); }
            set { SetValue(ErrorHintColorProperty, value); }
        }
        public static readonly DependencyProperty ControlHighlightChromeAltLowBrushProperty = DependencyProperty.Register(
            "ControlHighlightChromeAltLowBrush", typeof (Brush), typeof (ValidatingTextBox), 
            new PropertyMetadata((Brush)Application.Current.Resources["SystemControlHighlightChromeAltLowBrush"]));
        /// <summary>
        /// Brush used to bring keep the text box's border red when it's in an error state.
        /// </summary>
        public SolidColorBrush ControlHighlightChromeAltLowBrush
        {
            get { return (SolidColorBrush) GetValue(ControlHighlightChromeAltLowBrushProperty); }
            set { SetValue(ControlHighlightChromeAltLowBrushProperty, value); }
        }

        public static readonly DependencyProperty ControlHighlightAccentBrushProperty = DependencyProperty.Register(
            "ControlHighlightAccentBrush", typeof (SolidColorBrush), typeof (ValidatingTextBox), 
            new PropertyMetadata((SolidColorBrush)Application.Current.Resources["SystemControlHighlightAccentBrush"]));
        /// <summary>
        /// Brush used to bring keep the text box's border red when it's in an error state.
        /// </summary>
        public SolidColorBrush ControlHighlightAccentBrush
        {
            get { return (SolidColorBrush) GetValue(ControlHighlightAccentBrushProperty); }
            set { SetValue(ControlHighlightAccentBrushProperty, value); }
        }

        public static readonly DependencyProperty ControlForegroundChromeDisabledLowBrushProperty = DependencyProperty.Register(
            "ControlForegroundChromeDisabledLowBrush", typeof (SolidColorBrush), typeof (ValidatingTextBox), 
            new PropertyMetadata((SolidColorBrush)Application.Current.Resources["SystemControlForegroundChromeDisabledLowBrush"]));

        /// <summary>
        /// Brush used to bring keep the text box's border red when it's in an error state.
        /// </summary>
        public SolidColorBrush ControlForegroundChromeDisabledLowBrush
        {
            get { return (SolidColorBrush) GetValue(ControlForegroundChromeDisabledLowBrushProperty); }
            set { SetValue(ControlForegroundChromeDisabledLowBrushProperty, value); }
        }
        
        private void ValidateNewInput(string text)
        {
            //Don't do any validation if the input box hasn't been dirtied.
            if (!IsDirty)
            {
                return;
            }

            bool errorStateChanged = false;
            foreach (var validationPair in ValidationPairs)
            {                
                if (!validationPair.ValidationFunction(text) 
                    && !_errorList.Contains(validationPair.ErrorMessage))
                {
                    _errorList.Add(validationPair.ErrorMessage);
                    errorStateChanged = true;
                }
                else if (validationPair.ValidationFunction(text)
                         && _errorList.Contains(validationPair.ErrorMessage))
                {
                    _errorList.Remove(validationPair.ErrorMessage);
                    errorStateChanged = true;
                }
            }

            if (!errorStateChanged)
            {
                return;
            }

            //Update visual states
            if (_errorList.Count > 0)
            {
                IsValid = false;
                
                if (ErrorHint != null)
                {
                    VisualStateManager.GoToState(this, "ErrorHintVisible", true);
                    ControlHighlightAccentBrush = new SolidColorBrush(Colors.Red);
                    ControlHighlightChromeAltLowBrush = new SolidColorBrush(Colors.Red);    
                    ControlForegroundChromeDisabledLowBrush = new SolidColorBrush(Colors.Red);                
                }
            }
            else
            {
                IsValid = true;                
                if (ErrorHint != null)
                {
                    VisualStateManager.GoToState(this, "ErrorHintCollapsed", true);
                    ControlHighlightAccentBrush = (SolidColorBrush)Application.Current.Resources["SystemControlHighlightAccentBrush"];
                    ControlHighlightChromeAltLowBrush = (SolidColorBrush)Application.Current.Resources["SystemControlHighlightChromeAltLowBrush"];
                    ControlForegroundChromeDisabledLowBrush = (SolidColorBrush)Application.Current.Resources["SystemControlForegroundChromeDisabledLowBrush"];
                }
            }

            //Update flyout text            
            if (_errorFlyoutTextBlock == null)
            {
                return;
            }
            StringBuilder sb = new StringBuilder();
            foreach (var error in _errorList)
            {
                if (_errorList.Last() == error)
                {
                    sb.Append($"● {error}");
                }
                else
                {
                    sb.AppendLine($"● {error}");
                }
            }
            _errorFlyoutTextBlock.Text = sb.ToString();
        }

        private void ValidatingTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsDirty)
            {
                IsDirty = true;
            }

            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                ValidateNewInput(textBox.Text);
            }
        }

        private void ErrorHint_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _errorFlyout.ShowAt(_errorFlyoutHost);
        }
    }

    /// <summary>
    /// A combination of a validation function, and an error message to display should validation fail.
    /// </summary>
    public class ValidationPair
    {
        /// <summary>
        /// A validation function that accepts a string, and should return "true" if the string is valid, and "false" otherwise.
        /// </summary>
        public Func<string, bool> ValidationFunction { get; set; }
        /// <summary>
        /// The error message to display if the string fails validation.
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
