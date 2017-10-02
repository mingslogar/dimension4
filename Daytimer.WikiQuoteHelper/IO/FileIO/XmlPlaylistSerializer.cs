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
// Filename: XmlPlaylistSerializer.cs
// Copyright: Christian Hanser 2008
//

using System.IO;
using System.Runtime.Serialization;
using WikiquoteScreensaverLib.Common;

namespace WikiquoteScreensaverLib.IO.FileIO
{
    public sealed class XmlPlaylistSerializer 
        : PlaylistSerializer
    {
        readonly DataContractSerializer _serializer; 
 
        public XmlPlaylistSerializer(Playlist playlist)
            : base(playlist)
        {
            _serializer = new DataContractSerializer(Playlist.Items.GetType());
        } 
 
        protected override QuotePageCollection LoadData(string filepath)
        {
            QuotePageCollection returnValue;

            using (FileStream fileStream = new FileStream(filepath, FileMode.Open))
            {
                returnValue = (QuotePageCollection)_serializer.ReadObject(fileStream);
            }

            return returnValue;
        }

        protected override void SaveData(string filepath)
        {
            using (FileStream fileStream = new FileStream(filepath, FileMode.Create))
            {
                _serializer.WriteObject(fileStream, Playlist.Items);
            }
        } 
    }
}
