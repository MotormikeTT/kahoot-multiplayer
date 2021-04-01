/*
 * Program:         CardsLibrary.dll
 * Module:          Card.cs
 * Date:            January 19, 2021
 * Author:          T. Haworth
 * Description:     The Card class represents a standard playing card. (complete)
 * 
 * Modifications:   Feb 2, 2021
 *                  Added the ICard interface which the card class implements.
 *                  Now hiding the Card class from external clients.
 *                  
 *                  Mar 16, 2021
 *                  Turned the Card class into a WCF data contract
 *                  and removed the ICard interface which we aren't using.
 *                  Note that the Card class needed to be made "public"
 *                  so that the client is now able to see it.
 *                  
 *                  Note that we had to add a reference to the .NET Framework 
 *                  assembly System.Runtime.Serialization.dll.
 */

using System;
using System.Runtime.Serialization; // WCF data contract types

namespace CardsLibrary
{
    /*--------------------- Public type definitions --------------------*/

    public enum SuitID { Clubs, Diamonds, Hearts, Spades };
    public enum RankID { Ace, King, Queen, Jack, Ten, Nine, Eight, Seven, Six, Five, Four, Three, Two };


    // The class definition is now invisible from outside the CardsLibrary assembly
    [DataContract]
    public class Card 
    {
        /*-------------------------- Constructors --------------------------*/

        internal Card(SuitID s, RankID r)
        {
            Suit = s;
            Rank = r;
        }

        /*------------------ Public properties and methods -----------------*/

        [DataMember]
        public SuitID Suit { get; private set; }
        [DataMember]
        public RankID Rank { get; private set; }

        public override string ToString()
        {
            return Rank.ToString() + " of " + Suit.ToString();
        }

    } // end Card class
}
