using System;
using System.Collections.Generic;
using System.Text;
using Tao.OpenGl;

namespace AllView
{
    class Camera
    {
        double[] camPos = new double[3];
        double[] camDir = new double[3];
        double[] upDir = new double[3];
        public Camera()
        {
            camPos[0] = 0.0;
            camPos[1] = 0.0;
            camPos[2] = 0.0;

            camDir[0] = 0.0;
            camDir[1] = 0.0;
            camDir[2] = -1.0;

            upDir[0] = 0.0;
            upDir[1] = 1.0;
            upDir[2] = 0.0;
        }

        public double X
        {
            get { return camPos[0]; } 
            set { camPos[0] = value; }
        }

        public double Y
        {
            get { return camPos[1]; }
            set { camPos[1] = value; }
        }

        public double Z
        {
            get { return camPos[2]; }
            set { camPos[2] = value; }
        }

        public double dX
        {
            get { return camDir[0]; }
            set { camDir[0] = value; }
        }

        public double dY
        {
            get { return camDir[1]; }
            set { camDir[1] = value; }
        }

        public double dZ
        {
            get { return camDir[2]; }
            set { camDir[2] = value; }
        }

        public void SetPosition(double x, double y, double z)
        {
            camPos[0] = x;
            camPos[1] = y;
            camPos[2] = z;
        }

        public void SetDirection(double x, double y, double z)
        {
            camDir[0] = x;
            camDir[1] = y;
            camDir[2] = z;
            ReduceToUnit(camDir);

            if (camDir[0] == 0.0 && camDir[2] == 0.0)  // the view line is in y-direction;
            {
                upDir[0] = upDir[1] = 0.0;
                upDir[2] = camDir[1];
            }
            else
            {
                upDir[0] = -camDir[0] * camDir[1];
                upDir[1] = camDir[0] * camDir[0] + camDir[2] * camDir[2];
                upDir[2] = -camDir[1] * camDir[2];
                ReduceToUnit(upDir);
            }
        }

        public void SetViewVolume(double fovy, int FrameWidth, int FrameHeight, double near, double far)
        {
            /*
            Gl.glViewport(0, 0, FrameWidth, FrameHeight);
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();
            Glu.gluPerspective(fovy, (double)FrameWidth / (double)FrameHeight, near, far);
             */ 
            Gl.glViewport(0, 0, FrameWidth, FrameHeight);
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();
            double aspect = (double)FrameHeight / (double)FrameWidth;
            double x_extent = 0.1;
            Gl.glFrustum(-x_extent, x_extent, -aspect * x_extent, aspect * x_extent, near, far);

        }

        private void ReduceToUnit(double[] vector)
        {
            double length;

            // Calculate the length of the vector		
            length = Math.Sqrt((vector[0] * vector[0]) + (vector[1] * vector[1]) + (vector[2] * vector[2]));

            // Keep the program from blowing up by providing an exceptable
            // value for vectors that may calculated too close to zero.
            if (length == 0.0f) length = 1.0f;

            // Dividing each element by the length will result in a
            // unit normal vector.
            vector[0] /= length;
            vector[1] /= length;
            vector[2] /= length;
        }

        public void Slide(double distance)
        {
            camPos[0] += distance * camDir[0];
            camPos[1] += distance * camDir[1];
            camPos[2] += distance * camDir[2];

            //camPos[0] += distance * camDir[0];
            //camPos[2] += distance * camDir[2];
        }

        public void HSlide(double distance)
        {
            double[] HDir = new double[3];

            // HDir = camDir x upDir
            HDir[0] = camDir[1] * upDir[2] - camDir[2] * upDir[1];
            HDir[1] = camDir[2] * upDir[0] - camDir[0] * upDir[2];
            HDir[2] = camDir[0] * upDir[1] - camDir[1] * upDir[0]; 
            ReduceToUnit(HDir);

            camPos[0] += distance * HDir[0];
            camPos[1] += distance * HDir[1];
            camPos[2] += distance * HDir[2];
        }

        public void VSlide(double distance)
        {
            camPos[0] += distance * upDir[0];
            camPos[1] += distance * upDir[1];
            camPos[2] += distance * upDir[2];
        }

