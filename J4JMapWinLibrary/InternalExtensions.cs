using Microsoft.UI.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Core;

namespace J4JSoftware.J4JMapWinLibrary;

internal class InternalExtensions
{
    public static bool IsControlPressed() =>
        InputKeyboardSource.GetKeyStateForCurrentThread( VirtualKey.Control )
     == CoreVirtualKeyStates.Down;
}
