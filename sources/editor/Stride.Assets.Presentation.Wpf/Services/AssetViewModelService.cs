// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Stride.Assets.Models;
using Stride.Assets.Presentation.Templates;
using Stride.Assets.Presentation.ViewModel;
using Stride.Assets.SpriteFont;
using Stride.Core;
using Stride.Core.Annotations;
using Stride.Core.Assets;
using Stride.Core.Assets.Editor.Components.TemplateDescriptions.ViewModels;
using Stride.Core.Assets.Editor.Services;
using Stride.Core.Assets.Editor.ViewModel;
using Stride.Core.Assets.Presentation.ViewModels;
using Stride.Core.Assets.Quantum;
using Stride.Core.Assets.Templates;
using Stride.Core.Extensions;
using Stride.Core.IO;
using Stride.Core.Presentation.Commands;
using Stride.Core.Presentation.Dirtiables;
using Stride.Core.Presentation.Services;
using Stride.Core.Presentation.ViewModels;
using Stride.Core.Presentation.Windows;
using Stride.Core.Translation;
using Stride.Graphics;
using Stride.Rendering;

using Image = System.Windows.Controls.Image;

namespace Stride.Assets.Presentation.Services
{
    // FIXME xplat-editor this is a dummy implementation for the Wpf version
    // It should be properly rewritten to be compatible with plugins
    public partial class AssetViewModelService : IAssetViewModelService
    {
        private readonly IViewModelServiceProvider serviceProvider;

        // AssetViewModel
        private readonly ICommandBase clearArchetypeCommand;
        private readonly ICommandBase createDerivedAssetCommand;
        // ModelViewModel
        private readonly ICommandBase createSkeletonCommand;
        // SceneViewModel
        private readonly ICommandBase setAsDefaultCommand;
        // SpriteFontViewModel
        private readonly ICommandBase generatePrecompiledFontCommand;

        public AssetViewModelService([NotNull] IViewModelServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;

            clearArchetypeCommand = new AnonymousCommand<AssetViewModel>(serviceProvider, ClearArchetype, x => x?.Asset.Archetype is not null);
            createDerivedAssetCommand = new AnonymousCommand<AssetViewModel>(serviceProvider, CreateDerivedAsset, x => x?.CanDerive ?? false);

            createSkeletonCommand = new AnonymousTaskCommand<ModelViewModel>(serviceProvider, CreateSkeleton);

            setAsDefaultCommand = new AnonymousCommand<SceneViewModel>(serviceProvider, SetAsDefault, x =>
            {
                // Cannot set scene as default if
                // 1. session does not have a current package (not executable game)
                // 2. scene is not reachable from the main package (not a dependency), i.e. not in Package.AllAssets
                return x?.Session.CurrentProject?.AllAssets.OfType<SceneViewModel>().Contains(x) ?? false;
            });

            generatePrecompiledFontCommand = new AnonymousTaskCommand<SpriteFontViewModel>(serviceProvider, GeneratePrecompiledFont);
        }

        public IEnumerable<MenuCommandInfo> GetCommandsForAsset(AssetViewModel asset)
        {
            var commands = new List<MenuCommandInfo>
            {
                new(serviceProvider, clearArchetypeCommand)
                {
                    CommandParameter = asset,
                    DisplayName = "Clear archetype",
                    Icon = new Image { Source = new BitmapImage(new Uri("/Stride.Core.Assets.Editor.Wpf;component/Resources/Icons/delete_link-32.png", UriKind.RelativeOrAbsolute)) },
                },
            };
            if (asset.CanDerive)
            {
                commands.Add(new MenuCommandInfo(serviceProvider, createDerivedAssetCommand)
                {
                    CommandParameter = asset,
                    DisplayName = "Create derived asset",
                    Icon = new Image { Source = new BitmapImage(new Uri("/Stride.Core.Assets.Editor.Wpf;component/Resources/Icons/copy_link-32.png", UriKind.RelativeOrAbsolute)) },
                });
            }

            if (asset is ModelViewModel)
            {
                commands.Add(new MenuCommandInfo(serviceProvider, createSkeletonCommand)
                {
                    CommandParameter = asset,
                    DisplayName = "Create Skeleton",
                    Tooltip = "Create a skeleton asset",
                    Icon = new Image { Source = new BitmapImage(new Uri("/Stride.Assets.Presentation.Wpf;component/Resources/Icons/create_skeleton-16.png", UriKind.RelativeOrAbsolute)) },
                });
            }
            else if (asset is SceneViewModel)
            {
                commands.Add(new MenuCommandInfo(serviceProvider, setAsDefaultCommand)
                {
                    CommandParameter = asset,
                    DisplayName = "Set as default",
                    Tooltip = "Set as default scene"
                });
            }
            else if (asset is SpriteFontViewModel)
            {
                commands.Add(new MenuCommandInfo(serviceProvider, generatePrecompiledFontCommand)
                {
                    CommandParameter = asset,
                    DisplayName = "Generate precompiled font",
                    Tooltip = "Generate precompiled font"
                });
            }

            return commands;
        }

