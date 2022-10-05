using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Text.Json;
using System.IO;
using Currency_Data;
using System.Windows.Threading;
using System.Media;
using System.Globalization;
using System.Threading.Tasks;

namespace Currency_Converter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            InrTextBox.Focus();

            LoadJson();
        }


        int focusFlag = 1;
        int ptFlag = 0;
        string curr_InEu = "INR-EUR";
        string curr_InUs = "INR-USD";
        string curr_EuUs = "EUR-USD";
        Dictionary<string, double> exRates = new Dictionary<string, double>();
        Dictionary<string, double> exRates_temp = new Dictionary<string, double>();



        private async Task LoadJson()
        {
            if (!File.Exists(@".\Exchange_Rates.json"))
            {
                exRates.Add(curr_InEu, 0.0123);
                exRates.Add(curr_InUs, 0.0126);
                exRates.Add(curr_EuUs, 1.021);
            }
            else
            {
                //string jsonString = await File.ReadAllTextAsync(@".\Exchange_Rates.json");
                using FileStream readStream = File.OpenRead(@".\Exchange_Rates.json");
                {
                    exRates = await JsonSerializer.DeserializeAsync<Dictionary<string, double>>(readStream);
                }
            }
        }



        private void InrTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(InrTextBox.Text) && focusFlag == 1)
            {
                double inr = Convert.ToDouble(InrTextBox.Text);

                double _InEu = Math.Round((inr * exRates[curr_InEu]), 2);
                double _InUs = Math.Round((inr * exRates[curr_InUs]), 2);


                EurTextBox.Text = _InEu.ToString("N2", new CultureInfo("en-US"));
                UsdTextBox.Text = _InUs.ToString("N2", new CultureInfo("en-US"));

            }
        }

        private void EurTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(EurTextBox.Text) && focusFlag == 2)
            {
                double eur = Convert.ToDouble(EurTextBox.Text);

                double _EuIn = Math.Round((eur / exRates[curr_InEu]), 2);
                double _EuUs = Math.Round((eur * exRates[curr_EuUs]), 2);


                InrTextBox.Text = _EuIn.ToString("N2", new CultureInfo("en-IN"));
                UsdTextBox.Text = _EuUs.ToString("N2", new CultureInfo("en-US"));
            }
        }

        private void UsdTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(UsdTextBox.Text) && focusFlag == 3)
            {
                double usd = Convert.ToDouble(UsdTextBox.Text);

                double _UsIn = Math.Round((usd / exRates[curr_InUs]), 2);
                double _UsEu = Math.Round((usd / exRates[curr_EuUs]), 2);


                InrTextBox.Text = _UsIn.ToString("N2", new CultureInfo("en-IN"));
                EurTextBox.Text = _UsEu.ToString("N2", new CultureInfo("en-US"));
            }

        }



        private void InrTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            focusFlag = 1;
            InrTextBox.SelectAll();
        }

        private void EurTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            focusFlag = 2;
            EurTextBox.SelectAll();
        }

        private void UsdTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            focusFlag = 3;
            UsdTextBox.SelectAll();
        }




        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateButton.Visibility = Visibility.Hidden;
            UpdatingButton.Visibility = Visibility.Visible;
            ProgressBar.Visibility = Visibility.Visible;

            var progressHandler = new Progress<double>(value => ProgressBar.Value = value);
            var progress = progressHandler as IProgress<double>;


            exRates_temp = await GetExchangeRates.Main_Exchange(progress);


            if (exRates_temp.Values.ElementAt(0) > 0.0)
            {
                UpdatingButton.Visibility = Visibility.Hidden;
                UpdatedButton.Visibility = Visibility.Visible;
                ProgressBar.Visibility = Visibility.Hidden;

                // Updating exRates Dict only if the update was successful
                exRates = exRates_temp;


                // Hide Updated button after 1 interval of timer
                DispatcherTimer timer = new DispatcherTimer();

                timer.Tick += new EventHandler((object sender, EventArgs e) =>
                {
                    UpdatedButton.Visibility = Visibility.Hidden;
                    UpdateButton.Visibility = Visibility.Visible;
                });
                
                timer.Interval = new TimeSpan(0, 0, 5);
                timer.Start();

                // Saving the updated rates to json file
                using FileStream writeStream = File.Create(@".\Exchange_Rates.json");
                {
                    await JsonSerializer.SerializeAsync(writeStream, exRates, new JsonSerializerOptions { WriteIndented = true });
                }
            }
            else
            {
                UpdatingButton.Visibility = Visibility.Hidden;
                ProgressBar.Visibility = Visibility.Hidden;
                UpdateButton.Visibility = Visibility.Visible;

                string messageBoxText = "Update failed! Please check your Internet Connection and try again.";
                string caption = "Update Failed";
                MessageBoxResult result;

                result = MessageBox.Show(messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }




        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (focusFlag == 1)
            {
                if (string.IsNullOrWhiteSpace(InrTextBox.Text))
                {
                    ptFlag = 0;
                }
                else
                {
                    ptFlag = InrTextBox.Text.Split('.').Length - 1; 
                }
            }

            else if (focusFlag == 2)
            {
                if (string.IsNullOrWhiteSpace(EurTextBox.Text))
                {
                    ptFlag = 0;
                }
                else
                {
                    ptFlag = EurTextBox.Text.Split('.').Length - 1;
                }
            }

            else if (focusFlag == 3)
            {
                if (string.IsNullOrWhiteSpace(UsdTextBox.Text))
                {
                    ptFlag = 0;
                }
                else
                {
                    ptFlag = UsdTextBox.Text.Split('.').Length - 1;
                }
            }

            if (e.Key == Key.OemPeriod || e.Key == Key.Decimal)
            {
                ptFlag++;
            }


            // Defining the keygestures
            KeyGesture copy = new KeyGesture(Key.C, ModifierKeys.Control);
            KeyGesture paste = new KeyGesture(Key.V, ModifierKeys.Control);
            KeyGesture clipboard = new KeyGesture(Key.V, ModifierKeys.Windows);
            KeyGesture close = new KeyGesture(Key.F4, ModifierKeys.Alt);


            if (!((e.Key == Key.OemPeriod
                || (e.Key >= Key.D0 && e.Key <= Key.D9) 
                || e.Key == Key.Decimal
                || e.Key == Key.Back 
                || e.Key == Key.Delete
                || (e.Key >= Key.Left && e.Key <= Key.Down)
                || e.Key == Key.Tab
                || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
                || e.Key == Key.NumLock 
                || e.Key == Key.LeftShift 
                || e.Key == Key.RightShift 
                || e.Key == Key.LeftCtrl
                || e.Key == Key.RightCtrl
                || e.Key == Key.Home
                || e.Key == Key.End
                || e.Key == Key.LeftAlt 
                || e.Key == Key.RightAlt
                || e.Key == Key.LWin
                || e.Key == Key.OemComma
                || copy.Matches(null, e)
                || paste.Matches(null, e)
                || clipboard.Matches(null, e)
                || close.Matches(null, e)) && ptFlag <= 1))
            {

                SystemSounds.Exclamation.Play();
                if (e.Key == Key.OemPeriod || e.Key == Key.Decimal)
                {
                    ptFlag--;
                }
                e.Handled = true;


                DispatcherTimer ColTimer = new();
                ColTimer.Interval = new TimeSpan(0,0,1);


                // Showing error exclamation and sound depending on which textbox is in focus (flag value)
                if (focusFlag == 1)
                {
                    InrTextBox.Background = Brushes.IndianRed;
                    ColTimer.Tick += (source, eg) => { InrTextBox.Background = Brushes.White ; ColTimer.Stop(); };
                    ColTimer.Start();
                }

                else if (focusFlag == 2)
                {
                    EurTextBox.Background = Brushes.IndianRed;
                    ColTimer.Tick += (source, eg) => { EurTextBox.Background = Brushes.White; ColTimer.Stop(); };
                    ColTimer.Start();
                }

                else if (focusFlag == 3)
                {
                    UsdTextBox.Background = Brushes.IndianRed;
                    ColTimer.Tick += (source, eg) => { UsdTextBox.Background = Brushes.White; ColTimer.Stop(); };
                    ColTimer.Start();
                }

            }
        }
    }
}
