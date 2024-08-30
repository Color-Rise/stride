// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using Stride.Core.Presentation.Services;

namespace Stride.Core.Presentation.ViewModels;

public interface IDispatcherViewModel
{
    /// <summary>
    /// Gets a dispatcher that is capable of executing code in the UI thread.
    /// </summary>
    IDispatcherService Dispatcher { get; }
}