        public void RefreshCommands()
        {
            // HACK: forces CommandBase.InvokeCanExecute()
            clearArchetypeCommand.IsEnabled ^= true;
            clearArchetypeCommand.IsEnabled ^= true;
            
            createDerivedAssetCommand.IsEnabled ^= true;
            createDerivedAssetCommand.IsEnabled ^= true;
            
            createSkeletonCommand.IsEnabled ^= true;
            createSkeletonCommand.IsEnabled ^= true;
            
            setAsDefaultCommand.IsEnabled ^= true;
            setAsDefaultCommand.IsEnabled ^= true;
            
            generatePrecompiledFontCommand.IsEnabled ^= true;
            generatePrecompiledFontCommand.IsEnabled ^= true;
        }

        private static void CreateDerivedAsset([NotNull] AssetViewModel asset)
        {
            if (asset.CanDerive)
            {
                var targetDirectory = AssetViewModel.FindValidCreationLocation(asset.AssetItem.Asset.GetType(), asset.Directory, asset.Session.CurrentProject);

                if (targetDirectory == null)
                    return;

                var childName = NamingHelper.ComputeNewName(asset.Name + "-Derived", targetDirectory.Assets, x => x.Name);
                var childUrl = UFile.Combine(targetDirectory.Path, childName);
                var childAsset = asset.AssetItem.CreateDerivedAsset();
                var childAssetItem = new AssetItem(childUrl, childAsset);
                targetDirectory.Package.CreateAsset(targetDirectory, childAssetItem, true, null);
            }
        }

        private static void ClearArchetype([NotNull] AssetViewModel asset)
        {
            using (var transaction = asset.UndoRedoService.CreateTransaction())
            {
                // Clear the actual base
                asset.PropertyGraph.RootNode[nameof(Asset.Archetype)].Update(null);

                // Remove all overridden properties
                var clearedOverrides = asset.PropertyGraph.ClearAllOverrides();
                if (!asset.UndoRedoService.UndoRedoInProgress)
                {
                    asset.UndoRedoService.PushOperation(new AnonymousDirtyingOperation(asset.Dirtiables, () => RestoreArchetype(asset, clearedOverrides), () => ClearArchetype(asset)));
                }

                asset.UndoRedoService.SetName(transaction, "Clear archetype");
            }

            // Force refreshing the property grid
            asset.Session.AssetViewProperties.RefreshSelectedPropertiesAsync().Forget();
        }

        private static void RestoreArchetype([NotNull] AssetViewModel asset, [NotNull] List<AssetPropertyGraph.NodeOverride> clearedOverrides)
        {
            AssetViewModel baseViewModel = null;
            if (asset.Asset.Archetype != null)
            {
                baseViewModel = asset.Session.GetAssetById(asset.Asset.Archetype.Id);
                if (baseViewModel == null)
                    throw new InvalidOperationException($"Unable to find the base [{asset.Asset.Archetype.Location}] of asset [{asset.Url}].");
            }

            // Restore all overrides
            asset.PropertyGraph?.RestoreOverrides(clearedOverrides, baseViewModel?.PropertyGraph);

            // Refresh the base to ensure everything is clean
            asset.PropertyGraph?.RefreshBase();

            // Reconcile with base. This should not do anything!
            asset.PropertyGraph?.ReconcileWithBase();

            // Force refreshing the property grid
            asset.Session.AssetViewProperties.RefreshSelectedPropertiesAsync().Forget();
        }

        private static async Task CreateSkeleton([NotNull] ModelViewModel asset)
        {
            var source = asset.Asset.Source;
            if (UPath.IsNullOrEmpty(source))
                return;

            using var transaction = asset.UndoRedoService.CreateTransaction();
            var template = asset.Session.FindTemplates(TemplateScope.Asset).SingleOrDefault(x => x.Id == SkeletonFromFileTemplateGenerator.Id);
            if (template != null)
            {
                var viewModel = new TemplateDescriptionViewModel(asset.ServiceProvider, template);
                var skeleton = (await asset.Session.ActiveAssetView.RunAssetTemplate(viewModel, new[] { source })).SingleOrDefault();
                if (skeleton == null)
                    return;

                var skeletonNode = asset.PropertyGraph.RootNode[nameof(ModelAsset.Skeleton)];
                var reference = ContentReferenceHelper.CreateReference<Skeleton>(skeleton);
                skeletonNode.Update(reference);
            }
            asset.UndoRedoService.SetName(transaction, "Create Skeleton");
        }
        
