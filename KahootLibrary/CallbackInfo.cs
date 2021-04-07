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

namespace KahootLibrary
{
    [DataContract]
    public class CallbackInfo
    {
        [DataMember]
        public List<Question> Questions { get; private set; }
        [DataMember]
        public int NumQuestions { get; private set; }
        [DataMember]
        public int TimePerQuestion { get; private set; }
        [DataMember]
        public bool Ready { get; private set; }

        public CallbackInfo(List<Question> q, int n, int t, bool r)
        {
            Questions = q;
            NumQuestions = n;
            TimePerQuestion = t;
            Ready = r;
        }
    }
}
