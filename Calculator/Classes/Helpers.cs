using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace Calculator
{
    public static class Helpers
    {
        #region Helpers

        public static bool isDouble(string value)
        {
            if (value.Contains("."))
                return true;
            return false;
        }

        public static void equalsKeyFocus(Button equalKey, Page p)
        {
            equalKey.Dispatcher.Invoke(CoreDispatcherPriority.High, (s, a) =>
            {
                equalKey.Focus(Windows.UI.Xaml.FocusState.Pointer);
            }, p, null);
        }

        public static void cleanUpZeros(TextWindow window)
        {
            string temp = window.Get();
            if (temp.Length > 1 && temp[0] == '0' && temp[1] != '.')
            {
                window.RemoveFirst(false);
            }
        }

        // using PEMDAS
        // return : -x currentOp < prevOP
        //           0 currentOp == prevOp
        //           y currentOp > prevOp
        public static int compareOPs(string currentOP, string prevOP)
        {
            int returnValue = determineOPValue(prevOP) - determineOPValue(currentOP);
            return returnValue;
        }

        private static int determineOPValue(string op)
        {
            switch (op)
            {
                // Hack this is for the initial case where there is nothing previous
                case "":
                    return 30;
                case "(":
                case ")":
                    return 20;
                case "^":
                    return 15;
                case "*":
                case "/":
                case "÷":
                case "×":

                // logical cases
                case "&":
                case "|":
                case "⊕":
                    return 10;
                case "+":
                case "-":
                    return 5;
                default:
                    return -1;
            }
        }

        public static string GetTextFromObject(object sender)
        {
            try
            {
                return (((sender as Button).Content as Viewbox).Child as TextBlock).Text;
            }
            catch
            {
                try
                {
                    return (((sender as ToggleButton).Content as Viewbox).Child as TextBlock).Text;
                }
                catch { }
            }
            return string.Empty;
        }

        public static double convertToRadians(double deg)
        {
            return deg * (Math.PI / 180);
        }

        public static double convertToDegrees(double rad)
        {
            return rad * (180 / Math.PI);
        }

        #endregion
    }
}
