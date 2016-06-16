using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace UwpReusables.Controls.Standard.Input
{
    [ContentProperty(Name = "Items")]
    public sealed class SearchableComboBox : ItemsControl
    {
        //Visual state names
        private const string NormalState = "Normal";
        private const string PointerOverState = "PointerOver";

        private const string OpenedState = "Opened";
        private const string ClosedState = "Closed";

        private ScrollViewer _popupScrollViewer;
        private Popup _popup;        

        public static readonly DependencyProperty PlaceholderTextProperty = DependencyProperty.Register(
            "PlaceholderText", typeof(string), typeof(SearchableComboBox), new PropertyMetadata(default(string)));

        public string PlaceholderText
        {
            get { return (string)GetValue(PlaceholderTextProperty); }
            set { SetValue(PlaceholderTextProperty, value); }
        }

        public static readonly DependencyProperty DropDownOffsetProperty = DependencyProperty.Register(
            "DropDownOffset", typeof(double), typeof(SearchableComboBox), new PropertyMetadata(0.0));

        public double DropDownOffset
        {
            get { return (double)GetValue(DropDownOffsetProperty); }
            set { SetValue(DropDownOffsetProperty, value); }
        }

        public static readonly DependencyProperty DropDownOpenedHeightProperty = DependencyProperty.Register(
            "DropDownOpenedHeight", typeof(double), typeof(SearchableComboBox), new PropertyMetadata(200.0));

        public double DropDownOpenedHeight
        {
            get { return (double)GetValue(DropDownOpenedHeightProperty); }
            set { SetValue(DropDownOpenedHeightProperty, value); }
        }

        public static readonly DependencyProperty DropDownContentMinWidthProperty = DependencyProperty.Register(
            "DropDownContentMinWidth", typeof(double), typeof(SearchableComboBox), new PropertyMetadata(250.0));

        public double DropDownContentMinWidth
        {
            get { return (double)GetValue(DropDownContentMinWidthProperty); }
            set { SetValue(DropDownContentMinWidthProperty, value); }
        }

        public SearchableComboBox()
        {
            this.DefaultStyleKey = typeof(SearchableComboBox);                        
            this.PointerEntered += SearchableComboBox_PointerEntered;
            this.PointerExited += SearchableComboBox_PointerExited;
            this.PointerPressed += SearchableComboBox_PointerPressed;                       
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _popup = GetTemplateChild("Popup") as Popup;
            if (_popup != null)
            {
                _popup.Closed += _popup_Closed;
            }                        
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            ComboBoxItem cbItem = new ComboBoxItem();
            cbItem.PointerEntered += (sender, args) => VisualStateManager.GoToState(cbItem, "PointerOver", true);
            cbItem.PointerExited += (sender, args) => VisualStateManager.GoToState(cbItem, "Normal", true);
            cbItem.PointerCanceled += (sender, args) => VisualStateManager.GoToState(cbItem, "Normal", true);
            cbItem.PointerPressed += (sender, args) => VisualStateManager.GoToState(cbItem, "Pressed", true);            
            return cbItem;
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return (item is ComboBoxItem);
        }

        private void _popup_Closed(object sender, object e)
        {
            VisualStateManager.GoToState(this, ClosedState, true);
        }

        private void SearchableComboBox_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, OpenedState, true);
            _popup.IsOpen = true;
        }

        private void SearchableComboBox_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, NormalState, true);
        }

        private void SearchableComboBox_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, PointerOverState, true);
        }
    }
}