        private static void SetAsDefault([NotNull] SceneViewModel asset)
        {
            // TODO: find a better (faster?) way to access the game settings view model
            var gameSettings = asset.Session.CurrentProject?.AllAssets.OfType<GameSettingsViewModel>().FirstOrDefault();
            if (gameSettings == null)
                return;
            gameSettings.DefaultScene = asset;
        }
        
        private static async Task GeneratePrecompiledFont([NotNull] SpriteFontViewModel asset)
        {
            var font = (SpriteFontAsset)asset.AssetItem.Asset;
            var dialogService = asset.ServiceProvider.Get<IDialogService>();
            // Dynamic font cannot be precompiled
            if (font.FontType is RuntimeRasterizedSpriteFontType)
            {
                // Note: Markdown (**, _) are used to format the text.
                await dialogService.MessageBoxAsync(Tr._p("Message", "**Only static fonts can be precompiled.**\r\n\r\nClear the _Is Dynamic_ property on this font and try again."), MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            // Compute unique name
            var precompiledName = NamingHelper.ComputeNewName($"{asset.AssetItem.Location.GetFileNameWithoutExtension()} (Precompiled)", asset.Directory.Assets, x => x.Name);

            // Ask location for generated texture
            var initialPath = UDirectory.Combine(asset.Session.CurrentProject?.Package?.RootDirectory ?? asset.Session.SolutionPath.GetFullDirectory(), "Resources");
            var directory = await dialogService.OpenFolderPickerAsync(initialPath);
            if (directory is null)
                return;

            bool srgb;
            var gameSettings = asset.Session.CurrentProject?.Package.GetGameSettingsAsset();
            if (gameSettings == null)
            {
                var buttons = DialogHelper.CreateButtons(new[] { ColorSpace.Linear.ToString(), ColorSpace.Gamma.ToString(), Tr._p("Button", "Cancel") }, 1, 3);
                var result = await dialogService.MessageBoxAsync(Tr._p("Message", "Which color space do you want to use?"), buttons, MessageBoxImage.Question);
                // Close without clicking a button or Cancel
                if (result == 0 || result == 3)
                    return;
                srgb = result == 2;
            }
            else
            {
                srgb = gameSettings.GetOrCreate<RenderingSettings>().ColorSpace == ColorSpace.Linear;
            }

            var precompiledFontAsset = (font.FontType is SignedDistanceFieldSpriteFontType) ?
                font.GeneratePrecompiledSDFSpriteFont(asset.AssetItem, UFile.Combine(directory, precompiledName)) : 
                font.GeneratePrecompiledSpriteFont(asset.AssetItem, UFile.Combine(directory, precompiledName), srgb);

            // NOTE: following code could be factorized with AssetFactoryViewModel
            var defaultLocation = UFile.Combine(asset.Directory.Path, precompiledName);
            var assetItem = new AssetItem(defaultLocation, precompiledFontAsset);
            AssetViewModel assetViewModel;
            using (var transaction = asset.UndoRedoService.CreateTransaction())
            {
                // FIXME: do we need to delete the generated file upon undo?
                assetViewModel = asset.Directory.Package.CreateAsset(asset.Directory, assetItem, true, null);
                asset.UndoRedoService.SetName(transaction, $"Create Asset '{precompiledName}'");
            }

            asset.Session.CheckConsistency();
            if (assetViewModel != null)
            {
                asset.Session.ActiveAssetView.SelectAssetCommand.Execute(assetViewModel);
            }
        }
    }

    // FIXME xpla-editor these features don't depend on Stride assets (only core assets) and should eventually be moved elsewhere
    partial class AssetViewModelService
    {
        // CreateAsset()
        //  see PackageViewModel.CreateAsset()

        // MoveAsset()
        //  see AssetViewModel.MoveAsset()

        // RenameAsset()
        //  see AssetViewModel.Rename()

        // UpdateUrl()
        //  see AssetViewModel.UpdateUrl()
        //  might be internal and not exposed publicly

        // MovePackage()
        //  see PackageViewModel.MoveAsset()

        // RenamePackage()
        //  see PackageViewModel.Rename()

        // DeleteAsset()
        //  see implementations of UpdateIsDeletedStatus()
        //  not sure where this should be implemented
        //  same questions for Directory, Package, etc. should the delete feature be implemented at the specific viewmodel level, or should it be delegated to a service such as this one.
        //  In the latter case, there could be a NullService implementationthat is no-op to be used in other projects like viewers.
    }
}
