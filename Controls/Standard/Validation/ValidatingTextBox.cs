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

namespace Controls.Standard.Validation
{
    /// <summary>
    /// A textbox that can validate user input, and display errors should the input fail validation.
    /// </summary>
    [ContentProperty(Name = "ValidationPairs")]    
    public class ValidatingTextBox : TextBox
    {
        private bool _isMousedOver = false;
        private bool _isFocused = false;
        private bool _isValid = false;
        private bool _errorFlyoutManuallyOpened = false;        

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

            ValidationPairs = new List<ValidationPair>();

            this.TextChanged += ValidatingTextBox_TextChanged;
            this.IsEnabledChanged += ValidatingTextBox_IsEnabledChanged;
            this.PointerEntered += ValidatingTextBox_PointerEntered;
            this.PointerExited += ValidatingTextBox_PointerExited;
            this.GotFocus += ValidatingTextBox_GotFocus;
            this.LostFocus += ValidatingTextBox_LostFocus;            
        }                        

        private void ValidatingTextBox_LostFocus(object sender, RoutedEventArgs e)
        {            
            _isFocused = false;
            //MouseOver states
            if (_isMousedOver && IsEnabled)
            {
                
                VisualStateManager.GoToState(this, _isValid || !IsDirty
                    ? "ValidatingPointerOver" 
                    : "PointerOverError", 
                    false);
            }
            //Enabled states
            else if (IsEnabled)
            {                
                VisualStateManager.GoToState(this, _isValid || !IsDirty
                    ? "ValidatingNormal" 
                    : "NormalError", 
                    false);
            }
            //Disabled states
            else
            {                
                VisualStateManager.GoToState(this, _isValid || !IsDirty
                    ? "ValidatingDisabled" 
                    : "DisabledError",
                    false);
            }

            _errorFlyout.Hide();
        }

        private void ValidatingTextBox_GotFocus(object sender, RoutedEventArgs e)
        {            
            _isFocused = true;
            if (IsEnabled)
            {                
                VisualStateManager.GoToState(this, _isValid || !IsDirty
                    ? "ValidatingFocused" 
                    : "FocusedError", 
                    false);

                if (!IsValid && IsDirty && _errorFlyout != null) 
                {
                    _errorFlyout.ShowAt(_errorFlyoutHost);
                    this.Focus(FocusState.Programmatic);
                }
            }
        }

        private void ValidatingTextBox_PointerExited(object sender, PointerRoutedEventArgs e)
        {            
            _isMousedOver = false;
            if (!_isFocused && IsEnabled)
            {                
                VisualStateManager.GoToState(this, _isValid || !IsDirty
                    ? "ValidatingNormal" 
                    : "NormalError", 
                    false);
            }
        }

        private void ValidatingTextBox_PointerEntered(object sender, PointerRoutedEventArgs e)
        {            
            _isMousedOver = true;
            if (!_isFocused && IsEnabled)
            {                
                VisualStateManager.GoToState(this, _isValid || !IsDirty
                    ? "ValidatingPointerOver" 
                    : "PointerOverError", 
                    false);
            }
        }

        private void ValidatingTextBox_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {            
            VisualStateManager.GoToState(this, _isValid || !IsDirty
                ? "ValidatingDisabled" 
                : "DisabledError",
                false);
        }

        protected override void OnApplyTemplate()
        {
            _errorFlyoutHost = GetTemplateChild("BorderElement") as Border;
            _errorFlyout = GetTemplateChild("ErrorFlyout") as Flyout;
            _errorFlyoutTextBlock = GetTemplateChild("ErrorFlyoutTextBlock") as TextBlock;
            ErrorHint = GetTemplateChild("ErrorHint") as Button;

            if (_errorFlyout != null)
            {
                _errorFlyout.Opened += _errorFlyout_Opened;                
            }

            base.OnApplyTemplate();
        }

        public static readonly DependencyProperty IsDirtyProperty = 
            DependencyProperty.Register("IsDirty", typeof(bool), typeof(ValidatingTextBox), new PropertyMetadata(false, OnIsDirtyChanged));

        private static void OnIsDirtyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            bool oldValue = (bool)dependencyPropertyChangedEventArgs.OldValue;
            bool newValue = (bool) dependencyPropertyChangedEventArgs.NewValue;
            if (oldValue == newValue)
            {
                return;
            }

