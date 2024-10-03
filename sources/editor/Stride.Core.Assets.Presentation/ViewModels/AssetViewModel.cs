// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System.Collections.Specialized;
using System.Reflection;
using Stride.Core.Assets.Presentation.Components.Properties;
using Stride.Core.Assets.Presentation.Quantum;
using Stride.Core.Assets.Presentation.Services;
using Stride.Core.Assets.Quantum;
using Stride.Core.Presentation.Collections;
using Stride.Core.Presentation.Dirtiables;
using Stride.Core.Presentation.Quantum;
using Stride.Core.Quantum;

namespace Stride.Core.Assets.Presentation.ViewModels;

/// <summary>
/// An interface representing the view model of an <see cref="Asset"/>.
/// </summary>
/// <typeparam name="TAsset">The type of asset represented by this view model.</typeparam>
public interface IAssetViewModel<out TAsset>
    where TAsset : Asset
{
    /// <summary>
    /// The asset object related to this view model.
    /// </summary>
    TAsset Asset { get; }
}

/// <summary>
/// A generic version of the <see cref="AssetViewModel"/> class that allows to access directly the proper type of asset represented by this view model.
/// </summary>
/// <typeparam name="TAsset">The type of asset represented by this view model.</typeparam>
public class AssetViewModel<TAsset> : AssetViewModel, IAssetViewModel<TAsset>
    where TAsset : Asset
{
    public AssetViewModel(ConstructorParameters parameters)
        : base(parameters)
    {
        assetCommands.AddRange(ServiceProvider.Get<IAssetViewModelService>().GetCommandsForAsset<TAsset>());
    }

    /// <inheritdoc cref="IAssetViewModel{T}.Asset" />
    public override TAsset Asset => (TAsset)base.Asset;
}

public abstract class AssetViewModel : SessionObjectViewModel, IChildViewModel, IAssetPropertyProviderViewModel
{
    private AssetItem assetItem;
    private DirectoryBaseViewModel directory;
    private string name;
    private ThumbnailData? thumbnailData;

    // ReSharper disable once InconsistentNaming
    protected readonly ObservableList<MenuCommandInfo> assetCommands = [];

    protected AssetViewModel(ConstructorParameters parameters)
        : base(parameters.Directory.Session)
    {
        Initializing = true;

        assetItem = parameters.AssetItem;
        directory = parameters.Directory;
        var forcedRoot = AssetType.GetCustomAttribute<AssetDescriptionAttribute>()?.AlwaysMarkAsRoot ?? false;
        Dependencies = new AssetDependenciesViewModel(this, forcedRoot);
        Sources = new AssetSourcesViewModel(this);

        InitialUndelete(parameters.CanUndoRedoCreation);

        name = Path.GetFileName(assetItem.Location);

        Tags.AddRange(assetItem.Asset.Tags);
        RegisterMemberCollectionForActionStack(nameof(Tags), Tags);
        Tags.CollectionChanged += TagsCollectionChanged;

        PropertyGraph = Session.GraphContainer.TryGetGraph(assetItem.Id);
        if (PropertyGraph is not null)
        {
            PropertyGraph.BaseContentChanged += BaseContentChanged;
            PropertyGraph.Changed += AssetPropertyChanged;
            PropertyGraph.ItemChanged += AssetPropertyChanged;
        }

        Initializing = false;
    }

    public virtual Asset Asset => AssetItem.Asset;

    public AssetItem AssetItem
    {
        get => assetItem;
        set => SetValueUncancellable(ref assetItem, value);
    }

    public IAssetObjectNode? AssetRootNode => PropertyGraph?.RootNode;

    public Type AssetType => AssetItem.Asset.GetType();

    public AssetId Id => AssetItem.Id;

    public IReadOnlyObservableCollection<MenuCommandInfo> AssetCommands => assetCommands;

    /// <summary>
    /// Gets whether the properties of this asset can be edited.
    /// </summary>
    public override bool IsEditable => Directory?.Package?.IsEditable ?? false;

    public DirectoryBaseViewModel Directory
    {
        get => directory;
        private set => SetValue(ref directory, value);
    }

    public override IEnumerable<IDirtiable> Dirtiables => base.Dirtiables.Concat(Directory.Dirtiables);

