// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System.Collections.Generic;
using Stride.Core.Annotations;
using Stride.Core.Assets.Editor.ViewModel;
using Stride.Core.Assets.Presentation.ViewModels;

namespace Stride.Core.Assets.Editor.Services
{
    public interface IAssetViewModelService
    {
        // TODO API will change
        [NotNull]
        IEnumerable<MenuCommandInfo> GetCommandsForAsset([NotNull] AssetViewModel asset);

        void RefreshCommands();
    }
}
