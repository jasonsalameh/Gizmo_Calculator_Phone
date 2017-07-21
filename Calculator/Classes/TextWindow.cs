using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Calculator
{
    public class TextWindow
    {
        // bitness constants
        private const int LONG = 64;
        private const int INT = 32;
        private const int SHORT = 16;
        private const int BYTE = 8;

        // Calc bitness
        private const int MAX_BITNESS = LONG;

        // Window max chars this is JUST for scientific mode
        public const int MAX_CHARACTERS = 49;

        // Main page
        private MainPage _mainPage = null;

        // Current Page
        public CalculatorView currentView { get; set; }

        private bool _isProgrammerDec = false;
        public bool isProgrammerDec { set { _isProgrammerDec = value; } }

        private TextBlock _window = null;
        private bool _isMainWindow;
        private bool _clearOnNextUse;
        public bool clearOnNextUse { get { return _clearOnNextUse; } set { _clearOnNextUse = value; } }

        // for previous inputs
        public enum InputType { OperatorDouble = 1, OperatorSingle = 2, Int = 3, Double = 4, Rand = 5, Symbol = 5, Paran = 6, Negative = 7, NULL = 100 }
        private List<InputValue> _inputSequence = null;

        public TextWindow(TextBlock window, bool isMainWindow, MainPage mainPage)
        {
            _window = window;
            _inputSequence = new List<InputValue>();
            _clearOnNextUse = true;
            _isMainWindow = isMainWindow;
            _mainPage = mainPage;
        }

        #region Set/Add

        public void Set(string txt, InputType type, bool clearOnNextUse)
        {
            // clear input sequence
            _inputSequence.Clear();

            // add to input sequence both scientific and programmer
            AddInputSequence(txt, type, false, true);

            // update the screen text
            Redraw();

            // say the window has nothing in it
            _clearOnNextUse = clearOnNextUse;

            // update the recent list
            MakeRecent();
        }

        public void SetNoTranslation(string txt)
        {
            // clear input sequence
            _inputSequence.Clear();

            // add to input sequence both scientific and programmer
            AddInputSequence(txt, InputType.NULL, false, true);

            // update the screen text
            _window.Text = txt;

            // say the window has nothing in it
            _clearOnNextUse = true;

            // update the recent list
            MakeRecent();
        }

        public void Add(string txt, InputType type)
        {
            // if the user is trying to enter in more characters that what is supported
            if (_isMainWindow && GetTextFromList().Length > MAX_CHARACTERS)
                return;

            bool hasParan = false;
            if (_clearOnNextUse)
            {
                _inputSequence.Clear();
                _clearOnNextUse = false;
            }

            // if our current value and our last value are the "same"
            if (!_isMainWindow && (type == InputType.Int || type == InputType.Double) && (this.GetLastInputType() == InputType.Int || this.GetLastInputType() == InputType.Double))
            {
                // remove the previous element and now add a new one with updated text
                InputValue value = _inputSequence[_inputSequence.Count - 1];
                _inputSequence.RemoveAt(_inputSequence.Count - 1);
                txt = value.inputData + txt;
            }

            // if we have a single operator, simply remove the last inputvalue and change it
            else if (type == InputType.OperatorSingle)
            {
                InputValue value = _inputSequence[_inputSequence.Count - 1];
                _inputSequence.RemoveAt(_inputSequence.Count - 1);

                if (value.hasParan)
                    txt = txt + value.inputData;
                else
                    txt = txt + "(" + value.inputData + ")";
            }

            // if the user entered in a paran expression
            else if (!_isMainWindow && MainPage._numParans > 0 && _inputSequence.Count > 0)
            {
                // remove the previous element and now add a new one with updated text
                InputValue value = _inputSequence[_inputSequence.Count - 1];
                _inputSequence.RemoveAt(_inputSequence.Count - 1);
                txt = value.inputData + txt;
                hasParan = true;
            }

            // add to input sequence
            AddInputSequence(txt, type, hasParan, false);

            Redraw();

            // update the recent list
            MakeRecent();
        }

        private void AddInputSequence(string txt, InputType type, bool hasParan, bool isSet)
        {
            // if we're in a programmer mode
            if (_isMainWindow && _mainPage.isProgrammer() && isValue(type))
            {
                // get our decimal converted value
                string dec = GetTextFromList();

                // first get the original expression
                string expOrig = GetExpression(dec, false);

                // add onto it the new value
                string expLong = expOrig + GetExpression(txt, true);

                //string binRep
                expOrig += GetExpression(txt, false);

                // get the new decimal expression
                dec = GetDecimal(expLong, true);

                if (dec == string.Empty)
                    dec = "0";

                // for current bitness make sure the value isn't too big
                // Here we're right shifting since this stuff is signed
                if ((currentView as CalcViews.Programmer).currentBits == Calculator.CalcViews.Programmer.ProgrammerCalcBits.Byte && (long.Parse(dec) >> 1) > sbyte.MaxValue ||
                    (currentView as CalcViews.Programmer).currentBits == Calculator.CalcViews.Programmer.ProgrammerCalcBits.Word && (long.Parse(dec) >> 1) > short.MaxValue ||
                    (currentView as CalcViews.Programmer).currentBits == Calculator.CalcViews.Programmer.ProgrammerCalcBits.Dword && (long.Parse(dec) >> 1) > int.MaxValue ||
                    (currentView as CalcViews.Programmer).currentBits == Calculator.CalcViews.Programmer.ProgrammerCalcBits.Qword && (long.Parse(dec) >> 1) > long.MaxValue)
                    return;

                // now that we validated, get the real SIGNED value 
                dec = GetDecimal(expOrig, false);

                if (dec == string.Empty)
                    dec = "0";

                // clear input sequence
                _inputSequence.Clear();

                // re-add to the sequence
                _inputSequence.Add(new InputValue(InputType.Int, dec, hasParan));
            }

            // if we're in scientific mode
            else
                _inputSequence.Add(new InputValue(type, txt, hasParan));
        }

        #endregion

        #region Remove

        public bool RemoveLast()
        {

            // if we're in programmer mode we need to first validate which mode we're in
            // so that we can delete proper amounts of characters
            if (_mainPage.isProgrammer())
            {
                // As with all remove ops, check if it's empty first
                if (_window.Text == string.Empty || _inputSequence.Count <= 0 || GetLastInputType() == InputType.NULL)
                {
                    if (_isMainWindow)
                        Set("0", InputType.Int, true);
                    else
                        Set(string.Empty, InputType.NULL, true);

                    return false;
                }

                string expression = GetExpression(GetTextFromList(), false);
                
                // trim off the last character
                expression = expression.Remove(expression.Length - 1);

                Set(GetDecimal(expression, false), InputType.Int, false);
            }
            else
            {
                // As with all remove ops, check if it's empty first
                if (_window.Text == string.Empty || _inputSequence.Count <= 1 || GetLastInputType() == InputType.NULL)
                {
                    if (_isMainWindow)
                        Set("0", InputType.Int, true);
                    else
                        Set(string.Empty, InputType.NULL, true);

                    return false;
                }

                // Remove the last element from the list
                _inputSequence.RemoveAt(_inputSequence.Count - 1);
            }

            // Set our current Window and type
            Redraw();

            // update the recent list
            MakeRecent();

            return true;
        }

        public bool RemoveFirst(bool makeRecent)
        {
            // As with all remove ops, check if it's empty first
            if (_window.Text == string.Empty || _inputSequence.Count <= 1 || GetLastInputType() == InputType.NULL)
            {
                if (_isMainWindow)
                    Set("0", InputType.Int, false);
                else
                    Set(string.Empty, InputType.NULL, true);

                return false;
            }

            // Remove the last element from the list
            if (_mainPage.isProgrammer())
            {
                // get our decimal converted value
                string dec = GetTextFromList();

                // first get the original expression
                string exp = GetExpression(dec, false);

                // add onto it the new value
                exp = exp.Substring(1);

                // get the new decimal expression
                dec = GetDecimal(exp, false);

                // re-add to the sequence
                Set(dec, InputType.Int, false);
            }
            else
                _inputSequence.RemoveAt(0);

            // Set our current Window and type
            Redraw();

            // update the recent list
            if(makeRecent)
                MakeRecent();

            return true;
        }

        #endregion

        #region Get

        private string GetExpression(string dec, bool asLong)
        {
            string exp = dec;

            if (!_mainPage.isProgrammer())
                return exp;

            if (asLong)
            {
                if ((currentView as CalcViews.Programmer).calcState == CalcViews.Programmer.ProgrammerCalcState.Hexidecimal)
                    exp = CalcViews.Programmer.dec2Long(dec, 16);
                else if ((currentView as CalcViews.Programmer).calcState == CalcViews.Programmer.ProgrammerCalcState.Octal)
                    exp = CalcViews.Programmer.dec2Long(dec, 8);
                else if ((currentView as CalcViews.Programmer).calcState == CalcViews.Programmer.ProgrammerCalcState.Binary)
                    exp = CalcViews.Programmer.dec2Long(dec, 2);
            }

            else
            {
                if ((currentView as CalcViews.Programmer).calcState == CalcViews.Programmer.ProgrammerCalcState.Hexidecimal)
                    exp = (currentView as CalcViews.Programmer).dec2Hex(dec);
                else if ((currentView as CalcViews.Programmer).calcState == CalcViews.Programmer.ProgrammerCalcState.Octal)
                    exp = (currentView as CalcViews.Programmer).dec2Oct(dec);
                else if ((currentView as CalcViews.Programmer).calcState == CalcViews.Programmer.ProgrammerCalcState.Binary)
                    exp = (currentView as CalcViews.Programmer).dec2Bin(dec);
            }

            return exp;
        }

        // always returns "64-bit" representation
        public List<int> GetBinary()
        {
            if (!_mainPage.isProgrammer())
                return new List<int>();

            string bin = (currentView as CalcViews.Programmer).dec2Bin(Get()).PadLeft(MAX_BITNESS, '0');

            List<int> binArray = new List<int>();

            foreach (char c in bin.ToCharArray())
                binArray.Add(int.Parse(c.ToString()));

            return binArray;
        }

        public string GetDecimal(List<int> bin)
        {
            if (!_mainPage.isProgrammer())
                return string.Empty;

            string exp = string.Empty;
            foreach (int b in bin)
                exp += b.ToString();

            return (currentView as CalcViews.Programmer).bin2Dec(exp);
        }

        private string GetDecimal(string exp, bool asLong)
        {
            string dec = exp;

            if (!_mainPage.isProgrammer())
                return dec;

            if (asLong)
            {
                if ((currentView as CalcViews.Programmer).calcState == CalcViews.Programmer.ProgrammerCalcState.Hexidecimal)
                    dec = CalcViews.Programmer.hex2Long(exp);
                else if ((currentView as CalcViews.Programmer).calcState == CalcViews.Programmer.ProgrammerCalcState.Octal)
                    dec = CalcViews.Programmer.oct2Long(exp);
                else if ((currentView as CalcViews.Programmer).calcState == CalcViews.Programmer.ProgrammerCalcState.Binary)
                    dec = CalcViews.Programmer.bin2Long(exp);
            }

            else
            {
                if ((currentView as CalcViews.Programmer).calcState == CalcViews.Programmer.ProgrammerCalcState.Hexidecimal)
                    dec = (currentView as CalcViews.Programmer).hex2Dec(exp);
                else if ((currentView as CalcViews.Programmer).calcState == CalcViews.Programmer.ProgrammerCalcState.Octal)
                    dec = (currentView as CalcViews.Programmer).oct2Dec(exp);
                else if ((currentView as CalcViews.Programmer).calcState == CalcViews.Programmer.ProgrammerCalcState.Binary)
                    dec = (currentView as CalcViews.Programmer).bin2Dec(exp);
            }

            return dec;
        }

        private string GetTextFromListForWindow()
        {
            // for the small window, only print stuff if we're in scientific mode
            if (!_isMainWindow)
            {
                if (!_mainPage.isProgrammer())
                    return GetTextFromList();
                else
                    return string.Empty;
            }

            // if we're not in a normal numbers view return the normal text
            if (_mainPage.isProgrammer())
                return GetExpression(GetTextFromList(), false);

            // otherwise return scientific notation which include commas in the text
            else
                return GetTextFromList();
        }

        private string GetTextFromList()
        {
            string temp = string.Empty;

            foreach (InputValue v in _inputSequence)
                temp += v.inputData;

            return temp;
        }

        private void SeperateIntFromDecimal(out string intExp, out string decimalExp)
        {
            bool dotFound = false;
            intExp = decimalExp = string.Empty;

            foreach (InputValue v in _inputSequence)
            {
                if (v.inputData == ".")
                    dotFound = true;

                if (dotFound)
                    decimalExp += v.inputData;
                else
                    intExp += v.inputData;
            }
        }

        public string Get()
        {
            return GetTextFromList();
        }

        public string GetWindowText()
        {
            return GetTextFromListForWindow();
        }

        public string Get(int index)
        {
            return _inputSequence[index].inputData;
        }

        public InputValue GetLast()
        {
            return _inputSequence.Last();
        }

        public InputType GetLastInputType()
        {
            if (_inputSequence.Count > 0)
                return _inputSequence.Last().inputType;
            return InputType.NULL;
        }

        #endregion

        #region State

        public void MakeRecent()
        {
            MainPage.recentWindow = this;
        }

        public void Negate()
        {
            if (_inputSequence.Count > 0)
            {
                // first check to see if we're already negated
                if (_inputSequence[0].inputData.StartsWith("-"))
                {
                    // if this is the single negation operator remove it
                    if (_inputSequence[0].inputData.Length == 1)
                        _inputSequence.RemoveAt(0);
                    else
                    {
                        // get the old data and modify it
                        string data = _inputSequence[0].inputData.Substring(1);
                        InputType type = _inputSequence[0].inputType;
                        bool hasParan = _inputSequence[0].hasParan;

                        // remove it
                        _inputSequence.RemoveAt(0);

                        // re add to the list
                        _inputSequence.Insert(0, new InputValue(type, data, hasParan));
                    }
                }
                else
                    _inputSequence.Insert(0, new InputValue(InputType.Negative, "-", false));

                // redraw the content
                Redraw();
            }
        }

        public bool isDouble()
        {
            foreach (InputValue i in _inputSequence)
            {
                if (i.inputType == InputType.Double)
                    return true;
            }

            if (GetTextFromList().Contains("."))
                return true;
            return false;
        }

        public bool isValue()
        {
            InputType it = GetLastInputType();

            return isValue(it);
        }

        public bool isValue(InputType it)
        {
            return it == InputType.Int || it == InputType.Double || it == InputType.Rand || 
                   it == InputType.Symbol;
        }

        #endregion

        #region Redraw

        private string changeBitnessHelper(string bin, string dec, int MAX, CalcViews.Programmer.ProgrammerCalcBits currentBits, CalcViews.Programmer.ProgrammerCalcBits prevBits)
        {
            string retVal = string.Empty;

            // First we'll resize the bin string to have the right number of bits
            // per the prevBits value
            int prevMax = (prevBits == CalcViews.Programmer.ProgrammerCalcBits.Qword) ? LONG :
                (prevBits == CalcViews.Programmer.ProgrammerCalcBits.Dword) ? INT :
                (prevBits == CalcViews.Programmer.ProgrammerCalcBits.Word) ? SHORT : BYTE;

            int count = prevMax - bin.Length;
            for (int i = 0; i < count; i++)
                bin = "0" + bin;

            if (bin.StartsWith("1"))
            {
                count = MAX - bin.Length;
                string tempExp = string.Empty;

                for (int i = 0; i < count; i++)
                    tempExp += "1";

                return (currentView as CalcViews.Programmer).bin2Dec(tempExp + bin);
            }

            return dec;
        }

        public void ChangeBitness(CalcViews.Programmer.ProgrammerCalcBits prevBits)
        {
            if (!_mainPage.isProgrammer())
            {
                _mainPage.initCalcError();
                return;
            }

            string dec = Get();
            string bin = CalcViews.Programmer.dec2Long(dec,2);

            // next if the user has chosen a specific bitness
            if ((currentView as CalcViews.Programmer).currentBits == CalcViews.Programmer.ProgrammerCalcBits.Qword)
            {
                // ASSUME we're comming from a lower order bit
                dec = changeBitnessHelper(bin, dec, LONG, (currentView as CalcViews.Programmer).currentBits, prevBits);
            }
            else if ((currentView as CalcViews.Programmer).currentBits == CalcViews.Programmer.ProgrammerCalcBits.Dword)
            {
                // if we're going down in bitness
                if (bin.Length > INT)
                {
                    bin = bin.Substring(bin.Length - INT);
                    dec = (currentView as CalcViews.Programmer).bin2Dec(bin);
                }

                // if we're going up in bitness
                else
                    dec = changeBitnessHelper(bin, dec, INT, (currentView as CalcViews.Programmer).currentBits, prevBits);
            }
            else if ((currentView as CalcViews.Programmer).currentBits == CalcViews.Programmer.ProgrammerCalcBits.Word)
            {
                // if we're going down in bitness
                if (bin.Length > SHORT)
                {
                    bin = bin.Substring(bin.Length - SHORT);
                    dec = (currentView as CalcViews.Programmer).bin2Dec(bin);
                }

                // if we're going up in bitness
                else
                    dec = changeBitnessHelper(bin, dec, SHORT, (currentView as CalcViews.Programmer).currentBits, prevBits);
            }
            else if ((currentView as CalcViews.Programmer).currentBits == CalcViews.Programmer.ProgrammerCalcBits.Byte)
            {
                // if we're going down in bitness
                if (bin.Length > BYTE)
                {
                    bin = bin.Substring(bin.Length - BYTE);
                    //dec = mainPage.bin2Dec(bin);
                }

                // can't go smaller than byte
                dec = (currentView as CalcViews.Programmer).bin2Dec(bin);
            }

            Set(dec, GetLastInputType(), true);
        }

        public void Redraw()
        {
            // TBD : BINARY
            _window.Text = GetTextFromListForWindow();

            if (_isMainWindow && _mainPage.isProgrammer())
            {
                List<int> binArray = GetBinary();

                (currentView as CalcViews.Programmer).Redraw(binArray);
            }
        }

        #endregion
    }
}
