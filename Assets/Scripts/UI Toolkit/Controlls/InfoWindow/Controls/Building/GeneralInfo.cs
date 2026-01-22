using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UIElements;

namespace InfoWindowViews
{
    [UxmlElement] 
    public partial class GeneralInfo : InfoWindowControl
    {
        Label title;
        Label secText;

        public GeneralInfo() 
        {
            Add(title = new() { name = "title"});
            Add(secText = new() { name = "secText"});
        }


        public override void Open(object data)
        {
            switch(data)
            {
                case Pub pub:
                    title.text = "Increses happines of workers who live in the range.";
                    secText.text = $"Range: {pub.Range}";
                    break;
            }
        }
    }
}
