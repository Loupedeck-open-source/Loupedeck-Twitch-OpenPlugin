namespace Loupedeck.TwitchPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    internal class ActionHelpers
    {
        /// <summary>
        /// Helper function for filling lists
        /// </summary>
        /// <param name="e">state</param>
        /// <param name="controlName">Name of the control</param>
        /// <param name="fillItems">Action to fill LB requests</param>
        /// <returns>True if request was handled</returns>
        public static Boolean FillListBox(ActionEditorListboxItemsRequestedEventArgs e, String controlName, Action fillItems)
        {
            if (!e.ControlName.EqualsNoCase(controlName))
            {
                return false;
            }

            if (e.Items.Count == 0)
            {
                fillItems.Invoke();
            }

            if (e.Items.Count > 0)
            {
                var v_orig = e.ActionEditorState.GetControlValue(controlName);
                //if v_orig points at non-existing element
                var v_new = (String.IsNullOrEmpty(v_orig) || !e.Items.Exists(x => x.Name == v_orig)) ? e.Items.First().Name : v_orig;

                if (v_new != v_orig)
                {
                    //TwitchPlugin.Trace($"Setting new selection for list \"{controlName}\". Old one was \"{v_orig}\" and new one is \"{v_new}\"");
                    e.SetSelectedItemName(v_new);
                }
            }

            return true;
        }

    }
}
