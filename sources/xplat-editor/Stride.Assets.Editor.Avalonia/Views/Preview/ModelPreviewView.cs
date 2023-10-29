// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using Stride.Assets.Editor.Preview;
using Stride.Editor.Annotations;
using Stride.Editor.Avalonia.Preview.Views;

namespace Stride.Assets.Editor.Avalonia.Views.Preview;

[AssetPreviewView<ModelPreview>]
[AssetPreviewView<PrefabPreview>]
[AssetPreviewView<PrefabModelPreview>]
[AssetPreviewView<ProceduralModelPreview>]
[AssetPreviewView<SpriteStudioSheetPreview>]
public class ModelPreviewView : StridePreviewView
{
}