        public void Tilt(double angle)
        {
            float[] mat = new float[16];
            double[] axis = new double[3];
            double[] tmp = new double[3]; 

            // axis = camDir x upDir
            axis[0] = camDir[1] * upDir[2] - camDir[2] * upDir[1];
            axis[1] = camDir[2] * upDir[0] - camDir[0] * upDir[2];
            axis[2] = camDir[0] * upDir[1] - camDir[1] * upDir[0];

            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glPushMatrix();
            Gl.glLoadIdentity();
            Gl.glRotated(angle, axis[0], axis[1], axis[2]);
            Gl.glGetFloatv(Gl.GL_MODELVIEW_MATRIX, mat);
            tmp[0] = mat[0] * camDir[0] + mat[4] * camDir[1] + mat[8] * camDir[2];
            tmp[1] = mat[1] * camDir[0] + mat[5] * camDir[1] + mat[9] * camDir[2];
            tmp[2] = mat[2] * camDir[0] + mat[6] * camDir[1] + mat[10] * camDir[2];
            camDir[0] = tmp[0]; camDir[1] = tmp[1]; camDir[2] = tmp[2];
            //ReduceToUnit(camDir);
            Gl.glPopMatrix();

            // upDir = axis x camDir
            upDir[0] = axis[1] * camDir[2] - axis[2] * camDir[1];
            upDir[1] = axis[2] * camDir[0] - axis[0] * camDir[2];
            upDir[2] = axis[0] * camDir[1] - axis[1] * camDir[0];
            ReduceToUnit(upDir);
        }

        public void Pan(double angle)
        {
            float[] mat = new float[16];
            double[] tmp = new double[3]; 

            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glPushMatrix();
            Gl.glLoadIdentity();
            Gl.glRotated(angle, upDir[0], upDir[1], upDir[2]);
            Gl.glGetFloatv(Gl.GL_MODELVIEW_MATRIX, mat);
            tmp[0] = mat[0] * camDir[0] + mat[4] * camDir[1] + mat[8] * camDir[2];
            tmp[1] = mat[1] * camDir[0] + mat[5] * camDir[1] + mat[9] * camDir[2];
            tmp[2] = mat[2] * camDir[0] + mat[6] * camDir[1] + mat[10] * camDir[2];
            camDir[0] = tmp[0]; camDir[1] = tmp[1]; camDir[2] = tmp[2];
            //ReduceToUnit(camDir);
            Gl.glPopMatrix();
        }

        public void Roll(double angle)
        {
            float[] mat = new float[16];
            double[] tmp = new double[3];

            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glPushMatrix();
            Gl.glLoadIdentity();
            Gl.glRotated(angle, camDir[0], camDir[1], camDir[2]);
            Gl.glGetFloatv(Gl.GL_MODELVIEW_MATRIX, mat);
            tmp[0] = mat[0] * upDir[0] + mat[4] * upDir[1] + mat[8] * upDir[2];
            tmp[1] = mat[1] * upDir[0] + mat[5] * upDir[1] + mat[9] * upDir[2];
            tmp[2] = mat[2] * upDir[0] + mat[6] * upDir[1] + mat[10] * upDir[2];
            upDir[0] = tmp[0]; upDir[1] = tmp[1]; upDir[2] = tmp[2];
            //ReduceToUnit(upDir);
            Gl.glPopMatrix();
        }

        public void LookAt()
        {
            Glu.gluLookAt(camPos[0], camPos[1], camPos[2], camPos[0] + camDir[0], camPos[1] + camDir[1], camPos[2] + camDir[2], upDir[0], upDir[1], upDir[2]);
            /*
            if (camDir[0] == 0.0 && camDir[1] == 1.0 && camDir[2] == 0.0)
                Glu.gluLookAt(camPos[0], camPos[1], camPos[2], camPos[0] + camDir[0], camPos[1] + camDir[1], camPos[2] + camDir[2], 0.0, 0.0, 1.0);
            else if (camDir[0] == 0.0 && camDir[1] == -1.0 && camDir[2] == 0.0)
                Glu.gluLookAt(camPos[0], camPos[1], camPos[2], camPos[0] + camDir[0], camPos[1] + camDir[1], camPos[2] + camDir[2], 0.0, 0.0, -1.0);
            else
                Glu.gluLookAt(camPos[0], camPos[1], camPos[2], camPos[0] + camDir[0], camPos[1] + camDir[1], camPos[2] + camDir[2], 0.0, 1.0, 0.0);
             */ 
        }
    }
}
