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
using System.Reflection;
using Tao.DevIl;
using ModelLoadingByAssimp;
using System.Security.Policy;
using Assimp;
using AllView;

namespace _1102056_HW4
{
    public partial class Form1 : Form
    {
        const double DEGREE_TO_RAD = 0.01745329; // 3.1415926/180
        double Radius = 3.0, Longitude = 45.0, Latitude = 45.0;
        uint[] texName = new uint[3]; //建立儲存紋理編號的陣列
        double width = 5.0, height = 3.0;
        double table_w = 0.5, table_h = 0.8;

        Model myModel;
        float modelSize;//儲存模型約略大小
        float[] modelCenter = new float[3];//儲存模型中心座標

        bool light0 = true;
        bool light1 = false;
        bool light2 = false;
        bool light3 = false;
        bool light4 = false;


        AllView.Camera cam = new AllView.Camera();


        //double VSIZE, SIZE=200;


        public Form1()
        {
            InitializeComponent();
            this.openGLControl1.InitializeContexts();
            Glut.glutInit();
            Il.ilInit();
            Ilu.iluInit();
            Gl.ReloadFunctions();
        }

        private void openGLControl1_Load(object sender, EventArgs e)
        {
            //SetViewingVolume();
            cam.SetViewVolume(45.0, this.openGLControl1.Size.Width, this.openGLControl1.Size.Height, 0.1, 1000.0);
            MyInit();

            //myModel = new Model("C:\\Users\\my920\\Desktop\\OpenGl\\功課\\1102056_HW4\\1102056_HW4\\bin\\Debug\\14083_WWII_ship - UK - King_George_V_Class_Battleship_v1_L1.obj");
            myModel = new Model("C:\\Users\\my920\\Downloads\\1102056_HW4\\1102056_HW4\\1102056_HW4\\bin\\Debug\\14083_WWII_ship - UK - King_George_V_Class_Battleship_v1_L1.obj");

            float[] min = new float[3];
            float[] max = new float[3];
            myModel.ComputeBoundingBox(min, max);
            modelSize = (float)Math.Max(max[0] - min[0], Math.Max(max[1] - max[1], max[2] - max[2]));
            modelCenter[0] = 0.5f * (min[0] + max[0]);
            modelCenter[1] = 0.5f * (min[1] + max[1]);
            modelCenter[2] = 0.5f * (min[2] + max[2]);
        }

        private void openGLControl1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                    if (e.Control) cam.HSlide(-0.2);
                    else if (e.Alt) cam.Roll(0.2);
                    break;
                case Keys.Right:
                    if (e.Control) cam.HSlide(0.2);
                    else if (e.Alt) cam.Roll(-0.2);
                    break;
                case Keys.Up:
                    if (e.Control) cam.VSlide(0.2);
                    else cam.Tilt(1.0);
                    break;
                case Keys.Down:
                    if (e.Control) cam.VSlide(-0.2);
                    else cam.Tilt(-0.2);
                    break;
                case Keys.PageUp:
                    cam.Slide(0.2);
                    break;
                case Keys.PageDown:
                    cam.Slide(-0.2);
                    break;
                case Keys.O:
                    cam.Pan(0.2);
                    break;
                case Keys.P:
                    cam.Pan(-0.2);
                    break;
                case Keys.D0:
                    light0 = !light0;

                    if (light0)
                        Gl.glDisable(Gl.GL_LIGHT0);
                    else
                        Gl.glEnable(Gl.GL_LIGHT0);
                break;

                case Keys.D1:
                    light1 = !light1;

                    if (light1)
                        Gl.glEnable(Gl.GL_LIGHT1);
                    else
                        Gl.glDisable(Gl.GL_LIGHT1);
                break;

                case Keys.D2:
                    light2 = !light2;

                    if (light2)
                        Gl.glEnable(Gl.GL_LIGHT2);
                    else
                        Gl.glDisable(Gl.GL_LIGHT2);
                break;

                case Keys.D3:
                    light3 = !light3;

                    if (light3)
                        Gl.glEnable(Gl.GL_LIGHT3);
                    else
                        Gl.glDisable(Gl.GL_LIGHT3);
                break;

                case Keys.D4:
                    light4 = !light4;

                    if (light4)
                        Gl.glEnable(Gl.GL_LIGHT4);
                    else
                        Gl.glDisable(Gl.GL_LIGHT4);
                break;

                



                default:
                    break;
            }

