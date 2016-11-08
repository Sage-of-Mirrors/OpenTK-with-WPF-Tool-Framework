using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows;
using System.Windows.Input;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace OpenGL_in_WPF_Framework
{
    public enum ShaderAttributeIds
    {
        Position, Color,
        TexCoord, Normal
    }

    class Renderer
    {
        public Camera Cam;

        public List<IRenderable> RenderableObjs { get { return m_renderableObjs; } set { m_renderableObjs = value; } }
        private List<IRenderable> m_renderableObjs;
        
        private Timer m_intervalTimer;

        private GLControl m_control;

        private int _programID;
        private int _uniformMVP;
        private int _uniformColor;

        private Matrix4 ViewMatrix;
        private Matrix4 ProjectionMatrix;

        private Color4 debugRayColor = Color4.Yellow;

        #region Construction

        public Renderer(GLControl context, WindowsFormsHost host)
        {
            m_control = context;
            context.Width = (int)host.Width;
            context.Height = (int)host.Height;

            Cam = new Camera();

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

                if (host.IsFocused)
                {
                    Console.WriteLine(string.Format("Focused! {0}", mousePosGlobal.X));
                    Cam.Update();
                }

                Draw();
            };

            m_control.MouseUp += m_control_MouseUp;

            m_control.MouseDown += m_control_MouseDown;

            m_control.MouseMove += m_control_MouseMove;

            host.KeyUp += host_KeyUp;

            host.KeyDown += host_KeyDown;

            host.LayoutUpdated += host_LayoutUpdated;
        }

        private void SetUpViewport()
        {
            _programID = GL.CreateProgram();

            Cam = new Camera();

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

        private void LoadShader(string fileName, ShaderType type, int program, out int address)
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

        #endregion

        #region Rendering

        private void Draw()
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

            ViewMatrix = Cam.ViewMatrix;
            ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(65), width / height, 100, 500000);

            //Render stuff goes here

            RenderDebugCube();

            m_control.SwapBuffers();
        }

        private void RenderDebugTri()
        {
            Matrix4 modelMatrix = Matrix4.CreateTranslation(new Vector3(0, 0, 0)) * Matrix4.CreateFromQuaternion(Quaternion.Identity) * Matrix4.CreateScale(1);

            Matrix4 finalMatrix = modelMatrix * ViewMatrix * ProjectionMatrix;

            GL.UniformMatrix4(_uniformMVP, false, ref finalMatrix);

            GL.Uniform4(_uniformColor, debugRayColor);

            GL.Begin(PrimitiveType.Triangles);
            GL.Vertex3(0, 200, 0);
            GL.Vertex3(200, 0, 0);
            GL.Vertex3(-200, 0, 0);

            GL.End();
        }

        private void RenderDebugCube()
        {
            Matrix4 modelMatrix = Matrix4.CreateTranslation(new Vector3(0, 0, 0)) * Matrix4.CreateFromQuaternion(Quaternion.Identity) * Matrix4.CreateScale(1);

            Matrix4 finalMatrix = modelMatrix * ViewMatrix * ProjectionMatrix;

            GL.UniformMatrix4(_uniformMVP, false, ref finalMatrix);

            GL.Uniform4(_uniformColor, debugRayColor);

            GL.Begin(PrimitiveType.Triangles);
            GL.Vertex3(-25f, -25f, -25f);
            GL.Vertex3(-25f, 25f, 25f);
            GL.Vertex3(-25f, 25f, -25f);

            GL.Vertex3(-25f, -25f, -25f);
            GL.Vertex3(-25f, -25f, 25f);
            GL.Vertex3(-25f, 25f, 25f);

            GL.Vertex3(25f, -25f, -25f);
            GL.Vertex3(25f, 25f, -25f);
            GL.Vertex3(25f, 25f, 25f);

            GL.Vertex3(25f, 25f, 25f);
            GL.Vertex3(25f, -25f, 25f);
            GL.Vertex3(25f, -25f, -25f);

            GL.Vertex3(-25f, -25f, -25f);
            GL.Vertex3(25f, 25f, -25f);
            GL.Vertex3(25f, -25f, -25f);

            GL.Vertex3(-25f, -25f, -25f);
            GL.Vertex3(-25f, 25f, -25f);
            GL.Vertex3(25f, 25f, -25f);

            GL.Vertex3(-25f, -25f, 25f);
            GL.Vertex3(25f, -25f, 25f);
            GL.Vertex3(25f, 25f, 25f);

            GL.Vertex3(25f, 25f, 25f);
            GL.Vertex3(-25f, 25f, 25f);
            GL.Vertex3(-25f, -25f, 25f);

            GL.Vertex3(25f, 25f, -25f);
            GL.Vertex3(-25f, 25f, -25f);
            GL.Vertex3(25f, 25f, 25f);

            GL.Vertex3(25f, 25f, 25f);
            GL.Vertex3(-25f, 25f, -25f);
            GL.Vertex3(-25f, 25f, 25f);

            GL.Vertex3(-25f, -25f, -25f);
            GL.Vertex3(25f, -25f, -25f);
            GL.Vertex3(25f, -25f, 25f);

            GL.Vertex3(-25f, -25f, -25f);
            GL.Vertex3(25f, -25f, 25f);
            GL.Vertex3(-25f, -25f, 25f);

            /*
            GL.Vertex3(-25f, -25f, -25f);
            GL.Vertex3(25f, -25f, -25f);
            GL.Vertex3(25f, 25f, -25f);
            GL.Vertex3(-25f, 25f, -25f);
            GL.Vertex3(-25f, -25f, 25f);
            GL.Vertex3(25f, -25f, 25f);
            GL.Vertex3(25f, 25f, 25f);
            GL.Vertex3(-25f, 25f, 25f);
            */

            GL.End();
        }

        #endregion

        #region Events

        void m_control_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            Input.Internal_SetMouseBtnState(e.Button, false);
        }

        void m_control_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            Input.Internal_SetMouseBtnState(e.Button, true);
        }

        void m_control_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            debugRayColor = Cam.CastRay(e.X, e.Y, m_control.Width, m_control.Height, ProjectionMatrix);
        }

        void host_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            Input.Internal_SetKeyState((Keys)KeyInterop.VirtualKeyFromKey(e.Key), false);
        }

        void host_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            Input.Internal_SetKeyState((Keys)KeyInterop.VirtualKeyFromKey(e.Key), true);
        }

        void host_LayoutUpdated(object sender, EventArgs e)
        {
            GL.Viewport(0, 0, m_control.Width, m_control.Height);
        }

        #endregion
    }
}
