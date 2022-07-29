using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        struct block
        {
            public int x_min;
            public int x_max;
            public int y_min;
            public int y_max;
            public int pixel;
            public float ratio;
        };
        struct compare_graph
        {
            public Bitmap graph;
            public char label;
        };
        struct compare_answer
        {
            public char label;
            public float similar;
        }

        List<compare_graph> graph_7 = new List<compare_graph>();
        List<compare_graph> graph_8 = new List<compare_graph>();

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Image = new Bitmap(openFileDialog1.FileName);
            }
        }
        private Bitmap Histogram(Bitmap source)//直方圖等化
        {
            Bitmap target = new Bitmap(source.Width, source.Height);

            int w = target.Width;//grapgh height & width
            int h = target.Height;

            float[] R = new float[256];
            float[] G = new float[256];
            float[] B = new float[256];

            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    R[source.GetPixel(i, j).R]++;
                    G[source.GetPixel(i, j).G]++;
                    B[source.GetPixel(i, j).B]++;
                }
            }

            for (int i = 0; i < 256; i++) //機率
            {
                R[i] /= source.Height * source.Width;
                G[i] /= source.Height * source.Width;
                B[i] /= source.Height * source.Width;
            }
            for (int i = 1; i < 256; i++) //累加
            {
                R[i] += R[i - 1];
                G[i] += G[i - 1];
                B[i] += B[i - 1];
            }
            for (int i = 0; i < 256; i++) //像數回乘
            {
                R[i] *= 255;
                G[i] *= 255;
                B[i] *= 255;
            }
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    target.SetPixel(i, j, Color.FromArgb((int)R[source.GetPixel(i, j).R], (int)G[source.GetPixel(i, j).G], (int)B[source.GetPixel(i, j).B]));
                }
            }
            return target;
        }
        private Bitmap Gray(Bitmap source) //rgb to gray
        {
            Bitmap target = new Bitmap(source.Width, source.Height);

            int w = target.Width;//grapgh height & width
            int h = target.Height;

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    int gray = (source.GetPixel(x, y).R + source.GetPixel(x, y).G + source.GetPixel(x, y).B) / 3;
                    target.SetPixel(x, y, Color.FromArgb(gray, gray, gray));
                }
            }
            return target;
        }
        private int[] cout_gray(Bitmap source)//counter gray pixel
        {
            int[] result = new int[256];
            int w = source.Width;
            int h = source.Height;
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    result[source.GetPixel(x, y).R]++;
                }
            }
            return result;
        }
        private int otus(int[] historgram, int total) //大津演算法_閥值
        {
            int sumB = 0;
            int sum1 = 0;
            float wB = 0;
            float wF = 0;
            float mF = 0;
            float max_var = 0;
            float inter_var = 0;
            int threshold = 0;
            short index_histo = 0;

            for (index_histo = 1; index_histo < 256; ++index_histo)
            {
                sum1 += index_histo * historgram[index_histo];
            }

            for (index_histo = 1; index_histo < 256; ++index_histo)
            {
                wB = wB + historgram[index_histo];
                wF = total - wB;
                if (wB == 0 || wF == 0)
                {
                    continue;
                }
                sumB = sumB + index_histo * historgram[index_histo];
                mF = (sum1 + sumB) / wF;
                inter_var = wB * wF * ((sumB / wB) - mF) * ((sumB / wB) - mF);
                if (inter_var >= max_var)
                {
                    threshold = index_histo;
                    max_var = inter_var;
                }
            }
            return threshold / 2;
        }
        private Bitmap Binarization(Bitmap source, int threshold) //二值化
        {
            Bitmap target = new Bitmap(source.Width, source.Height);
            int h = source.Height;
            int w = source.Width;
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (source.GetPixel(x, y).R < threshold)
                    {
                        target.SetPixel(x, y, Color.FromArgb(0, 0, 0));
                    }
                    else
                    {
                        target.SetPixel(x, y, Color.FromArgb(255, 255, 255));
                    }
                }
            }

            return target;
        }
        private Bitmap erosion(Bitmap source) //侵蝕
        {
            Bitmap target = new Bitmap(source.Width, source.Height);
            int w = source.Width;
            int h = source.Height;
            for (int x = 1; x < w - 1; x++)
            {
                for (int y = 1; y < h - 1; y++)
                {
                    if (source.GetPixel(x, y).R == 0 && source.GetPixel(x - 1, y).R == 0 && source.GetPixel(x + 1, y).R == 0 && source.GetPixel(x, y - 1).R == 0 && source.GetPixel(x, y + 1).R == 0)
                    {
                        target.SetPixel(x, y, Color.FromArgb(0, 0, 0));
                    }
                    else
                    {
                        target.SetPixel(x, y, Color.FromArgb(255, 255, 255));
                    }
                }
            }

            return target;
        }
        private Bitmap expansion(Bitmap source) //擴張
        {
            Bitmap target = new Bitmap(source.Width, source.Height);
            int w = source.Width;
            int h = source.Height;
            for (int x = 1; x < w - 1; x++)
            {
                for (int y = 1; y < h - 1; y++)
                {
                    if (source.GetPixel(x, y).R == 0 || source.GetPixel(x - 1, y).R == 0 || source.GetPixel(x + 1, y).R == 0 || source.GetPixel(x, y - 1).R == 0 || source.GetPixel(x, y + 1).R == 0)
                    {
                        target.SetPixel(x, y, Color.FromArgb(0, 0, 0));
                    }
                    else
                    {
                        target.SetPixel(x, y, Color.FromArgb(255, 255, 255));
                    }
                }
            }

            return target;
        }
        private Bitmap opening(Bitmap source) //斷開
        {
            return expansion(erosion(source));
        }
        private Bitmap closing(Bitmap source) //閉合
        {
            return erosion(expansion(source));
        }
        private int[,] CCimage_new(Bitmap source) //連通圖
        {
            int[,] label = new int[source.Width, source.Height];
            int label_count = 1;
            int w = source.Width;
            int h = source.Height;
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    int pic = source.GetPixel(x, y).R;
                    if (pic == 0)
                    {
                        if (x == 0 && y == 0)
                        {
                            label[x, y] = label_count;
                            label_count++;
                        }
                        else if (y == 0 && x != 0) //top line
                        {
                            if (label[x - 1, y] != 0)
                            {
                                label[x, y] = label[x - 1, y];
                            }
                            else
                            {
                                label[x, y] = label_count;
                                label_count++;
                            }
                        }
                        else if (x == 0 && y != 0)//left line
                        {
                            if (label[x, y - 1] != 0)
                            {
                                label[x, y] = label[x, y - 1];
                            }
                            else
                            {
                                label[x, y] = label_count;
                                label_count++;
                            }
                        }
                        else
                        {
                            if (label[x, y - 1] != 0 && label[x - 1, y] == 0) //Top != 0
                            {
                                label[x, y] = label[x, y - 1];
                            }
                            else if (label[x - 1, y] != 0 && label[x, y - 1] == 0) //Left !=0
                            {
                                label[x, y] = label[x - 1, y];
                            }
                            else if (label[x - 1, y] != 0 && label[x, y - 1] != 0) //Top != 0 && Left !=0
                            {
                                label[x, y] = label[x, y - 1];
                                int Toplabel = label[x, y - 1];
                                int Leftlabel = label[x - 1, y];
                                if (Toplabel != Leftlabel)
                                {
                                    for (int i = 0; i < w; i++)
                                    {
                                        for (int j = 0; j < h; j++)
                                        {
                                            if (label[i, j] == Leftlabel)
                                            {
                                                label[i, j] = Toplabel;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                label[x, y] = label_count;
                                label_count++;
                            }
                        }
                    }
                    else
                    {
                        label[x, y] = 0;
                    }

                }
            }
            return label;
        }
        private List<Bitmap> cut_graph(Bitmap source, int[,] label)//切割
        {

            Bitmap target = new Bitmap(source);
            List<Bitmap> plat_s = new List<Bitmap>();
            int w = source.Width;
            int h = source.Height;

            //有幾個區塊
            List<int> label_c = new List<int>();
            foreach (int i in label) //元素
            {
                if (i != 0)
                {
                    if (!label_c.Contains(i))
                    {
                        label_c.Add(i);
                    }
                }
            }

            block[] block_s = new block[label_c.Count];
            int index = 0;
            //分析區塊(計算區塊)
            foreach (int compare in label_c)
            {
                block_s[index].x_min = Int32.MaxValue;
                block_s[index].x_max = Int32.MinValue;
                block_s[index].y_min = Int32.MaxValue;
                block_s[index].y_max = Int32.MinValue;
                for (int x = 0; x < w; x++)
                {
                    for (int y = 0; y < h; y++)
                    {
                        if (label[x, y] == compare) //要的區塊
                        {
                            if (x < block_s[index].x_min)
                            {
                                block_s[index].x_min = x;
                            }
                            if (x > block_s[index].x_max)
                            {
                                block_s[index].x_max = x;
                            }

                            if (y < block_s[index].y_min)
                            {
                                block_s[index].y_min = y;
                            }
                            if (y > block_s[index].y_max)
                            {
                                block_s[index].y_max = y;
                            }
                        }
                    }
                }
                index++;
            }
            //分析區塊(計算各區塊大小及比例)
            for (int i = 0; i < index; i++)
            {
                int width = block_s[i].x_max - block_s[i].x_min;
                int height = block_s[i].y_max - block_s[i].y_min;
                block_s[i].pixel = width * height;
                block_s[i].ratio = (float)width / (float)height;
            }

            //判斷是否為需要的區塊(需要修改)
            List<Point> block_center = new List<Point>();
            foreach (block c_block in block_s)
            {
                if (c_block.ratio < 0.8 && c_block.pixel > 700)
                {
                    //copy 
                    block_center.Add(new Point((int)(c_block.x_max + c_block.x_min) / 2, (int)(c_block.y_max + c_block.y_min) / 2));//中心點
                    plat_s.Add(source.Clone(new Rectangle(c_block.x_min, c_block.y_min, c_block.x_max - c_block.x_min, c_block.y_max - c_block.y_min), source.PixelFormat));
                }
            }

            int angle = get_theta(block_center[0], block_center[block_center.Count - 1]);//計算第一個字元與最後一個字元連線與水平線夾角
          
            for (int i = 0; i < plat_s.Count; i++)
            {
                plat_s[i] = rotate(plat_s[i], 360 - angle);
            }

            return plat_s;
        }
        private int get_theta(Point p1, Point p2) //計算角度
        {
            int angle = (int)(Math.Atan2((p2.Y - p1.Y), (p2.X - p1.X)) * 180 / Math.PI);
            return angle;
        }
        private Bitmap rotate(Bitmap source, int theta) //旋轉
        {
            Bitmap target = new Bitmap(source.Width, source.Height);
            int x, y;
            double vcos, vsin;
            int center_x = (source.Width - 1) / 2, center_y = (source.Height - 1) / 2;
            int after_x, after_y;

            vsin = Math.Sin((double)theta * 0.01745329252);
            vcos = Math.Cos((double)theta * 0.01745329252);

            for (int i = 0; i < source.Width; i++)
            {
                for (int j = 0; j < source.Height; j++)
                {
                    after_x = i - center_x;
                    after_y = j - center_y;
                    x = (int)(after_x * vcos + after_y * vsin + 0.5 + center_x);
                    y = (int)(-after_x * vsin + after_y * vcos + 0.5 + center_y);

                    if (y >= 0 && y < source.Height && x >= 0 && x < source.Width)
                    {
                        target.SetPixel(i, j, Color.FromArgb(source.GetPixel(x, y).R, source.GetPixel(x, y).R, source.GetPixel(x, y).R));
                    }
                    else
                    {
                        target.SetPixel(i, j, Color.FromArgb(255, 255, 255));
                    }
                }
            }

            return target;
        }

        private compare_answer compare_max(Bitmap source, List<compare_graph> template)
        {
            compare_answer predict = new compare_answer();
            predict.label = ' ';
            predict.similar = 0;
            Bitmap temp = new Bitmap(source, 160, 400);
            int w = 160;
            int h = 400;

            foreach (compare_graph temp_child in template)
            {
                Bitmap t_c = new Bitmap(temp_child.graph);
                int same_pixel = 0;
                for (int x = 0; x < w; x++)
                {
                    for (int y = 0; y < h; y++)
                    {
                        if (temp.GetPixel(x, y).R == 0 && t_c.GetPixel(x, y).R == 0)
                        {
                            same_pixel += 3;
                        }
                        else if (temp.GetPixel(x, y).R == 255) //溢出
                        {
                            same_pixel -= 1;
                        }
                        else if (temp.GetPixel(x, y).R == 0) //缺少
                        {
                            same_pixel -= 2;
                        }
                    }
                }
                float similar = (float)same_pixel / (w * h);
                if (similar > predict.similar)
                {
                    predict.similar = similar;
                    predict.label = temp_child.label;
                }
            }
            return predict;
        }

        private void button2_Click(object sender, EventArgs e)
        {

            panel1.Controls.Clear();
            Bitmap result = new Bitmap(pictureBox1.Image);

            //if (result.Height > result.Width) 
            //{
            //   result.RotateFlip(RotateFlipType.Rotate90FlipX);//旋轉90度
            //}

            //Bitmap result_re = new Bitmap(rotate(result, 180));
            result = Gray(result);
            //result_re = Gray(result_re);
            //result = Histogram(result);

            int[] gray_count = cout_gray(result);//計算灰階度
            //int[] gray_count_re = cout_gray(result_re);

            int threshold = otus(gray_count, result.Width * result.Height);//計算閥值
            //int threshold_re = otus(gray_count_re, result_re.Width * result_re.Height);


            result = Binarization(result, threshold);//二值化
            //result_re = Binarization(result_re, threshold_re);

            result = erosion(result);
            //result_re = erosion(result_re);

            int[,] labels = CCimage_new(result);//連通圖
            //int[,] labels_re = CCimage_new(result_re);

            List<Bitmap> result_list = cut_graph(result, labels);//分割結果
            //List<Bitmap> result_list_re = cut_graph(result_re, labels_re);


            //顯示字元
            int num = result_list.Count;
            PictureBox[] pb = new PictureBox[num];
            int index = 0;
            foreach (Bitmap i in result_list)
            {
                //顯示
                pb[index] = new System.Windows.Forms.PictureBox();
                pb[index].BringToFront();
                pb[index].Location = new Point(0, index * 75);
                pb[index].SizeMode = PictureBoxSizeMode.Zoom;
                pb[index].Image = i;
                panel1.Controls.Add(pb[index]);
                index++;

            }
            //比對結果
            List<compare_answer> answer = new List<compare_answer>();
            foreach (Bitmap i in result_list)
            {
                if (result_list.Count == 6)
                {
                    Bitmap a = erosion(i);
                    answer.Add(compare_max(i, graph_7));
                }
                else if (result_list.Count == 7)
                {
                    Bitmap a = expansion(i);
                    answer.Add(compare_max(a, graph_8));
                }
            }

            //List<compare_answer> answer_re = new List<compare_answer>();
            //foreach (Bitmap i in result_list_re)
            //{
            //    if (result_list_re.Count == 6)
            //    {
            //        Bitmap a = erosion(i);
            //        answer_re.Add(compare_max(i, graph_7));
            //    }
            //    else if (result_list_re.Count == 7)
            //    {
            //        Bitmap a = expansion(i);
            //        answer_re.Add(compare_max(a, graph_8));
            //    }
            //}

            float sum_answer = 0;
            //float sum_answer_re=0;

            foreach (compare_answer a in answer)
            {
                sum_answer += a.similar;
            }

            //foreach (compare_answer a in answer_re)
            //{
            //    sum_answer_re += a.similar;
            //}

            string charter = "";
            //if (sum_answer > sum_answer_re)
            //{
            foreach (compare_answer a in answer)
            {
                charter += a.label;
            }
            //}
            //else 
            //{
            //    foreach (compare_answer a in answer_re)
            //    {
            //        charter += a.label;
            //    }
            //}
            pictureBox2.Image = result;
            MessageBox.Show(charter);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            DirectoryInfo dir = new DirectoryInfo(@"../../graph_7/");//讀取對照圖
            foreach (var dchild in dir.GetFiles())
            {
                compare_graph temp = new compare_graph();
                temp.graph = new Bitmap(dchild.FullName);
                temp.graph = new Bitmap(temp.graph, 160, 400);
                temp.label = dchild.Name[0];
                graph_7.Add(temp);
            }

            dir = new DirectoryInfo(@"../../graph_8/");//讀取對照圖
            foreach (var dchild in dir.GetFiles())
            {
                compare_graph temp = new compare_graph();
                temp.graph = new Bitmap(dchild.FullName);
                temp.graph = new Bitmap(temp.graph, 160, 400);
                temp.label = dchild.Name[0];
                graph_8.Add(temp);
            }
        }
    }
}