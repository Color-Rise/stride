// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using Stride.Core.Annotations;
using Stride.Core.Assets.Analysis;
using Stride.Core.Assets.Editor.ViewModel;
using Stride.Core.Assets.Quantum;
using Stride.Core.Presentation.Collections;
using Stride.Core.Presentation.ViewModels;

namespace Stride.Core.Assets.Presentation.ViewModels
{
    /// <summary>
    /// Interface for sessions.
    /// </summary>
    public interface ISessionViewModel : IDispatcherViewModel, IViewModelBase
    {
        /// <summary>
        /// Gets all assets contained in this session.
        /// </summary>
        /// <remarks>
        /// Some assets in the session might not be accessible to some other assets/packages if they are located in another package that is not a dependency
        /// to the asset/package. To safely retrieve all assets accessible from a specific package, use <see cref="PackageViewModel.AllAssets"/>.
        /// </remarks>
        [NotNull]
        IEnumerable<AssetViewModel> AllAssets { get; }

        [NotNull]
        IEnumerable<PackageViewModel> AllPackages { get; }

        [NotNull]
        AssetNodeContainer AssetNodeContainer { get; }

        /// <summary>
        /// Gets the current active project for build/startup operations.
        /// </summary>
        [CanBeNull]
        ProjectViewModel CurrentProject { get; }

        /// <summary>
        /// Gets the dependency manager associated to this session.
        /// </summary>
        IAssetDependencyManager DependencyManager { get; }

        [NotNull]
        AssetPropertyGraphContainer GraphContainer { get; }

        IObservableCollection<PackageViewModel> LocalPackages { get; }

        [NotNull]
        IReadOnlyDictionary<string, PackageCategoryViewModel> PackageCategories { get; }

        IObservableCollection<PackageViewModel> StorePackages { get; }

        /// <summary>
        /// Raised when some assets are modified.
        /// </summary>
        event EventHandler<AssetChangedEventArgs> AssetPropertiesChanged;

        /// <summary>
        /// Raised when the session state changed (e.g. current package).
        /// </summary>
        event EventHandler<SessionStateChangedEventArgs> SessionStateChanged;

        /// <summary>
        /// Gets an <see cref="AssetViewModel"/> instance of the asset which as the given identifier, if available.
        /// </summary>
        /// <param name="id">The identifier of the asset to look for.</param>
        /// <returns>An <see cref="AssetViewModel"/> that matches the given identifier if available. Otherwise, <c>null</c>.</returns>
        [CanBeNull]
        AssetViewModel GetAssetById(AssetId id);

        /// <summary>
        /// Register an asset so it can be found using the <see cref="GetAssetById"/> method. This method is intended to be invoked only by <see cref="AssetViewModel"/>.
        /// </summary>
        /// <param name="asset">The asset to register.</param>
        void RegisterAsset(AssetViewModel asset);

        /// <summary>
        /// Unregister an asset previously registered with <see cref="RegisterAsset"/>. This method is intended to be invoked only by <see cref="AssetViewModel"/>.
        /// </summary>
        /// <param name="asset">The asset to register.</param>
        void UnregisterAsset(AssetViewModel asset);
    }
}
