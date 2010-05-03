/* ``The contents of this file are subject to the Erlang Public License,
 * Version 1.1, (the "License"); you may not use this file except in
 * compliance with the License. You should have received a copy of the
 * Erlang Public License along with this software. If not, it can be
 * retrieved via the world wide web at http://www.erlang.org/.
 * 
 * Software distributed under the License is distributed on an "AS IS"
 * basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See
 * the License for the specific language governing rights and limitations
 * under the License.
 * 
 * The Initial Developer of the Original Code is Ericsson Utvecklings AB.
 * Portions created by Ericsson are Copyright 1999, Ericsson Utvecklings
 * AB. All Rights Reserved.''
 * 
 *     $Id$
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace Otp
{
    class OtpSystem
    {

        private static bool xpidsports;

        public OtpSystem()
        {
        }

        public static bool useExtendedPidsPorts()
        {
            string rel = "0"; // System.getProperty("OtpCompatRel", "0");

            xpidsports = true;

            try
            {
                switch (Convert.ToInt32(rel))
                {
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                    case 8:
                    case 9:
                        xpidsports = false;
                        break;
                    case 0:
                    default:
                        break;
                }
            }
            //catch (NumberFormatException e)
            catch (Exception)
            {
                /* Ignore ... */
            }
            
            return xpidsports;
        }
    }
}


