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
// Filename: PlaylistExporter.cs
// Copyright: Christian Hanser 2008
//

using System;
using System.Linq;
using System.Threading;
using WikiquoteScreensaverLib.Common;
using WikiquoteScreensaverLib.Common.ErrorHandling;

namespace WikiquoteScreensaverLib.IO.FileIO
{
    public abstract class PlaylistExporter 
        : PlaylistIO
    {
        public TaggedQuote[] LoadedData { get; private set; } 
 
        protected abstract TaggedQuote[] LoadData(string filepath);
        protected abstract void SaveData(TaggedQuote[] quotes, string filepath); 
 
        private static TaggedQuote[] FilterSelectedQuotes(Playlist playlist)
        {
            var quotes = from selectedQuotePage in
                             from quotePage in playlist
                             where quotePage.Selected
                             select quotePage
                         from quote in selectedQuotePage
                         where quote.Selected
                         select new TaggedQuote(selectedQuotePage, quote);

            return quotes.ToArray();
        } 
 
        public override void Load(string filepath)
        {
            CheckFilepathPreconditions(filepath);

            try
            {
                LoadedData = LoadData(filepath);
            }
            catch (Exception ex)
            {
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
                        LoadedData = LoadData(filepath);
                        OnDataLoaded();
                    }
                    catch (Exception ex)
                    {
                        OnErrorLoadingData(new ErrorLoadingDataEventArgs(filepath, ex));
                    }
                });
        }

        public void Save(Playlist playlist, string filepath)
        {
            // checking preconditions
            if (playlist == null || filepath == null)
            {
                throw new ArgumentNullException();
            }
            else if (filepath.Length == 0)
            {
                throw new ArgumentException();
            }

            try
            {
                SaveData(FilterSelectedQuotes(playlist), filepath);
            }
            catch (Exception ex)
            {
                throw new ErrorSavingDataException(filepath, ex);
            }
        } 
    }
}
