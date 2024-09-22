// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using Stride.Core.Assets.Presentation.ViewModels;

namespace Stride.Core.Assets.Presentation.Services;

// FIXME xplat-editor this interface and its implementations are temporary until we figure out better patterns for modifying assets without having too much of the logic on the non-editor assemblies.
public interface IAssetViewModelService
{
    // TODO API will change
    IEnumerable<MenuCommandInfo> GetCommandsForAsset<TAsset>() where TAsset : Asset; // FIXME xplat-editor or AssetViewModel?

    void RefreshCommands();

    bool RenameAsset(AssetViewModel asset, string newName) => newName != asset.Name; // FIXME xplat-editor see notes on Rename/UpdateUrl/MoveAsset
}
