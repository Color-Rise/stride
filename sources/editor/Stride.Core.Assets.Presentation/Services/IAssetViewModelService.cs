// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

namespace Stride.Core.Assets.Presentation.Services;

public interface IAssetViewModelService
{
    // TODO API will change
    IEnumerable<MenuCommandInfo> GetCommandsForAsset<TAsset>() where TAsset : Asset; // FIXME xplat-editor or AssetViewModel?
}