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
    public sealed partial class Statistical : Page, CalculatorView
    {
        private MainPage _mainPage;
        private List<string> _currentStatValues;

        public Statistical(MainPage mainPage)
        {
            this.InitializeComponent();

            this._mainPage = mainPage;

            _currentStatValues = new List<string>();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        #region Deviation / Variance

        private void Population_Variation_Button_Click(object sender, RoutedEventArgs e)
        {
            try { _mainPage.SetMainText(Variance(true).ToString()); }
            catch { _mainPage.initCalcError(); }

            // reset the equals key focus when we change view
            Helpers.equalsKeyFocus(_mainPage.currentView.GetEqualsKey(), this);
        }

        private void Population_Standard_Deviation_Button_Click(object sender, RoutedEventArgs e)
        {
            try { _mainPage.SetMainText(Math.Sqrt(Variance(true)).ToString()); }
            catch { _mainPage.initCalcError(); }

            // reset the equals key focus when we change view
            Helpers.equalsKeyFocus(_mainPage.currentView.GetEqualsKey(), this);
        }

        private double Variance(bool Population)
        {
            // first find the average
            double avg = Average_Values();

            // create the list of deviations
            double sumOfDeviations = 0;

            foreach (string data in _currentStatValues)
               sumOfDeviations += Math.Pow(double.Parse(data) - avg, 2);

            int subtract = 1;
            if (Population)
                subtract = 0;

            // last we divide the sum of the deviations by the number of values
            double variance = sumOfDeviations / (_currentStatValues.Count - subtract);

            return variance;
        }

        #endregion

        #region Averages

        private void Average_Button_Click(object sender, RoutedEventArgs e)
        {
            try { _mainPage.SetMainText(Average_Values().ToString()); }
            catch { _mainPage.initCalcError(); }

            // reset the equals key focus when we change view
            Helpers.equalsKeyFocus(_mainPage.currentView.GetEqualsKey(), this);
        }

        private double Average_Values()
        {
            double sum = Sum_Values();
            double avg = sum / _currentStatValues.Count;

            return avg;
        }

        private double Sum_Values()
        {
            double value = 0;
            foreach (string data in _currentStatValues)
                value += double.Parse(data);
            return value;
        }

        private void Average_Squared_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double sum = Sum_Values();
                double avg = sum / _currentStatValues.Count;

                avg = Math.Pow(avg, 2);

                _mainPage.SetMainText(avg.ToString());
            }
            catch { _mainPage.initCalcError(); }

            // reset the equals key focus when we change view
            Helpers.equalsKeyFocus(_mainPage.currentView.GetEqualsKey(), this);
        }

        #endregion

        #region Summation

        private void Summation_Button_Click(object sender, RoutedEventArgs e)
        {
            try { _mainPage.SetMainText(Sum_Values(false)); }
            catch { _mainPage.initCalcError(); }

            // reset the equals key focus when we change view
            Helpers.equalsKeyFocus(_mainPage.currentView.GetEqualsKey(), this);
        }

        private string Sum_Values(bool squareValues)
        {
            double value = 0;
            bool isDouble = false;
            foreach (string data in _currentStatValues)
            {
                if (Helpers.isDouble(data))
                    isDouble = true;

                if(squareValues)
                    value += Math.Pow(double.Parse(data), 2);
                else
                    value += double.Parse(data);
            }

            if (isDouble)
                return value.ToString();
            else
                return ((int)value).ToString();
        }

        private void Summation_Squared_Button_Click(object sender, RoutedEventArgs e)
        {
            try { _mainPage.SetMainText(Sum_Values(true)); }
            catch { _mainPage.initCalcError(); }

            // reset the equals key focus when we change view
            Helpers.equalsKeyFocus(_mainPage.currentView.GetEqualsKey(), this);
        }

        #endregion

        #region Interface

        public Button GetEqualsKey()
        {
            return this.EqualStatistics;
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

            ClearGrid();
            _mainPage.Stat_Init_Button_Click();
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

        #region Statistical Calc settings

        public void Stat_Add_Button_Click(object sender, RoutedEventArgs e)
        {
            string text = _mainPage.GetMainText();

            // get the text block
            TextBlock tb = new TextBlock();
            tb.Text = text;
            tb.FontSize = 60;

            Viewbox vb = new Viewbox();
            vb.Height = TemplateStatButtonHeight.ActualHeight;
            vb.Child = tb;

            AddToGrid(vb, text);

            _mainPage.Stat_Add_Button_Click(_currentStatValues.Count.ToString());

            // reset the equals key focus when we change view
            Helpers.equalsKeyFocus(_mainPage.currentView.GetEqualsKey(), this);
        }

        #endregion

        #region Grid

        private void AddToGrid(Viewbox vb, string text)
        {
            StatisticalGridView.Items.Add(vb);
            _currentStatValues.Add(text);
        }

        private void ClearGrid()
        {
            _currentStatValues.Clear();
            StatisticalGridView.Items.Clear();
        }

        #endregion
    }
}
