using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;

namespace UwpReusables.Controls.Standard.Validation
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
               
        private ToolTip _errorToolTip;

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

            CloseToolTip();
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

                if (!IsValid && IsDirty && _errorToolTip != null)
                {
                    OpenToolTip();
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
            _errorToolTip = GetTemplateChild("ErrorToolTip") as ToolTip;            
            ErrorHint = GetTemplateChild("ErrorHint") as Button;            

            base.OnApplyTemplate();
        }        

        public static readonly DependencyProperty IsDirtyProperty =
            DependencyProperty.Register("IsDirty", typeof(bool), typeof(ValidatingTextBox), new PropertyMetadata(false, OnIsDirtyChanged));

        private static void OnIsDirtyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            bool oldValue = (bool)dependencyPropertyChangedEventArgs.OldValue;
            bool newValue = (bool)dependencyPropertyChangedEventArgs.NewValue;
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
            set { SetValue(ValidationPairsProperty, value); }
        }

        public static readonly DependencyProperty ErrorHintColorProperty =
            DependencyProperty.Register("ErrorHintColor", typeof(Brush), typeof(ValidatingTextBox), new PropertyMetadata(new SolidColorBrush(Colors.Red)));
        /// <summary>
        /// Gets or sets a brush that defines the color of the error hint circle. Defaults to a Red SolidColorBrush.
        /// </summary>
        public Brush ErrorHintColor
        {
            get { return (Brush)GetValue(ErrorHintColorProperty); }
            set { SetValue(ErrorHintColorProperty, value); }
        }

        public static readonly DependencyProperty ErrorHintGlyphProperty = DependencyProperty.Register(
            "ErrorHintGlyph", typeof(string), typeof(ValidatingTextBox), new PropertyMetadata(""));
        /// <summary>
        /// Gets or sets the glyph that appears in the far right side of the textbox
        /// when input is invalid. Uses Segoe MDL2 Assets symbol font.        
        /// </summary>
        public string ErrorHintGlyph
        {
            get { return (string)GetValue(ErrorHintGlyphProperty); }
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
            if (_errorToolTip == null)
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

            _errorToolTip.Content = sb.ToString();
            if (errorsList?.Count > 0)
            {
                OpenToolTip();
            }
            if (String.IsNullOrWhiteSpace(sb.ToString()))
            {
                CloseToolTip();
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
            if(_errorToolTip?.IsOpen == true)
            {
                CloseToolTip();
            }
            else if(_errorToolTip?.IsOpen == false)
            {
                OpenToolTip();
            }
        }

        private void OpenToolTip()
        {
            if (_errorToolTip != null)
            {
                _errorToolTip.Visibility = Visibility.Visible;
                _errorToolTip.IsOpen = true;
            }
        }

        private void CloseToolTip()
        {
            if (_errorToolTip != null)
            {
                _errorToolTip.Visibility = Visibility.Collapsed;
                _errorToolTip.IsOpen = false;
            }
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
