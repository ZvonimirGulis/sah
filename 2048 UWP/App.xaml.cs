using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace _2048_UWP
{
    /// <summary>
    /// Provides application-specific behavior to complement the default application class.
    /// </summary>
    sealed partial class App : Application
    {
 
        public App()
        {
            Microsoft.ApplicationInsights.WindowsAppInitializer.InitializeAsync(
                Microsoft.ApplicationInsights.WindowsCollectors.Metadata |
                Microsoft.ApplicationInsights.WindowsCollectors.Session);
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

   
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            // Override default minimum size.
            var view = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
            view.SetPreferredMinSize(new Size { Width = 320, Height = 480 });

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat application initialization when the window already contains content, repeat application initialization when the window is already contained,
            // Just make sure the window is active
            if (rootFrame == null)
            {
                //  Create a frame to act as a navigation context and navigate to the first page
                rootFrame = new Frame();
                //page navigation
                rootFrame.Navigated += delegate
                {
                    SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility
                    = rootFrame.CanGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
                };
                SystemNavigationManager.GetForCurrentView().BackRequested += (sender, args) =>
                {
                    if (rootFrame.CanGoBack)
                    {
                        args.Handled = true;
                        rootFrame.GoBack();
                    }
                };
                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated || e.PreviousExecutionState == ApplicationExecutionState.ClosedByUser)
                {
                    //Load state from previously suspended application
                    UserData.Load();
                }

                //  Put the frame in the current window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                //  When the navigation stack has not been restored, navigate to the first page,
                // and configure it by passing in the required information as navigation parameters
                // parameter
                 rootFrame.Navigate(typeof(MainPage), e.Arguments);
            }

            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            {
                Windows.Phone.UI.Input.HardwareButtons.BackPressed += (s1, e1) =>
                {
                    if (rootFrame != null)
                    {
                        if (rootFrame.CanGoBack)
                        {
                            e1.Handled = true;
                            rootFrame.GoBack();
                        }
                    }
                };
            }

            // Make sure the current window is active
            Window.Current.Activate();
        }

        /// <summary>
        ///Called when navigating to a specific page fails
        /// </summary>
        ///<param name="sender">Navigation failed frame</param>
        ///<param name="e">Details about navigation failures</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Called when application execution is about to be suspended. without knowing the application
        ///No need to know if the application will be terminated or resumed,
        /// and leave the memory contents unchanged.
        /// </summary>
        /// <param name="sender">The source of the pending request。</param>
        /// <param name="e"></param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            UserData.Save();
            deferral.Complete();
        }

        public class UserData
        {
            private static UserData _userData = null;
            static UserData() { _userData = new UserData(); }
            public static UserData CurrentInstance { get { return _userData; } }

            string score = "0", best = "0"; int[,] num = new int[4, 4];
            public string Score { get { return score; } set { score = value; } }
            public string Best { get { return best; } set { best = value; } }
            public int[,] Num { get { return num; } set { num = value; } }

            byte[,] accent = new byte[2, 4];
            public byte[,] AccentAndBg { get { return accent; } set { accent = value; } }
            public int Nth { get; set; }

            public static void Load()
            {
                Debug.WriteLine("Loading data...");
                var rs = ApplicationData.Current.RoamingSettings;
                object v = null;
                if (rs.Values.TryGetValue("score", out v)) { CurrentInstance.Score = (string)v; } else { CurrentInstance.Score = "0"; }
                if (rs.Values.TryGetValue("best", out v)) { CurrentInstance.Best = (string)v; } else { CurrentInstance.Best = "0"; }
                if (rs.Values.TryGetValue("nth", out v)) { CurrentInstance.Nth = (int)v; } else { CurrentInstance.Nth = 0; }
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        if (rs.Values.TryGetValue(i + "" + j, out v)) { CurrentInstance.Num[i, j] = (int)v; }
                        else { CurrentInstance.Num[i, j] = 0; }
                        if (i < 2)
                        {
                            if (rs.Values.TryGetValue(i + "a" + j, out v)) { CurrentInstance.AccentAndBg[i, j] = (byte)v; }

                        }
                    }
                }//for
                byte txtr = 0, txtg = 0, txtb = 0;
                if (rs.Values.TryGetValue("txtr", out v)) { txtr = (byte)v; }
                if (rs.Values.TryGetValue("txtg", out v)) { txtg = (byte)v; }
                if (rs.Values.TryGetValue("txtb", out v)) { txtb = (byte)v; }
                (Current.Resources["txt"] as SolidColorBrush).Color = Color.FromArgb(255, txtr, txtg, txtb);

            }//load

            public static void Save()
            {
                Debug.WriteLine("Spremanje podataka");
                var rs = ApplicationData.Current.RoamingSettings;
                rs.Values["score"] = CurrentInstance.Score;
                rs.Values["best"] = CurrentInstance.best;
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        rs.Values[i + "" + j] = CurrentInstance.Num[i, j];
                        if (i < 2)
                        {
                            rs.Values[i + "a" + j] = CurrentInstance.AccentAndBg[i, j];
                        }
                    }
                }
                rs.Values["txtr"] = (Current.Resources["txt"] as SolidColorBrush).Color.R;
                rs.Values["txtg"] = (Current.Resources["txt"] as SolidColorBrush).Color.G;
                rs.Values["txtb"] = (Current.Resources["txt"] as SolidColorBrush).Color.B;
                rs.Values["nth"] = CurrentInstance.Nth;
            }//save
        }
    }

}
