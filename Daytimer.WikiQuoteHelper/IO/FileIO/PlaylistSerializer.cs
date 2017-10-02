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
// Filename: PlaylistSerializer.cs
// Copyright: Christian Hanser 2008
//

using System;
using System.Threading;
using WikiquoteScreensaverLib.Common;
using WikiquoteScreensaverLib.Common.ErrorHandling;

namespace WikiquoteScreensaverLib.IO.FileIO
{
    public abstract class PlaylistSerializer 
        : PlaylistIO
    {
        readonly Playlist _playlist; 
 
        protected PlaylistSerializer(Playlist playlist)
        {
            // checking preconditions
            if (playlist == null)
            {
                throw new ArgumentNullException();
            }

            _playlist = playlist;
        } 
 
        public Playlist Playlist
        {
            get { return _playlist; }
        } 
 
        protected abstract QuotePageCollection LoadData(string filepath);
        protected abstract void SaveData(string filepath); 
 
        public override void Load(string filepath)
        {
            CheckFilepathPreconditions(filepath);

            try
            {
                Playlist.Items = LoadData(filepath);
                Playlist.AddItemEventHandlers();
            }
            catch (Exception ex)
            {
                if (Playlist.Items == null)
                {
                    Playlist.Items = new QuotePageCollection();
                }
                throw new ErrorLoadingDataException(filepath, ex);
            }
        }

        public override void LoadAsync(string filepath)
        {
            CheckFilepathPreconditions(filepath);

            ThreadPool.QueueUserWorkItem((unused) =>
                {
                    try
                    {
                        Playlist.Items = LoadData(filepath);
                        Playlist.AddItemEventHandlers();
                        OnDataLoaded();
                    }
                    catch (Exception ex)
                    {
                        if (Playlist.Items == null)
                        {
                            Playlist.Items = new QuotePageCollection();
                        }
                        OnErrorLoadingData(new ErrorLoadingDataEventArgs(filepath, ex));
                    }
                });
        }

        public void Save(string filepath)
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

            try
            {
                SaveData(filepath);
            }
            catch (Exception ex)
            {
                throw new ErrorSavingDataException(filepath, ex);
            }
        } 
    }
}
