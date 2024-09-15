// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using Stride.Assets.SpriteFont;
using Stride.Core;
using Stride.Core.Assets.Editor.Annotations;
using Stride.Core.Assets.Editor.ViewModel;
using Stride.Core.Assets.Presentation.ViewModels;

namespace Stride.Assets.Presentation.ViewModel
{
    [AssetViewModel<SpriteFontAsset>]
    public class SpriteFontViewModel : AssetViewModel<SpriteFontAsset>
    {
        /// <summary>
        /// The name of the category for font-related properties. Must match the category name in the <see cref="DisplayAttribute"/> of these properties.
        /// </summary>
        public const string FontCategory = "Font";
        /// <summary>
        /// The name of the category for character-related properties. Must match the category name in the <see cref="DisplayAttribute"/> of these properties.
        /// </summary>
        public const string CharactersCategory = "Characters";

        public SpriteFontViewModel(AssetViewModelConstructionParameters parameters)
            : base(parameters)
        {
        }
    }
}
