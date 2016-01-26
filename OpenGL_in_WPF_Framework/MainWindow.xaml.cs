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
using System.Windows.Shapes;
using OpenTK;
using OpenTK.Graphics;

namespace OpenGL_in_WPF_Framework
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GLControl m_glControl;
        private ViewModel m_viewModel;

        public MainWindow()
        {
            m_viewModel = new ViewModel();

            InitializeComponent();
        }

        private void GLHost_Initialized(object sender, EventArgs e)
        {
            m_glControl = new GLControl(new GraphicsMode(32,24), 3, 0, GraphicsContextFlags.Default);
            m_glControl.MakeCurrent();
            m_glControl.MouseDown += m_glControl_MouseDown;
            m_glControl.MouseUp += m_glControl_MouseUp;
            m_glControl.Dock = System.Windows.Forms.DockStyle.Fill;
            m_glControl.AllowDrop = true;
            m_glControl.BackColor = System.Drawing.Color.Black;
            m_viewModel.OnGraphicsContextInitialized(m_glControl, GLHost);

            GLHost.Child = m_glControl;
            GLHost.AllowDrop = true;
        }

        void m_glControl_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            m_viewModel.SetMouseState(e.Button, true);
        }

        void m_glControl_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            m_viewModel.SetMouseState(e.Button, false);
        }

        private void GLHost_KeyDown(object sender, KeyEventArgs e)
        {
            m_viewModel.SetKeyboardState((System.Windows.Forms.Keys)KeyInterop.VirtualKeyFromKey(e.Key), true);
        }

        private void GLHost_KeyUp(object sender, KeyEventArgs e)
        {
            m_viewModel.SetKeyboardState((System.Windows.Forms.Keys)KeyInterop.VirtualKeyFromKey(e.Key), false);
        }

        private void GLHost_LayoutUpdated(object sender, EventArgs e)
        {
            if (m_glControl != null)
            {
                m_viewModel.ResizeViewport();
            }
        }
    }
}
