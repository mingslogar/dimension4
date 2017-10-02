// 
// This file is part of WikiquoteScreensaverLib.
//
// WikiquoteScreensaverLib is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// WikiquoteScreensaverLib is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with WikiquoteScreensaverLib.  If not, see <http://www.gnu.org/licenses/>.
//
// Filename: PlaylistIO.cs
// Copyright: Christian Hanser 2008
//

using System;
using System.IO;

namespace WikiquoteScreensaverLib.IO.FileIO
{
    public abstract class PlaylistIO
    {
        public event EventHandler DataLoaded;
        public event EventHandler<ErrorLoadingDataEventArgs> ErrorLoadingData; 
 
        public abstract void Load(string filepath);
        public abstract void LoadAsync(string filepath); 
 
        protected virtual void OnErrorLoadingData(ErrorLoadingDataEventArgs e)
        {
            EventHandler<ErrorLoadingDataEventArgs> handler = ErrorLoadingData;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnDataLoaded()
        {
            EventHandler handler = DataLoaded;
            if (handler != null)
            {
                handler(this, null);
            }
        }

        protected static void CheckFilepathPreconditions(string filepath)
        {
            CheckFilepathPreconditions(filepath, true);
        }

        protected static void CheckFilepathPreconditions(string filepath, bool checkFileExists)
        {
            // checking preconditions
            if (filepath == null)
            {
                throw new ArgumentNullException();
            }
            else if (filepath.Length == 0)
            {
                throw new ArgumentException();
            }
            else if (checkFileExists && !File.Exists(filepath))
            {
                throw new FileNotFoundException();
            }
        } 
    }
}
