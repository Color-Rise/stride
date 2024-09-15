// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System.Collections.Generic;
using Stride.Core.Annotations;

namespace Stride.Core.Assets.Presentation.ViewModels
{
    public interface IAssetViewModelService
    {
        // TODO API will change
        [NotNull]
        IEnumerable<MenuCommandInfo> GetCommandsForAsset([NotNull] AssetViewModel asset);

        void RefreshCommands();
    }
}