            var vtb = dependencyObject as ValidatingTextBox;
            vtb?.ValidateNewInput(vtb.Text);
        }

        /// <summary>
        /// Whether or not the input box has been modified by user input.
        /// </summary>
        public bool IsDirty
        {
            get { return (bool)GetValue(IsDirtyProperty); }
            set { SetValue(IsDirtyProperty, value); }
        }

        public static readonly DependencyProperty IsValidProperty = 
            DependencyProperty.Register("IsValid", typeof(bool), typeof(ValidatingTextBox), new PropertyMetadata(false));
        /// <summary>
        /// Whether or not the textbox's current input is valid according to all <see cref="ValidationPair"/>s.
        /// </summary>
        public bool IsValid
        {
            get { return (bool)GetValue(IsValidProperty); }
            set { SetValue(IsValidProperty, value); }
        }

        public static readonly DependencyProperty ValidationPairsProperty =
            DependencyProperty.Register("ValidationPairs", typeof(IList<ValidationPair>), typeof(ValidatingTextBox), new PropertyMetadata(null));        

        /// <summary>
        /// A list of <see cref="ValidationPair"/>s the textbox validates input against.
        /// </summary>
        public IList<ValidationPair> ValidationPairs
        {
            get { return (IList<ValidationPair>)GetValue(ValidationPairsProperty); }
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

        public static readonly DependencyProperty ErrorHintGlyphProperty = DependencyProperty.Register(
            "ErrorHintGlyph", typeof (string), typeof (ValidatingTextBox), new PropertyMetadata(""));
        /// <summary>
        /// Gets or sets the glyph that appears in the far right side of the textbox
        /// when input is invalid. Uses Segoe MDL2 Assets symbol font.        
        /// </summary>
        public string ErrorHintGlyph
        {
            get { return (string) GetValue(ErrorHintGlyphProperty); }
            set { SetValue(ErrorHintGlyphProperty, value); }
        }


        private void ValidatingTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsDirty)
            {
                IsDirty = true;
            }

            ValidateNewInput(this.Text);
        }

        private void ValidateNewInput(string text)
        {
            //Don't do any validation if the input box hasn't been dirtied.
            if (!IsDirty)
            {                
                UpdateVisualStates(null);
                UpdateFlyoutState(null);
                return;
            }

            List<string> errorsList = ValidateInput(text);            
            UpdateVisualStates(errorsList);
            UpdateFlyoutState(errorsList);

            if (errorsList.Count > 0)
            {
                _isValid = false;
                IsValid = false;
            }
            else
            {
                _isValid = true;
                IsValid = true;
            }                                   
        }

        private void UpdateVisualStates(List<string> errorsList)
        {
            if (errorsList?.Count > 0)
            {
                if (ErrorHint != null)
                {
                    VisualStateManager.GoToState(this, "ErrorHintVisible", true);
                }
            }
            else
            {
                if (ErrorHint != null)
                {
                    VisualStateManager.GoToState(this, "ErrorHintCollapsed", true);
                }
            }
        }

        private void UpdateFlyoutState(List<string> errorsList)
        {
            if (_errorFlyoutTextBlock == null || _errorFlyout == null)
            {
                return;
            }
            StringBuilder sb = new StringBuilder();
            if (errorsList != null)
            {
                foreach (var error in errorsList)
                {
                    if (errorsList.Last() == error)
                    {
                        sb.Append($"● {error}");
                    }
                    else
                    {
                        sb.AppendLine($"● {error}");
                    }
                }
            }

            _errorFlyoutTextBlock.Text = sb.ToString();
            if (errorsList?.Count > 0)
            {
                _errorFlyout.ShowAt(_errorFlyoutHost);
            }
            if (String.IsNullOrWhiteSpace(sb.ToString()))
            {
                _errorFlyout.Hide();
            }           
        }

        private List<string> ValidateInput(string text)
        {
            List<string> errorsList = new List<string>();
            foreach (var validationPair in ValidationPairs)
            {
                if (!validationPair.ValidationFunction(text)
                    && !errorsList.Contains(validationPair.ErrorMessage))
                {
                    errorsList.Add(validationPair.ErrorMessage);                    
                }               
            }
            return errorsList;
        }

        private void ErrorHint_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (_errorFlyout != null)
            {
                _errorFlyoutManuallyOpened = true;
                _errorFlyout.ShowAt(_errorFlyoutHost);
            }
        }

        private void _errorFlyout_Opened(object sender, object e)
        {
            //Don't force-focus the textbox if the user opens the flyout by tapping the error hint
            if (!_errorFlyoutManuallyOpened)
            {
                this.Focus(FocusState.Programmatic);
            }
            _errorFlyoutManuallyOpened = false; //Reset
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
