/*
  LICENSE
  -------
  Copyright (C) 2007-2010 Ray Molenkamp

  This source code is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this source code or the software it produces.

  Permission is granted to anyone to use this source code for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this source code must not be misrepresented; you must not
     claim that you wrote the original source code.  If you use this source code
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original source code.
  3. This notice may not be removed or altered from any source distribution.
*/

//
// Modified by:	Ming Slogar
// Date:		November 14, 2013
// Reason:		CA1712:	Do not prefix enum values with type name.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreAudioApi
{
	public enum AudioSessionState
	{
		Inactive = 0,
		Active = 1,
		Expired = 2
	}
}
