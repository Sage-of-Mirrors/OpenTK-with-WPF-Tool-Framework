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
using System.Windows.Forms.Integration;
using System.IO;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace OpenGL_in_WPF_Framework
{
    class ViewModel
    {
        private System.Windows.Forms.Timer m_intervalTimer;
        private GLControl m_control;

        private Camera m_cam;

        private int _programID;
        private int _uniformMVP;
        private int _uniformColor;

        private Matrix4 ViewMatrix;

        private Matrix4 ProjMatrix;

        internal void OnGraphicsContextInitialized(GLControl context, WindowsFormsHost host)
        {
            m_control = context;

            m_cam = new Camera();

            SetUpViewport();

            m_intervalTimer = new System.Windows.Forms.Timer();
            m_intervalTimer.Interval = 16; // 60 FPS roughly
            m_intervalTimer.Enabled = true;
            m_intervalTimer.Tick += (args, o) =>
            {
                Vector2 mousePosGlobal = new Vector2(System.Windows.Forms.Control.MousePosition.X, System.Windows.Forms.Control.MousePosition.Y);
                Vector2 glControlPosGlobal = new Vector2((float)host.PointToScreen(new Point(0, 0)).X, (float)host.PointToScreen(new Point(0, 0)).Y);

                Input.Internal_SetMousePos(new Vector2(System.Windows.Forms.Control.MousePosition.X, System.Windows.Forms.Control.MousePosition.Y));

                Input.Internal_UpdateInputState();

                m_cam.Update();

                Draw();
            };
        }

        internal void SetMouseState(System.Windows.Forms.MouseButtons mouseButton, bool down)
        {
            Input.Internal_SetMouseBtnState(mouseButton, down);
        }

        internal void SetKeyboardState(System.Windows.Forms.Keys key, bool down)
        {
            Input.Internal_SetKeyState(key, down);
        }

        public enum ShaderAttributeIds
        {
            Position, Color,
            TexCoord, Normal
        }

        internal void SetUpViewport()
        {
            _programID = GL.CreateProgram();

            m_cam = new Camera();

            int vertShaderId, fragShaderId;
            LoadShader("vs.glsl", ShaderType.VertexShader, _programID, out vertShaderId);
            LoadShader("fs.glsl", ShaderType.FragmentShader, _programID, out fragShaderId);

            GL.DeleteShader(vertShaderId);
            GL.DeleteShader(fragShaderId);

            GL.BindAttribLocation(_programID, (int)ShaderAttributeIds.Position, "vertexPos");

            GL.LinkProgram(_programID);

            _uniformMVP = GL.GetUniformLocation(_programID, "modelview");
            _uniformColor = GL.GetUniformLocation(_programID, "col");

            if (GL.GetError() != ErrorCode.NoError)
                Console.WriteLine(GL.GetProgramInfoLog(_programID));
        }

        protected void LoadShader(string fileName, ShaderType type, int program, out int address)
        {
            address = GL.CreateShader(type);
            using (var streamReader = new StreamReader(fileName))
            {
                GL.ShaderSource(address, streamReader.ReadToEnd());
            }

            GL.CompileShader(address);

            GL.AttachShader(program, address);

            int compileSuccess;
            GL.GetShader(address, ShaderParameter.CompileStatus, out compileSuccess);

            if (compileSuccess == 0)
                Console.WriteLine(GL.GetShaderInfoLog(address));
        }

        internal void ResizeViewport()
        {
            GL.Viewport(0, 0, m_control.Width, m_control.Height);
        }

        internal void Draw()
        {
            GL.ClearColor(new Color4(.36f, .25f, .94f, 1f));
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(_programID);

            GL.Enable(EnableCap.DepthTest);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            float width, height;

            if (m_control.Width == 0)
                width = 1f;

            else
                width = m_control.Width;

            if (m_control.Height == 0)
                height = 1f;

            else
                height = m_control.Height;

            ViewMatrix = m_cam.ViewMatrix;
            ProjMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(65), width / height, 100, 500000);

            //Render stuff goes here

            RenderDebugTri();

            m_control.SwapBuffers();
        }

        internal void RenderDebugTri()
        {
            Matrix4 modelMatrix = Matrix4.CreateTranslation(new Vector3(0, 0, 0)) * Matrix4.Rotate(Quaternion.Identity) * Matrix4.Scale(1);

            Matrix4 finalMatrix = modelMatrix * ViewMatrix * ProjMatrix;

            GL.UniformMatrix4(_uniformMVP, false, ref finalMatrix);

            GL.Uniform4(_uniformColor, Color4.Yellow);

            GL.Begin(PrimitiveType.Triangles);
            GL.Vertex3(10, 20, 0);
            GL.Vertex3(100, 20, 0);
            GL.Vertex3(100, 50, 0);

            GL.End();
        }
    }
}
