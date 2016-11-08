using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Forms.Integration;
using System.IO;
using Microsoft.Win32;
using GameFormatReader.Common;
using WArchiveTools.FileSystem;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace OpenGL_in_WPF_Framework
{
    class ViewModel
    {
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