            this.openGLControl1.Refresh();
        }

        private void openGLControl1_Resize(object sender, EventArgs e)
        {
            //SetViewingVolume();
            cam.SetViewVolume(45.0, this.openGLControl1.Size.Width, this.openGLControl1.Size.Height, 0.1, 1000.0);
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

        private void LoadTexture(string filename, uint texture)
        {
            if (Il.ilLoadImage(filename)) //載入影像檔
            {
                int BitsPerPixel = Il.ilGetInteger(Il.IL_IMAGE_BITS_PER_PIXEL); //取得儲存每個像素的位元數
                int Depth = Il.ilGetInteger(Il.IL_IMAGE_DEPTH);
                Ilu.iluScale(512, 512, Depth);
                Ilu.iluFlipImage(); //顛倒影像
                Gl.glBindTexture(Gl.GL_TEXTURE_2D, texture); //連結紋理物件
                                                             //設定紋理參數
                Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_S, Gl.GL_REPEAT);
                Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_T, Gl.GL_REPEAT);
                Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);
                Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR);
                //建立紋理物件
                if (BitsPerPixel == 24) Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, Gl.GL_RGB, 512, 512, 0,
                Il.ilGetInteger(Il.IL_IMAGE_FORMAT), Il.ilGetInteger(Il.IL_IMAGE_TYPE), Il.ilGetData());
                if (BitsPerPixel == 32) Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, Gl.GL_RGBA, 512, 512, 0,
                Il.ilGetInteger(Il.IL_IMAGE_FORMAT), Il.ilGetInteger(Il.IL_IMAGE_TYPE), Il.ilGetData());
                //Gl.glGenerateMipmap(Gl.GL_TEXTURE_2D);
            }
            else
            { // 若檔案無法開啟，顯示錯誤訊息
                string message = "Cannot open file " + filename + ".";
                MessageBox.Show(message, "Image file open error!!!", MessageBoxButtons.OK,
                MessageBoxIcon.Exclamation);
            }
        }


        private void MyInit()//只執行一次
        {
            Gl.glClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            Gl.glClearDepth(1.0);

            Gl.glColorMaterial(Gl.GL_FRONT_AND_BACK, Gl.GL_AMBIENT_AND_DIFFUSE);

            //Gl.glPolygonMode(Gl.GL_FRONT_AND_BACK, Gl.GL_LINE);//變成線框形式

            Gl.glEnable(Gl.GL_NORMALIZE);//法向量規劃
            Gl.glEnable(Gl.GL_DEPTH_TEST);
            Gl.glEnable(Gl.GL_LIGHTING);

            Gl.glGenTextures(6, texName); //產生紋理物件
            LoadTexture("LUFFY.png", texName[0]);
            LoadTexture("ZORO.png", texName[1]);
            LoadTexture("CHOPPER.png", texName[2]);
            Gl.glTexEnvf(Gl.GL_TEXTURE_ENV, Gl.GL_TEXTURE_ENV_MODE, Gl.GL_MODULATE);//設定紋理環境參數

            Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);//設定混和計算公式


            cam.SetPosition(0.0, 1.5, 2.0);
            cam.SetDirection(0.0, 0.0, -1.0);


            float[] global_ambient = new float[] { 0.2f, 0.2f, 0.2f, 1.0f };
            Gl.glLightModeli(Gl.GL_LIGHT_MODEL_TWO_SIDE, Gl.GL_TRUE);
            Gl.glLightModeli(Gl.GL_LIGHT_MODEL_LOCAL_VIEWER, Gl.GL_TRUE);
            Gl.glLightModelfv(Gl.GL_LIGHT_MODEL_AMBIENT, global_ambient);


            float[] light0_ambient = new float[] { 0.2f, 0.2f, 0.2f, 1.0f };
            float[] light0_diffuse = new float[] { 0.6f, 0.6f, 0.6f, 1.0f };
            float[] light0_specular = new float[] { 1.0f, 1.0f, 1.0f, 1.0f };
            Gl.glEnable(Gl.GL_LIGHT0);
            Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_AMBIENT, light0_ambient);
            Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_DIFFUSE, light0_diffuse);
            Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_SPECULAR, light0_specular);


            float[] light1_ambient = new float[] { 0.2f, 0.2f, 0.2f, 1.0f };
            float[] light1_diffuse = new float[] { 0.6f, 0.6f, 0.6f, 1.0f };
            float[] light1_specular = new float[] { 1.0f, 1.0f, 1.0f, 1.0f };
            //Gl.glEnable(Gl.GL_LIGHT1);
            Gl.glLightfv(Gl.GL_LIGHT1, Gl.GL_AMBIENT, light1_ambient);
            Gl.glLightfv(Gl.GL_LIGHT1, Gl.GL_DIFFUSE, light1_diffuse);
            Gl.glLightfv(Gl.GL_LIGHT1, Gl.GL_SPECULAR, light1_specular);

            float[] light2_ambient = new float[] { 0.2f, 0.2f, 0.2f, 1.0f };
            float[] light2_diffuse = new float[] { 0.6f, 0.6f, 0.6f, 1.0f };
            float[] light2_specular = new float[] { 1.0f, 1.0f, 1.0f, 1.0f };
            //Gl.glEnable(Gl.GL_LIGHT2);
            Gl.glLightfv(Gl.GL_LIGHT2, Gl.GL_AMBIENT, light2_ambient);
            Gl.glLightfv(Gl.GL_LIGHT2, Gl.GL_DIFFUSE, light2_diffuse);
            Gl.glLightfv(Gl.GL_LIGHT2, Gl.GL_SPECULAR, light2_specular);

            float[] light3_ambient = new float[] { 0.2f, 0.2f, 0.2f, 1.0f };
            float[] light3_diffuse = new float[] { 0.6f, 0.6f, 0.6f, 1.0f };
            float[] light3_specular = new float[] { 1.0f, 1.0f, 1.0f, 1.0f };
            //Gl.glEnable(Gl.GL_LIGHT3);
            Gl.glLightfv(Gl.GL_LIGHT3, Gl.GL_AMBIENT, light3_ambient);
            Gl.glLightfv(Gl.GL_LIGHT3, Gl.GL_DIFFUSE, light3_diffuse);
            Gl.glLightfv(Gl.GL_LIGHT3, Gl.GL_SPECULAR, light3_specular);

            float[] light4_ambient = new float[] { 0.2f, 0.2f, 0.2f, 1.0f };
            float[] light4_diffuse = new float[] { 0.6f, 0.6f, 0.6f, 1.0f };
            float[] light4_specular = new float[] { 1.0f, 1.0f, 1.0f, 1.0f };
            //Gl.glEnable(Gl.GL_LIGHT4);
            Gl.glLightfv(Gl.GL_LIGHT4, Gl.GL_AMBIENT, light4_ambient);
            Gl.glLightfv(Gl.GL_LIGHT4, Gl.GL_DIFFUSE, light4_diffuse);
            Gl.glLightfv(Gl.GL_LIGHT4, Gl.GL_SPECULAR, light4_specular);




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

        private void MySolidCube(double size, int slices)
        {
            double s = 1.0 / slices;

            Gl.glPushMatrix();
            Gl.glScaled(size, size, size);
            for (int i = 0; i < slices; i++)
            {
                for (int j = 0; j < slices; j++)
                {
                    Gl.glPushMatrix();
                    Gl.glTranslated(-0.5 + i * s, 0.0, -0.5 + j * s);
                    Gl.glScaled(s, 1.0, s);
                    Gl.glTranslated(0.5, 0.0, 0.5);
                    Glut.glutSolidCube(1.0);
                    Gl.glPopMatrix();
                }
            }
            Gl.glPopMatrix();


        }
        private void wall()
        {
            //Gl.glEnable(Gl.GL_COLOR_MATERIAL);

            //Gl.glColor3ub(165, 42, 42);

            ////Gl.glPushMatrix();
            ////Gl.glScaled(width, 0.02, width);
            ////Gl.glTranslated(-0.5, 0.0, -0.5);
            ////MySolidCube(1.0, 100);
            ////Gl.glPopMatrix();

            //////上
            ////Gl.glPushMatrix();
            ////Gl.glTranslated(-0.5, 2.0, -0.5);
            ////Gl.glScaled(2.0, 0.02, 3.0);
            ////MySolidCube(1.0, 100);
            //////Glut.glutSolidCube(1.0);
            ////Gl.glPopMatrix();

            //////左牆
            ////Gl.glPushMatrix();
            ////Gl.glRotated(-90.0, 0.0, 0.0, 1.0);
            ////Gl.glTranslated(-1.0, -1.5, -0.5);
            ////Gl.glScaled(2.0, 0.02, 3.0);
            ////MySolidCube(1.0, 100);
            //////Glut.glutSolidCube(1.0);
            ////Gl.glPopMatrix();

            //////右牆
            ////Gl.glPushMatrix();
            ////Gl.glRotated(-90.0, 0.0, 0.0, 1.0);
            ////Gl.glTranslated(-1.0, 0.5, -0.5);
            ////Gl.glScaled(2.0, 0.02, 3.0);
            ////MySolidCube(1.0, 100);
            //////Glut.glutSolidCube(1.0);
            ////Gl.glPopMatrix();

            //////前
            ////Gl.glPushMatrix();
            ////Gl.glRotated(90.0, 1.0, 0.0, 0.0);
            ////Gl.glTranslated(-0.5, -2.0, -1.0);
            ////Gl.glScaled(2.0, 0.02, 2.0);
            ////MySolidCube(1.0, 100);
            //////Glut.glutSolidCube(1.0);
            ////Gl.glPopMatrix();

            //////後
            ////Gl.glPushMatrix();
            ////Gl.glRotated(90.0, 1.0, 0.0, 0.0);
            ////Gl.glTranslated(-0.5, 1.0, -1.0);
            ////Gl.glScaled(2.0, 0.02, 2.0);
            ////MySolidCube(1.0, 100);
            //////Glut.glutSolidCube(1.0);
            ////Gl.glPopMatrix();



            //Gl.glDisable(Gl.GL_COLOR_MATERIAL);



            Gl.glEnable(Gl.GL_COLOR_MATERIAL);

            Gl.glColor3ub(165, 42, 42);

            //底
            Gl.glPushMatrix();
            Gl.glTranslated(-0.5, 0.0, -0.5);
            Gl.glScaled(2.0, 0.02, 3.0);
            MySolidCube(1.0, 100);
            //Glut.glutSolidCube(1.0);
            Gl.glPopMatrix();

            //上
            Gl.glPushMatrix();
            Gl.glTranslated(-0.5, 2.0, -0.5);
            Gl.glScaled(2.0, 0.02, 3.0);
            MySolidCube(1.0, 100);
            //Glut.glutSolidCube(1.0);
            Gl.glPopMatrix();

            //左牆
            Gl.glPushMatrix();
            Gl.glRotated(-90.0, 0.0, 0.0, 1.0);
            Gl.glTranslated(-1.0, -1.5, -0.5);
            Gl.glScaled(2.0, 0.02, 3.0);
            MySolidCube(1.0, 100);
            //Glut.glutSolidCube(1.0);
            Gl.glPopMatrix();

            //右牆
            Gl.glPushMatrix();
            Gl.glRotated(-90.0, 0.0, 0.0, 1.0);
            Gl.glTranslated(-1.0, 0.5, -0.5);
            Gl.glScaled(2.0, 0.02, 3.0);
            MySolidCube(1.0, 100);
            //Glut.glutSolidCube(1.0);
            Gl.glPopMatrix();

            //前
            Gl.glPushMatrix();
            Gl.glRotated(90.0, 1.0, 0.0, 0.0);
            Gl.glTranslated(-0.5, -2.0, -1.0);
            Gl.glScaled(2.0, 0.02, 2.0);
            MySolidCube(1.0, 100);
            //Glut.glutSolidCube(1.0);
            Gl.glPopMatrix();

            //後
            Gl.glPushMatrix();
            Gl.glRotated(90.0, 1.0, 0.0, 0.0);
            Gl.glTranslated(-0.5, 1.0, -1.0);
            Gl.glScaled(2.0, 0.02, 2.0);
            MySolidCube(1.0, 100);
            //Glut.glutSolidCube(1.0);
            Gl.glPopMatrix();



            Gl.glDisable(Gl.GL_COLOR_MATERIAL);
        }


        private void plane(int slices)
        {
            double dx = 1.0 / slices;
            double dz = 1.0 / slices;

            Gl.glBegin(Gl.GL_QUADS); //繪製要貼圖的長方形
            Gl.glNormal3d(0.0, 1.0, 0.0); //設定長方形法向量以產生正確的光影
            for(double x=0;x<1.0;x+=dx)
            {
                for(double z=0;z<1.0;z+=dz)
                {
                    Gl.glVertex3d(x, 0.0, z);
                    Gl.glVertex3d(x, 0.0, z + dz);
                    Gl.glVertex3d(x + dx, 0.0, z + dz);
                    Gl.glVertex3d(x + dx, 0.0, z);
                }
            }
            Gl.glEnd();
        }

        private void poster_luffy(int slices)
        {
            double dx = 1.0 / slices;
            double dz = 1.0 / slices;

            Gl.glEnable(Gl.GL_COLOR_MATERIAL);
            Gl.glColor3ub(255, 255, 255); //將背景設為白底，以免背景顏色影響海報的顏色
            Gl.glEnable(Gl.GL_TEXTURE_2D); //開啟紋理映射功能
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, texName[0]); //連結紋理物件

            Gl.glBegin(Gl.GL_QUADS); //繪製要貼圖的長方形
            Gl.glNormal3d(0.0, 1.0, 0.0); //設定長方形法向量以產生正確的光影
            for (double x = 0; x < 1.0; x += dx)
            {
                for (double z = 0; z < 1.0; z += dz)
                {
                    Gl.glTexCoord2d(x, z);
                    Gl.glVertex3d(x, 0.0, z);
                    Gl.glTexCoord2d(x, z + dz);
                    Gl.glVertex3d(x, 0.0, z + dz);
                    Gl.glTexCoord2d(x + dx, z + dz);
                    Gl.glVertex3d(x + dx, 0.0, z + dz);
                    Gl.glTexCoord2d(x + dx, z);
                    Gl.glVertex3d(x + dx, 0.0, z);
                }
            }
            Gl.glEnd();

            Gl.glDisable(Gl.GL_TEXTURE_2D); //關閉紋理映射功能
            Gl.glDisable(Gl.GL_COLOR_MATERIAL);
        }

        private void poster_zoro(int slices)
        {
            double dx = 1.0 / slices;
            double dz = 1.0 / slices;

            Gl.glEnable(Gl.GL_COLOR_MATERIAL);
            Gl.glColor3ub(255, 255, 255); //將背景設為白底，以免背景顏色影響海報的顏色
            Gl.glEnable(Gl.GL_TEXTURE_2D); //開啟紋理映射功能
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, texName[1]); //連結紋理物件

            Gl.glBegin(Gl.GL_QUADS); //繪製要貼圖的長方形
            Gl.glNormal3d(0.0, 1.0, 0.0); //設定長方形法向量以產生正確的光影
            for (double x = 0; x < 1.0; x += dx)
            {
                for (double z = 0; z < 1.0; z += dz)
                {
                    Gl.glTexCoord2d(x, z);
                    Gl.glVertex3d(x, 0.0, z);
                    Gl.glTexCoord2d(x, z + dz);
                    Gl.glVertex3d(x, 0.0, z + dz);
                    Gl.glTexCoord2d(x + dx, z + dz);
                    Gl.glVertex3d(x + dx, 0.0, z + dz);
                    Gl.glTexCoord2d(x + dx, z);
                    Gl.glVertex3d(x + dx, 0.0, z);
                }
            }
            Gl.glEnd();

            Gl.glDisable(Gl.GL_TEXTURE_2D); //關閉紋理映射功能
            Gl.glDisable(Gl.GL_COLOR_MATERIAL);
        }
        private void poster_chopper(int slices)
        {
            double dx = 1.0 / slices;
            double dz = 1.0 / slices;

            Gl.glEnable(Gl.GL_COLOR_MATERIAL);
            Gl.glColor3ub(255, 255, 255); //將背景設為白底，以免背景顏色影響海報的顏色
            Gl.glEnable(Gl.GL_TEXTURE_2D); //開啟紋理映射功能
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, texName[2]); //連結紋理物件

            Gl.glBegin(Gl.GL_QUADS); //繪製要貼圖的長方形
            Gl.glNormal3d(0.0, 1.0, 0.0); //設定長方形法向量以產生正確的光影
            for (double x = 0; x < 1.0; x += dx)
            {
                for (double z = 0; z < 1.0; z += dz)
                {
                    Gl.glTexCoord2d(x, z);
                    Gl.glVertex3d(x, 0.0, z);
                    Gl.glTexCoord2d(x, z + dz);
                    Gl.glVertex3d(x, 0.0, z + dz);
                    Gl.glTexCoord2d(x + dx, z + dz);
                    Gl.glVertex3d(x + dx, 0.0, z + dz);
                    Gl.glTexCoord2d(x + dx, z);
                    Gl.glVertex3d(x + dx, 0.0, z);
                }
            }
            Gl.glEnd();

            Gl.glDisable(Gl.GL_TEXTURE_2D); //關閉紋理映射功能
            Gl.glDisable(Gl.GL_COLOR_MATERIAL);
        }
        private void walls()
        {
            //底
            Gl.glPushMatrix();
            Gl.glScaled(width, 1.0, width);
            Gl.glTranslated(-0.5, 0.0, -0.5);
            plane(100);
            Gl.glPopMatrix();

            //頂
            Gl.glPushMatrix();
            Gl.glScaled(width, 1.0, width);
            Gl.glTranslated(-0.5, 0.0, -0.5);
            Gl.glTranslated(0.0, height, 0.0);
            plane(100);
            Gl.glPopMatrix();

            //前
            Gl.glPushMatrix();
            Gl.glTranslated(0.0, 0.0, -width / 2.0);
            Gl.glTranslated(0.0, height / 2.0, 0.0);
            Gl.glRotated(90, 1, 0, 0);
            Gl.glScaled(width, 1.0, height);
            Gl.glTranslated(-0.5, 0.0, -0.5);
            plane(100);
            Gl.glPopMatrix();

            //後
            Gl.glPushMatrix();
            Gl.glTranslated(0.0, 0.0, width / 2.0);
            Gl.glTranslated(0.0, height / 2.0, 0.0);
            Gl.glRotated(90, 1, 0, 0);
            Gl.glScaled(width, 1.0, height);
            Gl.glTranslated(-0.5, 0.0, -0.5);
            plane(100);
            Gl.glPopMatrix();

            //左
            Gl.glPushMatrix();
            Gl.glTranslated(-width / 2.0, 0.0, 0.0);
            Gl.glTranslated(0.0, height / 2.0, 0.0);
            Gl.glRotated(-90, 0, 0, 1);
            Gl.glScaled(height, 1.0, width);
            Gl.glTranslated(-0.5, 0.0, -0.5);
            plane(100);
            Gl.glPopMatrix();

            //右
            Gl.glPushMatrix();
            Gl.glTranslated(width / 2.0, 0.0, 0.0);
            Gl.glTranslated(0.0, height / 2.0, 0.0);
            Gl.glRotated(-90, 0, 0, 1);
            Gl.glScaled(height, 1.0, width);
            Gl.glTranslated(-0.5, 0.0, -0.5);
            plane(100);
            Gl.glPopMatrix();

        }
        private void glass()
        {
            Gl.glEnable(Gl.GL_BLEND);
            Gl.glEnable(Gl.GL_COLOR_MATERIAL);


            //頂
            Gl.glColor4d(0.0, 0.0, 1.0, 0.1);
            Gl.glPushMatrix();
            Gl.glNormal3d(0.0, 1.0, 0.0);
            Gl.glScaled(table_w, 1.0, table_w);
            Gl.glTranslated(-0.5, 0.0, -0.5);
            Gl.glTranslated(0.0, table_h, 0.0);
            plane(100);
            Gl.glPopMatrix();

            //前
            Gl.glColor4d(0.0, 0.0, 1.0, 0.1);
            Gl.glPushMatrix();
            Gl.glNormal3d(0.0, 0.0, 1.0);
            Gl.glTranslated(0.0, 0.0, -table_w / 2.0);
            Gl.glTranslated(0.0, table_h / 2.0, 0.0);
            Gl.glRotated(90, 1, 0, 0);
            Gl.glScaled(table_w, 1.0, table_h);
            Gl.glTranslated(-0.5, 0.0, -0.5);
            plane(100);
            Gl.glPopMatrix();

            //後
            Gl.glColor4d(0.0, 0.0, 1.0, 0.1);
            Gl.glPushMatrix();
            Gl.glNormal3d(0.0, 0.0, 1.0);
            Gl.glTranslated(0.0, 0.0, table_w / 2.0);
            Gl.glTranslated(0.0, table_h / 2.0, 0.0);
            Gl.glRotated(90, 1, 0, 0);
            Gl.glScaled(table_w, 1.0, table_h);
            Gl.glTranslated(-0.5, 0.0, -0.5);
            plane(100);
            Gl.glPopMatrix();

            //左
            Gl.glColor4d(0.0, 0.0, 1.0, 0.1);
            Gl.glPushMatrix();
            Gl.glNormal3d(1.0, 0.0, 0.0);
            Gl.glTranslated(-table_w / 2.0, 0.0, 0.0);
            Gl.glTranslated(0.0, table_h / 2.0, 0.0);
            Gl.glRotated(-90, 0, 0, 1);
            Gl.glScaled(table_h, 1.0, table_w);
            Gl.glTranslated(-0.5, 0.0, -0.5);
            plane(100);
            Gl.glPopMatrix();

            //右
            Gl.glColor4d(0.0, 0.0, 1.0, 0.1);
            Gl.glPushMatrix();
            Gl.glNormal3d(1.0, 0.0, 0.0);
            Gl.glTranslated(table_w / 2.0, 0.0, 0.0);
            Gl.glTranslated(0.0, table_h / 2.0, 0.0);
            Gl.glRotated(-90, 0, 0, 1);
            Gl.glScaled(table_h, 1.0, table_w);
            Gl.glTranslated(-0.5, 0.0, -0.5);
            plane(100);
            Gl.glPopMatrix();

            Gl.glDisable(Gl.GL_BLEND);
            Gl.glDisable(Gl.GL_COLOR_MATERIAL);
        }

       
        private void light_0()
        {
            float[] light0_position = new float[] { 0.0f, 0.0f, 0.0f, 1.0f };
            float[] light0_direction = new float[] { 0.0f, 0.0f, -1.0f };
            Gl.glEnable(Gl.GL_COLOR_MATERIAL);
            Gl.glPushMatrix();
            Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_POSITION, light0_position);
            Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_SPOT_DIRECTION, light0_direction);
            Gl.glLightf(Gl.GL_LIGHT0, Gl.GL_SPOT_CUTOFF, (float)(Math.Atan(100.0) * 180.0 / Math.PI));
            Gl.glLightf(Gl.GL_LIGHT0, Gl.GL_SPOT_EXPONENT, 10.0f);
            Gl.glColor3ub(255, 0, 0);
            Gl.glTranslated(0.0, 0.0, -1.0);
            //Glut.glutSolidCone(0.3, 1.0, 10, 10);
            Gl.glColor3ub(255, 255, 0);
            Gl.glScaled(1.0, 1.0, 0.01);
            //Gl.glDisable(Gl.GL_LIGHT0);
            Glut.glutSolidSphere(0.1, 10, 10);
            //Gl.glEnable(Gl.GL_LIGHT0);
            Gl.glPopMatrix();
            Gl.glDisable(Gl.GL_COLOR_MATERIAL);

        }

        private void light_1()
        {
            float[] light1_position = new float[] { 0.0f, 0.0f, 0.0f, 1.0f };
            float[] light1_direction = new float[] { 0.0f, 0.0f, -1.0f };
            Gl.glEnable(Gl.GL_COLOR_MATERIAL);
            Gl.glPushMatrix();
            Gl.glLightfv(Gl.GL_LIGHT1, Gl.GL_POSITION, light1_position);
            Gl.glLightfv(Gl.GL_LIGHT1, Gl.GL_SPOT_DIRECTION, light1_direction);
            Gl.glLightf(Gl.GL_LIGHT1, Gl.GL_SPOT_CUTOFF, (float)(Math.Atan(100.0) * 180.0 / Math.PI));
            Gl.glLightf(Gl.GL_LIGHT1, Gl.GL_SPOT_EXPONENT, 10.0f);
            Gl.glColor3ub(255, 0, 0);
            Gl.glTranslated(0.0, 0.0, -1.0);
            //Glut.glutSolidCone(0.3, 1.0, 10, 10);
            Gl.glColor3ub(255, 255, 0);
            Gl.glScaled(1.0, 1.0, 0.01);
            //Gl.glDisable(Gl.GL_LIGHT1);
            Glut.glutSolidSphere(0.1, 10, 10);
            //Gl.glEnable(Gl.GL_LIGHT1);
            Gl.glPopMatrix();
            Gl.glDisable(Gl.GL_COLOR_MATERIAL);

        }

        private void light_2()
        {
            float[] light2_position = new float[] { 0.0f, 0.0f, 0.0f, 1.0f };
            float[] light2_direction = new float[] { 0.0f, 0.0f, -1.0f };
            Gl.glEnable(Gl.GL_COLOR_MATERIAL);
            Gl.glPushMatrix();
            Gl.glLightfv(Gl.GL_LIGHT2, Gl.GL_POSITION, light2_position);
            Gl.glLightfv(Gl.GL_LIGHT2, Gl.GL_SPOT_DIRECTION, light2_direction);
            Gl.glLightf(Gl.GL_LIGHT2, Gl.GL_SPOT_CUTOFF, (float)(Math.Atan(100.0) * 180.0 / Math.PI));
            Gl.glLightf(Gl.GL_LIGHT2, Gl.GL_SPOT_EXPONENT, 10.0f);
            Gl.glColor3ub(255, 0, 0);
            Gl.glTranslated(0.0, 0.0, -1.0);
            //Glut.glutSolidCone(0.3, 1.0, 10, 10);
            Gl.glColor3ub(255, 255, 0);
            Gl.glScaled(1.0, 1.0, 0.01);
            //Gl.glDisable(Gl.GL_LIGHT2);
            Glut.glutSolidSphere(0.1, 10, 10);
            //Gl.glEnable(Gl.GL_LIGHT2);
            Gl.glPopMatrix();
            Gl.glDisable(Gl.GL_COLOR_MATERIAL);

        }

        private void light_3()
        {
            float[] light3_position = new float[] { 0.0f, 0.0f, 0.0f, 1.0f };
            float[] light3_direction = new float[] { 0.0f, 0.0f, -1.0f };
            Gl.glEnable(Gl.GL_COLOR_MATERIAL);
            Gl.glPushMatrix();
            Gl.glLightfv(Gl.GL_LIGHT3, Gl.GL_POSITION, light3_position);
            Gl.glLightfv(Gl.GL_LIGHT3, Gl.GL_SPOT_DIRECTION, light3_direction);
            Gl.glLightf(Gl.GL_LIGHT3, Gl.GL_SPOT_CUTOFF, (float)(Math.Atan(100.0) * 180.0 / Math.PI));
            Gl.glLightf(Gl.GL_LIGHT3, Gl.GL_SPOT_EXPONENT, 10.0f);
            Gl.glColor3ub(255, 0, 0);
            Gl.glTranslated(0.0, 0.0, -1.0);
            //Glut.glutSolidCone(0.3, 1.0, 10, 10);
            Gl.glColor3ub(255, 255, 0);
            Gl.glScaled(1.0, 1.0, 0.01);
            //Gl.glDisable(Gl.GL_LIGHT3);
            Glut.glutSolidSphere(0.1, 10, 10);
            //Gl.glEnable(Gl.GL_LIGHT3);
            Gl.glPopMatrix();
            Gl.glDisable(Gl.GL_COLOR_MATERIAL);

        }

        private void light_4()
        {
            float[] light4_position = new float[] { 0.0f, 0.0f, 0.0f, 1.0f };
            float[] light4_direction = new float[] { 0.0f, 0.0f, -1.0f };
            Gl.glEnable(Gl.GL_COLOR_MATERIAL);
            Gl.glPushMatrix();
            Gl.glLightfv(Gl.GL_LIGHT4, Gl.GL_POSITION, light4_position);
            Gl.glLightfv(Gl.GL_LIGHT4, Gl.GL_SPOT_DIRECTION, light4_direction);
            Gl.glLightf(Gl.GL_LIGHT4, Gl.GL_SPOT_CUTOFF, (float)(Math.Atan(100.0) * 180.0 / Math.PI));
            Gl.glLightf(Gl.GL_LIGHT4, Gl.GL_SPOT_EXPONENT, 10.0f);
            Gl.glColor3ub(255, 0, 0);
            Gl.glTranslated(0.0, 0.0, -1.0);
            //Glut.glutSolidCone(0.3, 1.0, 10, 10);
            Gl.glColor3ub(255, 255, 0);
            Gl.glScaled(1.0, 1.0, 0.01);
            //Gl.glDisable(Gl.GL_LIGHT4);
            Glut.glutSolidSphere(0.1, 10, 10);
            //Gl.glEnable(Gl.GL_LIGHT4);
            Gl.glPopMatrix();
            Gl.glDisable(Gl.GL_COLOR_MATERIAL);

        }
        private void table()
        {
            //底
            Gl.glPushMatrix();
            Gl.glScaled(table_w, 1.0, table_w);
            Gl.glTranslated(-0.5, 0.0, -0.5);
            plane(100);
            Gl.glPopMatrix();

            //頂
            Gl.glPushMatrix();
            Gl.glScaled(table_w, 1.0, table_w);
            Gl.glTranslated(-0.5, 0.0, -0.5);
            Gl.glTranslated(0.0, table_h, 0.0);
            plane(100);
            Gl.glPopMatrix();

            //前
            Gl.glPushMatrix();
            Gl.glTranslated(0.0, 0.0, -table_w / 2.0);
            Gl.glTranslated(0.0, table_h / 2.0, 0.0);
            Gl.glRotated(90, 1, 0, 0);
            Gl.glScaled(table_w, 1.0, table_h);
            Gl.glTranslated(-0.5, 0.0, -0.5);
            plane(100);
            Gl.glPopMatrix();

            //後
            Gl.glPushMatrix();
            Gl.glTranslated(0.0, 0.0, table_w / 2.0);
            Gl.glTranslated(0.0, table_h / 2.0, 0.0);
            Gl.glRotated(90, 1, 0, 0);
            Gl.glScaled(table_w, 1.0, table_h);
            Gl.glTranslated(-0.5, 0.0, -0.5);
            plane(100);
            Gl.glPopMatrix();

            //左
            Gl.glPushMatrix();
            Gl.glTranslated(-table_w / 2.0, 0.0, 0.0);
            Gl.glTranslated(0.0, table_h / 2.0, 0.0);
            Gl.glRotated(-90, 0, 0, 1);
            Gl.glScaled(table_h, 1.0, table_w);
            Gl.glTranslated(-0.5, 0.0, -0.5);
            plane(100);
            Gl.glPopMatrix();

            //右
            Gl.glPushMatrix();
            Gl.glTranslated(table_w / 2.0, 0.0, 0.0);
            Gl.glTranslated(0.0, table_h / 2.0, 0.0);
            Gl.glRotated(-90, 0, 0, 1);
            Gl.glScaled(table_h, 1.0, table_w);
            Gl.glTranslated(-0.5, 0.0, -0.5);
            plane(100);
            Gl.glPopMatrix();
        }
        private void poster()
        {
            //海報
            Gl.glEnable(Gl.GL_COLOR_MATERIAL);
            Gl.glColor3ub(255, 255, 255); //將背景設為白底，以免背景顏色影響海報的顏色
            Gl.glEnable(Gl.GL_TEXTURE_2D); //開啟紋理映射功能
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, texName[0]); //連結紋理物件
            Gl.glBegin(Gl.GL_QUADS); //繪製要貼圖的長方形
            Gl.glNormal3d(0.0, 1.0, 0.0); //設定長方形法向量以產生正確的光影
            Gl.glTexCoord2d(1.0, 0.0); //設定右下角的紋理座標
            Gl.glVertex3d(0.001, 0.3, 0.2); //長方形右下角的頂點
            Gl.glTexCoord2d(1.0, 1.0); //設定右上角的紋理座標
            Gl.glVertex3d(0.001, 0.3 + 0.5, 0.2); //長方形右上角的頂點
            Gl.glTexCoord2d(0.0, 1.0); //設定左上角的紋理座標
            Gl.glVertex3d(0.001, 0.3 + 0.5, 0.2 + 0.5 * 0.68298); //長方形左上角的頂點
            Gl.glTexCoord2d(0.0, 0.0); //設定左下角的紋理座標
            Gl.glVertex3d(0.001, 0.3, 0.2 + 0.5 * 0.68298); //長方形左下角的頂點
            Gl.glEnd();
            Gl.glDisable(Gl.GL_TEXTURE_2D); //關閉紋理映射功能
            Gl.glDisable(Gl.GL_COLOR_MATERIAL);
        }
        private void openGLControl1_Paint(object sender, PaintEventArgs e)
        {
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);//清除色彩、深度緩衝器  


            Gl.glMatrixMode(Gl.GL_MODELVIEW);//設定攝影機位置
            Gl.glLoadIdentity();


            cam.LookAt();



            Gl.glPushMatrix();
            Gl.glTranslated(0.0, 4.0, 0.0);
            Gl.glRotated(-90.0, 1.0, 0.0, 0.0);
            light_0();
            Gl.glPopMatrix();

            Gl.glPushMatrix();
            Gl.glTranslated(1.5, 4.0, -1.5);
            Gl.glRotated(-90.0, 1.0, 0.0, 0.0);
            light_1();
            Gl.glPopMatrix();

            Gl.glPushMatrix();
            Gl.glTranslated(1.5, 4.0, 1.5);
            Gl.glRotated(-90.0, 1.0, 0.0, 0.0);
            light_2();
            Gl.glPopMatrix();

            Gl.glPushMatrix();
            Gl.glTranslated(-1.5, 4.0, 1.5);
            Gl.glRotated(-90.0, 1.0, 0.0, 0.0);
            light_3();
            Gl.glPopMatrix();

            Gl.glPushMatrix();
            Gl.glTranslated(-1.5, 4.0, -1.5);
            Gl.glRotated(-90.0, 1.0, 0.0, 0.0);
            light_4();
            Gl.glPopMatrix();

            axes(5.0);

            Gl.glPushMatrix();
            Gl.glTranslated(0.0, 0.0, -width / 2.0);
            Gl.glTranslated(0.0, height / 2.0, 0.0);
            Gl.glRotated(-90, 1, 0, 0);
            poster_luffy(100);
            Gl.glPopMatrix();

            Gl.glPushMatrix();
            Gl.glTranslated(-width / 2.0, 0.0, 0.0);
            Gl.glTranslated(0.0, height / 2.0, 0.0);
            Gl.glRotated(90, 0, 0, 1);
            Gl.glRotated(90, 0, 1, 0);
            poster_zoro(100);
            Gl.glPopMatrix();

            Gl.glPushMatrix();
            Gl.glTranslated(width / 2.0, 0.0, 0.0);
            Gl.glTranslated(0.0, height / 2.0, 0.0);
            Gl.glRotated(90, 0, 0, 1);
            Gl.glRotated(90, 0, 1, 0);
            Gl.glRotated(-180, 0, 0, 1);
            poster_chopper(100);
            Gl.glPopMatrix();

            walls();
            table();

            


            Gl.glPushMatrix();
            float scale = 2.0f / modelSize;
            Gl.glScalef(scale, scale, scale);
            Gl.glTranslatef(-modelCenter[0], -modelCenter[1], -modelCenter[2]);
            Gl.glTranslated(0.0,table_h+0.5,0.0);
            Gl.glTranslated(0.0,-0.2, 0.3);
            Gl.glRotated(-90, 1, 0, 0);
            Gl.glScaled(0.2,0.2,0.2);
            myModel.DrawByOpenGL2();
            Gl.glPopMatrix();


            Gl.glPushMatrix();
            Gl.glTranslated(0.0, table_h, 0.0);
            Gl.glScaled(1.0, 0.5, 1.0);
            glass();//要放在最後面不然玻璃不會透
            Gl.glPopMatrix();

        }

    }
}
