// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using Stride.Core.Presentation.Commands;
using Stride.Core.Presentation.ViewModels;

namespace Stride.Core.Assets.Presentation.Services;

// FIXME xplat-editor could be a generic command wrapper and moved to Core.Presentation/Commands
public class MenuCommandInfo : DispatcherViewModel
{
    private string? gesture;
    private string? displayName;
    private object? icon;
    private string? tooltip;

    public MenuCommandInfo(IViewModelServiceProvider serviceProvider, ICommandBase command)
        : base(serviceProvider)
    {
        Command = command;
    }

    /// <summary>
    /// Gets the command.
    /// </summary>
    public ICommandBase Command { get; }

    /// <summary>
    /// Gets or sets the name that will be displayed in the UI.
    /// </summary>
    public string? DisplayName
    {
        get => displayName;
        set => SetValue(ref displayName, value);
    }

    /// <summary>
    /// Gets or sets the gesture text associated with this command.
    /// </summary>
    public string? Gesture
    {
        get => gesture;
        set => SetValue(ref gesture, value);
    }

    /// <summary>
    /// Gets or sets the icon that appears in a menu.
    /// </summary>
    public object? Icon
    {
        get => icon;
        set => SetValue(ref icon, value);
    }

    /// <summary>
    /// Gets or sets the tooltip text that is shown when the governing UI control is hovered.
    /// </summary>
    public string? Tooltip
    {
        get => tooltip;
        set => SetValue(ref tooltip, value);
    }
}