    /// <summary>
    /// Gets the dependencies of this asset.
    /// </summary>
    public AssetDependenciesViewModel Dependencies { get; }

    /// <inheritdoc/>
    public override string Name
    {
        get => name;
        set => SetValue(ServiceProvider.Get<IAssetViewModelService>().RenameAsset(this, value), () => name = value);
    }

    public AssetPropertyGraph? PropertyGraph { get; }

    /// <summary>
    /// Gets the view model of the sources of this asset.
    /// </summary>
    public AssetSourcesViewModel Sources { get; }

    /// <summary>
    /// Gets or sets the collection of tags associated to this asset.
    /// </summary>
    public ObservableList<string> Tags { get; } = [];

    /// <summary>
    /// The <see cref="ThumbnailData"/> associated to this <see cref="AssetViewModel"/>.
    /// </summary>
    public ThumbnailData? ThumbnailData
    {
        get => thumbnailData;
        set => SetValueUncancellable(ref thumbnailData, value);
    }

    /// <summary>
    /// Gets the display name of the type of this asset.
    /// </summary>
    public override string TypeDisplayName { get { var desc = DisplayAttribute.GetDisplay(AssetType); return desc != null ? desc.Name : AssetType.Name; } }

    /// <summary>
    /// Gets the url of this asset.
    /// </summary>
    public string Url => AssetItem.Location;

    protected bool Initializing { get; }

    protected Package Package => Directory.Package.Package;

    public override void Destroy()
    {
        EnsureNotDestroyed(nameof(AssetViewModel));
        Session.GraphContainer.UnregisterGraph(assetItem.Id);
        PropertyGraph?.Dispose();
        base.Destroy();
    }

    /// <summary>
    /// Initializes this asset. This method is guaranteed to be called once every other assets are loaded in the session.
    /// </summary>
    /// <remarks>
    /// Inheriting classes should override it when necessary, provided that they also call the base implementation.
    /// </remarks>
    public virtual void Initialize()
    {
        using var transaction = UndoRedoService?.CreateTransaction();
        PropertyGraph?.Initialize();
        UndoRedoService?.SetName(transaction!, $"Reconcile {Url} with its archetypes");
    }

    protected virtual GraphNodePath GetPathToPropertiesRootNode()
    {
        return new GraphNodePath(AssetRootNode);
    }

    protected virtual IObjectNode? GetPropertiesRootNode()
    {
        return AssetRootNode;
    }

    [Obsolete]
    protected virtual void OnAssetPropertyChanged(string? propertyName, IGraphNode node, NodeIndex index, object oldValue, object newValue)
    {
        ServiceProvider.Get<IAssetViewModelService>().RefreshCommands();
    }

    protected override void OnDirtyFlagSet()
    {
        // We write the dirty flag of the asset item even if it has not changed,
        // since it triggers some processes we want to do at each modification.
        assetItem.IsDirty = IsDirty;
    }

    protected virtual bool ShouldConstructPropertyItem(IObjectNode collection, NodeIndex index) => true;

    protected virtual bool ShouldConstructPropertyMember(IMemberNode member) => true;

    /// <inheritdoc/>
    protected override void UpdateIsDeletedStatus()
    {
        if (IsDeleted)
        {
            Package.Assets.Remove(AssetItem);
            Session.UnregisterAsset(this);
            Directory.Package.DeletedAssetsInternal.Add(this);
            if (PropertyGraph != null)
            {
                Session.GraphContainer.UnregisterGraph(Id);
            }
        }
        else
        {
            Package.Assets.Add(AssetItem);
            Session.RegisterAsset(this);
            Directory.Package.DeletedAssetsInternal.Remove(this);
            if (!Initializing && PropertyGraph != null)
            {
                Session.GraphContainer.RegisterGraph(PropertyGraph);
            }
        }
        AssetItem.IsDeleted = IsDeleted;
        Session.SourceTracker?.UpdateAssetStatus(this);
    }

    public static HashSet<AssetViewModel> ComputeRecursiveReferencerAssets(IEnumerable<AssetViewModel> assets)
    {
        var result = new HashSet<AssetViewModel>(assets.SelectMany(x => x.Dependencies.RecursiveReferencerAssets));
        return result;
    }

