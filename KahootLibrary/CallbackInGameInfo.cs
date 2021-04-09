/*
 * Program:         KahootLibrary.dll
 * Module:          CallbackInGameInfo.cs
 * Author:          George Moussa, Michael Mac Lean
 * Date:            April 4, 2021
 * Description:     The CallbackInGameInfo class represents a WCF data contract for sending
 *                  realtime in game updates to connected clients regarding changes to the 
 *                  state of the Game (service object).
 *                  
 *                  Note that we had to add a reference to the .NET assembly 
 *                  System.Runtime.Serialization to create a DataContract.
*/

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace KahootLibrary
{
    [DataContract]
    public class CallbackInGameInfo
    {
        [DataMember]
        public List<Player> Players { get; private set; }
        [DataMember]
        public Question Question { get; private set; }
        [DataMember]
        public bool EndGame { get; private set; }
        [DataMember]
        public bool GameHost { get; private set; }

        public CallbackInGameInfo(List<Player> ps, Question q, bool e, bool gh)
        {
            Players = ps;
            Question = q;
            EndGame = e;
            GameHost = gh;
        }
    }
}