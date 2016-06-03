using System;
using System.Collections.Generic;
using UwpReusables.Controls.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Markup;

namespace UwpReusables.Controls.Standard.Validation
{
    /// <summary>
    /// A textbox that can validate user input, and display errors should the input fail validation.
    /// </summary>
    [ContentProperty(Name = "ValidationPairs")]
    public class ValidatingTextBox : ValidatingTextBoxBase
    {      
        public ValidatingTextBox()
        {
            this.DefaultStyleKey = typeof(ValidatingTextBoxBase);

            ValidationPairs = new List<ValidationPair>();

            this.TextChanged += ValidatingTextBox_TextChanged;
            this.IsEnabledChanged += ValidatingTextBox_IsEnabledChanged;
            this.PointerEntered += ValidatingTextBox_PointerEntered;
            this.PointerExited += ValidatingTextBox_PointerExited;
            this.GotFocus += ValidatingTextBox_GotFocus;
            this.LostFocus += ValidatingTextBox_LostFocus;
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

                       
        protected override List<string> ValidateInput(string text)
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
