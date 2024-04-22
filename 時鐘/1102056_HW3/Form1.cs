using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tao.FreeGlut;
using Tao.OpenGl;


namespace _1102056_HW3
{
    public partial class Form1 : Form
    {
        const double DEGREE_TO_RAD = 0.01745329; // 3.1415926/180
        double Radius = 3.0, Longitude = 45.0, Latitude = 45.0;
        
        public Form1()
        {
            InitializeComponent();
            this.openGLControl1.InitializeContexts();
            Glut.glutInit();
        }

        private void openGLControl1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                //case Keys.Left:
                //    Longitude -= 5.0;
                //    break;
                //case Keys.Right:
                //    Longitude += 5.0;
                //    break;
                //case Keys.Up:
                //    Latitude += 5.0;
                //    if (Latitude >= 90.0) Latitude = 89.0;
                //    break;
                //case Keys.Down:
                //    Latitude -= 5.0;
                //    if (Latitude <= -90.0) Latitude = -89.0;
                //    break;
                case Keys.PageUp:
                    Radius += 0.1;
                    break;
                case Keys.PageDown:
                    Radius -= 0.1;
                    if (Radius < 0.1) Radius = 0.1;
                    break;
                case Keys.L:
                    Gl.glDisable(Gl.GL_LIGHTING);
                    Gl.glDisable(Gl.GL_LIGHT0);
                    break;
                case Keys.O:
                    Gl.glEnable(Gl.GL_LIGHTING);
                    Gl.glEnable(Gl.GL_LIGHT0);
                    break;
                default:
                    break;
            }
            this.openGLControl1.Refresh();
        }
        private void openGLControl1_Resize(object sender, EventArgs e)
        {
            SetViewingVolume();
        }

        private void openGLControl1_Load(object sender, EventArgs e)
        {
            SetViewingVolume();
            MyInit();
        }

        private void SetViewingVolume()
        {
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();
            Gl.glViewport(0, 0, openGLControl1.Size.Width, openGLControl1.Size.Height);
            double aspect = (double)openGLControl1.Size.Width /
                            (double)openGLControl1.Size.Height;

            Glu.gluPerspective(45, aspect, 0.1, 100.0);//透視投影
        }

        private void MyInit()//只執行一次
        {
            Gl.glClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            Gl.glClearDepth(1.0);

            Gl.glColorMaterial(Gl.GL_FRONT, Gl.GL_AMBIENT_AND_DIFFUSE);

            //Gl.glPolygonMode(Gl.GL_FRONT_AND_BACK, Gl.GL_LINE);//變成線框形式

            Gl.glEnable(Gl.GL_NORMALIZE);
            Gl.glEnable(Gl.GL_DEPTH_TEST);
            Gl.glEnable(Gl.GL_LIGHTING);
            Gl.glEnable(Gl.GL_LIGHT0);


            Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);//設定混和計算公式

            float[] global_ambient = new float[] { 0.2f, 0.2f, 0.2f }; //全域環境光的數值
            float[] light0_ambient = new float[] { 0.2f, 0.2f, 0.2f }; //偏白色的環境光
            float[] light0_diffuse = new float[] { 0.7f, 0.7f, 0.7f }; //偏白色的散射光
            float[] light0_specular = new float[] { 0.9f, 0.9f, 0.9f }; //偏白色的鏡射光
            Gl.glLightModelfv(Gl.GL_LIGHT_MODEL_AMBIENT, global_ambient); //設定全域環境光
            Gl.glLightModeli(Gl.GL_LIGHT_MODEL_LOCAL_VIEWER, Gl.GL_TRUE); //觀者位於場景內
            Gl.glLightModeli(Gl.GL_LIGHT_MODEL_TWO_SIDE, Gl.GL_FALSE); //只對物體正面進行光影計算
            Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_AMBIENT, light0_ambient); //設定第一個光源的環境光成份
            Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_DIFFUSE, light0_diffuse); //設定第一個光源的散射光成份
            Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_SPECULAR, light0_specular); //設定第一個光源的鏡射光成份







        }

        private void axes(double length)//畫座標軸
        {
            Gl.glDisable(Gl.GL_LIGHTING);
            Gl.glBegin(Gl.GL_LINES);
            //x軸
            Gl.glColor3ub(255, 0, 0);
            Gl.glVertex3d(0, 0, 0);
            Gl.glVertex3d(length, 0, 0);
            //y軸
            Gl.glColor3ub(0, 255, 0);
            Gl.glVertex3d(0, 0, 0);
            Gl.glVertex3d(0, length, 0);
            //z軸
            Gl.glColor3ub(0, 0, 255);
            Gl.glVertex3d(0, 0, 0);
            Gl.glVertex3d(0, 0, length);
            Gl.glEnd();

            Gl.glEnable(Gl.GL_LIGHTING);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.openGLControl1.Refresh();//更新視窗
        }

        private void clock()
        {
            
            Gl.glEnable(Gl.GL_COLOR_MATERIAL);
            //外框
            Gl.glColor3ub(48, 122, 8);
            Gl.glPushMatrix();
            Gl.glRotated(45, 0, 1, 0);
            Gl.glRotated(-45, 1, 0, 0);
            Gl.glScaled(0.5, 0.5, 0.3);
            Glut.glutSolidTorus(0.1, 1.0, 64, 64);//環內半徑,環外半徑,切片個數
            Gl.glPopMatrix();


            //面
            Gl.glColor3ub(237, 229, 16);
            Gl.glPushMatrix();
            Gl.glRotated(45, 0, 1, 0);
            Gl.glRotated(-45, 1, 0, 0);
            Gl.glScaled(0.5, 0.5, 0.01);
            Glut.glutSolidSphere(1.0, 64, 64);//radius: 球半徑slices & stacks:水平及垂直切割片數
            Gl.glPopMatrix();

            //時間小刻度
            for (int i=0;i<60;i++)
            {
                Gl.glColor3ub(135, 8, 8);

                Gl.glPushMatrix();
                Gl.glRotated(45, 0, 1, 0);
                Gl.glRotated(-45, 1, 0, 0);
                Gl.glRotated(i*6, 0, 0, 1);
                Gl.glTranslated(0.015, 0.425, 0.015);
                Gl.glScaled(0.01, 0.05, 0.01);
                Glut.glutSolidCube(1);
                Gl.glPopMatrix();
            }

            //時間大刻度
            Gl.glColor3ub(135, 8, 8);
            for(int i=0;i<12;i++)
            {
                Gl.glPushMatrix();
                Gl.glRotated(45, 0, 1, 0);
                Gl.glRotated(-45, 1, 0, 0);
                Gl.glRotated(i*30, 0, 0, 1);
                Gl.glTranslated(0.015, 0.4, 0.015);
                Gl.glScaled(0.015, 0.1, 0.015);
                Glut.glutSolidCube(1);
                Gl.glPopMatrix();
            }
            

            //秒針
            Gl.glColor3ub(161, 8, 196);
            Gl.glPushMatrix();
            int second = DateTime.Now.Second;
            Gl.glRotated(45, 0, 1, 0);
            Gl.glRotated(-45, 1, 0, 0);
            Gl.glRotated(-second * 6.0, 0.0, 0.0, 1.0);
            Gl.glTranslated(0.01, 0.2, 0.015);
            Gl.glScaled(0.01, 0.4, 0.1);
            Glut.glutSolidCube(1);
            Gl.glPopMatrix();

            //分針
            Gl.glColor3ub(8, 57, 163);
            Gl.glPushMatrix();
            int minute = DateTime.Now.Minute;
            Gl.glRotated(45, 0, 1, 0);
            Gl.glRotated(-45, 1, 0, 0);
            Gl.glRotated(-minute * 6.0, 0.0, 0.0, 1.0);
            Gl.glTranslated(0.015, 0.175, 0.015);
            Gl.glScaled(0.02, 0.35, 0.1);
            Glut.glutSolidCube(1);
            Gl.glPopMatrix();

            //時針
            Gl.glColor3ub(8, 57, 163);
            Gl.glPushMatrix();
            int hour = DateTime.Now.Hour;
            Gl.glRotated(45, 0, 1, 0);
            Gl.glRotated(-45, 1, 0, 0);
            Gl.glRotated(-(hour + (minute / 60.0)) * 30.0, 0.0, 0.0, 1.0);
            Gl.glTranslated(0.01, 0.125, 0.015);
            Gl.glScaled(0.02, 0.25, 0.1);
            Glut.glutSolidCube(1);
            Gl.glPopMatrix();


            //中間圓心
            Gl.glColor3ub(0, 0, 0);
            Gl.glPushMatrix();
            Gl.glRotated(45, 0, 1, 0);
            Gl.glRotated(-45, 1, 0, 0);
            Gl.glTranslated(0.01, 0, 0.01);
            Gl.glScaled(0.04, 0.04, 0.15);
            Glut.glutSolidSphere(1.0, 16, 16);//radius: 球半徑slices & stacks:水平及垂直切割片數
            Gl.glPopMatrix();

            Gl.glDisable(Gl.GL_COLOR_MATERIAL);


        }
        private void glass()
        {
            Gl.glEnable(Gl.GL_BLEND);
            Gl.glEnable(Gl.GL_COLOR_MATERIAL);

            Gl.glColor4d(0.0, 0.0, 1.0, 0.3);
            Gl.glPushMatrix();
            //Gl.glNormal3d(0.0, 0.0, 1.0);
            Gl.glRotated(45, 0, 1, 0);
            Gl.glRotated(-45, 1, 0, 0);
            Gl.glTranslated(0.0,0.0,0.1);
            Gl.glScaled(0.5, 0.5, 0.01);
            Glut.glutSolidSphere(0.9, 16, 16);
            Gl.glPopMatrix();


            Gl.glDisable(Gl.GL_BLEND);
            Gl.glDisable(Gl.GL_COLOR_MATERIAL);
        }
        private void openGLControl1_Paint(object sender, PaintEventArgs e)
        {
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);//清除色彩、深度緩衝器  


            Gl.glMatrixMode(Gl.GL_MODELVIEW);//設定攝影機位置
            Gl.glLoadIdentity();


            Glu.gluLookAt(Radius * Math.Cos(Latitude * DEGREE_TO_RAD)
                     * Math.Sin(Longitude * DEGREE_TO_RAD),
              Radius * Math.Sin(Latitude * DEGREE_TO_RAD),
              Radius * Math.Cos(Latitude * DEGREE_TO_RAD)
                     * Math.Cos(Longitude * DEGREE_TO_RAD),
              0.0, 0.0, 0.0, 0.0, 1.0, 0.0);

            







            //axes(4);
            clock();
            glass();
        }

        
    }
}
