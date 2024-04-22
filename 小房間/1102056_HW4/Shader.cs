#region License
/*
MIT License
Copyright (c) 2012  iNSANE iDEA  <info@insaneidea.hu>
http://www.insaneidea.hu
All rights reserved.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
#endregion License

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tao.OpenGl;

namespace MyShader
{
    /// <summary>
    /// Class to implement a standalone shader object, consisting of a
    /// vertex shader and a fragment shader.
    /// </summary>
    class Shader
    {

        /// <summary>
        /// Shader program handle used by OpenGL
        /// </summary>
        private int program = 0;

        /// <summary>
        /// Vertex program handle used by OpenGL
        /// </summary>
        private int vsID = 0;

        /// <summary>
        /// Fragment program handle used by OpenGL
        /// </summary>
        private int fsID = 0;

        /// <summary>
        /// Property to access the shader program ID in a read-only way
        /// </summary>
        public int Program
        {
            get
            {
                return program;
            }
        }

        /// <summary>
        /// Set to true to suppress warnings during shader compilation
        /// </summary>
        public bool SupressWarningsInInfoLog { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public Shader()
        {
            SupressWarningsInInfoLog = false;
        }

        /// <summary>
        /// Constructs a shader from the provided sources
        /// </summary>
        /// <param name="pFsSource">The source code of the fragment shader</param>
        /// <param name="pVsSource">The source code of the vertex shader</param>
        public Shader(String pVsSource, String pFsSource)
            : this()
        {
            Build(pVsSource, pFsSource);
        }

        /// <summary>
        /// Builds a shader from two specified strings.
        /// The two strings hold the source code of the vertex
        /// and fragment programs, which will be linked together.
        /// </summary>
        /// <param name="pFsSource">The source code of the fragment shader</param>
        /// <param name="pVsSource">The source code of the vertex shader</param>
        public void Build(String pVsSource, String pFsSource)
        {

            // Build vertex shader
            vsID = Gl.glCreateShader(Gl.GL_VERTEX_SHADER);
            CompileVertexShader(vsID, pVsSource);

            // Build fragment shader
            fsID = Gl.glCreateShader(Gl.GL_FRAGMENT_SHADER);
            CompileFragmentShader(fsID, pFsSource);

            // Build shader program
            program = Gl.glCreateProgram();

            LinkShaderProgram(program, new int[] { vsID, fsID });

            Gl.glDetachShader(program, vsID);
            Gl.glDetachShader(program, fsID);
            Gl.glDeleteShader(vsID);
            Gl.glDeleteShader(fsID);
        }

        /// <summary>
        /// Tries to compile a given vertex shader source code and add it to the supplied
        /// vertex shader ID.
        /// </summary>
        /// <param name="vsID">The shader ID</param>
        /// <param name="pVsSource">The source code of the shader</param>
        /// <exception cref="Exception">If the compilation fails</exception>
        private void CompileVertexShader(int vsID, string pVsSource)
        {
            Gl.glShaderSource(vsID, 1, new String[] { pVsSource }, IntPtr.Zero);
            Gl.glCompileShader(vsID);
            String result = GetShaderInfoLog(vsID);
            if (result != null)
            {
                throw new Exception(String.Format("Error in vertex shader: \n{0}", result));
            }
        }

        /// <summary>
        /// Tries to link together the shaders identified by their supplied IDs
        /// with the given shader program.
        /// </summary>
        /// <param name="program">The ID of the shader program</param>
        /// <param name="shaders">Array of the IDs of the shaders</param>
        /// <exception cref="Exception">If the linking fails</exception>
        public void LinkShaderProgram(int program, int[] shaders)
        {

            foreach (int shader in shaders)
            {
                Gl.glAttachShader(program, shader);
                //Gl.glDeleteShader(shader);
            }
            Gl.glLinkProgram(program);

            String result = GetProgramInfoLog(program);
            if (result != null)
            {
                throw new Exception(String.Format("Linker error in shader program: \n{0}", result));
            }
        }



        /// <summary>
        /// Tries to compile a given fragment shader source code and add it to the supplied
        /// fragment shader ID.
        /// </summary>
        /// <param name="fsID">The shader ID</param>
        /// <param name="pFsSource">The source code of the shader</param>
        /// <exception cref="Exception">If the compilation fails</exception>
        public void CompileFragmentShader(int fsID, String pFsSource)
        {
            Gl.glShaderSource(fsID, 1, new String[] { pFsSource }, IntPtr.Zero);
            Gl.glCompileShader(fsID);
            String result = GetShaderInfoLog(fsID);
            if (result != null)
            {
                throw new Exception(String.Format("Error in fragment shader: \n{0}", result));
            }
        }


        /// <summary>
        /// Sets the shader as the one, that will be used
        /// until another Bind() or Unbind() call occurs.
        /// </summary>
        public void Bind()
        {
            Gl.glUseProgram(Program);
        }

        /// <summary>
        /// Unbinds the shader. Unless another shader's Bind()
        /// call follows this, fixed function pipeline will be active.
        /// </summary>
        public void Unbind()
        {
            Gl.glUseProgram(0);
        }

        /// <summary>
        /// Returns the shader info log for the given shader. If the shader ID
        /// is invalid, null is returned. The info log is the result of the
        /// compiling process and created by the driver's embedded compiler.
        /// This method can be used for vertex or fragment shaders, but not for
        /// shader programs!
        /// </summary>
        /// <param name="shader"> The ID of the shader, either the fragment or the vertex one</param>
        /// <returns>The shader info log or null</returns>
        private String GetShaderInfoLog(int shader)
        {

            int[] length = new int[32];
            int maxLength = 0;

            Gl.glGetShaderiv(shader, Gl.GL_INFO_LOG_LENGTH, length);
            if (length[0] > 1)
            {
                maxLength = length[0];
                StringBuilder infoLog = new StringBuilder(maxLength);
                Gl.glGetShaderInfoLog(shader, maxLength, length, infoLog);

                if (SupressWarningsInInfoLog && !infoLog.ToString().Contains(": error"))
                {
                    return null;
                }
                else
                {
                    return infoLog.ToString();
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the shader program info log for the given shader program. 
        /// If the shader program ID is invalid, null is returned. The info log 
        /// is the result of the linking process and created by the driver's 
        /// embedded linker. This method can be used only for shader programs!
        /// </summary>
        /// <returns>The shader program info log or null</returns>
        private String GetProgramInfoLog(int program)
        {

            int[] length = new int[32];
            int maxLength = 0;

            Gl.glGetProgramiv(program, Gl.GL_INFO_LOG_LENGTH, length);
            if (length[0] > 1)
            {
                maxLength = length[0];
                StringBuilder infoLog = new StringBuilder(maxLength);
                Gl.glGetProgramInfoLog(program, maxLength, length, infoLog);

                if (SupressWarningsInInfoLog && !infoLog.ToString().Contains(": error"))
                {
                    return null;
                }
                else
                {
                    return infoLog.ToString();
                }
            }
            return null;
        }

        // TODO: Document these!!

        public void setVertexAttrib1f(String name, float v0)
        {

            int attrib = Gl.glGetAttribLocation(program, name);
            Gl.glVertexAttrib1f(attrib, v0);
        }

        public void setVertexAttrib2f(String name, float v0, float v1)
        {

            int attrib = Gl.glGetAttribLocation(program, name);
            Gl.glVertexAttrib2f(attrib, v0, v1);
        }

        public void setVertexAttrib3f(String name, float v0, float v1, float v2)
        {

            int attrib = Gl.glGetAttribLocation(program, name);
            Gl.glVertexAttrib3f(attrib, v0, v1, v2);
        }

        public void setVertexAttrib4f(String name, float v0, float v1, float v2, float v3)
        {

            int attrib = Gl.glGetAttribLocation(program, name);
            Gl.glVertexAttrib4f(attrib, v0, v1, v2, v3);
        }

        public void setUniform1i(String name, int v0)
        {

            int attrib = Gl.glGetUniformLocation(program, name);
            Gl.glUniform1i(attrib, v0);
        }


        public void setUniform1iv(String name, int count, int[] v)
        {

            int attrib = Gl.glGetUniformLocation(program, name);
            Gl.glUniform1iv(attrib, count, v);
        }

        public void setUniform2i(String name, int v0, int v1)
        {

            int attrib = Gl.glGetUniformLocation(program, name);
            Gl.glUniform2i(attrib, v0, v1);
        }

        public void setUniform2iv(String name, int count, int[] v)
        {

            int attrib = Gl.glGetUniformLocation(program, name);
            Gl.glUniform2iv(attrib, count, v);
        }

        public void setUniform3i(String name, int v0, int v1, int v2)
        {

            int attrib = Gl.glGetUniformLocation(program, name);
            Gl.glUniform3i(attrib, v0, v1, v2);
        }


        public void setUniform3iv(String name, int count, int[] v)
        {

            int attrib = Gl.glGetUniformLocation(program, name);
            Gl.glUniform3iv(attrib, count, v);
        }

        public void setUniform4i(String name, int v0, int v1, int v2, int v3)
        {

            int attrib = Gl.glGetUniformLocation(program, name);
            Gl.glUniform4i(attrib, v0, v1, v2, v3);
        }

        public void setUniform4iv(String name, int count, int[] v)
        {

            int attrib = Gl.glGetUniformLocation(program, name);
            Gl.glUniform4iv(attrib, count, v);
        }

        public void setUniform1f(String name, float v0)
        {

            int attrib = Gl.glGetUniformLocation(program, name);
            Gl.glUniform1f(attrib, v0);
        }

        public void setUniform1fv(String name, int count, float[] v)
        {

            int attrib = Gl.glGetUniformLocation(program, name);
            Gl.glUniform1fv(attrib, count, v);
        }

        public void setUniform2f(String name, float v0, float v1)
        {

            int attrib = Gl.glGetUniformLocation(program, name);
            Gl.glUniform2f(attrib, v0, v1);

        }

        public void setUniform2fv(String name, int count, float[] v)
        {

            int attrib = Gl.glGetUniformLocation(program, name);
            Gl.glUniform2fv(attrib, count, v);
        }

        public void setUniform3f(String name, float v0, float v1, float v2)
        {
            int attrib = Gl.glGetUniformLocation(program, name);
            Gl.glUniform3f(attrib, v0, v1, v2);
        }

        public void setUniform3fv(String name, int count, float[] v)
        {

            int attrib = Gl.glGetUniformLocation(program, name);
            Gl.glUniform3fv(attrib, count, v);
        }

        public void setUniform4f(String name, float v0, float v1, float v2, float v3)
        {

            int attrib = Gl.glGetUniformLocation(program, name);
            Gl.glUniform4f(attrib, v0, v1, v2, v3);
        }

        public void setUniform4fv(String name, int count, float[] v)
        {

            int attrib = Gl.glGetUniformLocation(program, name);
            Gl.glUniform4fv(attrib, count, v);
        }

        public void setUniformMatrix4f(String name, bool transpose, float[] value)
        {

            int attrib = Gl.glGetUniformLocation(program, name);
            Gl.glUniformMatrix4fv(attrib, 1, transpose, value);
        }

        public void setTextureSampler(String name, int textureUnit)
        {

            int attrib = Gl.glGetUniformLocation(program, name);
            Gl.glUniform1i(attrib, textureUnit);
        }
    }
}
