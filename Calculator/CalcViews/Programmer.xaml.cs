using System;
using System.Collections.Generic;
using System.Globalization;
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
    public sealed partial class Programmer : Page, CalculatorView
    {
        private MainPage _mainPage;

        // Bitness
        public enum ProgrammerCalcBits { Qword = 10, Dword = 8, Word = 5, Byte = 2 }
        public ProgrammerCalcBits currentBits = ProgrammerCalcBits.Qword;

        // State
        public enum ProgrammerCalcState { Hexidecimal, Decimal, Octal, Binary }
        public ProgrammerCalcState calcState = ProgrammerCalcState.Hexidecimal;

        public Programmer(MainPage mainPage)
        {
            this.InitializeComponent();

            this._mainPage = mainPage;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        #region Interface

        public Button GetEqualsKey()
        {
            return this.EqualProgrammer;
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

        #region Programmer Calc Bitness

        public void QWord_Bitness_Click(object sender, RoutedEventArgs e)
        {
            Change_Bitness_Click(ProgrammerCalcBits.Qword);
        }

        public void DWord_Bitness_Click(object sender, RoutedEventArgs e)
        {
            Change_Bitness_Click(ProgrammerCalcBits.Dword);
        }

        public void Word_Bitness_Click(object sender, RoutedEventArgs e)
        {
            Change_Bitness_Click(ProgrammerCalcBits.Word);
        }

        public void Byte_Bitness_Click(object sender, RoutedEventArgs e)
        {
            Change_Bitness_Click(ProgrammerCalcBits.Byte);
        }

        public void Change_Bitness_Click(ProgrammerCalcBits bitness)
        {
            if (bitness == ProgrammerCalcBits.Qword)
            {
                // toggle button
                QWordButton.IsChecked = true;
                DWordButton.IsChecked = false;
                WordButton.IsChecked = false;
                ByteButton.IsChecked = false;

                // set UI to show bitness
                QwordBitness.Visibility = Windows.UI.Xaml.Visibility.Visible;
                DwordBitness.Visibility = Windows.UI.Xaml.Visibility.Visible;
                WordBitness.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            else if (bitness == ProgrammerCalcBits.Dword)
            {
                QWordButton.IsChecked = false;
                DWordButton.IsChecked = true;
                WordButton.IsChecked = false;
                ByteButton.IsChecked = false;

                QwordBitness.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                DwordBitness.Visibility = Windows.UI.Xaml.Visibility.Visible;
                WordBitness.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            else if (bitness == ProgrammerCalcBits.Word)
            {
                QWordButton.IsChecked = false;
                DWordButton.IsChecked = false;
                WordButton.IsChecked = true;
                ByteButton.IsChecked = false;

                QwordBitness.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                DwordBitness.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                WordBitness.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            else if (bitness == ProgrammerCalcBits.Byte)
            {
                QWordButton.IsChecked = false;
                DWordButton.IsChecked = false;
                WordButton.IsChecked = false;
                ByteButton.IsChecked = true;

                QwordBitness.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                DwordBitness.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                WordBitness.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }

            _mainPage.Change_Bitness_Click(bitness);
        }

        #endregion

        #region Programmer Calc Settings

        public void ProgrammerSettings_Click(ProgrammerCalcState mode)
        {
            if (mode == ProgrammerCalcState.Hexidecimal)
            {
                // Buttons
                HexSpecificButtons(true);
                DecimalSpecificButtons(true);
                OctalSpecificButtons(true);
            }
            else if (mode == ProgrammerCalcState.Decimal)
            {
                // Buttons
                HexSpecificButtons(false);
                DecimalSpecificButtons(true);
                OctalSpecificButtons(true);
            }
            else if (mode == ProgrammerCalcState.Octal)
            {
                // Buttons
                HexSpecificButtons(false);
                DecimalSpecificButtons(false);
                OctalSpecificButtons(true);
            }
            else if (mode == ProgrammerCalcState.Binary)
            {
                // Buttons
                HexSpecificButtons(false);
                DecimalSpecificButtons(false);
                OctalSpecificButtons(false);
            }
        }

        // what turns off from Hex to Dec
        private void HexSpecificButtons(bool enabled)
        {
            AButton.IsEnabled = enabled;
            BButton.IsEnabled = enabled;
            CButton.IsEnabled = enabled;
            DButton.IsEnabled = enabled;
            EButton.IsEnabled = enabled;
            FButton.IsEnabled = enabled;
        }

        // what turns off from Dec to Oct
        private void DecimalSpecificButtons(bool enabled)
        {
            EightButton.IsEnabled = enabled;
            NineButton.IsEnabled = enabled;
        }

        // what turns off from Oct to Bin
        private void OctalSpecificButtons(bool enabled)
        {
            SevenButton.IsEnabled = enabled;
            SixButton.IsEnabled = enabled;
            FiveButton.IsEnabled = enabled;
            FourButton.IsEnabled = enabled;
            ThreeButton.IsEnabled = enabled;
            TwoButton.IsEnabled = enabled;
        }

        #endregion

        #region Conversion Functions

        public string toDec(ProgrammerCalcState type, string exp)
        {
            if (type == ProgrammerCalcState.Binary)
                return bin2Dec(exp);
            else if (type == ProgrammerCalcState.Hexidecimal)
                return hex2Dec(exp);
            else if (type == ProgrammerCalcState.Octal)
                return oct2Dec(exp);

            return exp;
        }

        #region Dec To...

        public string dec2Hex(string data)
        {
            try
            {
                if (currentBits == ProgrammerCalcBits.Qword)
                    return dec2Long(data, 16);
                else if (currentBits == ProgrammerCalcBits.Dword)
                    return dec2Int(data, 16);
                else if (currentBits == ProgrammerCalcBits.Word)
                    return dec2Short(data, 16);
                else if (currentBits == ProgrammerCalcBits.Byte)
                    return dec2Byte(data, 16);
            }
            catch { }

            return string.Empty;
        }

        public string dec2Oct(string data)
        {
            try
            {
                if (currentBits == ProgrammerCalcBits.Qword)
                    return dec2Long(data, 8);
                else if (currentBits == ProgrammerCalcBits.Dword)
                    return dec2Int(data, 8);
                else if (currentBits == ProgrammerCalcBits.Word)
                    return dec2Short(data, 8);
                else if (currentBits == ProgrammerCalcBits.Byte)
                    return dec2Byte(data, 8);
            }
            catch { }

            return string.Empty;
        }

        public string dec2Bin(string data)
        {
            try
            {
                if (currentBits == ProgrammerCalcBits.Qword)
                    return dec2Long(data, 2);
                else if (currentBits == ProgrammerCalcBits.Dword)
                    return dec2Int(data, 2);
                else if (currentBits == ProgrammerCalcBits.Word)
                    return dec2Short(data, 2);
                else if (currentBits == ProgrammerCalcBits.Byte)
                    return dec2Byte(data, 2);
            }
            catch { }

            return string.Empty;
        }

        private static string dec2Byte(string data, int numBase)
        {
            try
            {
                // since convert doesn't support going smaller than word we'll 
                // initially convert to binary then clip off the top 8 bits


                if (numBase == 2)
                {
                    string binVal = Convert.ToString(sbyte.Parse(data), 2).PadLeft(16, '0').Substring(8);
                    return binVal;
                }
                else if (numBase == 16)
                    return sbyte.Parse(data).ToString("x", CultureInfo.InvariantCulture);
                else if (numBase == 10)
                {
                    int val = int.Parse(data);
                    if (val <= sbyte.MaxValue || val >= sbyte.MinValue)
                        return data;
                }
                else if (numBase == 8)
                {
                    int val = int.Parse(data);
                    return int.Parse(string.Format(@"{0}{1}{2}",
                            ((val >> 6) & 3),
                            ((val >> 3) & 7),
                            (val & 7)
                        )).ToString();
                }
            }
            catch { }

            return string.Empty;
        }

        private static string dec2Short(string data, int numBase)
        {
            try { return Convert.ToString(short.Parse(data), numBase); }
            catch { }

            return string.Empty;
        }

        private static string dec2Int(string data, int numBase)
        {
            try { return Convert.ToString(int.Parse(data), numBase); }
            catch { }

            return string.Empty;
        }

        public static string dec2Long(string data, int numBase)
        {
            try { return Convert.ToString(long.Parse(data), numBase); }
            catch { }

            return string.Empty;
        }

        #endregion

        #region Hex To...

        public string hex2Dec(string data)
        {
            if (currentBits == ProgrammerCalcBits.Qword)
                return hex2Long(data);
            else if (currentBits == ProgrammerCalcBits.Dword)
                return hex2Int(data);
            else if (currentBits == ProgrammerCalcBits.Word)
                return hex2Short(data);
            else if (currentBits == ProgrammerCalcBits.Byte)
                return hex2Byte(data);

            return string.Empty;
        }

        private string hex2Oct(string data)
        {
            try { return hex2Dec(dec2Oct(data)); }
            catch { }

            return string.Empty;
        }

        private string hex2Bin(string data)
        {
            try { return hex2Dec(dec2Bin(data)); }
            catch { }

            return string.Empty;
        }


        private static string hex2Byte(string data)
        {
            try { return sbyte.Parse(data, System.Globalization.NumberStyles.HexNumber).ToString(); }
            catch { }

            return string.Empty;
        }

        private static string hex2Short(string data)
        {
            try { return short.Parse(data, System.Globalization.NumberStyles.HexNumber).ToString(); }
            catch { }

            return string.Empty;
        }

        private static string hex2Int(string data)
        {
            try { return int.Parse(data, System.Globalization.NumberStyles.HexNumber).ToString(); }
            catch { }

            return string.Empty;
        }

        public static string hex2Long(string data)
        {
            try { return long.Parse(data, System.Globalization.NumberStyles.HexNumber).ToString(); }
            catch { }

            return string.Empty;
        }

        #endregion

        #region Oct To...

        public string oct2Dec(string data)
        {
            if (currentBits == ProgrammerCalcBits.Qword)
                return oct2Long(data);
            else if (currentBits == ProgrammerCalcBits.Dword)
                return oct2Int(data);
            else if (currentBits == ProgrammerCalcBits.Word)
                return oct2Short(data);
            else if (currentBits == ProgrammerCalcBits.Byte)
                return oct2Byte(data);

            return string.Empty;
        }

        private string oct2Hex(string data)
        {
            try { return dec2Hex(oct2Dec(data)); }
            catch { }

            return string.Empty;
        }

        private string oct2Bin(string data)
        {
            try { return dec2Bin(oct2Dec(data)); }
            catch { }

            return string.Empty;
        }

        private static string oct2Byte(string data)
        {
            try { return Convert.ToSByte(data, 8).ToString(); }
            catch { }

            return string.Empty;
        }

        private static string oct2Short(string data)
        {
            try { return Convert.ToInt16(data, 8).ToString(); }
            catch { }

            return string.Empty;
        }

        private static string oct2Int(string data)
        {
            try { return Convert.ToInt32(data, 8).ToString(); }
            catch { }

            return string.Empty;
        }

        public static string oct2Long(string data)
        {
            try { return Convert.ToInt64(data, 8).ToString(); }
            catch { }

            return string.Empty;
        }

        #endregion

        #region Bin To...

        public string bin2Dec(string data)
        {
            if (currentBits == ProgrammerCalcBits.Qword)
                return bin2Long(data);
            else if (currentBits == ProgrammerCalcBits.Dword)
                return bin2Int(data);
            else if (currentBits == ProgrammerCalcBits.Word)
                return bin2Short(data);
            else if (currentBits == ProgrammerCalcBits.Byte)
                return bin2Byte(data);

            return string.Empty;
        }

        private string bin2Hex(string data)
        {
            try { return dec2Hex(bin2Dec(data)); }
            catch { }

            return string.Empty;
        }

        private string bin2Oct(string data)
        {
            try { return dec2Oct(bin2Dec(data)); }
            catch { }

            return string.Empty;
        }

        private static string bin2Byte(string data)
        {
            try { return Convert.ToSByte(data, 2).ToString(); }
            catch { }

            return string.Empty;
        }

        private static string bin2Short(string data)
        {
            try { return Convert.ToInt16(data, 2).ToString(); }
            catch { }

            return string.Empty;
        }

        private static string bin2Int(string data)
        {
            try { return Convert.ToInt32(data, 2).ToString(); }
            catch { }

            return string.Empty;
        }

        public static string bin2Long(string data)
        {
            try { return Convert.ToInt64(data, 2).ToString(); }
            catch { }

            return string.Empty;
        }

        #endregion

        #endregion

        #region Redraw

        public void Redraw(List<int> binArray)
        {
            try
            {
                // byte
                BinaryFirst.Text = arrayToString(binArray.GetRange(32, 4));
                BinarySecond.Text = arrayToString(binArray.GetRange(36, 4));

                // word
                BinaryThird.Text = arrayToString(binArray.GetRange(40, 4));
                BinaryFourth.Text = arrayToString(binArray.GetRange(44, 4));

                // dword
                BinaryFifth.Text = arrayToString(binArray.GetRange(48, 4));
                BinarySixth.Text = arrayToString(binArray.GetRange(52, 4));
                BinarySeventh.Text = arrayToString(binArray.GetRange(56, 4));
                BinaryEight.Text = arrayToString(binArray.GetRange(60, 4));

                // qword
                BinaryFirstUp.Text = arrayToString(binArray.GetRange(0, 4));
                BinarySecondUp.Text = arrayToString(binArray.GetRange(4, 4));
                BinaryThirdUp.Text = arrayToString(binArray.GetRange(8, 4));
                BinaryFourthUp.Text = arrayToString(binArray.GetRange(12, 4));
                BinaryFifthUp.Text = arrayToString(binArray.GetRange(16, 4));
                BinarySixthUp.Text = arrayToString(binArray.GetRange(20, 4));
                BinarySeventhUp.Text = arrayToString(binArray.GetRange(24, 4));
                BinaryEightUp.Text = arrayToString(binArray.GetRange(28, 4));
            }
            catch
            {
                BinaryFirst.Text = string.Empty;
                BinarySecond.Text = string.Empty;
                BinaryThird.Text = string.Empty;
                BinaryFourth.Text = string.Empty;
                BinaryFifth.Text = string.Empty;
                BinarySixth.Text = string.Empty;
                BinarySeventh.Text = string.Empty;
                BinaryEight.Text = string.Empty;

                BinaryFirstUp.Text = string.Empty;
                BinarySecondUp.Text = string.Empty;
                BinaryThirdUp.Text = string.Empty;
                BinaryFourthUp.Text = string.Empty;
                BinaryFifthUp.Text = string.Empty;
                BinarySixthUp.Text = string.Empty;
                BinarySeventhUp.Text = string.Empty;
                BinaryEightUp.Text = string.Empty;
            }
        }

        private string arrayToString(List<int> arr)
        {
            string retString = string.Empty;
            foreach (int i in arr)
                retString += i.ToString();

            return retString;
        }

        #endregion
    }
}
