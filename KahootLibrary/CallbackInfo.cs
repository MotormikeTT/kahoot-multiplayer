/*
 * Program:         CardsLibrary.dll
 * Module:          CallbackInfo.cs
 * Date:            Mar 16, 2021
 * Author:          T. Haworth
 * Description:     The CallbackInfo class represents a WCF data contract for sending
 *                  realtime updates to connected clients regarding changes to the 
 *                  state of the Shoe (service object).
 *                  
 *                  Note that we had to add a reference to the .NET assembly 
 *                  System.Runtime.Serialization to create a DataContract.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Serialization; // WCF data contract types

namespace CardsLibrary
{
    [DataContract]
    public class CallbackInfo
    {
        [DataMember]
        public int NumCards { get; private set; }
        [DataMember] 
        public int NumDecks { get; private set; }
        [DataMember] 
        public bool EmptyTheHand { get; private set; }

        public CallbackInfo(int c, int d, bool e)
        {
            NumCards = c;
            NumDecks = d;
            EmptyTheHand = e;
        }
    }
}
