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

namespace Controls.Validation
{
    [ContentProperty(Name = "ValidationPairs")]    
    public class ValidatingTextBox : TextBox
    {
        private bool _isMousedOver = false;
        private bool _isFocused = false;
        private bool _isValid = false;
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

        public bool IsDirty
        {
            get { return (bool)GetValue(IsDirtyProperty); }
            set { SetValue(IsDirtyProperty, value); }
        }

        public static readonly DependencyProperty IsValidProperty = 
            DependencyProperty.Register("IsValid", typeof(bool), typeof(ValidatingTextBox), new PropertyMetadata(false));
        /// <summary>
        /// Surface the current validity of the textbox.
        /// </summary>
        public bool IsValid
        {
            get { return (bool)GetValue(IsValidProperty); }
            set { SetValue(IsValidProperty, value); }
        }

        public static readonly DependencyProperty ValidationPairsProperty =
            DependencyProperty.Register("ValidationPairs", typeof(List<ValidationPair>), typeof(ValidatingTextBox), new PropertyMetadata(null));        
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
                _errorList.Clear();
                UpdateVisualStates();
                UpdateFlyoutText();
                return;
            }
            bool errorStateChanged = UpdateErrorsList(text);

            if (_errorList.Count > 0)
            {
                _isValid = false;
                IsValid = false;
            }
            else
            {
                _isValid = true;
                IsValid = true;
            }

            if (!errorStateChanged)
            {
                return;
            }            
            UpdateVisualStates();                      
            UpdateFlyoutText();
        }

        private void UpdateFlyoutText()
        {
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

        private void UpdateVisualStates()
        {
            if (_errorList.Count > 0)
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

        private bool UpdateErrorsList(string text)
        {
            foreach (var validationPair in ValidationPairs)
            {
                if (!validationPair.ValidationFunction(text)
                    && !_errorList.Contains(validationPair.ErrorMessage))
                {
                    _errorList.Add(validationPair.ErrorMessage);
                    return true;
                }
                else if (validationPair.ValidationFunction(text)
                         && _errorList.Contains(validationPair.ErrorMessage))
                {
                    _errorList.Remove(validationPair.ErrorMessage);
                    return true;
                }
            }
            return false;
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
