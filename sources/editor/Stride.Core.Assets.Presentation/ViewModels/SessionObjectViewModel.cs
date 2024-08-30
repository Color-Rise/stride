// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using Stride.Core.IO;
using Stride.Core.Presentation.ViewModels;
using Stride.Core.Translation;

namespace Stride.Core.Assets.Presentation.ViewModels;

public abstract class SessionObjectViewModel : DirtiableEditableViewModel, ISessionObjectViewModel
{
    private bool isDeleted = true;

    protected SessionObjectViewModel(ISessionViewModel session)
        : base(session.ServiceProvider)
    {
        Session = session;
    }

    /// <summary>
    /// Gets whether this object is currently deleted.
    /// </summary>
    public bool IsDeleted { get { return isDeleted; } set { SetValue(ref isDeleted, value, UpdateIsDeletedStatus); } }

    public abstract string Name { get; set; }

    /// <summary>
    /// Gets the session in which this object is currently in.
    /// </summary>
    public ISessionViewModel Session { get; }

    /// <summary>
    /// Gets the display name of the type of this <see cref="SessionObjectViewModel"/>.
    /// </summary>
    public abstract string TypeDisplayName { get; }

    /// <summary>
    /// Marks this view model as undeleted.
    /// </summary>
    /// <param name="canUndoRedoCreation">Indicates whether a transaction should be created when doing this operation.</param>
    /// <remarks>
    /// This method is intended to be called from constructors, to allow the creation of this view model
    /// to be undoable or not.
    /// </remarks>
    protected void InitialUndelete(bool canUndoRedoCreation)
    {
        if (canUndoRedoCreation)
        {
            SetValue(ref isDeleted, false, UpdateIsDeletedStatus, nameof(IsDeleted));
        }
        else
        {
            SetValueUncancellable(ref isDeleted, false, UpdateIsDeletedStatus, nameof(IsDeleted));
        }
    }

    /// <summary>
    /// Updates related session objects when the <see cref="IsDeleted"/> property changes.
    /// </summary>
    protected abstract void UpdateIsDeletedStatus();

    protected virtual bool IsValidName(string value, out string? error)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));

        if (value.Length > 240)
        {
            error = Tr._p("Message", "The name is too long.");
            return false;
        }

        if (value.Contains(UPath.DirectorySeparatorChar) || value.Contains(UPath.DirectorySeparatorCharAlt) || !UPath.IsValid(value))
        {
            error = Tr._p("Message", "The name contains invalid characters.");
            return false;
        }

        if (string.IsNullOrEmpty(value))
        {
            error = Tr._p("Message", "The name is empty.");
            return false;
        }

        error = null;

        return true;
    }
}
