using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Calculator.CalcViews
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Scientific : Page, CalculatorView
    {
        private MainPage _mainPage;
        private bool _funcVisible;

        // Calc State
        public enum ScientificCalcState { Degrees, Radians }
        public ScientificCalcState scientificCalcState = ScientificCalcState.Degrees;

        public Scientific(MainPage mainPage)
        {
            this.InitializeComponent();

            _mainPage = mainPage;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _funcVisible = false;
        }

        #region Interface

        public Button GetEqualsKey()
        {
            return this.EqualScientific;
        }

        public void Add_Memory_Click(object sender, RoutedEventArgs e)
        {
            _mainPage.Add_Memory_Click(sender, e);
        }

        public void Clear_All_Memory_Click(object sender, RoutedEventArgs e)
        {
            _mainPage.Clear_All_Memory_Click(sender, e);
        }

        public void Metro_Memory_Add_Button_Click(object sender, RoutedEventArgs e)
        {
            _mainPage.Metro_Memory_Add_Button_Click(sender, e);
        }

        public void Metro_Memory_Restore_Button_Click(object sender, RoutedEventArgs e)
        {
            _mainPage.Metro_Memory_Restore_Button_Click(sender, e);
        }

        public void Memory_Add_Button_Click(object sender, RoutedEventArgs e)
        {
            _mainPage.Memory_Add_Button_Click(sender, e);
        }

        public void Memory_Subtract_Button_Click(object sender, RoutedEventArgs e)
        {
            _mainPage.Memory_Subtract_Button_Click(sender, e);
        }

        public void Memory_Restore_Button_Click(object sender, RoutedEventArgs e)
        {
            _mainPage.Memory_Restore_Button_Click(sender, e);
        }

        public void Memory_Clear_Button_Click(object sender, RoutedEventArgs e)
        {
            _mainPage.Memory_Clear_Button_Click(sender, e);
        }

        public void Dot_Button_Click(object sender, RoutedEventArgs e)
        {
            _mainPage.Dot_Button_Click(sender, e);
        }

        public void Symbol_Button_Click(object sender, RoutedEventArgs e)
        {
            _mainPage.Symbol_Button_Click(sender, e);
        }

        public void Value_Button_Click(object sender, RoutedEventArgs e)
        {
            _mainPage.Value_Button_Click(sender, e);
        }

        public void Rand_Button_Click(object sender, RoutedEventArgs e)
        {
            _mainPage.Rand_Button_Click(sender, e);
        }

        public void Negate_Button_Click(object sender, RoutedEventArgs e)
        {
            _mainPage.Negate_Button_Click(sender, e);
        }

        public void Paran_Button_Click(object sender, RoutedEventArgs e)
        {
            _mainPage.Paran_Button_Click(sender, e);
        }

        public void Operator_Single_Button_Click(object sender, RoutedEventArgs e)
        {
            _mainPage.Operator_Single_Button_Click(sender, e);
        }

        public void Operator_Double_Button_Click(object sender, RoutedEventArgs e)
        {
            _mainPage.Operator_Double_Button_Click(sender, e);
        }

        public void Delete_Last_Button_Click(object sender, RoutedEventArgs e)
        {
            _mainPage.Delete_Last_Button_Click(sender, e);
        }

        public void Clear_Calc_Button_Click(object sender, RoutedEventArgs e)
        {
            _mainPage.Clear_Calc_Button_Click(sender, e);
        }

        public void Clear_Error_Button_Click(object sender, RoutedEventArgs e)
        {
            _mainPage.Clear_Error_Button_Click(sender, e);
        }

        public void Equal_Button_Click(object sender, RoutedEventArgs e)
        {
            _mainPage.Equal_Button_Click(sender, e);
        }

        #endregion

        #region Function button

        public void Function_Button_Click(object sender, RoutedEventArgs e)
        {
            // standard buttons
            SinButton.Visibility = !_funcVisible ? Visibility.Collapsed : Visibility.Visible;
            CosButton.Visibility = !_funcVisible ? Visibility.Collapsed : Visibility.Visible;
            TanButton.Visibility = !_funcVisible ? Visibility.Collapsed : Visibility.Visible;
            NatLogButton.Visibility = !_funcVisible ? Visibility.Collapsed : Visibility.Visible;
            SinhButton.Visibility = !_funcVisible ? Visibility.Collapsed : Visibility.Visible;
            CoshButton.Visibility = !_funcVisible ? Visibility.Collapsed : Visibility.Visible;
            TanhButton.Visibility = !_funcVisible ? Visibility.Collapsed : Visibility.Visible;
            EXButton.Visibility = !_funcVisible ? Visibility.Collapsed : Visibility.Visible;

            // function buttons
            SinButtonF.Visibility = _funcVisible ? Visibility.Collapsed : Visibility.Visible;
            CosButtonF.Visibility = _funcVisible ? Visibility.Collapsed : Visibility.Visible;
            TanButtonF.Visibility = _funcVisible ? Visibility.Collapsed : Visibility.Visible;
            NatLogButtonF.Visibility = _funcVisible ? Visibility.Collapsed : Visibility.Visible;
            SinhButtonF.Visibility = _funcVisible ? Visibility.Collapsed : Visibility.Visible;
            CoshButtonF.Visibility = _funcVisible ? Visibility.Collapsed : Visibility.Visible;
            TanhButtonF.Visibility = _funcVisible ? Visibility.Collapsed : Visibility.Visible;
            EXButtonF.Visibility = _funcVisible ? Visibility.Collapsed : Visibility.Visible;

            _funcVisible = !_funcVisible;

            // reset the equals key focus when we change view
            Helpers.equalsKeyFocus(_mainPage.currentView.GetEqualsKey(), this);
        }

        #endregion
    }
}
