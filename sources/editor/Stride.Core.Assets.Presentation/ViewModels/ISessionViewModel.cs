// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using Stride.Core.Assets.Analysis;
using Stride.Core.Assets.Quantum;
using Stride.Core.Presentation.Collections;
using Stride.Core.Presentation.ViewModels;
using Stride.Core.Translation;

namespace Stride.Core.Assets.Presentation.ViewModels;

public interface ISessionViewModel : IDispatcherViewModel, IViewModelBase
{
    public static string StorePackageCategoryName = Tr._("External packages");
    public static string LocalPackageCategoryName = Tr._("Local packages");

    IEnumerable<AssetViewModel> AllAssets => AllPackages.SelectMany(x => x.Assets);

    IEnumerable<PackageViewModel> AllPackages => PackageCategories.Values.SelectMany(x => x.Content);

    AssetNodeContainer AssetNodeContainer { get; }

    ProjectViewModel? CurrentProject { get; }

    IAssetDependencyManager DependencyManager { get; }

    AssetPropertyGraphContainer GraphContainer { get; }

    bool IsInFixupAssetContext { get; }

    IObservableCollection<PackageViewModel> LocalPackages => PackageCategories[LocalPackageCategoryName].Content;

    IReadOnlyDictionary<string, PackageCategoryViewModel> PackageCategories { get; }

    IAssetSourceTrackerViewModel? SourceTracker { get; }

    IObservableCollection<PackageViewModel> StorePackages => PackageCategories[StorePackageCategoryName].Content;

    /// <summary>
    /// Raised when some assets are modified.
    /// </summary>
    event EventHandler<AssetChangedEventArgs>? AssetPropertiesChanged;

    /// <summary>
    /// Raised when the session state changed (e.g. current package).
    /// </summary>
    event EventHandler<SessionStateChangedEventArgs>? SessionStateChanged;

    /// <summary>
    /// Gets an <see cref="AssetViewModel"/> instance of the asset which as the given identifier, if available.
    /// </summary>
    /// <param name="id">The identifier of the asset to look for.</param>
    /// <returns>An <see cref="AssetViewModel"/> that matches the given identifier if available. Otherwise, <c>null</c>.</returns>
    AssetViewModel? GetAssetById(AssetId id);

    // FIXME xplat-editor: ideally this should be defined and used on the editor-side only
    Type GetAssetViewModelType(AssetItem assetItem);

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
