// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Stride.Assets.Presentation;
using Stride.Core.Assets.Editor.Services;
using Stride.Core.Assets.Presentation;
using Stride.Core.Presentation.Avalonia.Services;
using Stride.Core.Presentation.ViewModels;
using Stride.GameStudio.Avalonia.Services;
using Stride.GameStudio.Avalonia.ViewModels;
using Stride.GameStudio.Avalonia.Views;

namespace Stride.GameStudio.Avalonia;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        InitializePlugins();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel(InitializeServiceProvider())
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel(InitializeServiceProvider())
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void InitializePlugins()
    {
        AssetsPlugin.RegisterPlugin(typeof(StrideDefaultAssetsPlugin));
    }

    private static IViewModelServiceProvider InitializeServiceProvider()
    {
        var dispatcherService = DispatcherService.Create();
        var services = new object[]
        {
            dispatcherService,
            new PluginService()
        };
        var serviceProvider = new ViewModelServiceProvider(services);
        serviceProvider.RegisterService(new EditorDialogService(serviceProvider));
        return serviceProvider;
    }
}