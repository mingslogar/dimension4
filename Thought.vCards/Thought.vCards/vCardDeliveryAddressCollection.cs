
/* =======================================================================
 * vCard Library for .NET
 * Copyright (c) 2007-2009 David Pinch; http://wwww.thoughtproject.com
 * See LICENSE.TXT for licensing information.
 * ======================================================================= */

using System;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

namespace Thought.vCards
{

	/// <summary>
	///     A collection of <see cref="vCardDeliveryAddress"/> objects.
	/// </summary>
	/// <seealso cref="vCardDeliveryAddress"/>
	[ComVisible(false)]
	public class vCardDeliveryAddressCollection : Collection<vCardDeliveryAddress>
	{
	}

}