using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Calculator
{
    public interface CalculatorView
    {
        #region Equals key
        Button GetEqualsKey();
        #endregion

        #region Metro Memory
        void Add_Memory_Click(object sender, RoutedEventArgs e);
        void Clear_All_Memory_Click(object sender, RoutedEventArgs e);
        void Metro_Memory_Add_Button_Click(object sender, RoutedEventArgs e);
        void Metro_Memory_Restore_Button_Click(object sender, RoutedEventArgs e);
        #endregion

        #region Legacy Memroy
        void Memory_Add_Button_Click(object sender, RoutedEventArgs e);
        void Memory_Subtract_Button_Click(object sender, RoutedEventArgs e);
        void Memory_Restore_Button_Click(object sender, RoutedEventArgs e);
        void Memory_Clear_Button_Click(object sender, RoutedEventArgs e);
        #endregion

        #region Values/Symbol/Dot
        void Dot_Button_Click(object sender, RoutedEventArgs e);
        void Symbol_Button_Click(object sender, RoutedEventArgs e);
        void Value_Button_Click(object sender, RoutedEventArgs e);
        void Rand_Button_Click(object sender, RoutedEventArgs e);
        #endregion

        #region Negation
        void Negate_Button_Click(object sender, RoutedEventArgs e);
        #endregion

        #region Paran
        void Paran_Button_Click(object sender, RoutedEventArgs e);
        #endregion

        #region Operators
        void Operator_Single_Button_Click(object sender, RoutedEventArgs e);
        void Operator_Double_Button_Click(object sender, RoutedEventArgs e);
        #endregion

        #region Delete
        void Delete_Last_Button_Click(object sender, RoutedEventArgs e);
        void Clear_Calc_Button_Click(object sender, RoutedEventArgs e);
        void Clear_Error_Button_Click(object sender, RoutedEventArgs e);
        #endregion

        #region Equals
        void Equal_Button_Click(object sender, RoutedEventArgs e);
        #endregion
    }
}
