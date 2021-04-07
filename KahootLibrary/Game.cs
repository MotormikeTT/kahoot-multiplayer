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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;  // WCF types

namespace KahootLibrary
{
    //[ServiceContract]
    public interface ICallback
    {
        [OperationContract(IsOneWay = true)]
        void UpdateGameRules(CallbackInfo info);
        [OperationContract(IsOneWay = true)]
        void UpdateClient(CallbackInfo info);
    }

    // Converted IShoe to a WCF service contract
    [ServiceContract(CallbackContract = typeof(ICallback))]
    public interface IGame
    {
        /*[OperationContract(IsOneWay = true)]
        void StartGame();*/
        [OperationContract]
        Question GetQuestion();
        int NumQuestions { [OperationContract] get; [OperationContract(IsOneWay = true)] set; }
        string Category { [OperationContract] get; [OperationContract(IsOneWay = true)] set; }
        int TimePerQuestion { [OperationContract] get; [OperationContract(IsOneWay = true)] set; }
        [OperationContract(IsOneWay = true)]
        void RegisterForCallbacks();
        [OperationContract(IsOneWay = true)]
        void UnregisterFromCallbacks();

    }

    // The class that implements the service
    // ServiceBehavior is used here to select the desired instancing behaviour
    // of the Shoe class
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class Game : IGame
    {
        /*------------------------ Member Variables ------------------------*/

        private List<Question> questions = null;    // collection of cards
        private List<Player> players = null;
        private int questionIdx;                // index of the next card to be dealt
        private int numQuestions;               // number of decks in the shoe
        private int timePerQuestion;
        private string category;

        private HashSet<ICallback> callbacks = new HashSet<ICallback>();

        /*-------------------------- Constructors --------------------------*/

        public Game()
        {
            Console.WriteLine($"Creating Game object");

            questions = new List<Question>();
            numQuestions = 5;
            category = "brain-teasers";
            populate();
        }

        /*------------------ Public properties and methods -----------------*/

        // Randomizes the sequence of the Cards in the cards collection
        public void Shuffle()
        {
            // Randomize the cards collection
            Random rng = new Random();
            questions = questions.OrderBy(question => rng.Next()).ToList().GetRange(0, numQuestions);

            // Reset the cards index
            questionIdx = 0;

            updateClients(true);
        }

        // Returns a copy of the next Card in the cards collection
        public Question GetQuestion()
        {
            if (questionIdx >= questions.Count)
                throw new ArgumentException("No more questions.");

            Question question = questions[questionIdx++];

            updateClients(false);

            return question;
        }

        // Lets the client read or modify the number of decks in the shoe
        public int NumQuestions
        {
            get
            {
                return numQuestions;
            }
            set
            {
                if (numQuestions != value)
                {
                    numQuestions = value;
                    populate();
                }
            }
        }

        // Lets the client read the number of cards remaining in the shoe
        public string Category
        {
            get
            {
                return category;
            }
            set
            {
                if (category != value)
                {
                    category = value;
                    populate();
                }
            }
        }

        public int TimePerQuestion
        {
            get
            {
                return timePerQuestion;
            }
            set
            {
                if (timePerQuestion != value)
                {
                    timePerQuestion = value;
                    populate();
                }
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
            string[] questions = File.ReadAllText(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), $@".\categories\{category}.txt")).Split(new string[] { "#Q " }, StringSplitOptions.None);
            List<Question> listOfQuestions = new List<Question>();
            foreach (string question in questions)
            {
                string[] questionSplit = question.Trim().Split('\n');
                if (questionSplit.Length == 6)
                {
                    Question newQuestion = new Question(questionSplit[0], questionSplit[1].Remove(0, 2), questionSplit.Skip(2).ToArray());
                    listOfQuestions.Add(newQuestion);
                }
            }

            Shuffle();
        }

        // Uses the client callback objects to send current Shoe information 
        // to clients. If the change in teh Shoe state was triggered by a method call 
        // from a specific client, then that particular client will be excluded from
        // the update since it will already be updated directly by the call.
        private void updateClients(bool restart)
        {
            // Identify which client just changed the Shoe object's state
            ICallback thisClient = null;
            if (OperationContext.Current != null)
                thisClient = OperationContext.Current.GetCallbackChannel<ICallback>();

            // Prepare the CallbackInfo parameter
            CallbackInfo info = new CallbackInfo(questions, numQuestions, timePerQuestion, restart);

            // Update all clients except thisClient
            foreach (ICallback cb in callbacks)
                if (thisClient == null || thisClient != cb)
                    cb.UpdateClient(info);
        }


    } // end Shoe class
}