    public static HashSet<AssetViewModel> ComputeRecursiveReferencedAssets(IEnumerable<AssetViewModel> assets)
    {
        var result = new HashSet<AssetViewModel>(assets.SelectMany(x => x.Dependencies.RecursiveReferencedAssets));
        return result;
    }

    // FIXME xplat-editor revisit this. Should it be implemented here or in an editor class?
    private void AssetPropertyChanged(object? sender, INodeChangeEventArgs e)
    {
        // Ignore asset property change if we are fixing up assets.
        if (Session.IsInFixupAssetContext)
            return;

        var index = (e as ItemChangeEventArgs)?.Index ?? NodeIndex.Empty;
        var assetNodeChange = (IAssetNodeChangeEventArgs)e;
        var node = (IAssetNode)e.Node;
        var memberName = (node as IMemberNode)?.Name;
        if (UndoRedoService?.UndoRedoInProgress == false)
        {
            // Don't create action items if the change comes from the Base
            if (PropertyGraph?.UpdatingPropertyFromBase == false)
            {
                var overrideChange = new AssetContentValueChangeOperation(node, e.ChangeType, index, e.OldValue, e.NewValue, assetNodeChange.PreviousOverride, assetNodeChange.NewOverride, assetNodeChange.ItemId, Dirtiables);
                UndoRedoService.PushOperation(overrideChange);
            }
        }

        OnAssetPropertyChanged(memberName, node, index, e.OldValue, e.NewValue);
    }

    private void BaseContentChanged(INodeChangeEventArgs e, IGraphNode node)
    {
        // Ignore base change if we are fixing up assets.
        if (Session.IsInFixupAssetContext)
            return;

        if (UndoRedoService?.UndoRedoInProgress == false)
        {
            // Ensure this asset will be marked as dirty
            UndoRedoService.PushOperation(new EmptyDirtyingOperation(Dirtiables));
        }
    }

    private void TagsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            throw new InvalidOperationException("Reset is not supported on the tag collection.");
        }
        if (e.OldItems != null)
        {
            foreach (string oldItem in e.OldItems)
            {
                assetItem.Asset.Tags.Remove(oldItem);
            }
        }
        if (e.NewItems != null)
        {
            foreach (string newItem in e.NewItems)
            {
                assetItem.Asset.Tags.Add(newItem);
            }
        }
    }

    IChildViewModel IChildViewModel.GetParent()
    {
        return Directory;
    }

    string IChildViewModel.GetName()
    {
        return Name;
    }

    AssetViewModel IAssetPropertyProviderViewModel.RelatedAsset => this;

    bool IPropertyProviderViewModel.CanProvidePropertiesViewModel => !IsDeleted && IsEditable;

    GraphNodePath IAssetPropertyProviderViewModel.GetAbsolutePathToRootNode() => GetPathToPropertiesRootNode();

    IObjectNode? IPropertyProviderViewModel.GetRootNode() => GetPropertiesRootNode();

    bool IPropertyProviderViewModel.ShouldConstructItem(IObjectNode collection, NodeIndex index) => ShouldConstructPropertyItem(collection, index);

    bool IPropertyProviderViewModel.ShouldConstructMember(IMemberNode member) => ShouldConstructPropertyMember(member);

    public readonly struct ConstructorParameters
    {
        public ConstructorParameters(AssetItem assetItem, DirectoryBaseViewModel directory, bool canUndoRedoCreation)
        {
            if (directory.Package is null) throw new ArgumentException("The provided directory must be in a project when creating an asset.");

            AssetItem = assetItem;
            CanUndoRedoCreation = canUndoRedoCreation;
            Directory = directory;
        }

        /// <summary>
        /// Gets the <see cref="AssetItem"/> instance representing the asset to construct.
        /// </summary>
        internal readonly AssetItem AssetItem;

        /// <summary>
        /// Gets whether the creation of this asset can be undone/redone.
        /// </summary>
        internal readonly bool CanUndoRedoCreation;

        /// <summary>
        /// Gets the directory containing the asset to construct.
        /// </summary>
        internal readonly DirectoryBaseViewModel Directory;
    }
}
