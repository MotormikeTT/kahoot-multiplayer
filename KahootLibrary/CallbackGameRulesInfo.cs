/*
 * Program:         KahootLibrary.dll
 * Module:          CallbackGameRulesInfo.cs
 * Author:          George Moussa, Michael Mac Lean
 * Date:            April 4, 2021
 * Description:     The CallbackGameRulesInfo class represents a WCF data contract for sending
 *                  realtime game rules updates to connected clients regarding changes to the 
 *                  state of the Game (service object).
 *                  
 *                  Note that we had to add a reference to the .NET assembly 
 *                  System.Runtime.Serialization to create a DataContract.
*/

using System.Collections.Generic;
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
        public bool GameHost { get; private set; }

        public CallbackGameRulesInfo(List<Player> ps, string c, int n, int t, bool gh)
        {
            Players = ps;
            Category = c;
            NumQuestions = n;
            TimePerQuestion = t;
            GameHost = gh;
        }
    }
}
