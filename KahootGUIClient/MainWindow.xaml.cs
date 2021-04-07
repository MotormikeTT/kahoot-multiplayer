/*
 * Program:         CardsGuiClient.exe
 * Module:          MainWindow.xaml.cs
 * Author:          T. Haworth
 * Date:            March 9, 2021
 * Description:     A Windows WPF client that uses CardsLibrary.dll via a WCF service.
 * 
 *                  Note that we had to add a reference to the .NET Framework 
 *                  assembly System.ServiceModel.dll.
 *                  
 * Modifications:   Mar 16, 2021
 *                  The client now receives a Card object from the Shoe's Draw() method 
 *                  now returns a  instead of a string because Card is now a data 
 *                  contract. Now uses an administrative endpoint which is configured
 *                  in the project's App.config file.
 *                  
 *                  Mar 23, 2021
 *                  Implements the ICallback contract and registers (and unregisters) for 
 *                  the callbacks service so that the client will reflect real time updates 
 *                  about the state of the Shoe.
 */

using System;
using System.Windows;
using System.ServiceModel;  // WCF  types
using KahootLibrary;

namespace CardsGUIClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [CallbackBehavior(ConcurrencyMode=ConcurrencyMode.Reentrant, UseSynchronizationContext=false)]
    public partial class MainWindow : Window, ICallback 
    {
        // Private member variables
        private IGame game = null;

        // C'tor
        public MainWindow()
        {
            InitializeComponent();

            // Note that the client no longer creates a Shoe object locally!

            // Connect to the WCF service endpoint 
            DuplexChannelFactory<IGame> channel = new DuplexChannelFactory<IGame>(this, "KahootEndPoint");
            game = channel.CreateChannel();

            // The variable "shoe" above does NOT reference a Shoe object 
            // directly. Instead it references a local "transparent proxy" object which 
            // implements the IShoe interface for the purpose of communicating
            // all client requests to the service host application

            // Register for the callback service
            game.RegisterForCallbacks();

            // Initialize the GUI
            sliderQuestions.Minimum = 1;
            sliderQuestions.Maximum = 20;
            sliderQuestions.Value = game.NumQuestions;
            txtCategories.ItemsSource = game.Categories;
            //updateCardCounts(false);

        } // end default C'tor

        private void btnReady_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ///RUN SOMETHING
                game.RegisterPlayer(txtPlayerName.Text);

                // add to list of players
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        } // end btnReady_Click()

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ///RUN SOMETHING
                game.RegisterPlayer(txtPlayerName.Text);

                // Update the GUI
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        } // end btnStart_Click()

        private void btnAnswer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ///RUN SOMETHING

                // Update the GUI
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        } // end btnAnswer_Click()

        private void sliderQuestions_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                if (game != null)
                {
                    game.NumQuestions = (int)sliderQuestions.Value;

                    // update GUI
                    txtQuestionCount.Text = $"{sliderQuestions.Value} Questions";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        } // end sliderQuestions_ValueChanged()


        private void txtCategories_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                if (game != null)
                {
                    game.Category = sender.ToString();

                    // update GUI
                    txtQuestionCount.Text = $"{sliderQuestions.Value} Questions";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        } // end txtCategories_SelectionChanged()

        private void txtTimePerQuestion_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            try
            {
                if (game != null)
                {
                    game.TimePerQuestion = int.Parse(sender.ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        } // end txtTimePerQuestion_TextChanged()

        // Helper methods

        //private void updateCardCounts(bool emptyHand)
        //{
        //    if (emptyHand)
        //        // Only "throw out" drawn cards if the Shoe was shuffled 
        //        // or the number of decks was changed
        //        lstCards.Items.Clear();

        //    txtHandCount.Text = lstCards.Items.Count.ToString();
        //    txtShoeCount.Text = shoe.NumCards.ToString();
        //} // end updateCardCounts()

        //// Event handlers

        //private void btnDraw_Click(object sender, RoutedEventArgs e)
        //{
        //    try 
        //    {
        //        Card card = shoe.Draw();

        //        // Update the GUI
        //        lstCards.Items.Insert(0, card);
        //        updateCardCounts(false);
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message);
        //    }
        //} // end btnDraw_Click()

        //private void btnShuffle_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        shoe.Shuffle();

        //        // Update the GUI
        //        updateCardCounts(true);
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message);
        //    }
        //} // end btnShuffle_Click()

        //private void sliderDecks_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        //{
        //    try
        //    {
        //        if (shoe != null)
        //        {
        //            // Reset the number of decks
        //            shoe.NumDecks = (int)sliderDecks.Value;

        //            // Update the GUI
        //            updateCardCounts(true);
        //            int n = shoe.NumDecks;
        //            txtDeckCount.Text = n + " deck" + (n == 1 ? "" : "s");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message);
        //    }
        //} // end sliderDecks_ValueChanged()

        //private void btnClose_Click(object sender, RoutedEventArgs e)
        //{
        //    this.Close();
        //} // end btnClose_Click()


        private delegate void ClientUpdateDelegate(CallbackGameRulesInfo info);

        public void UpdateClient(CallbackGameRulesInfo info)
        {
            if (System.Threading.Thread.CurrentThread == this.Dispatcher.Thread)
            {
                // Update the GUI
                //txtShoeCount.Text = info.NumCards.ToString();
                //sliderDecks.Value = info.NumDecks;
                //txtDeckCount.Text = info.NumDecks + " deck" + (info.NumDecks == 1 ? "" : "s");
                //if (info.EmptyTheHand)
                //{
                //    lstCards.Items.Clear();
                //    txtHandCount.Text = "0";
                //}
            }
            else
            {
                // Not the dispatcher thread that's calling this method!
                this.Dispatcher.BeginInvoke(new ClientUpdateDelegate(UpdateClient), info);
            }
        }

        public void UpdateGameRules(CallbackGameRulesInfo info)
        {
            if (System.Threading.Thread.CurrentThread == this.Dispatcher.Thread)
            {
                sliderQuestions.Value = info.NumQuestions;
                txtQuestionCount.Text = $"{info.NumQuestions} Questions";
                txtTimePerQuestion.Text = info.TimePerQuestion.ToString();
                txtTimer.Text = $"{info.TimePerQuestion}s";
                txtCategories.Text = info.Category;
                lstPlayers.ItemsSource = info.Players;
                btnStart.Visibility = info.GameHost ? Visibility.Visible : Visibility.Hidden;
                btnReady.Visibility = info.GameHost ? Visibility.Hidden : Visibility.Visible;
            }
            else
            {
                // Not the dispatcher thread that's calling this method!
                this.Dispatcher.BeginInvoke(new ClientUpdateDelegate(UpdateGameRules), info);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            game?.UnregisterFromCallbacks();
        }

    } // end MainWindow class
}
