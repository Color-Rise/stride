# ViewModels

State of view models between xplat version and legacy (Wpf) version
```
- [x]: The item exists in both implementations
- [-]: The item exists in the legacy version but wasn't kept in the xplat one.
- [ ]: The item doesn't exist on one side. It probably should so that we can eventually share the implementation.
       In other words, it's a TODO.
```

**`ISessionObjectViewModel`**

- [x] Properties
    - [-] IsEditable
        Not necessary, as it is covered by the `EditableViewModel` concrete type and the `IIsEditableViewModel` interface.
    - [x] Name
    - [x] Session
        Use `ISessionObjectViewModel` interface instead of concrete class.
    - [-] ThumbnailData
        Not necessary to be on the interface.
    - [-] TypeDisplayName
        Not necessary to be on the interface.

**`SessionObjectViewModel`**

- [x] Constructor
    Param: use `ISessionViewModel` interface instead of concrete class.
- [x] Properties
    - [x] Name
    - [ ] Session
        Use `ISessionViewModel` interface instead of concrete class.
        It is necessary as the concrete implementation is not known at this point.
        It has implications (e.g. with `SessionNodeContainer`).
    - [x] TypeDisplayName
- [ ] Methods
    - [x] InitialUndelete
    - [x] UpdateIsDeletedStatus
    - [ ] IAddChildViewModel.CanAddChildren
        Only purpose is to by default consider this object as an invalid area for drag & drop.
    - [-] IAddChildViewModel.AddChildren
        Does nothing. Only here because `IAddChildViewModel` is implemented (see above).
    - [x] IsValidName

**`ISessionViewModel`**

- [ ] Properties
    - [x] AllAssets
    - [x] AllPackages
    - [x] AssetNodeContainer
    - [x] CurrentProject
    - [x] DependencyManager
    - [x] Dispatcher
        OK, through the IDispatcherViewModel interface
    - [x] GraphContainer
    - [x] LocalPackages
    - [x] PackageCategories
    - [x] ServiceProvider
        OK, through the IViewModelBase interface
    - [ ] SourceTracker
      - Legacy version still uses the concrete class.
    - [x] StorePackages
- [x] Events
  - [x] AssetPropertiesChanged
  - [x] SessionStateChanged
- [x] Methods
    - [x] GetAssetById
    - [x] RegisterAsset
    - [x] UnregisterAsset

**`AssetViewModel`**

- [x] `IAssetViewModel`
- [x] `AssetViewModel<TAsset>`

- [ ] Remove any editing capability implemented there and move it to some services/helpers and/or to the `SessionViewModel`.
    - e.g. Asset commands should be implemented elsewhere, so that they can be provided by plugins

*editing capabilities*

- [ ] Rename()/UpdateUrl()/MoveAsset()
    Can probably stay here for now, but could also be a virtual (default no-op) method, or an event.
    Could also be delegated to an `IAssetViewModelService` (see if this should be merged with `StrideAssetsViewModel`).

    Pb: UpdateUrl, Rename, MoveAsset as well as equivalent in PackageViewModel have tight dependencies with each other (and call each other leading to potentiel loops)
    We need to rethink this whole capability in a central place and only expose the minimum necessary in both AssetViewModel and PackageViewModel.
    - [ ] IsNewNameValid()
        Name validity could be dependent on engine implementation or other plugins.
- [x] AssetsCommands
    Shouldn't be there, by provided elsewhere (e.g. through a plugin system).
    At the very most, we could have the collection defined but initialized empty.
    Then they could be filled by the aforementioned `IAssetViewModelService`.
    - [x] CreateDerivedAsset()
    - [x] ClearArchetype()
    - [x] RestoreArchetype()
    - [x] Also commands in Model, Scene and SpriteFont
