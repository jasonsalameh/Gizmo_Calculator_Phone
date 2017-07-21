using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculator
{
    public class InputValue
    {
        public TextWindow.InputType inputType { get; set; }
        public string inputData { get; set; }
        public bool hasParan { get; set; }

        public InputValue(TextWindow.InputType inputType, string inputData, bool hasParan)
        {
            this.inputData = inputData;
            this.inputType = inputType;
            this.hasParan = hasParan;
        }
    }
}
