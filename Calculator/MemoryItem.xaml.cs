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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Calculator
{
    public sealed partial class MemoryItem : UserControl
    {
        private MainPage _page;
        private bool _wasJustRemoved;
        public bool isProgrammer { get; set; }

        // The textual value which is displayed relative to the mode (Dec, Oct, Bin, Hex)
        public string Text
        {
            get { return MemoryValue.Text; }
            set { MemoryValue.Text = value; }
        }

        // the decimal representation for the actual value
        private string _value;
        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }

        // the type of text which is displayed (Dec, Oct, Bin, Hex)
        private CalcViews.Programmer.ProgrammerCalcState _type;
        public CalcViews.Programmer.ProgrammerCalcState Type
        {
            get { return _type; }
            set
            {
                MemoryType.Text = value.ToString().Substring(0,3);
                _type = value;
            }
        }

        // used only during loading state
        public CalcViews.Programmer.ProgrammerCalcState StateType
        {
            get { return _type; }
            set { _type = value; }
        }

        public MemoryItem()
        {
            this.InitializeComponent();
            _wasJustRemoved = false;
        }

        public void InitializeWindow(MainPage page)
        {
            this._page = page;

            isProgrammer = true;
            _type = CalcViews.Programmer.ProgrammerCalcState.Decimal;

        }

        
        private void Remove_Memory_Click(object sender, RoutedEventArgs e)
        {
            _page.Remove_Memory_Click(this);
            _wasJustRemoved = true;
        }

        private void Use_Memory_Click(object sender, TappedRoutedEventArgs e)
        {
            if(!_wasJustRemoved)
                _page.Use_Memory_Click(this);
        }
    }
}
