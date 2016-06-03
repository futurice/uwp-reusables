using System;
using System.Collections.Generic;
using UwpReusables.Controls.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Markup;

namespace UwpReusables.Controls.Portable.Validation
{
    /// <summary>
    /// A textbox that can validate user input, and display errors should the input fail validation.
    /// </summary>
    [ContentProperty(Name = "ValidationFunctions")]
    public class ValidatingTextBoxPortable : ValidatingTextBoxBase
    {      
        public ValidatingTextBoxPortable()
        {
            this.DefaultStyleKey = typeof(ValidatingTextBoxBase);

            ValidationFunctions = new List<Func<string, string>>();

            this.TextChanged += ValidatingTextBox_TextChanged;
            this.IsEnabledChanged += ValidatingTextBox_IsEnabledChanged;
            this.PointerEntered += ValidatingTextBox_PointerEntered;
            this.PointerExited += ValidatingTextBox_PointerExited;
            this.GotFocus += ValidatingTextBox_GotFocus;
            this.LostFocus += ValidatingTextBox_LostFocus;
        }               

        public static readonly DependencyProperty ValidationFunctionsProperty = DependencyProperty.Register(
            "ValidationFunctions", typeof(IList<Func<string, string>>), typeof(ValidatingTextBoxPortable), new PropertyMetadata(null));

        /// <summary>
        /// The collection of functions to validate user input against. On failure, each function should return an error message. On success, each function should return null.
        /// </summary>
        public IList<Func<string, string>> ValidationFunctions
        {
            get { return (IList<Func<string, string>>)GetValue(ValidationFunctionsProperty); }
            set { SetValue(ValidationFunctionsProperty, value); }
        }               

        protected override List<string> ValidateInput(string text)
        {
            List<string> errorsList = new List<string>();
            foreach (var validationFunc in ValidationFunctions)
            {
                string errorMessage = validationFunc(text);

                if (errorMessage != null && !errorsList.Contains(errorMessage))
                {
                    errorsList.Add(errorMessage);
                }
            }
            return errorsList;
        }       
    }
}
