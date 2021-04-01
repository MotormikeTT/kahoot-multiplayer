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
 * Modificatons:    Mar 16, 2021
 *                  Draw() now returns a Card object instead of a string.
 *                  This is only possible because Card is a data contract.
 *                  This enables WCF to serialize Cards so that may be sent
 *                  to the client via the message-passing mechanism.
 *                  
 *                  Mar 16, 2021
 *                  Adds a client callback contract called ICallback
 *                  
 *                  Mar 23, 2021
 *                  Uses the callbacks to update "registered" clients
 */

using System;
using System.Collections.Generic;
using System.Linq;

using System.ServiceModel;  // WCF types

namespace CardsLibrary
{
    //[ServiceContract]
    public interface ICallback
    {
        [OperationContract(IsOneWay=true)]
        void UpdateClient(CallbackInfo info);
    }

    // Converted IShoe to a WCF service contract
    [ServiceContract(CallbackContract=typeof(ICallback))]
    public interface IShoe
    {
        [OperationContract(IsOneWay = true)]
        void Shuffle();
        [OperationContract] 
        Card Draw();
        int NumDecks { [OperationContract] get; [OperationContract(IsOneWay = true)] set; }
        int NumCards { [OperationContract] get; }
        [OperationContract(IsOneWay = true)]
        void RegisterForCallbacks();
        [OperationContract(IsOneWay = true)]
        void UnregisterFromCallbacks();

    }

    // The class that implements the service
    // ServiceBehavior is used here to select the desired instancing behaviour
    // of the Shoe class
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class Shoe : IShoe
    {
        /*------------------------ Member Variables ------------------------*/

        private List<Card> cards = null;    // collection of cards
        private int cardIdx;                // index of the next card to be dealt
        private int numDecks;               // number of decks in the shoe
        
        private static uint objCount = 0;
        private uint objNum;
        private HashSet<ICallback> callbacks = new HashSet<ICallback>();

        /*-------------------------- Constructors --------------------------*/

        public Shoe()
        {
            objNum = ++objCount;
            Console.WriteLine($"Creating Shoe object #{objNum}");

            cards = new List<Card>();
            numDecks = 1;
            populate();
        }

        /*------------------ Public properties and methods -----------------*/
        
        // Randomizes the sequence of the Cards in the cards collection
        public void Shuffle()
        {
            Console.WriteLine($"Shoe #{objNum} Shuffling");

            // Randomize the cards collection
            Random rng = new Random();
            cards = cards.OrderBy(card => rng.Next()).ToList();

            // Reset the cards index
            cardIdx = 0;

            updateClients(true);
        }

        // Returns a copy of the next Card in the cards collection
        public Card Draw()
        {
            if (cardIdx >= cards.Count)
                throw new ArgumentException("The shoe is empty.");

            Card card = cards[cardIdx++];

            Console.WriteLine($"Shoe #{objNum} dealing {cards[cardIdx].ToString()}");

            updateClients(false);

            return card;
        }

        // Lets the client read or modify the number of decks in the shoe
        public int NumDecks 
        { 
            get
            {
                return numDecks;
            }
            set
            {
                if (numDecks != value)
                {
                    numDecks = value;
                    populate();
                }
            }
        }

        // Lets the client read the number of cards remaining in the shoe
        public int NumCards
        {
            get
            {
                return cards.Count - cardIdx;
            }
        }

        public void RegisterForCallbacks()
        {
            // Identify which client is calling this method
            ICallback cb = OperationContext.Current.GetCallbackChannel<ICallback>();

            // Add client's callback object to the collection
            if (!callbacks.Contains(cb))
                callbacks.Add(cb);
        }

        public void UnregisterFromCallbacks()
        {
            // Identify which client is calling this method
            ICallback cb = OperationContext.Current.GetCallbackChannel<ICallback>();

            // Remove client's callback object from the collection
            if (callbacks.Contains(cb))
                callbacks.Remove(cb);
        }


        /*------------------------- Helper methods -------------------------*/

        // Populates the cards attribute with Card objects and then shuffles it 
        private void populate()
        {
            Console.WriteLine($"Shoe #{objNum} repopulating with {numDecks} decks");

            // Clear-out all the "old' cards
            cards.Clear();

            // For each deck in numDecks...
            for (int d = 0; d < numDecks; ++d)
                // For each suit..
                foreach (SuitID s in Enum.GetValues(typeof(SuitID)))
                    // For each rank..
                    foreach (RankID r in Enum.GetValues(typeof(RankID)))
                        cards.Add(new Card(s, r));

            Shuffle();
        }

        // Uses the client callback objects to send current Shoe information 
        // to clients. If the change in teh Shoe state was triggered by a method call 
        // from a specific client, then that particular client will be excluded from
        // the update since it will already be updated directly by the call.
        private void updateClients(bool emptyHand)
        {
            // Identify which client just changed the Shoe object's state
            ICallback thisClient = null;
            if (OperationContext.Current != null)
                thisClient = OperationContext.Current.GetCallbackChannel<ICallback>();

            // Prepare the CallbackInfo parameter
            CallbackInfo info = new CallbackInfo(cards.Count - cardIdx, numDecks, emptyHand);

            // Update all clients except thisClient
            foreach (ICallback cb in callbacks)
                if (thisClient == null || thisClient != cb)
                    cb.UpdateClient(info);
        }

       
    } // end Shoe class
}
