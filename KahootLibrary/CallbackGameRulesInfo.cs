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
    public class CallbackGameRulesInfo
    {
        [DataMember]
        public List<Player> Players { get; private set; }
        [DataMember]
        public string Category { get; private set; }
        [DataMember]
        public int NumQuestions { get; private set; }
        [DataMember]
        public int TimePerQuestion { get; private set; }
        [DataMember]
        public bool GameStarted { get; private set; }
        [DataMember]
        public bool GameHost { get; private set; }

        public CallbackGameRulesInfo(List<Player> ps, string c, int n, int t, bool gt, bool gh)
        {
            Players = ps;
            Category = c;
            NumQuestions = n;
            TimePerQuestion = t;
            GameStarted = gt;
            GameHost = gh;
        }
    }
}
