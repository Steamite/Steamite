using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace AbstractControls
{
    [UxmlElement]
    public partial class TextRadioButton : CustomRadioButton
    {
        public string data;

        public TextRadioButton() : base()
        {
            text = "string";
            data = "string";
        }
        public TextRadioButton(string _styleClass, int i, bool _inGroup, string labelText) : base(_styleClass, i, _inGroup)
        {
            text = labelText;
            data = labelText;
        }

    }
}
