using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;

namespace Controls.Standard.Formatting
{
    /// <summary>
    /// Set of two attached properties that can be used to add arbitrary formatting to a <see cref="TextBlock"/> based on 
    /// XML-formatted strings enclosed in top-level "format" tags. Users must set the <see cref="TextProperty"/>. 
    /// Additionally, the <see cref="TransformerProperty"/> can be set to a transformer Func that returns the corresponding
    /// <see cref="Span"/> tag for each <see cref="XElement"/> in the <see cref="TextProperty"/> XML, or <c>null</c> for 
    /// unrecognized values. The default value of <see cref="TransformerProperty"/> recognizes the <see cref="Bold"/>, 
    /// <see cref="Italic"/> and <see cref="Underline"/> tags (case-insensitive).
    /// 
    /// Inspired by http://stackoverflow.com/questions/5565885/how-to-bind-a-textblock-to-a-resource-containing-formatted-text
    /// </summary>
    public static class InlineFormatter
    {
        /// <summary>
        /// Defines the "Text" attached property.
        /// </summary>
        public static readonly DependencyProperty TextProperty =
             DependencyProperty.RegisterAttached("Text",
             typeof(string),
             typeof(InlineFormatter),
             new PropertyMetadata(null, TryParseCallback));

        /// <summary>
        /// Required setter for "Text" attached property.
        /// </summary>
        public static void SetText(DependencyObject d, string value)
        {
            d.SetValue(TextProperty, value);
        }

        /// <summary>
        /// Required getter for "Text" attached property.
        /// </summary>
        public static string GetText(DependencyObject d)
        {
            return (string)d.GetValue(TextProperty);
        }

        /// <summary>
        /// Default transform function.
        /// </summary>
        /// <param name="el">Format string XML element.</param>
        /// <returns>Corresponding <see cref="Span"/>, or <c>null</c> for unrecognized elements.</returns>
        public static Span DefaultTransform(XElement el)
        {
            switch (el.Name.LocalName.ToLowerInvariant())
            {
                case "bold":
                    return new Bold();
                case "italic":
                    return new Italic();
                case "underline":
                    return new Underline();
                default:
                    return null;
            }
        }

        /// <summary>
        /// The default value of <see cref="TranformerProperty"/>.
        /// </summary>
        public static Func<XElement, Span> DefaultTransformer { get { return (el) => DefaultTransform(el); } }

        /// <summary>
        /// Defines the "Transformer" attached property.
        /// </summary>
        public static readonly DependencyProperty TransformerProperty =
             DependencyProperty.RegisterAttached("Transformer",
             typeof(Func<XElement, Span>),
             typeof(InlineFormatter),
             new PropertyMetadata(DefaultTransformer, TryParseCallback));

        /// <summary>
        /// Required setter for "Transformer" attached property.
        /// </summary>
        public static void SetTransformer(DependencyObject d, Func<XElement, Span> value)
        {
            d.SetValue(TransformerProperty, value);
        }

        /// <summary>
        /// Required getter for "Transformer" attached property.
        /// </summary>
        public static Func<XElement, Span> GetTransformer(DependencyObject d)
        {
            return (Func<XElement, Span>)d.GetValue(TransformerProperty);
        }

        /// <summary>
        /// Check if both Text and Tranformer attached properties are defined, and perform
        /// the formatting if so, assigning the resulting span to <see cref="TextBlock.Inlines"/>.
        /// </summary>
        private static void TryParseCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var inlineText = d.GetValue(TextProperty) as string;
            var transformer = d.GetValue(TransformerProperty) as Func<XElement, Span>;

            if (inlineText != null && transformer != null && d is TextBlock)
            {
                var tb = d as TextBlock;
                tb.Inlines.Clear();
                tb.Inlines.Add(ParseInlineText(inlineText, transformer));
            }
        }

        /// <summary>
        /// Name of the top-level tag that all formatted strings must be enclosed in.
        /// </summary>
        private const string FORMAT_TAG_NAME = "format";

        /// <summary>
        /// Construct and populate the top-level <see cref="Span"/> from the formatted string using 
        /// the transformer to set the style properties from XML tags.
        /// 
        /// If there is an error, we return the original source text as-is (wrapped in a <see cref="Run"/>).
        /// </summary>
        private static Inline ParseInlineText(string source, Func<XElement, Span> transformer)
        {
            try
            {
                var elem = XElement.Parse(source, LoadOptions.PreserveWhitespace);
                if (elem.Name.LocalName.ToLowerInvariant() == FORMAT_TAG_NAME)
                {
                    return ParseRecursor(elem, transformer);
                }
            }
            catch { }

            // Show text as-is as fallback if parsing fails or other error
            var fallback = new Run();
            fallback.Text = source;
            return fallback;
        }

        /// <summary>
        /// A depth-first XML tree recursor that performs the actual tranformation.
        /// </summary>
        /// <param name="element">XML element in the format string tree.</param>
        /// <param name="transformer">Current transformer function.</param>
        /// <returns><see cref="Span"/> with desired formatting applied.</returns>
        private static Span ParseRecursor(XElement element, Func<XElement, Span> transformer)
        {
            var span = transformer(element) ?? new Span();
            foreach (var node in element.Nodes())
            {
                if (node is XText)
                {
                    var run = new Run() { Text = (node as XText).Value };
                    span.Inlines.Add(run);
                }
                else if (node is XElement)
                {
                    var childSpan = ParseRecursor(node as XElement, transformer);
                    span.Inlines.Add(childSpan);
                }
            }
            return span;
        }
    }
}