using System;
using System.Diagnostics;
using System.Windows;

namespace VirtualizingDataTest
{
    public partial class App : Application
    {
        public App()
        {
            UnhandledException += Application_UnhandledException;

            InitializeComponent();
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
            {
	            Debug.WriteLine(e.ExceptionObject);
                // An unhandled exception has occurred, break in the debugger
				//Debugger.Break();
            }
            else
            {
                // By default show the error
                e.Handled = true;
                MessageBox.Show(e.ExceptionObject.Message + Environment.NewLine + e.ExceptionObject.StackTrace,
                    "Error", MessageBoxButton.OK);
            }
        }
    }
}