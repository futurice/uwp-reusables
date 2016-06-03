using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace UwpReusables.Controls.Core
{
    public abstract class ValidatingTextBoxBase : TextBox
    {
        protected bool _isMousedOver = false;
        protected bool _isFocused = false;
        protected bool _isValid = false;

        protected ToolTip _errorToolTip;

        private Button _errorHint;
        protected Button ErrorHint
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

        protected void ValidatingTextBox_LostFocus(object sender, RoutedEventArgs e)
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

        protected void ValidatingTextBox_GotFocus(object sender, RoutedEventArgs e)
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

        protected void ValidatingTextBox_PointerExited(object sender, PointerRoutedEventArgs e)
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

        protected void ValidatingTextBox_PointerEntered(object sender, PointerRoutedEventArgs e)
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

        protected void ValidatingTextBox_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
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
           DependencyProperty.Register("IsDirty", typeof(bool), typeof(ValidatingTextBoxBase), new PropertyMetadata(false, OnIsDirtyChanged));

        private static void OnIsDirtyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            bool oldValue = (bool)dependencyPropertyChangedEventArgs.OldValue;
            bool newValue = (bool)dependencyPropertyChangedEventArgs.NewValue;
            if (oldValue == newValue)
            {
                return;
            }

            var vtb = dependencyObject as ValidatingTextBoxBase;
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
            DependencyProperty.Register("IsValid", typeof(bool), typeof(ValidatingTextBoxBase), new PropertyMetadata(false));
        /// <summary>
        /// Whether or not the textbox's current input is valid according to all <see cref="ValidationPair"/>s.
        /// </summary>
        public bool IsValid
        {
            get { return (bool)GetValue(IsValidProperty); }
            set { SetValue(IsValidProperty, value); }
        }

        public static readonly DependencyProperty ErrorHintColorProperty =
          DependencyProperty.Register("ErrorHintColor", typeof(Brush), typeof(ValidatingTextBoxBase), new PropertyMetadata(new SolidColorBrush(Colors.Red)));
        /// <summary>
        /// Gets or sets a brush that defines the color of the error hint circle. Defaults to a Red SolidColorBrush.
        /// </summary>
        public Brush ErrorHintColor
        {
            get { return (Brush)GetValue(ErrorHintColorProperty); }
            set { SetValue(ErrorHintColorProperty, value); }
        }

        public static readonly DependencyProperty ErrorHintGlyphProperty = DependencyProperty.Register(
            "ErrorHintGlyph", typeof(string), typeof(ValidatingTextBoxBase), new PropertyMetadata(""));
        /// <summary>
        /// Gets or sets the glyph that appears in the far right side of the textbox
        /// when input is invalid. Uses Segoe MDL2 Assets symbol font.        
        /// </summary>
        public string ErrorHintGlyph
        {
            get { return (string)GetValue(ErrorHintGlyphProperty); }
            set { SetValue(ErrorHintGlyphProperty, value); }
        }

        protected void ValidatingTextBox_TextChanged(object sender, TextChangedEventArgs e)
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
                UpdateTooltipState(null);
                return;
            }

            List<string> errorsList = ValidateInput(text);
            UpdateVisualStates(errorsList);
            UpdateTooltipState(errorsList);

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

        protected abstract List<string> ValidateInput(string text);

        protected void UpdateVisualStates(List<string> errorsList)
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

        protected void UpdateTooltipState(List<string> errorsList)
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

        private void ErrorHint_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (_errorToolTip?.IsOpen == true)
            {
                CloseToolTip();
            }
            else if (_errorToolTip?.IsOpen == false)
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
}
