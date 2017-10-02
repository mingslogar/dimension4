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
// Filename: TopicAmbiguousException.cs
// Copyright: Christian Hanser 2008
//

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using WikiquoteScreensaverLib.IO.WebIO;

namespace WikiquoteScreensaverLib.Common.ErrorHandling
{
    /// <summary>
    /// Exception class used to indicate that the specified topic is ambiguous.
    /// </summary>
    [Serializable]
    public sealed class TopicAmbiguousException 
        : WikiquoteScreensaverLibException
    {
        readonly string _topic;
        readonly IEnumerable<TopicChoice> _topicChoices;

        internal TopicAmbiguousException(string topic, IEnumerable<TopicChoice> topicChoices)
            : base()
        {
            _topic = topic;
            _topicChoices = topicChoices;
        }

        internal TopicAmbiguousException()
            : this(null, null)
        {
        }

        private TopicAmbiguousException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public string Topic
        {
            get { return _topic; }
        }

        public IEnumerable<TopicChoice> TopicChoices
        {
            get { return _topicChoices; }
        }  
    }
}

