using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tao.OpenGl;
using Tao.FreeGlut;

namespace _1102056_HW2
{
    public partial class Form1 : Form
    {
        double cx = 0.0, cy = 0.0, cz = 0.0;//球心
        double xstep = 0.5, ystep = 0.4, zstep = 0.3;//球移動的方向=座標增加量
        double rot = 0.0;
        double radius = 1.5;
        double r = 0, g = 0, b = 0;

        private void timer1_Tick(object sender, EventArgs e)
        {
            

            Random rn = new Random();

            if (cx + radius >= 10.0 || cx - radius <= -10.0)
            {
                r = rn.Next(0, 255);
                g = rn.Next(0, 255);
                b = rn.Next(0, 255);
                
                xstep = -xstep;
            }

            if (cy + radius >= 10.0 || cy - radius <= -10.0)
            {
                r = rn.Next(0, 255);
                g = rn.Next(0, 255);
                b = rn.Next(0, 255);
                
                ystep = -ystep;
            }

            if (cz + radius >= 10.0 || cz - radius <= -10.0)
            {
                r = rn.Next(0, 255);
                g = rn.Next(0, 255);
                b = rn.Next(0, 255);
                
                zstep = -zstep;
            }

            cx += xstep;
            cy += ystep;
            cz += zstep;
            rot += 0.5;

            this.openGLControl1.Refresh();//更新視窗
        }

        private void openGLControl1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (radius == 0.5)
                {
                    MessageBox.Show("最小了，只能加大");
                }
                else radius -= 0.5;

            }
            if (e.Button == MouseButtons.Right)
            {
                radius += 0.5;
            }
            this.openGLControl1.Refresh();//更新視窗 視窗重繪
        }

        private void openGLControl1_Paint(object sender, PaintEventArgs e)
        {
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glLoadIdentity();
            Glu.gluLookAt(0.0, 0.0, 35.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0);


            Gl.glBegin(Gl.GL_QUADS);

            
            //右綠色
            Gl.glColor3ub((byte)200, (byte)255, (byte)200);
            Gl.glVertex3d(10.0, 10.0, -10.0);
            Gl.glVertex3d(10.0, 10.0, 10.0);
            Gl.glVertex3d(10.0, -10.0, 10.0);
            Gl.glVertex3d(10.0, -10.0, -10.0);

            //左綠色
            Gl.glVertex3d(-10.0, 10.0, -10.0);
            Gl.glVertex3d(-10.0, 10.0, 10.0);
            Gl.glVertex3d(-10.0, -10.0, 10.0);
            Gl.glVertex3d(-10.0, -10.0, -10.0);

            

            //中間紅色
            Gl.glColor3ub((byte)255, (byte)200, (byte)200);
            Gl.glVertex3d(10.0, 10.0, -10.0);
            Gl.glVertex3d(-10.0, 10.0, -10.0);
            Gl.glVertex3d(-10.0, -10.0, -10.0);
            Gl.glVertex3d(10.0, -10.0, -10.0);

            
            //上藍色
            Gl.glColor3ub((byte)200, (byte)200, (byte)255);
            Gl.glVertex3d(10.0, 10.0, 10.0);
            Gl.glVertex3d(-10.0, 10.0, 10.0);
            Gl.glVertex3d(-10.0, 10.0, -10.0);
            Gl.glVertex3d(10.0, 10.0, -10.0);

            //下藍色
            Gl.glVertex3d(10.0, -10.0, 10.0);
            Gl.glVertex3d(-10.0, -10.0, 10.0);
            Gl.glVertex3d(-10.0, -10.0, -10.0);
            Gl.glVertex3d(10.0, -10.0, -10.0);
            Gl.glEnd();


            //球的顏色 
            Gl.glColor3ub((byte)r, (byte)g, (byte)b);

            //建立線框式的球
            Gl.glPushMatrix();
            Gl.glTranslated(cx, cy, cz);//球心
            Gl.glRotated(rot, 1, 1, 1);
            Glut.glutWireSphere(radius, 16, 16);
            Gl.glPopMatrix();
        }

        private void openGLControl1_Resize(object sender, EventArgs e)
        {
            SetViewingVolume();
        }

        public Form1()
        {
            InitializeComponent();
            this.openGLControl1.InitializeContexts();
            Glut.glutInit();
        }
        private void SetViewingVolume()
        {
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();
            Gl.glViewport(0, 0, openGLControl1.Size.Width, openGLControl1.Size.Height);
            double aspect = (double)openGLControl1.Size.Width /
                            (double)openGLControl1.Size.Height;

            Glu.gluPerspective(45, aspect, 0.3, 100.0);//透視投影
        }

        private void MyInit()
        {
            Gl.glClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            Gl.glClearDepth(1.0);
            Gl.glEnable(Gl.GL_DEPTH_TEST);
        }


        private void openGLControl1_Load(object sender, EventArgs e)
        {
            SetViewingVolume();
            MyInit();
        }
    }
}
