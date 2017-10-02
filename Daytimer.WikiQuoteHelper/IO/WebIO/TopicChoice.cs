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
// Filename: TopicChoice.cs
// Copyright: Christian Hanser 2008
//

namespace WikiquoteScreensaverLib.IO.WebIO
{
    public sealed class TopicChoice
    {
        readonly string _topicId;
        readonly string _topicName;
        readonly string _topicDescription;

        public TopicChoice(string topicId, string topicName, string topicDescription)
        {
            _topicId = topicId;
            _topicName = topicName;
            _topicDescription = topicDescription;
        }

        public string TopicName
        {
            get { return _topicName; }
        }

        public string TopicDescription
        {
            get { return _topicDescription; }
        }

        public override string ToString()
        {
            return _topicId;
        }
    }
}
