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
            comboCategories.ItemsSource = game.Categories;

            // by default
            btnStart.Visibility = Visibility.Hidden;
            sliderQuestions.IsEnabled = false;
            txtQuestionCount.IsEnabled = false;
            txtTimePerQuestion.IsEnabled = false;
            txtTimer.IsEnabled = false;
            comboCategories.IsEnabled = false;
        } // end default C'tor

        private void btnReady_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ///RUN SOMETHING
                bool registered = game.RegisterPlayer(txtPlayerName.Text);

                if (!registered)
                {
                    txtPlayerName.Text = "";
                    MessageBox.Show("Name is already taken. please use a different one", null, MessageBoxButton.OK);
                }
                else
                    btnReady.IsEnabled = false;
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
                bool registered = game.RegisterPlayer(txtPlayerName.Text);

                if (!registered)
                {
                    txtPlayerName.Text = "";
                    MessageBox.Show("Name is already taken. please use a different one", null, MessageBoxButton.OK);
                }
                else
                {
                    btnStart.IsEnabled = false;
                    game.StartGame();

                    // UPDATE GUI
                    
                }
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


        private void comboCategories_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                if (game != null)
                {
                    game.Category = ((System.Windows.Controls.ComboBox)sender).SelectedValue.ToString();
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
                    game.TimePerQuestion = int.Parse(((System.Windows.Controls.TextBox)sender).Text);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        } // end txtTimePerQuestion_TextChanged()

        private delegate void ClientUpdateInGameDelegate(CallbackInGameInfo info);

        public void UpdateInGame(CallbackInGameInfo info)
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
                this.Dispatcher.BeginInvoke(new ClientUpdateInGameDelegate(UpdateInGame), info);
            }
        }

        private delegate void ClientUpdateGameRulesDelegate(CallbackGameRulesInfo info);

        public void UpdateGameRules(CallbackGameRulesInfo info)
        {
            if (System.Threading.Thread.CurrentThread == this.Dispatcher.Thread)
            {
                sliderQuestions.Value = info.NumQuestions;
                txtQuestionCount.Text = $"{info.NumQuestions} Questions";
                txtTimePerQuestion.Text = info.TimePerQuestion.ToString();
                txtTimer.Text = $"{info.TimePerQuestion}s";
                comboCategories.Text = info.Category;
                lstPlayers.ItemsSource = info.Players;
                if (info.GameHost)
                {
                    btnStart.Visibility = Visibility.Visible;
                    btnReady.Visibility = Visibility.Hidden;
                    sliderQuestions.IsEnabled = true;
                    txtQuestionCount.IsEnabled = true;
                    txtTimePerQuestion.IsEnabled = true;
                    txtTimer.IsEnabled = true;
                    comboCategories.IsEnabled = true;
                }
                if (info.GameStarted)
                {
                    //btnStart.Visibility = Visibility.Visible;
                    //btnReady.Visibility = Visibility.Hidden;
                }
            }
            else
            {
                // Not the dispatcher thread that's calling this method!
                this.Dispatcher.BeginInvoke(new ClientUpdateGameRulesDelegate(UpdateGameRules), info);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            game?.UnregisterFromCallbacks();
        }
    } // end MainWindow class
}
