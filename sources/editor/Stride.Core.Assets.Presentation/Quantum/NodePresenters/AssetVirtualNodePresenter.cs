// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using Stride.Core.Assets.Presentation.Components.Properties;
using Stride.Core.Assets.Presentation.ViewModels;
using Stride.Core.Assets.Quantum;
using Stride.Core.Presentation.Quantum;
using Stride.Core.Presentation.Quantum.Presenters;
using Stride.Core.Quantum;

namespace Stride.Core.Assets.Presentation.Quantum.NodePresenters;

public class AssetVirtualNodePresenter : VirtualNodePresenter, IAssetNodePresenter
{
    private readonly Func<bool>? hasBase;
    private readonly Func<bool>? isInherited;
    private readonly Func<bool>? isOverridden;

    public AssetVirtualNodePresenter(INodePresenterFactoryInternal factory, IPropertyProviderViewModel? propertyProvider, INodePresenter parent, string name, Type type, int? order, Func<object> getter, Action<object> setter, Func<bool>? hasBase = null, Func<bool>? isInherited = null, Func<bool>? isOverridden = null)
        : base(factory, propertyProvider, parent, name, type, order, getter, setter)
    {
        this.hasBase = hasBase;
        this.isInherited = isInherited;
        this.isOverridden = isOverridden;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            if (AssociatedNode.Node != null)
            {
                ((IAssetNode)AssociatedNode.Node).OverrideChanging -= OnOverrideChanging;
                ((IAssetNode)AssociatedNode.Node).OverrideChanged -= OnOverrideChanged;
            }
        }
    }

    public new IAssetNodePresenter this[string childName] => (IAssetNodePresenter)base[childName];

    public bool HasBase => hasBase?.Invoke() ?? (AssociatedNode.Node as IAssetNode)?.BaseNode != null;

    public bool IsInherited => isInherited?.Invoke() ?? IsAssociatedNodeInherited();

    public bool IsOverridden => isOverridden?.Invoke() ?? IsAssociatedNodeOverridden();

    public AssetViewModel? Asset => (PropertyProvider as IAssetPropertyProviderViewModel)?.RelatedAsset;

    public new AssetNodePresenterFactory Factory => (AssetNodePresenterFactory)base.Factory;

    public event EventHandler<EventArgs>? OverrideChanging;

    public event EventHandler<EventArgs>? OverrideChanged;

    public override void RegisterAssociatedNode(NodeAccessor associatedNodeAccessor)
    {
        base.RegisterAssociatedNode(associatedNodeAccessor);
        ((IAssetNode)AssociatedNode.Node).OverrideChanging += OnOverrideChanging;
        ((IAssetNode)AssociatedNode.Node).OverrideChanged += OnOverrideChanged;
    }

    public bool IsObjectReference(object value)
    {
        return AssociatedNode.Node != null && (Asset?.PropertyGraph?.Definition.IsObjectReference(AssociatedNode, value) ?? false);
    }

    public void ResetOverride()
    {
        // TODO: for now we cannot reset override if we don't have an AssociatedNode. We could provide a delegate via the constructor for custom reset.
        var memberNode = AssociatedNode.Node as IAssetMemberNode;
        memberNode?.ResetOverrideRecursively();

        var objectNode = AssociatedNode.Node as IAssetObjectNode;
        objectNode?.ResetOverrideRecursively(AssociatedNode.Index);
    }

    private bool IsAssociatedNodeInherited()
    {
        if (AssociatedNode.Node is IAssetMemberNode memberNode)
        {
            return memberNode.IsContentInherited();
        }
        if (AssociatedNode.Node is IAssetObjectNode objectNode)
        {
            return objectNode.IsItemInherited(AssociatedNode.Index);
        }
        return false;
    }

    private bool IsAssociatedNodeOverridden()
    {
        if (AssociatedNode.Node is IAssetMemberNode memberNode)
        {
            return memberNode.IsContentOverridden();
        }
        if (AssociatedNode.Node is IAssetObjectNode objectNode)
        {
            return objectNode.IsItemOverridden(AssociatedNode.Index);
        }
        return false;
    }

    private void OnOverrideChanging(object? sender, EventArgs e)
    {
        OverrideChanging?.Invoke(sender, e);
    }

    private void OnOverrideChanged(object? sender, EventArgs e)
    {
        OverrideChanged?.Invoke(sender, e);
    }
}
