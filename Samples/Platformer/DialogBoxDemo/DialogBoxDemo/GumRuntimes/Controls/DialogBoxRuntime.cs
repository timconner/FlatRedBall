using System;
using System.Collections.Generic;
using System.Linq;

namespace DialogBoxDemo.GumRuntimes.Controls
{
    public partial class DialogBoxRuntime
    {
        partial void CustomInitialize () 
        {
            X = 5;
            Y = 5;
            Width = 200;
            Height = 50;
            TextInstance.FontSize = 10;
        }
    }
}
