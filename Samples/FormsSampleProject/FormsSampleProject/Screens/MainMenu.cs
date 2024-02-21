using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using FlatRedBall;
using FlatRedBall.Input;
using FlatRedBall.Instructions;
using FlatRedBall.AI.Pathfinding;
using FlatRedBall.Graphics.Animation;
using FlatRedBall.Gui;
using FlatRedBall.Math;
using FlatRedBall.Math.Geometry;
using FlatRedBall.Localization;
using Microsoft.Xna.Framework;
using FlatRedBall.Forms.Controls.Popups;
using FormsSampleProject.ViewModels;
using FlatRedBall.Forms.Controls.Games;




namespace FormsSampleProject.Screens
{
    public partial class MainMenu
    {
        MainMenuViewModel ViewModel;
        void CustomInitialize()
        {
            ViewModel = new MainMenuViewModel();
            ViewModel.ComboBoxItems.Add("Combo Box Item 1");
            ViewModel.ComboBoxItems.Add("Combo Box Item 2");
            ViewModel.ComboBoxItems.Add("Combo Box Item 3");
            ViewModel.ComboBoxItems.Add("Combo Box Item 4");
            ViewModel.ComboBoxItems.Add("Combo Box Item 5");

            Forms.ComboBoxInstance.SetBinding(
                nameof(Forms.ComboBoxInstance.Items),
                nameof(ViewModel.ComboBoxItems));

            Forms.ListBoxInstance.SetBinding(
                nameof(Forms.ListBoxInstance.Items),
                nameof(ViewModel.ListBoxItems));

            Forms.BindingContext = ViewModel;
            Forms.ButtonStandardInstance.Click += HandleButtonClicked;
            Forms.AddItemButton.Click += HandleAddItemClicked;

            Forms.ShowDialogButton.Click += HandleShowDialogButtonClicked;


        }

        private async void HandleShowDialogButtonClicked(object sender, EventArgs e)
        {
            var dialog = new DialogBox();
            dialog.IsFocused = true;
            var dialogVisual = dialog.Visual;
            dialogVisual.XUnits = Gum.Converters.GeneralUnitType.PixelsFromMiddle;
            dialogVisual.XOrigin = RenderingLibrary.Graphics.HorizontalAlignment.Center;
            dialogVisual.Y = -20;
            dialogVisual.YUnits = Gum.Converters.GeneralUnitType.PixelsFromLarge;
            dialogVisual.YOrigin = RenderingLibrary.Graphics.VerticalAlignment.Bottom;

            dialog.LettersPerSecond = 60;
            dialog.LettersPerSecond = null;
            //string textToDisplay =
            //    "Now that I've found the [Color=Yellow]ring[/Color], " +
            //    "I can return it back to the [Color=Green]king[/Color]. " +
            //    "I should hurry before [Color=Purple]nightfall[/Color].";

            //string textToDisplay = "0\n1\n[Color=Red]2[/Color]\n3\n4\n5\n6";

            //await dialog.ShowAsync(textToDisplay);



            await dialog.ShowAsync("This is [Color=Orange]some really[/Color] long [Color=Pink]text[/Color]. " +
                "[Color=Purple]We[/Color] want to show long text so that it " +
                "line wraps [Color=Cyan]and[/Color] so that it has [Color=Green]enough[/Color] text to fill an " +
                "[Color=Yellow]entire page[/Color]. The DialogBox control " +
                "should automatically detect if the text is too long for a single page and it should break " +
                "it up into multiple pages. You can advance this dialog by clicking on it with the [Color=Blue]mouse[/Color] or " +
                "by pressing the [Color=Gold]space bar[/Color] on the keyboard.");

            //await dialog.ShowAsync("[Color=Cyan]and[/Color] so that it has [Color=Green]enough[/Color] text to fill an " +
            //    "[Color=Yellow]entire page[/Color]. The DialogBox control " +
            //    "should\n0\n1\n2\n3");

            //await dialog.ShowAsync("and so that it has enough text to fill an \nentire");
            dialog.Visual.RemoveFromManagers();
        }

        private void HandleButtonClicked(object sender, EventArgs e)
        {
            ToastManager.Show(
                $"This is a toast instance.\nThe button was clicked at {DateTime.Now}.");
        }

        private void HandleAddItemClicked(object sender, EventArgs e)
        {
            ViewModel.ListBoxItems.Add($"Item @ {DateTime.Now}");
        }

        void CustomActivity(bool firstTimeCalled)
        {


        }

        void CustomDestroy()
        {


        }

        static void CustomLoadStaticContent(string contentManagerName)
        {


        }

    }
}
