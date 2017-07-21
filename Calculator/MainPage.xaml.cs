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
using System.Threading.Tasks;
using System.CodeDom.Compiler;
using Windows.UI.ViewManagement;
using System.Reflection;
using Windows.System;
using Windows.UI.Core;
using System.Numerics;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Search;
using System.Globalization;
using Windows.UI.ApplicationSettings;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Calculator
{
    public sealed partial class MainPage : Page
    {
        // Most recent drawing window
        public static TextWindow recentWindow = null;

        // So we can make using input keys work (from keyboard) we
        // need to maintain which "equals" key is the current one
        private Button _equalKey = null;

        // Drawing Windows
        private TextWindow _smallWindow = null;
        private TextWindow _mainWindow = null;

        // Paranthesis
        public static int _numParans = 0;

        // Prev operator
        private string _prevOperator = string.Empty;

        // Memory
        private string _memory;
        private TextWindow.InputType _memoryType;
        private List<MemoryItem> _memoryItems;
        private const int MAX_MEMORY_ITEMS = 10;

        // Evaluation
        private EvaluationEngine _eval = new EvaluationEngine();

        // State
        private const char DELIMETER = '#';

        // Search
        private SearchPane _searchPane = null;

        // Calc States
        private enum CalcSettings { Scientific, Programmer, Statistics, Snapped, Portrait }
        private CalcSettings calcSettings = CalcSettings.Scientific;

        // Current Page
        public CalculatorView currentView;

        public MainPage()
        {
            this.InitializeComponent();
        }

        #region Navigation

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // set function handlers for our evaltuator
            _eval.ProcessSymbol += ProcessSymbol;
            _eval.ProcessFunction += ProcessFunction;

            // Init drawing boxes
            _smallWindow = new TextWindow(OutputWindow, false, this);
            _mainWindow = new TextWindow(OutputWindowMain, true, this);

            // for character inputs
            CoreWindow.GetForCurrentThread().CharacterReceived += CharacterReceived;

            // set viewstate handler
            ApplicationView.GetForCurrentView().ViewStateChanged += new TypedEventHandler<ApplicationView, ApplicationViewStateChangedEventArgs>(MainPage_ViewStateChanged);

            // init calc view
            MainPage_ViewStateChanged(ApplicationView.Value);

            // init the calc to zeroz
            initCalc();

            // init the search function
            initSearch();

            // Init our list of currently active memory items
            _memoryItems = new List<MemoryItem>();

            // state handler for changing state
            LoadState();
            Windows.Storage.ApplicationData.Current.DataChanged += State_DataChanged;

            // if we had something that was copy pasted in
            try
            {
                string val = e.Parameter as string;
                if (val != string.Empty)
                    insertToWindow(val);
            }
            catch { }
        }

        #endregion

        #region View

        private void SetGridMargin(double value)
        {
            Thickness th = new Thickness(value, 0, value, value);
            CalcViewGrid.Margin = th;
        }

        private void SetPage(CalcSettings calcSettings)
        {
            // first remove the current page from the grid
            CalcViewGrid.Children.Remove(currentView as Page);

            double value = 20;

            if (calcSettings == CalcSettings.Programmer)
                currentView = new CalcViews.Programmer(this);
            else if (calcSettings == CalcSettings.Statistics)
                currentView = new CalcViews.Statistical(this);
            else if (calcSettings == CalcSettings.Portrait)
            {
                currentView = new CalcViews.Portrait(this);
                value = 5;
            }
            else if (calcSettings == CalcSettings.Snapped)
            {
                currentView = new CalcViews.Snapped(this);
                value = 5;
            }
            else
                currentView = new CalcViews.Scientific(this);

            // reset margin for new view
            SetGridMargin(value);

            // reset the current view
            _mainWindow.currentView = currentView;

            // re-add the page
            CalcViewGrid.Children.Add(currentView as Page);

            // setup equal key for focus
            _equalKey = currentView.GetEqualsKey();

            // reset the equals key focus when we change view
            Helpers.equalsKeyFocus(currentView.GetEqualsKey(), this);
        }

        private void MainPage_ViewStateChanged(ApplicationView sender, ApplicationViewStateChangedEventArgs args)
        {
            MainPage_ViewStateChanged(args.ViewState);
        }

        private void MainPage_ViewStateChanged(ApplicationViewState ViewState)
        {
            CloseAppBars();

            if (ViewState == ApplicationViewState.Snapped)
            {
                SetPage(CalcSettings.Snapped);

                // Appbar
                SettingsAppBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                MemoryAppBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                MemoryAppBar.IsEnabled = false;
                CalcState.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

                // for now clear the state on the calc when we flip it
                if (!isScientific())
                    CalcSettings_Click(CalcSettings.Scientific);
            }
            else if (ViewState == ApplicationViewState.FullScreenPortrait)
            {
                SetPage(CalcSettings.Portrait);

                // Appbar
                SettingsAppBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                MemoryAppBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                MemoryAppBar.IsEnabled = false;
                CalcState.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

                // for now clear the state on the calc when we flip it
                if (!isScientific())
                    CalcSettings_Click(CalcSettings.Scientific);
            }
            else
            {
                if (isScientific())
                {
                    SetPage(CalcSettings.Scientific);
                    ScientificSettings_Click(CalcViews.Scientific.ScientificCalcState.Degrees);
                }
                else if (isProgrammer())
                {
                    SetPage(CalcSettings.Programmer);
                    ProgrammerSettings_Click(CalcViews.Programmer.ProgrammerCalcState.Hexidecimal);
                }
                else if (isStatistics())
                {
                    SetPage(CalcSettings.Statistics);
                    Stat_Add_Button_Click("0");
                }

                // Appbar
                SettingsAppBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
                CalcState.Visibility = Windows.UI.Xaml.Visibility.Visible;
                MemoryAppBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
                MemoryAppBar.IsEnabled = true;
            }

            // reset the equals key focus when we change view
            Helpers.equalsKeyFocus(currentView.GetEqualsKey(), this);
        }

        #endregion

        #region AppBar

        private void CloseAppBars()
        {
            try
            {
                MemoryAppBar.IsOpen = false;
                SettingsAppBar.IsOpen = false;
            }
            catch { }
        }

        #endregion

        #region Add to Expression

        private void addContentToExpression(string data, TextWindow.InputType currentInputType)
        {
            bool exception = false;
            try
            {
                #region Numbers

                #region Scientific

                if (_mainWindow.isValue(currentInputType) && !isProgrammer())
                {
                    // Here the user entered in a value after entering in another value like 55 or 55.55
                    if ((currentInputType == TextWindow.InputType.Int && recentWindow.GetLastInputType() == TextWindow.InputType.Double) ||
                        (currentInputType == TextWindow.InputType.Double && recentWindow.GetLastInputType() == TextWindow.InputType.Int) ||
                        (currentInputType == TextWindow.InputType.Int && recentWindow.GetLastInputType() == TextWindow.InputType.Int) ||
                        (currentInputType == TextWindow.InputType.Double && recentWindow.GetLastInputType() == TextWindow.InputType.Double))
                    {
                        _mainWindow.Add(data, currentInputType);
                    }

                    // Here the user entered in a regular value that is to be on its own so we put that into the main window
                    else if (currentInputType == TextWindow.InputType.Int || currentInputType == TextWindow.InputType.Double)
                    {
                        // Here the user corrected what he did last so what we need to do is remove the last element from the _smallWindow list
                        if (_smallWindow.GetLastInputType() == TextWindow.InputType.OperatorSingle || _smallWindow.isValue())
                        {
                            _smallWindow.RemoveLast();
                        }

                        // make sure to touch MAIN last so it's most recent
                        _mainWindow.Set(data, currentInputType, false);
                    }

                    else if (currentInputType == TextWindow.InputType.Rand || currentInputType == TextWindow.InputType.Symbol)
                    {
                        // Here the user corrected what he did last so what we need to do is remove the last element from the _smallWindow list
                        if (_smallWindow.GetLastInputType() == TextWindow.InputType.OperatorSingle || _smallWindow.isValue())
                        {
                            _smallWindow.RemoveLast();
                        }

                        // make sure to touch MAIN last so it's most recent
                        _mainWindow.Set(data, currentInputType, true);
                    }
                }

                #endregion

                #region Programmer

                else if (_mainWindow.isValue(currentInputType) && isProgrammer())
                {
                    // Here the user entered in a number after another
                    if (recentWindow.GetLastInputType() == TextWindow.InputType.Int)
                    {
                        _mainWindow.Add((currentView as CalcViews.Programmer).toDec((currentView as CalcViews.Programmer).calcState, data), TextWindow.InputType.Int);
                    }

                    // Here the user entered in a regular value that is to be on its own so we put that into the main window
                    else
                    {
                        // Here the user corrected what he did last so what we need to do is remove the last element from the _smallWindow list
                        if (_smallWindow.GetLastInputType() == TextWindow.InputType.OperatorSingle || _smallWindow.isValue())
                        {
                            _smallWindow.RemoveLast();
                        }

                        // make sure to touch MAIN last so it's most recent
                        _mainWindow.Set(data, currentInputType, false);
                    }
                }

                #endregion

                #endregion

                #region Parans

                else if (currentInputType == TextWindow.InputType.Paran && data == "(")
                {
                    _smallWindow.Add(data, currentInputType);
                }

                // Here the user just closed the last paran in the expression so evaluate it
                else if (currentInputType == TextWindow.InputType.Paran && data == ")" && _numParans == 0 && recentWindow.isValue())
                {
                    // Hack 
                    _numParans++;

                    // if it was a valid value, use it in the expression
                    if (_mainWindow.isValue() && recentWindow == _mainWindow)
                        _smallWindow.Add(_mainWindow.Get(), _mainWindow.GetLastInputType());

                    _smallWindow.Add(data, currentInputType);

                    // Hack
                    _numParans = 0;

                    Equal_Button_Click(null, null);
                }

                #endregion

                #region Operators

                // only go in here if there really was two operators after eachother, we check this via the _mainwindow usedvalue var which tells me if 
                // if we've already used whats in the mainwindow buffer
                else if (currentInputType == TextWindow.InputType.OperatorDouble && currentInputType == recentWindow.GetLastInputType() && recentWindow == _smallWindow)
                {
                    // if we could remove the last element ok proceed
                    if (_smallWindow.RemoveLast())
                        _smallWindow.Add(data, currentInputType);
                    else
                        Clear_Calc_Button_Click(null, null);
                }

                // here the user entered in a "weaker" operator via PEMDAS and there are no parans in the expression
                // also validate the user didn't just enter in * then /
                else if (currentInputType == TextWindow.InputType.OperatorDouble && _prevOperator != string.Empty && (Helpers.compareOPs(data, _prevOperator) >= 0) && _numParans == 0)
                {
                    // if it was a valid value, use it in the expression
                    if (_mainWindow.isValue() && recentWindow == _mainWindow)
                        _smallWindow.Add(_mainWindow.Get(), _mainWindow.GetLastInputType());

                    Equal_Button_Click(null, null);

                    _smallWindow.Add(data, currentInputType);
                }

                // just add the opeartor
                else if (currentInputType == TextWindow.InputType.OperatorDouble)
                {
                    // if it was a valid value, use it in the expression
                    if (_mainWindow.isValue() && recentWindow == _mainWindow)
                        _smallWindow.Add(_mainWindow.Get(), _mainWindow.GetLastInputType());

                    _smallWindow.Add(data, currentInputType);
                }

                // user entered in a single operator like n! or log
                else if (currentInputType == TextWindow.InputType.OperatorSingle)
                {
                    // if it was a valid value, use it in the expression
                    if (_mainWindow.isValue() && recentWindow == _mainWindow)
                        _smallWindow.Add(_mainWindow.Get(), _mainWindow.GetLastInputType());

                    _smallWindow.Add(data, currentInputType);

                    string inputData = _smallWindow.GetLast().inputData;
                    string tempExpression = _eval.Execute(inputData).ToString();

                    // here we set the input type to Null so that the user can't just continue to enter in additional values
                    _mainWindow.Set(tempExpression, TextWindow.InputType.NULL, false);
                    _smallWindow.MakeRecent();
                }

                #endregion

                #region Finishing Functions

                // incase the user types in zeros in the front of the expression
                Helpers.cleanUpZeros(_mainWindow);

                // finally give focus back to the equals key
                Helpers.equalsKeyFocus(currentView.GetEqualsKey(), this);

                #endregion
            }
            catch { exception = true; }
            if (exception)
            {
                initCalcError();
            }
        }

        #endregion

        #region Insert To Window

        private void insertToWindow(string data)
        {
            long valueL = 0;
            double valueD = 0;
            bool exception = false;

            if (long.TryParse(data, out valueL))
                _mainWindow.Set(data, TextWindow.InputType.Int, false);
            else if (double.TryParse(data, out valueD))
                _mainWindow.Set(data, TextWindow.InputType.Double, false);
            else
            {
                try
                {
                    string tempExpression = _eval.Execute(data).ToString();

                    // here we set the input type to Null so that the user can't just continue to enter in additional values
                    _smallWindow.Set(data, TextWindow.InputType.NULL, true);
                    _mainWindow.Set(tempExpression, tempExpression.Contains(".") ? TextWindow.InputType.Double : TextWindow.InputType.Int, false);
                }
                catch { exception = true; }
                if (exception)
                {
                    initCalcError();
                }
            }

            _mainWindow.MakeRecent();
        }

        #endregion

        #region Memory

        #region Metro Memory Buttons (FullView & Landscape)

        public void Remove_Memory_Click(MemoryItem memItem)
        {
            try
            {
                // remove this specific item
                MemoryGrid.Items.Remove(memItem);
                _memoryItems.Remove(memItem);
                RemoveFromState(memItem);
            }

            catch { }

            // reset the equals key focus when we change view
            Helpers.equalsKeyFocus(currentView.GetEqualsKey(), this);
        }

        public void Use_Memory_Click(MemoryItem memItem)
        {
            try
            {
                // if we're not in scientific or stat mode you can't add doubles
                if (isProgrammer() && Helpers.isDouble(memItem.Value))
                    return;

                // first clear whats already in the viewing mode (including small
                // window too)
                Clear_Calc_Button_Click(null, null);

                // if the mainwindow is in programmer mode setup the memory so 
                // it comes back to the right state
                _mainWindow.Set(memItem.Value, TextWindow.InputType.Int, true);

                // close appbars
                CloseAppBars();
            }
            catch { initCalcError(); }

            // reset the equals key focus when we change view
            Helpers.equalsKeyFocus(currentView.GetEqualsKey(), this);

        }

        public void Add_Memory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string mainWindowDecText = _mainWindow.Get();
                // first check this exact value doesn't already exist

                foreach (MemoryItem item in _memoryItems)
                    if (item.Text.CompareTo(mainWindowDecText) == 0)
                        return;

                // create a new item
                MemoryItem memItem = new MemoryItem();
                memItem.InitializeWindow(this);
                memItem.Value = mainWindowDecText;
                memItem.Text = _mainWindow.GetWindowText();

                // easy if we're in scientific or statistical
                if (!isProgrammer())
                {
                    memItem.isProgrammer = false;
                    memItem.Type = CalcViews.Programmer.ProgrammerCalcState.Decimal;
                }

                // otherwise set additional parameters
                else
                {
                    memItem.isProgrammer = true;
                    memItem.Type = (currentView as CalcViews.Programmer).calcState;
                }

                Add_Memory_Click(memItem,false);
                AddToState(memItem);
            }
            catch { initCalcError(); }

            // reset the equals key focus when we change view
            Helpers.equalsKeyFocus(currentView.GetEqualsKey(), this);
        }

        public void Add_Memory_Click(MemoryItem memItem, bool addToFront)
        {
            // if we have too much stuff in memory start killing off
            if (_memoryItems.Count > MAX_MEMORY_ITEMS)
            {
                MemoryItem toRemove = _memoryItems.First();

                _memoryItems.Remove(toRemove);
                MemoryGrid.Items.Remove(toRemove);
            }

            // insert item
            if (addToFront)
                MemoryGrid.Items.Insert(0,memItem);
            else
                MemoryGrid.Items.Add(memItem);
            _memoryItems.Add(memItem);

            // make sure we make the last item the most recent
            MemoryGrid.UpdateLayout();
            MemoryGrid.ScrollIntoView(memItem);

            _mainWindow.clearOnNextUse = true;
        }

        public void Clear_All_Memory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // remove all items from memory
                foreach (MemoryItem mi in _memoryItems)
                {
                    if (MemoryGrid.Items.Contains(mi))
                        MemoryGrid.Items.Remove(mi);
                }

                _memoryItems.Clear();

                ClearState();
            }
            catch { initCalcError(); }

            // reset the equals key focus when we change view
            Helpers.equalsKeyFocus(currentView.GetEqualsKey(), this);
        }

        public async void Metro_Memory_Add_Button_Click(object sender, RoutedEventArgs e)
        {
            // open and close the appbar as feedback
            MemoryAppBar.IsOpen = true;

            Add_Memory_Click(null, null);

            await Task.Delay(2000);
            MemoryAppBar.IsOpen = false;

            // reset the equals key focus when we change view
            Helpers.equalsKeyFocus(currentView.GetEqualsKey(), this);
        }

        public void Metro_Memory_Restore_Button_Click(object sender, RoutedEventArgs e)
        {
            Use_Memory_Click(_memoryItems.Last());
        }

        #endregion

        #region Legacy Memory Buttons (Snapped & Portrait)

        public void Memory_Add_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _memory = (double.Parse(_memory) + double.Parse(_mainWindow.Get())).ToString();

                // make sure if it was a double we keep it that way
                _memoryType = _mainWindow.GetLastInputType() == TextWindow.InputType.Double ||
                              _memoryType == TextWindow.InputType.Double ||
                              _memory.Contains(".") ? TextWindow.InputType.Double : TextWindow.InputType.Int;

                MemoryWindow.Text = "M";

                _mainWindow.clearOnNextUse = true;
            }
            catch { initCalcError(); }

            // reset the equals key focus when we change view
            Helpers.equalsKeyFocus(currentView.GetEqualsKey(), this);
        }

        public void Memory_Subtract_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _memory = (double.Parse(_memory) - double.Parse(_mainWindow.Get())).ToString();

                // make sure if it was a double we keep it that way
                _memoryType = _mainWindow.GetLastInputType() == TextWindow.InputType.Double ||
                              _memoryType == TextWindow.InputType.Double ||
                              _memory.Contains(".") ? TextWindow.InputType.Double : TextWindow.InputType.Int;

                MemoryWindow.Text = "M";

                _mainWindow.clearOnNextUse = true;
            }
            catch { initCalcError(); }

            // reset the equals key focus when we change view
            Helpers.equalsKeyFocus(currentView.GetEqualsKey(), this);
        }

        public void Memory_Restore_Button_Click(object sender, RoutedEventArgs e)
        {
            Clear_Calc_Button_Click(null, null);

            _mainWindow.Set(_memory, _memoryType, true);

            // reset the equals key focus when we change view
            Helpers.equalsKeyFocus(currentView.GetEqualsKey(), this);
        }

        public void Memory_Clear_Button_Click(object sender, RoutedEventArgs e)
        {
            _memory = "0";
            _memoryType = TextWindow.InputType.Int;
            MemoryWindow.Text = string.Empty;

            // reset the equals key focus when we change view
            Helpers.equalsKeyFocus(currentView.GetEqualsKey(), this);
        }

        #endregion

        #endregion

        #region State

        private void AddToState(MemoryItem memItem)
        {
            try
            {
                Windows.Storage.ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;

                int type = (int)memItem.Type;

                roamingSettings.Values.Add(memItem.Text, memItem.Value.ToString() + DELIMETER + type);
            }
            catch { }
        }

        private void RemoveFromState(MemoryItem memItem)
        {
            try
            {
                Windows.Storage.ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;

                roamingSettings.Values.Remove(memItem.Text);
            }
            catch { }
        }

        private void ClearState()
        {
            try
            {
                Windows.Storage.ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;

                roamingSettings.Values.Clear();
            }
            catch { }
        }

        private void LoadState()
        {
            try
            {
                Windows.Storage.ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
                foreach (string key in roamingSettings.Values.Keys)
                {
                    // create a new item
                    MemoryItem mi = new MemoryItem();
                    mi.InitializeWindow(this);
                    mi.Text = key;

                    // the first part is the value
                    mi.Value = (roamingSettings.Values[key] as string).Split(DELIMETER)[0];
                    mi.Type = (CalcViews.Programmer.ProgrammerCalcState)int.Parse((roamingSettings.Values[key] as string).Split(DELIMETER)[1]);

                    Add_Memory_Click(mi, false);
                }
            }
            catch { }
        }

        private void State_DataChanged(Windows.Storage.ApplicationData sender, object args)
        {
            //throw new NotImplementedException();
        }

        #endregion

        #region Key Input

        private async void CharacterReceived(CoreWindow sender, CharacterReceivedEventArgs args)
        {
            uint keyCode = args.KeyCode;

            int tempParse = -1;

            // if the value was 0-9
            if (int.TryParse(((char)keyCode).ToString(), out tempParse) && tempParse >= 0 && tempParse <= 9)
            {
                args.Handled = true;
                Value_Button_Click(((char)keyCode).ToString());
            }

            // if the value was a-f and we're in programmer hex mode
            else if (isProgrammer() && (currentView as CalcViews.Programmer).calcState == CalcViews.Programmer.ProgrammerCalcState.Hexidecimal && (char)keyCode >= 'a' && (char)keyCode <= 'f')
            {
                args.Handled = true;
                Value_Button_Click(((char)keyCode).ToString());
            }

            // if the value was + - * /
            else if (keyCode == '+' || keyCode == '-' || keyCode == '*' || keyCode == '/')
            {
                args.Handled = true;
                Operator_Double_Button_Click(((char)keyCode).ToString());
            }

            // if the value was ( )
            else if (keyCode == ')' || keyCode == '(')
            {
                args.Handled = true;
                Operator_Double_Button_Click(((char)keyCode).ToString());
            }

            // if the value was .
            else if (keyCode == '.')
            {
                args.Handled = true;
                Dot_Button_Click(".");
            }

            // if the value was .
            else if (keyCode == '\b')
            {
                args.Handled = true;
                Delete_Last_Button_Click(null, null);
            }

            // if the value was enter
            else if (keyCode == '\r')
            {
                args.Handled = true;
                Equal_Button_Click(new object(), null);
            }

            // ctrl-c
            // if the text is '' yes there is actually stuff in there it's not empty
            // its the character 3
            else if (keyCode == '')
            {
                args.Handled = true;

                DataPackage dp = new DataPackage();
                dp.SetText(_mainWindow.Get());

                Clipboard.SetContent(dp);
            }

            // ctrl-v
            // if the text is '' yes there is actually stuff in there it's not empty
            else if (keyCode == '')
            {
                args.Handled = true;

                DataPackageView dpv = Clipboard.GetContent();

                Clear_Calc_Button_Click(null, null);

                string newText = (await dpv.GetTextAsync()).ToLower();

                insertToWindow(newText);
            }

            // esc
            // if the text is '' Yes there is actually text in there
            else if (keyCode == '')
            {
                args.Handled = true;
                Clear_Calc_Button_Click(null, null);
            }

            if (args.Handled)
            {
                CloseAppBars();

                // reset the equals key focus when we change view
                Helpers.equalsKeyFocus(currentView.GetEqualsKey(), this);
            }
        }

        #endregion

        #region Values/Symbols/Dot

        public void Dot_Button_Click(object sender, RoutedEventArgs e)
        {
            string data = Helpers.GetTextFromObject(sender);

            Dot_Button_Click(data);
        }

        private void Dot_Button_Click(string data)
        {
            // can't enter more than one dot if we're alredy a double
            if (_mainWindow.isDouble())
                return;

            addContentToExpression(data, TextWindow.InputType.Double);
        }

        public void Symbol_Button_Click(object sender, RoutedEventArgs e)
        {
            string data = Helpers.GetTextFromObject(sender);

            if (data == "π")
                data = Math.PI.ToString();
            else if (data == "e")
                data = Math.E.ToString();

            addContentToExpression(data, TextWindow.InputType.Symbol);
        }

        // add a value to the expression string
        public void Value_Button_Click(object sender, RoutedEventArgs e)
        {
            string data = Helpers.GetTextFromObject(sender);

            Value_Button_Click(data);
        }

        private void Value_Button_Click(string data)
        {
            addContentToExpression(data, TextWindow.InputType.Int);
        }

        // adding a new random value to the mix
        public void Rand_Button_Click(object sender, RoutedEventArgs e)
        {
            Random r = new Random();
            double d = r.NextDouble();

            addContentToExpression(d.ToString(), TextWindow.InputType.Rand);
        }

        #endregion

        #region Calc Settings

        public bool isScientific()
        {
            return this.calcSettings == CalcSettings.Scientific;
        }

        public bool isProgrammer()
        {
            return this.calcSettings == CalcSettings.Programmer;
        }

        public bool isStatistics()
        {
            return this.calcSettings == CalcSettings.Statistics;
        }

        #region Set Settings

        public void SciSettings_Click(object sender, RoutedEventArgs e)
        {
            CalcSettings_Click(CalcSettings.Scientific);
        }
        public void ProgSettings_Click(object sender, RoutedEventArgs e)
        {
            CalcSettings_Click(CalcSettings.Programmer);
        }
        public void StatSettings_Click(object sender, RoutedEventArgs e)
        {
            CalcSettings_Click(CalcSettings.Statistics);
        }
        private void CalcSettings_Click(CalcSettings calcMode)
        {
            initCalc();

            _mainWindow.isProgrammerDec = false;

            if (calcMode == CalcSettings.Scientific)
            {
                this.calcSettings = CalcSettings.Scientific;

                // Appbar buttons
                ProgrammerButton.IsChecked = false;
                ScientificButton.IsChecked = true;
                StatisticsButton.IsChecked = false;

                // Appbar settings
                ScientificSettings.Visibility = Windows.UI.Xaml.Visibility.Visible;
                ProgrammerSettings.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            else if (calcMode == CalcSettings.Programmer)
            {
                this.calcSettings = CalcSettings.Programmer;

                // Appbar buttons
                ProgrammerButton.IsChecked = true;
                ScientificButton.IsChecked = false;
                StatisticsButton.IsChecked = false;

                // Appbar settings
                ScientificSettings.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                ProgrammerSettings.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }

            else if (calcMode == CalcSettings.Statistics)
            {
                this.calcSettings = CalcSettings.Statistics;

                // Appbar buttons
                ProgrammerButton.IsChecked = false;
                ScientificButton.IsChecked = false;
                StatisticsButton.IsChecked = true;

                // Appbar settings
                ScientificSettings.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                ProgrammerSettings.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }

            MainPage_ViewStateChanged(ApplicationView.Value);
        }

        #endregion

        #region Programmer Calc Settings

        public void HexSettings_Click(object sender, RoutedEventArgs e)
        {
            ProgrammerSettings_Click(CalcViews.Programmer.ProgrammerCalcState.Hexidecimal);
        }
        public void DecSettings_Click(object sender, RoutedEventArgs e)
        {
            ProgrammerSettings_Click(CalcViews.Programmer.ProgrammerCalcState.Decimal);
        }
        public void OctSettings_Click(object sender, RoutedEventArgs e)
        {
            ProgrammerSettings_Click(CalcViews.Programmer.ProgrammerCalcState.Octal);
        }
        public void BinSettings_Click(object sender, RoutedEventArgs e)
        {
            ProgrammerSettings_Click(CalcViews.Programmer.ProgrammerCalcState.Binary);
        }

        public void ProgrammerSettings_Click(CalcViews.Programmer.ProgrammerCalcState mode)
        {
            if (mode == CalcViews.Programmer.ProgrammerCalcState.Hexidecimal)
            {
                // Appbar
                HexButton.IsChecked = true;
                DecButton.IsChecked = false;
                OctButton.IsChecked = false;
                BinButton.IsChecked = false;

                // View
                CalcState.Text = "Hex";
            }
            else if (mode == CalcViews.Programmer.ProgrammerCalcState.Decimal)
            {
                _mainWindow.isProgrammerDec = true;

                // Appbar
                HexButton.IsChecked = false;
                DecButton.IsChecked = true;
                OctButton.IsChecked = false;
                BinButton.IsChecked = false;

                // View
                CalcState.Text = "Dec";
            }
            else if (mode == CalcViews.Programmer.ProgrammerCalcState.Octal)
            {
                // Appbar
                HexButton.IsChecked = false;
                DecButton.IsChecked = false;
                OctButton.IsChecked = true;
                BinButton.IsChecked = false;

                // View
                CalcState.Text = "Oct";
            }
            else if (mode == CalcViews.Programmer.ProgrammerCalcState.Binary)
            {
                // Appbar
                HexButton.IsChecked = false;
                DecButton.IsChecked = false;
                OctButton.IsChecked = false;
                BinButton.IsChecked = true;

                // View
                CalcState.Text = "Bin";
            }

            // set the state
            (currentView as CalcViews.Programmer).calcState = mode;

            // get the view to redraw buttons
            (currentView as CalcViews.Programmer).ProgrammerSettings_Click(mode);

            _mainWindow.clearOnNextUse = true;

            // redraw whatever is on the mainWindow
            _mainWindow.Redraw();
        }


        #endregion

        #region Scientific Calc Settings

        public void DegSettings_Click(object sender, RoutedEventArgs e)
        {
            ScientificSettings_Click(CalcViews.Scientific.ScientificCalcState.Degrees);
        }

        public void RadSettings_Click(object sender, RoutedEventArgs e)
        {
            ScientificSettings_Click(CalcViews.Scientific.ScientificCalcState.Radians);
        }

        private void ScientificSettings_Click(CalcViews.Scientific.ScientificCalcState mode)
        {
            if (mode == CalcViews.Scientific.ScientificCalcState.Degrees)
            {
                (currentView as CalcViews.Scientific).scientificCalcState = CalcViews.Scientific.ScientificCalcState.Degrees;
                RadiansButton.IsChecked = false;
                DegreesButton.IsChecked = true;

                CalcState.Text = "Deg";
            }
            else if (mode == CalcViews.Scientific.ScientificCalcState.Radians)
            {
                (currentView as CalcViews.Scientific).scientificCalcState = CalcViews.Scientific.ScientificCalcState.Radians;
                RadiansButton.IsChecked = true;
                DegreesButton.IsChecked = false;

                CalcState.Text = "Rad";
            }
        }

        #endregion

        #region Statistics Calc Settings

        public void Stat_Add_Button_Click(string data)
        {
            // set the main window to clear
            _mainWindow.clearOnNextUse = true;
            CalcState.Text = "Cnt = " + data;
        }

        public void Stat_Init_Button_Click()
        {
            Stat_Add_Button_Click("0");
        }

        public string GetMainText()
        {
            return _mainWindow.Get();
        }

        public void SetMainText(string data)
        {
            initCalc();

            if (Helpers.isDouble(data))
                _mainWindow.Set(data, TextWindow.InputType.Double, true);
            else
                _mainWindow.Set(data, TextWindow.InputType.Int, true);
        }

        #endregion

        #region Programmer Calc Bitness

        public void Change_Bitness_Click(CalcViews.Programmer.ProgrammerCalcBits bitness)
        {
            // we shouldn't be here if we're not in programmer mode
            if (!isProgrammer())
            {
                initCalcError();
                return;
            }

            CalcViews.Programmer.ProgrammerCalcBits prevBits = (currentView as CalcViews.Programmer).currentBits;

            (currentView as CalcViews.Programmer).currentBits = bitness;

            _mainWindow.ChangeBitness(prevBits);

            // redraw whatever is on the mainWindow
            _mainWindow.Redraw();
        }

        #endregion

        #endregion

        #region Negation

        public void Negate_Button_Click(object sender, RoutedEventArgs e)
        {
            try { recentWindow.Negate(); }
            catch { initCalcError(); }

            // reset the equals key focus when we change view
            Helpers.equalsKeyFocus(currentView.GetEqualsKey(), this);
        }

        #endregion

        #region Paran

        // add a value to the expression string
        public void Paran_Button_Click(object sender, RoutedEventArgs e)
        {
            string data = Helpers.GetTextFromObject(sender);

            Paran_Button_Click(data);

        }

        private void Paran_Button_Click(string data)
        {
            // open paran
            if (data == "(")
            {
                _numParans++;
            }

            // close paran (and there are more than 1 open paran)
            else if (data == ")" && _numParans > 0)
            {
                // if the user entered an operator they shouldn't be able to close the paran
                if (recentWindow.GetLastInputType() == TextWindow.InputType.OperatorDouble && recentWindow == _smallWindow)
                    return;

                // if the user entered in a double like 55. they shouldn't be able to close the paran
                else if (_mainWindow.GetLastInputType() == TextWindow.InputType.Double && _mainWindow.GetLast().inputData.EndsWith(".") && recentWindow == _mainWindow)
                    return;

                _numParans--;
            }

            // otherwise they tried to close a paran without opening one
            else
                return;

            addContentToExpression(data, TextWindow.InputType.Paran);
        }

        #endregion

        #region Operators

        // add a operator to the expression string
        public void Operator_Single_Button_Click(object sender, RoutedEventArgs e)
        {
            string data = Helpers.GetTextFromObject(sender);

            data = determineRealOp(data);

            if (isProgrammer() && data == "RGB")
            {
                try
                {
                    string hexCode = (currentView as CalcViews.Programmer).dec2Hex(long.Parse(_mainWindow.Get()).ToString());

                    // should always be 6 characters long
                    if (hexCode.Length != 6)
                        throw new Exception();

                    string R = hexCode.Substring(0, 2);
                    string G = hexCode.Substring(2, 2);
                    string B = hexCode.Substring(4, 2);

                    _mainWindow.SetNoTranslation((currentView as CalcViews.Programmer).hex2Dec(R) + "-" + (currentView as CalcViews.Programmer).hex2Dec(G) + "-" + (currentView as CalcViews.Programmer).hex2Dec(B));

                }
                catch { initCalcError(); }
            }
            else
                addContentToExpression(data, TextWindow.InputType.OperatorSingle);
        }

        private string determineRealOp(string input)
        {
            string data = input;

            // Single operator
            if (data == "1/x")
                data = "inv";
            else if (data == "x²")
                data = "powtwo";
            else if (data == "x³")
                data = "powthree";
            else if (data == "%")
                data = "percent";
            else if (data == "eˣ")
                data = "epow";
            else if (data == "sinˉ¹")
                data = "asin";
            else if (data == "cosˉ¹")
                data = "acos";
            else if (data == "tanˉ¹")
                data = "atan";
            else if (data == "sinhˉ¹")
                data = "asinh";
            else if (data == "coshˉ¹")
                data = "acosh";
            else if (data == "tanhˉ¹")
                data = "atanh";
            else if (data == "log₂")
                data = "logtwo";
            else if (data == "2ˣ")
                data = "twopow";
            else if (data == "z→Q")
                data = "Q";
            else if (data == "Q→z")
                data = "Z";

            // Double Operator
            else if (data == "ₙPₖ")
                data = "P";
            else if (data == "ₙCₖ")
                data = "C";
            else if (data == "yˣ")
                data = "^";
            else if (data == "And")
                data = "&";
            else if (data == "Or")
                data = "|";
            else if (data == "Xor")
                data = "⊕";
            else if (data == "Lsh")
                data = "<";
            else if (data == "Rsh")
                data = ">";
            else if (data == "Mod")
                data = "%";

            return data;
        }

        // add a operator to the expression string
        public void Operator_Double_Button_Click(object sender, RoutedEventArgs e)
        {
            string data = Helpers.GetTextFromObject(sender);

            data = determineRealOp(data);

            Operator_Double_Button_Click(data);
        }

        private void Operator_Double_Button_Click(string data)
        {
            addContentToExpression(data, TextWindow.InputType.OperatorDouble);

            _prevOperator = data;
        }

        #endregion

        #region Delete + Clear

        public void Delete_Last_Button_Click(object sender, RoutedEventArgs e)
        {
            // if the mainwindow was reset since the user hit back alot, clear the entire calc
            if (recentWindow == _mainWindow && !_mainWindow.RemoveLast())
                Clear_Calc_Button_Click(null, null);

            // reset the equals key focus when we change view
            Helpers.equalsKeyFocus(currentView.GetEqualsKey(), this);
        }

        public void Clear_Calc_Button_Click(object sender, RoutedEventArgs e)
        {
            _smallWindow.Set(string.Empty, TextWindow.InputType.NULL, true);
            _mainWindow.Set("0", TextWindow.InputType.Int, false);
            _numParans = 0;
            _prevOperator = string.Empty;

            // reset the equals key focus when we change view
            Helpers.equalsKeyFocus(currentView.GetEqualsKey(), this);
        }

        public void Clear_Error_Button_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.Set("0", TextWindow.InputType.Int, false);
            _numParans = 0;

            // since we just cleared main, small is now the most recent
            _smallWindow.MakeRecent();

            // reset the equals key focus when we change view
            Helpers.equalsKeyFocus(currentView.GetEqualsKey(), this);
        }

        public void initCalcError()
        {
            _smallWindow.Set(string.Empty, TextWindow.InputType.NULL, true);
            _mainWindow.Set("Err", TextWindow.InputType.NULL, true);
            _numParans = 0;
            _prevOperator = string.Empty;

            Memory_Clear_Button_Click(null, null);

            // reset the equals key focus when we change view
            Helpers.equalsKeyFocus(currentView.GetEqualsKey(), this);
        }

        private void initCalc()
        {
            Clear_Calc_Button_Click(null, null);
            Memory_Clear_Button_Click(null, null);

            // reset the equals key focus when we change view
            Helpers.equalsKeyFocus(currentView.GetEqualsKey(), this);
        }

        #endregion

        #region Equals

        public void Equal_Button_Click(object sender, RoutedEventArgs e)
        {
            bool exception = false;

            try
            {
                // get the expression
                string tempExpression = _smallWindow.Get();

                string smallExp = string.Empty;

                // if this was the user that hit equal this means we have an expression 
                if (sender != null && _mainWindow.isValue() && recentWindow == _mainWindow)
                    tempExpression += _mainWindow.Get();

                smallExp = tempExpression;

                // clear the calc
                _numParans = 0;
                _prevOperator = string.Empty;

                // get the evaluation
                tempExpression = _eval.Execute(tempExpression).ToString();

                _mainWindow.Set(tempExpression, tempExpression.Contains(".") ? TextWindow.InputType.Double : TextWindow.InputType.Int, true);

                // if the user hit enter update the small window 
                if (sender != null)
                {
                    _smallWindow.Set(string.Empty, TextWindow.InputType.NULL, true);
                    _mainWindow.MakeRecent();
                }
                else
                    _smallWindow.MakeRecent();
            }
            catch { exception = true; }
            if (exception)
            {
                initCalcError();
            }

            // reset the equals key focus when we change view
            Helpers.equalsKeyFocus(currentView.GetEqualsKey(), this);
        }

        #endregion

        #region Search

        private void initSearch()
        {
            _searchPane = SearchPane.GetForCurrentView();

            // While the user is typing this gets called to update the list
            _searchPane.SuggestionsRequested += searchPane_SuggestionsRequested;

            // When the user hits enter after they search
            _searchPane.QuerySubmitted += searchPane_QuerySubmitted;

            // While the user is typing this gets called for every new character they type (Internal UI update)
            _searchPane.QueryChanged += searchPane_QueryChanged;
        }

        private void searchPane_SuggestionsRequested(SearchPane sender, SearchPaneSuggestionsRequestedEventArgs args)
        {
            // try to see if the user entered in something valid
            try
            {
                // get the expression 
                string tempExpression = _eval.Execute(args.QueryText).ToString();

                args.Request.SearchSuggestionCollection.AppendQuerySuggestion(tempExpression);
            }
            catch { }
        }

        private void searchPane_QuerySubmitted(SearchPane sender, SearchPaneQuerySubmittedEventArgs args)
        {
            // assume they entered in something valid
            insertToWindow(args.QueryText);
        }

        private void searchPane_QueryChanged(SearchPane sender, SearchPaneQueryChangedEventArgs args)
        {
            // probably don't need to do anything here
        }


        #endregion

        #region Process Text

        // Implement expression symbols
        private void ProcessSymbol(object sender, SymbolEventArgs e)
        {
            if (String.Compare(e.Name, "π") == 0)
            {
                e.Result = Math.PI;
            }
            else if (String.Compare(e.Name, "e") == 0)
            {
                e.Result = Math.E;
            }
            else if (String.Compare(e.Name.ToLower(), "infinity") == 0)
            {
                e.Result = double.PositiveInfinity;
            }
            else if (String.Compare(e.Name.ToLower(), "nan") == 0)
            {
                e.Result = double.NaN;
            }
            // Unknown symbol name
            else e.Status = SymbolStatus.UndefinedSymbol;
        }

        // Implement expression functions
        private void ProcessFunction(object sender, FunctionEventArgs e)
        {
            #region Standard

            if (String.Compare(e.Name, "abs") == 0)
            {
                if (e.Parameters.Count == 1)
                    e.Result = Math.Abs(e.Parameters[0]).ToString();
                else
                    e.Status = FunctionStatus.WrongParameterCount;
            }
            //else if (String.Compare(e.Name, "pow") == 0)
            //{
            //    if (e.Parameters.Count == 2)
            //        e.Result = Math.Pow(e.Parameters[0], e.Parameters[1]).ToString();
            //    else
            //        e.Status = FunctionStatus.WrongParameterCount;
            //}
            else if (String.Compare(e.Name, "epow") == 0)
            {
                if (e.Parameters.Count == 1)
                    e.Result = Math.Pow(Math.E, e.Parameters[0]).ToString();
                else
                    e.Status = FunctionStatus.WrongParameterCount;
            }
            else if (String.Compare(e.Name, "round") == 0)
            {
                if (e.Parameters.Count == 1)
                    e.Result = Math.Round(e.Parameters[0]).ToString();
                else
                    e.Status = FunctionStatus.WrongParameterCount;
            }
            else if (String.Compare(e.Name, "√") == 0)
            {
                if (e.Parameters.Count == 1)
                    e.Result = Math.Sqrt(e.Parameters[0]).ToString();
                else
                    e.Status = FunctionStatus.WrongParameterCount;
            }

            else if (String.Compare(e.Name, "ln") == 0)
            {
                if (e.Parameters.Count == 1)
                    e.Result = Math.Log(e.Parameters[0], 2).ToString();
                else
                    e.Status = FunctionStatus.WrongParameterCount;
            }
            else if (String.Compare(e.Name, "log") == 0)
            {
                if (e.Parameters.Count == 1)
                    e.Result = Math.Log(e.Parameters[0]).ToString();
                else
                    e.Status = FunctionStatus.WrongParameterCount;
            }
            else if (String.Compare(e.Name, "logtwo") == 0)
            {
                if (e.Parameters.Count == 1)
                    e.Result = Math.Log(e.Parameters[0], 2).ToString();
                else
                    e.Status = FunctionStatus.WrongParameterCount;
            }
            else if (String.Compare(e.Name, "inv") == 0)
            {
                if (e.Parameters.Count == 1)
                {
                    if (e.Parameters[0] == 0)
                    {
                        throw new DivideByZeroException();
                    }
                    e.Result = (1.0 / e.Parameters[0]).ToString();
                }
                else
                    e.Status = FunctionStatus.WrongParameterCount;
            }
            else if (String.Compare(e.Name, "percent") == 0)
            {
                if (e.Parameters.Count == 1)
                {
                    e.Result = (e.Parameters[0] / 100).ToString();
                }

                else
                    e.Status = FunctionStatus.WrongParameterCount;
            }
            else if (String.Compare(e.Name, "powtwo") == 0)
            {
                if (e.Parameters.Count == 1)
                    e.Result = (e.Parameters[0] * e.Parameters[0]).ToString();
                else
                    e.Status = FunctionStatus.WrongParameterCount;
            }
            else if (String.Compare(e.Name, "twopow") == 0)
            {
                if (e.Parameters.Count == 1)
                    e.Result = (Math.Pow(2, e.Parameters[0])).ToString();
                else
                    e.Status = FunctionStatus.WrongParameterCount;
            }
            else if (String.Compare(e.Name, "powthree") == 0)
            {
                if (e.Parameters.Count == 1)
                    e.Result = (e.Parameters[0] * e.Parameters[0] * e.Parameters[0]).ToString();
                else
                    e.Status = FunctionStatus.WrongParameterCount;
            }
            else if (String.Compare(e.Name, "x!") == 0)
            {
                if (e.Parameters.Count == 1)
                {
                    try
                    {
                        e.Result = factorial(e.Parameters[0]);
                    }
                    catch
                    {
                        e.Status = FunctionStatus.WrongParameterCount;
                    }
                }
                else
                    e.Status = FunctionStatus.WrongParameterCount;
            }

            #endregion

            #region Statistical

            else if (String.Compare(e.Name, "Z") == 0)
            {
                if (e.Parameters.Count == 1)
                {
                    if (e.Parameters[0] < 0 || e.Parameters[0] > 1)
                    {
                        e.Status = FunctionStatus.WrongParameterCount;
                    }
                    else
                    {
                        e.Result = Math.Abs(critz(e.Parameters[0])).ToString();
                    }
                }
                else
                    e.Status = FunctionStatus.WrongParameterCount;
            }

            else if (String.Compare(e.Name, "Q") == 0)
            {
                if (e.Parameters.Count == 1)
                {
                    if (Math.Abs(e.Parameters[0]) > Z_MAX)
                    {
                        //alert("Error: z value must be between -6 and 6.\nValues outside this range have probabilities\nwhich exceed the precision of calculation used in this page.");
                        e.Status = FunctionStatus.WrongParameterCount;
                    }
                    else
                    {
                        double qz = 1 - poz(Math.Abs(e.Parameters[0]));
                        e.Result = qz.ToString();
                    }
                }
                else
                    e.Status = FunctionStatus.WrongParameterCount;
            }

            #endregion

            #region Logic

            else if (String.Compare(e.Name, "Not") == 0)
            {
                if (e.Parameters.Count == 1)
                {
                    List<int> binRep = _mainWindow.GetBinary();

                    // next we'll do inversion
                    List<int> binNot = new List<int>();
                    foreach (int b in binRep)
                    {
                        if (b == 1)
                            binNot.Add(0);
                        else if (b == 0)
                            binNot.Add(1);
                        else
                        {
                            e.Result = "";
                            break;
                        }
                    }

                    e.Result = _mainWindow.GetDecimal(binNot);
                }
                else
                    e.Status = FunctionStatus.WrongParameterCount;
            }

            else if (String.Compare(e.Name, "RoL") == 0)
            {
                if (e.Parameters.Count == 1)
                {
                    e.Result = ((long)e.Parameters[0] << 1).ToString();
                }
                else
                    e.Status = FunctionStatus.WrongParameterCount;
            }

            else if (String.Compare(e.Name, "RoR") == 0)
            {
                if (e.Parameters.Count == 1)
                {
                    e.Result = ((long)e.Parameters[0] >> 1).ToString();
                }
                else
                    e.Status = FunctionStatus.WrongParameterCount;
            }

            #endregion

            #region Cos

            else if (String.Compare(e.Name, "cos") == 0)
            {
                if (e.Parameters.Count == 1)
                {
                    // choose for degrees vs radians
                    if ((currentView as CalcViews.Scientific).scientificCalcState == CalcViews.Scientific.ScientificCalcState.Degrees)
                        e.Result = Math.Cos(Helpers.convertToRadians(e.Parameters[0])).ToString();
                    else
                        e.Result = Math.Cos(e.Parameters[0]).ToString();
                }
                else
                    e.Status = FunctionStatus.WrongParameterCount;
            }
            else if (String.Compare(e.Name, "cosh") == 0)
            {
                // No difference for deg vs rad
                if (e.Parameters.Count == 1)
                    e.Result = Math.Cosh(e.Parameters[0]).ToString();
                else
                    e.Status = FunctionStatus.WrongParameterCount;
            }
            else if (String.Compare(e.Name, "acos") == 0)
            {
                if (e.Parameters.Count == 1)
                {
                    // choose for degrees vs radians
                    if ((currentView as CalcViews.Scientific).scientificCalcState == CalcViews.Scientific.ScientificCalcState.Degrees)
                        e.Result = Helpers.convertToDegrees(Math.Acos(e.Parameters[0])).ToString();
                    else
                        e.Result = Math.Acos(e.Parameters[0]).ToString();
                }
                else
                    e.Status = FunctionStatus.WrongParameterCount;
            }
            else if (String.Compare(e.Name, "acosh") == 0)
            {
                // No difference for deg vs rad
                if (e.Parameters.Count == 1)
                    e.Result = Math.Log(e.Parameters[0] + Math.Sqrt(e.Parameters[0] * e.Parameters[0] - 1)).ToString();
                else
                    e.Status = FunctionStatus.WrongParameterCount;
            }

            #endregion

            #region Sin

            else if (String.Compare(e.Name, "sin") == 0)
            {
                if (e.Parameters.Count == 1)
                {
                    if ((currentView as CalcViews.Scientific).scientificCalcState == CalcViews.Scientific.ScientificCalcState.Degrees)
                        e.Result = Math.Sin(Helpers.convertToRadians(e.Parameters[0])).ToString();
                    else
                        e.Result = Math.Sin(e.Parameters[0]).ToString();
                }
                else
                    e.Status = FunctionStatus.WrongParameterCount;
            }
            else if (String.Compare(e.Name, "sinh") == 0)
            {
                if (e.Parameters.Count == 1)
                    e.Result = Math.Sinh(e.Parameters[0]).ToString();
                else
                    e.Status = FunctionStatus.WrongParameterCount;
            }
            else if (String.Compare(e.Name, "asin") == 0)
            {
                if (e.Parameters.Count == 1)
                {
                    if ((currentView as CalcViews.Scientific).scientificCalcState == CalcViews.Scientific.ScientificCalcState.Degrees)
                        e.Result = Helpers.convertToDegrees(Math.Asin(e.Parameters[0])).ToString();
                    else
                        e.Result = Math.Asin(e.Parameters[0]).ToString();
                }
                else
                    e.Status = FunctionStatus.WrongParameterCount;
            }
            else if (String.Compare(e.Name, "asinh") == 0)
            {
                if (e.Parameters.Count == 1)
                {
                    double x;
                    int sign;
                    if (e.Parameters[0] == 0.0)
                    {
                        e.Result = e.Parameters[0].ToString();
                    }
                    else
                    {
                        if (e.Parameters[0] < 0.0)
                        {
                            sign = -1;
                            x = -e.Parameters[0];
                        }
                        else
                        {
                            sign = 1;
                            x = e.Parameters[0];
                        }
                        e.Result = (sign * Math.Log(x + Math.Sqrt(x * x + 1))).ToString();
                    }
                }
                else
                    e.Status = FunctionStatus.WrongParameterCount;
            }

            #endregion

            #region Tan

            else if (String.Compare(e.Name, "tan") == 0)
            {
                if (e.Parameters.Count == 1)
                {
                    if ((currentView as CalcViews.Scientific).scientificCalcState == CalcViews.Scientific.ScientificCalcState.Degrees)
                        e.Result = Math.Tan(Helpers.convertToRadians(e.Parameters[0])).ToString();
                    else
                        e.Result = Math.Tan(e.Parameters[0]).ToString();
                }
                else
                    e.Status = FunctionStatus.WrongParameterCount;
            }
            else if (String.Compare(e.Name, "tanh") == 0)
            {
                if (e.Parameters.Count == 1)
                    e.Result = Math.Tanh(e.Parameters[0]).ToString();
                else
                    e.Status = FunctionStatus.WrongParameterCount;
            }
            else if (String.Compare(e.Name, "atan") == 0)
            {
                if (e.Parameters.Count == 1)
                {
                    if ((currentView as CalcViews.Scientific).scientificCalcState == CalcViews.Scientific.ScientificCalcState.Degrees)
                        e.Result = Helpers.convertToDegrees(Math.Atan(e.Parameters[0])).ToString();
                    else
                        e.Result = Math.Atan(e.Parameters[0]).ToString();
                }
                else
                    e.Status = FunctionStatus.WrongParameterCount;
            }
            else if (String.Compare(e.Name, "atanh") == 0)
            {
                if (e.Parameters.Count == 1)
                    e.Result = (Math.Log((1 + e.Parameters[0]) / (1 - e.Parameters[0])) * 0.5).ToString();
                else
                    e.Status = FunctionStatus.WrongParameterCount;
            }

            #endregion

            // Unknown function name
            else e.Status = FunctionStatus.UndefinedFunction;
        }

        #endregion

        #region Factorial

        public static string factorial(double x)
        {
            if (x > 5000)
            {
                throw new OverflowException();
            }
            double d = Math.Abs(x);
            if (Math.Floor(d) == d) return facInt((long)x);
            else return gamma(x + 1.0);
        }

        private static string facInt(long j)
        {
            BigInteger i = j;
            BigInteger d = 1;
            if (j < 0) i = Math.Abs(j);
            while (i > 1)
            {
                d *= i--;
            }
            if (j < 0) return (-d).ToString();
            else return d.ToString();
        }

        /// <summary>
        /// Returns the gamma function of the specified number.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        private static string gamma(double x)
        {
            double[] P = {
						 1.60119522476751861407E-4,
						 1.19135147006586384913E-3,
						 1.04213797561761569935E-2,
						 4.76367800457137231464E-2,
						 2.07448227648435975150E-1,
						 4.94214826801497100753E-1,
						 9.99999999999999996796E-1
					 };
            double[] Q = {
						 -2.31581873324120129819E-5,
						 5.39605580493303397842E-4,
						 -4.45641913851797240494E-3,
						 1.18139785222060435552E-2,
						 3.58236398605498653373E-2,
						 -2.34591795718243348568E-1,
						 7.14304917030273074085E-2,
						 1.00000000000000000320E0
					 };

            double p, z;

            double q = Math.Abs(x);

            if (q > 33.0)
            {
                if (x < 0.0)
                {
                    p = Math.Floor(q);
                    if (p == q) throw new ArithmeticException("gamma: overflow");
                    //int i = (int)p;
                    z = q - p;
                    if (z > 0.5)
                    {
                        p += 1.0;
                        z = q - p;
                    }
                    z = q * Math.Sin(Math.PI * z);
                    if (z == 0.0) throw new ArithmeticException("gamma: overflow");
                    z = Math.Abs(z);
                    z = Math.PI / (z * stirf(q));

                    return (-z).ToString();
                }
                else
                {
                    return stirf(x).ToString();
                }
            }

            z = 1.0;
            while (x >= 3.0)
            {
                x -= 1.0;
                z *= x;
            }

            while (x < 0.0)
            {
                if (x == 0.0)
                {
                    throw new ArithmeticException("gamma: singular");
                }
                else if (x > -1.0E-9)
                {
                    return (z / ((1.0 + 0.5772156649015329 * x) * x)).ToString();
                }
                z /= x;
                x += 1.0;
            }

            while (x < 2.0)
            {
                if (x == 0.0)
                {
                    throw new ArithmeticException("gamma: singular");
                }
                else if (x < 1.0E-9)
                {
                    return (z / ((1.0 + 0.5772156649015329 * x) * x)).ToString();
                }
                z /= x;
                x += 1.0;
            }

            if ((x == 2.0) || (x == 3.0)) return z.ToString();

            x -= 2.0;
            p = polevl(x, P, 6);
            q = polevl(x, Q, 7);
            return (z * p / q).ToString();

        }

        /// <summary>
        /// Evaluates polynomial of degree N
        /// </summary>
        /// <param name="x"></param>
        /// <param name="coef"></param>
        /// <param name="N"></param>
        /// <returns></returns>
        private static double polevl(double x, double[] coef, int N)
        {
            double ans;

            ans = coef[0];

            for (int i = 1; i <= N; i++)
            {
                ans = ans * x + coef[i];
            }

            return ans;
        }

        /// <summary>
        /// Return the gamma function computed by Stirling's formula.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        private const double SQTPI = 2.50662827463100050242E0;
        private static double stirf(double x)
        {
            double[] STIR = {
							7.87311395793093628397E-4,
							-2.29549961613378126380E-4,
							-2.68132617805781232825E-3,
							3.47222221605458667310E-3,
							8.33333333333482257126E-2,
		};
            double MAXSTIR = 143.01608;

            double w = 1.0 / x;
            double y = Math.Exp(x);

            w = 1.0 + w * polevl(w, STIR, 4);

            if (x > MAXSTIR)
            {
                /* Avoid overflow in Math.Pow() */
                double v = Math.Pow(x, 0.5 * x - 0.25);
                y = v * (v / y);
            }
            else
            {
                y = Math.Pow(x, x - 0.5) / y;
            }
            y = SQTPI * y * w;
            return y;
        }

        #endregion

        #region ZScore

        private const int Z_MAX = 6;                    // Maximum ±z value
        private const int ROUND_FLOAT = 6;              // Decimal places to round numbers

        /*  The following JavaScript functions for calculating normal and
            chi-square probabilities and critical values were adapted by
            John Walker from C implementations
            written by Gary Perlman of Wang Institute, Tyngsboro, MA
            01879.  Both the original C code and this JavaScript edition
            are in the public domain.  */

        /*  POZ  --  probability of normal z value

            Adapted from a polynomial approximation in:
                    Ibbetson D, Algorithm 209
                    Collected Algorithms of the CACM 1963 p. 616
            Note:
                    This routine has six digit accuracy, so it is only useful for absolute
                    z values <= 6.  For z values > to 6.0, poz() returns 0.0.
        */

        private double poz(double z)
        {
            double y, x, w;

            if (z == 0.0)
            {
                x = 0.0;
            }
            else
            {
                y = 0.5 * Math.Abs(z);
                if (y > (Z_MAX * 0.5))
                {
                    x = 1.0;
                }
                else if (y < 1.0)
                {
                    w = y * y;
                    x = ((((((((0.000124818987 * w
                             - 0.001075204047) * w + 0.005198775019) * w
                             - 0.019198292004) * w + 0.059054035642) * w
                             - 0.151968751364) * w + 0.319152932694) * w
                             - 0.531923007300) * w + 0.797884560593) * y * 2.0;
                }
                else
                {
                    y -= 2.0;
                    x = (((((((((((((-0.000045255659 * y
                                   + 0.000152529290) * y - 0.000019538132) * y
                                   - 0.000676904986) * y + 0.001390604284) * y
                                   - 0.000794620820) * y - 0.002034254874) * y
                                   + 0.006549791214) * y - 0.010557625006) * y
                                   + 0.011630447319) * y - 0.009279453341) * y
                                   + 0.005353579108) * y - 0.002141268741) * y
                                   + 0.000535310849) * y + 0.999936657524;
                }
            }
            return z > 0.0 ? ((x + 1.0) * 0.5) : ((1.0 - x) * 0.5);
        }


        /*  CRITZ  --  Compute critical normal z value to
                       produce given p.  We just do a bisection
                       search for a value within CHI_EPSILON,
                       relying on the monotonicity of pochisq().  */

        private double critz(double p)
        {
            double Z_EPSILON = 0.000001;     /* Accuracy of z approximation */
            double minz = -Z_MAX;
            double maxz = Z_MAX;
            double zval = 0.0;
            double pval;

            if (p < 0.0 || p > 1.0)
            {
                return -1;
            }

            while ((maxz - minz) > Z_EPSILON)
            {
                pval = poz(zval);
                if (pval > p)
                {
                    maxz = zval;
                }
                else
                {
                    minz = zval;
                }
                zval = (maxz + minz) * 0.5;
            }
            return (zval);
        }

        #endregion

        #region debug

        public static async void printdbg(string txt)
        {
            await new Windows.UI.Popups.MessageDialog(txt).ShowAsync();
        }

        #endregion
    }
}
