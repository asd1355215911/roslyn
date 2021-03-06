﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roslyn.Editor.InteractiveWindow.Commands
{
    public static class InteractiveWindowCommandExtensions
    {
        /// <summary>
        /// Gets the IInteractiveWindowCommands instance for the current interactive window if one is defined.
        /// 
        /// Returns null if the interactive commands have not been created for this window.
        /// </summary>
        public static IInteractiveWindowCommands GetInteractiveCommands(this IInteractiveWindow window)
        {
            IInteractiveWindowCommands commands;
            if (window.Properties.TryGetProperty(typeof(IInteractiveWindowCommands), out commands))
            {
                return commands;
            }

            return null;
        }
    }
}
