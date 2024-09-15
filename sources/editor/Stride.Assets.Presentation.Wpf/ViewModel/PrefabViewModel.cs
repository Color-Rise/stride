// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using Stride.Assets.Entities;
using Stride.Core.Assets.Editor.Annotations;
using Stride.Core.Assets.Editor.ViewModel;
using Stride.Core.Annotations;
using Stride.Core.Assets.Presentation.ViewModels;
using Stride.Core.Quantum;

namespace Stride.Assets.Presentation.ViewModel
{
    [AssetViewModel<PrefabAsset>]
    public class PrefabViewModel : EntityHierarchyViewModel, IAssetViewModel<PrefabAsset>
    {
        public PrefabViewModel([NotNull] AssetViewModelConstructionParameters parameters)
            : base(parameters)
        {
            
        }

        /// <inheritdoc />
        public new PrefabAsset Asset => (PrefabAsset)base.Asset;

        protected override IObjectNode GetPropertiesRootNode()
        {
            // We don't use CanProvideProperties because we still want the button to open in editor. But we don't want to display any property directly
            return null;
        }
    }
}
