using System.Windows.Forms.Integration;
using System.IO;
using Microsoft.Win32;
using WArchiveTools.FileSystem;
using OpenTK;
using System.ComponentModel;

namespace OpenTKFramework.src.Viewmodel
{
    partial class ViewModel : INotifyPropertyChanged
    {
        #region string WindowTitle
        private string m_windowTitle;

        public string WindowTitle
        {
            get { return string.Format("{0} - OpenTK with WPF Framework", m_windowTitle); }
            set
            {
                if (m_windowTitle != value)
                {
                    m_windowTitle = value;
                    NotifyPropertyChanged();
                }
            }
        }
        #endregion

        private VirtualFilesystemDirectory m_loadedRarc;
        private Renderer m_renderer;

        internal void CreateGraphicsContext(GLControl ctrl, WindowsFormsHost host)
        {
            m_renderer = new Renderer(ctrl, host);
        }
        
        internal void Open()
        {
            OpenFileDialog openFile = new OpenFileDialog();

            if ((bool)openFile.ShowDialog())
            {
                string fileName = openFile.FileName;
                WindowTitle = fileName;

                switch(Path.GetExtension(fileName))
                {
                    case ".arc":
                        LoadArchive(fileName);
                        break;
                }
            }
        }

        internal void LoadArchive(string fileName)
        {
            m_loadedRarc = WArchiveTools.ArchiveUtilities.LoadArchive(fileName);
        }
    }
}
