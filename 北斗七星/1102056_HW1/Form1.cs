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

namespace _1102056_HW1
{
    public partial class Form1 : Form
    {
        int x = 0, y = 0;
        public Form1()
        {
            InitializeComponent();
            this.openGLControl1.InitializeContexts();
            MessageBox.Show("可以按WSAD的上下左右移動北斗七星喔!");
        }

        private void openGLControl1_Load(object sender, EventArgs e)
        {
            Gl.glClearColor(0.0f, 0.0f, 0.0f, 1.0f);  //設定清除畫面的-顏色-

            Gl.glMatrixMode(Gl.GL_PROJECTION); //告知系統目前要更改的是-投影矩陣-
            Gl.glLoadIdentity(); //將目前矩陣設為-單位矩陣Identity-(即初始化)
            Glu.gluOrtho2D(0.0f, this.openGLControl1.Size.Width, 0.0f, this.openGLControl1.Size.Height); //可設定繪圖範圍(left,right,bottom,top)
            //物體如果在範圍外不會顯示
        }

        private void openGLControl1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.D:
                    x += 5;
                    break;
                case Keys.A:
                    x += -5;
                    break;
                case Keys.W:
                    y += 5;
                    break;
                case Keys.S:
                    y += -5;
                    break;
            }
            this.openGLControl1.Refresh();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            label1.Text = "左右平移" + x + "上下平移" + y;
        }

        private void openGLControl1_Paint(object sender, PaintEventArgs e)
        {
            //繪製小星星
            Random rn = new Random(0); //亂數產生器  裡面可放種子  一樣亂數序列就會相同
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT); //清除畫面 將色彩緩衝器全部清除
            Gl.glPointSize(1.0f);
            Gl.glColor3ub(255,255,255);
            Gl.glBegin(Gl.GL_POINTS);
            for (int i = 0; i < 200; i++)  //會有200星星
            {
                Gl.glVertex2i(rn.Next(0, this.openGLControl1.Size.Width),
                rn.Next(0, this.openGLControl1.Size.Height));//0-799/0-449之間隨機產生
            }
            Gl.glEnd();


            Gl.glPointSize(4.0f);//設定北斗七星大小//不能放在begin/end中間


            //繪製北斗七星
            Gl.glBegin(Gl.GL_POINTS);//成對出現


            Gl.glColor3ub((byte)255, (byte)0, (byte)0);//red
            Gl.glVertex2i(320 + x, 200 + y);//設定位置
            Gl.glColor3ub((byte)255, (byte)128, (byte)0);//orange
            Gl.glVertex2i(340 + x, 160 + y);
            Gl.glColor3ub((byte)255, (byte)255, (byte)0);//yellow
            Gl.glVertex2i(295 + x, 110 + y);
            Gl.glColor3ub((byte)0, (byte)128, (byte)0);//green
            Gl.glVertex2i(260 + x, 135 + y);
            Gl.glColor3ub((byte)0, (byte)0, (byte)255);//blue
            Gl.glVertex2i(210 + x, 120 + y);
            Gl.glColor3ub((byte)255, (byte)0, (byte)255);//pink
            Gl.glVertex2i(165 + x, 110 + y);
            Gl.glColor3ub((byte)128, (byte)0, (byte)128);//purple
            Gl.glVertex2i(120 + x, 50 + y);


            Gl.glEnd();//成對出現

            


        }
    }
}
