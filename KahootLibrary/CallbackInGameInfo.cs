/*
 * Program:         CardsLibrary.dll
 * Module:          Shoe.cs
 * Author:          T. Haworth
 * Date:            March 9, 2021
 * Description:     Defines a WCF service contract called IShoe as well as 
 *                  a Shoe class that implements the service. This is just 
 *                  a slightly modified version of the earlier CardsLibrary 
 *                  example.
 *                  
 *                  Note that we had to add a reference to the .NET Framework 
 *                  assembly System.ServiceModel.dll.
 *                  
 * Modificatons:    
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

        public CallbackInGameInfo(List<Player> ps, Question q, bool e)
        {
            Players = ps;
            Question = q;
            EndGame = e;
        }
    }
}