//---------------------------------------------------------------------------
//
// Copyright (C) Microsoft Corporation.  All rights reserved.
//
//---------------------------------------------------------------------------

using System.Windows.Input;

namespace Microsoft.Windows.Input
{
    /// <summary>
    ///   An interface extending normal Commands to provide a means for previewing commands without actually committing the action.
    /// </summary>
    public interface IPreviewCommand : ICommand
    {
        /// <summary>
        ///   Defines the method that should be executed when the command is previewed.
        /// </summary>
        /// <param name="parameter">A parameter that may be used in executing the preview command. This parameter may be ignored by some implementations.</param>
        void Preview (object parameter);

        /// <summary>
        ///   Defines the method that should be executed to cancel previewing of the command.
        /// </summary>
        void CancelPreview ();
    }
}
