// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

namespace Stride.Core.Presentation.ViewModels;

public interface IViewModelBase
{
    /// <summary>
    /// An <see cref="IViewModelServiceProvider"/> that allows to retrieve various service objects.
    /// </summary>
    IViewModelServiceProvider ServiceProvider { get; }
}
