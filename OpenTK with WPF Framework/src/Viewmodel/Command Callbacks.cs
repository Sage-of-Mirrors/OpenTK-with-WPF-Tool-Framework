/* This file contains the callbacks that the menu bar at the top of 
 * the main window uses to do things when the options are clicked.
 * It also has the methods that the OnRequestApplication Exit,
 * OnRequestReportBug, and OnRequestOpenWiki callbacks use. */

using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace OpenTKFramework.src.Viewmodel
{
    partial class ViewModel : INotifyPropertyChanged
    {
        /// <summary> The user has requested to open a file. </summary>
        public ICommand OnRequestOpenFile
        {
            get { return new RelayCommand(x => Open()); }
        }

        /// <summary> The user has requested to save the currently open data back to the file they originally opened. </summary>
        public ICommand OnRequestSave
        {
            get { return new RelayCommand(x => Open(), x => false); }
        }

        /// <summary> The user has requested to save the currently open data to a new file. </summary>
        public ICommand OnRequestSaveAs
        {
            get { return new RelayCommand(x => Open(), x => false); }
        }

        /// <summary> The user has requested to unload the currently open data. </summary>
        public ICommand OnRequestClose
        {
            get { return new RelayCommand(x => Open(), x => false); }
        }

        /// <summary> The user has pressed Alt + F4, chosen Exit from the File menu, or clicked the close button. </summary>
        public ICommand OnRequestApplicationExit
        {
            get { return new RelayCommand(x => ExitApplication()); }
        }

        /// <summary> The user has clicked Report a Bug... from the Help menu. </summary>
        public ICommand OnRequestReportBug
        {
            get { return new RelayCommand(x => ReportBug()); }
        }

        /// <summary> The user has clicked Report a Bug... from the Help menu. </summary>
        public ICommand OnRequestOpenWiki
        {
            get { return new RelayCommand(x => OpenWiki()); }
        }

        /// <summary>
        /// Exits the application.
        /// </summary>
        private void ExitApplication()
        {
            Application.Current.MainWindow.Close();
        }

        /// <summary>
        /// Opens the user's default browser to OpenGL_in_WPF_Framework's Issues page.
        /// </summary>
        private void ReportBug()
        {
            System.Diagnostics.Process.Start("https://github.com/Sage-of-Mirrors/OpenTK_with_WPF_Framework/issues");
        }

        /// <summary>
        /// Opens the user's default browser to OpenGL_in_WPF_Framework's Wiki page.
        /// </summary>
        private void OpenWiki()
        {
            System.Diagnostics.Process.Start("https://github.com/Sage-of-Mirrors/OpenTK_with_WPF_Framework/wiki");
        }
    }
}
