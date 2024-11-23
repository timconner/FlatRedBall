using System;
using System.Collections.Generic;
using System.Linq;
using MultiplayerPlatformerDemo.ViewModels;

namespace MultiplayerPlatformerDemo.GumRuntimes
{
    public partial class IndividualJoinComponentRuntime
    {
        IndividualJoinViewModel ViewModel => BindingContext as IndividualJoinViewModel;
        partial void CustomInitialize()
        {
            this.SetBinding(nameof(this.CurrentJoinCategoryState), nameof(ViewModel.JoinState));
        }
    }
}
