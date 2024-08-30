# ViewModels

State of view models between xplat version and legacy (Wpf) version

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
		Does nothing. Only here because `IAddChildViewModel` is implemented.
	- [x] IsValidName
