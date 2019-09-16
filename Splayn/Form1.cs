using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Drawing2D;

namespace Splayn
{
    public partial class Form1 : Form
    {
        //List<Point> points = new List<Point>();
        List<Tuple<double, double>> points = new List<Tuple<double, double>>();
        List<Tuple<double, double>> Sortpoint = new List<Tuple<double, double>>();
        double[] masA; //массив a2, a3... для канонических уравнений
        double[] hi;      //значения hi
        double[] masCi;//массив C2, C3 ...
        double[] coefA; //коэффициент А для прогонки
        double[] coefB; //коэффициент Б для прогонки
        double[] Bi; //значения b для канонических уравнений
        double[] Di; //значения d для канонических уравнений
        double[] PiX;
        double[][] SysLinEq; //система линейных уравнений
        float ScaleLines = 30; //масштаб делений на оси X
        int indSort = 0; //индекс для сортировки (количество замен)

        bool[] kanonPolin; // рисование канонических полиномов 

        public Form1()
        {
            InitializeComponent();
        }

        private void textBox2_TextChanged(object sender, EventArgs e) //ввод Х точки
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e) //ввод У точки
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e) //формируем массив точек
        {

        }

        private void button1_Click(object sender, EventArgs e) //добавляем точку
        {
            if (textBox2.Text == "") { textBox2.Text = "0"; }
            if (textBox3.Text == "") { textBox3.Text = "0"; }
            //points.Add(new Point(int.Parse(textBox2.Text), int.Parse(textBox3.Text)));
            if (points.Count == 8) { MessageBox.Show("Вы ввели 8 точек, больше нельзя"); }
            else if (points.Count < 8)
            {
                points.Add(new Tuple<double, double>(double.Parse(textBox2.Text), double.Parse(textBox3.Text))); //добавляем точку
                label5.Text = "";
                textBox2.Clear(); textBox3.Clear();
                for (int i = 0; i < points.Count; i++)
                {
                    Console.WriteLine(points[i].Item1);
                    Console.WriteLine(points[i].Item2);
                    Console.WriteLine(points[i].Item1 + points[i].Item2);
                    Console.WriteLine("    ");
                    //Console.WriteLine(points.Count);
                    label5.Text += "T" + (i+1) + " = ( " + points[i].Item1 + " ; " + points[i].Item2 + " ), ";
                }
            }
            kanonPolin = new bool[points.Count - 1]; //значения для рисования n-1 полиномов
            for (int i = 0; i < points.Count - 1; i++)
            {
                kanonPolin[i] = false;
            }

            //отсортируем элементы пузырьком
            double tempX;
            double tempY;
            
            for (int i = 0; i < points.Count; i++)
            {
                for (int j = i + 1; j < points.Count; j++)
                {
                    if (points[i].Item1 > points[j].Item1)
                    {
                        tempX = points[i].Item1;
                        tempY = points[i].Item2;
                        points[i] = points[j];
                        Sortpoint.Add(new Tuple<double, double>(tempX, tempY)); //для присвоения значения 
                        points[j] = Sortpoint[indSort]; //присваиваем значение
                        indSort++; //повышаем индекс

                    }
                }
            }
            label5.Text = "";
            for (int i = 0; i < points.Count; i++) //вывод отсортированных точек
            {
               label5.Text += "T" + (i + 1) + " = ( " + points[i].Item1 + " ; " + points[i].Item2 + " ), ";
            }
            

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (points.Count <= 3) { MessageBox.Show("Вы ввели недостаточное количество точек, минимальное количество точек = 4"); Environment.Exit(0); }
            masA = new double[points.Count - 1]; //Используются в канонических уравнениях
            Bi = new double[points.Count - 1];
            Di = new double[points.Count - 1];
            masCi = new double[points.Count];

            hi = new double[points.Count - 1];
            coefA = new double[points.Count - 3];
            coefB = new double[points.Count - 3];
            int ShiftMatr = 0; //сдвиг строки матрицы при переходе на новые строки
            int indI, indJ; //индексы для матрицы
            bool DelIn0 = false; //деление на 0
                           
            SysLinEq = new double[points.Count - 2][]; indI = points.Count - 2;
            indJ = points.Count - 1;


            for (int i = 0; i < indI; i++)
            {
                SysLinEq[i] = new double[indJ]; //количество уравнений
                for (int j = 0; j < indJ; j++)
                {
                    SysLinEq[i][j] = 0;
                }
            }

            masCi[0] = 0; masCi[points.Count - 1] = 0; //первый и последний коэффициент = 0 (естественный кубический сплайн)
            for (int i = 0; i < points.Count - 1; i++)
            {
                masA[i] = points[i].Item2;
                hi[i] = points[i + 1].Item1 - points[i].Item1;
                //Console.WriteLine("------------------");
                //Console.WriteLine(hi[i]);
            }
            for (int i = 0; i < indI; i++)
            {
                if ((hi[i] + hi[i + 1]) == 0 || hi[i] == 0 || hi[i] == 0) { MessageBox.Show("Случилось деление на 0, введите другие точки"); DelIn0 = true; }
                else
                {
                    if (i == 0) { SysLinEq[0][0] = 1; SysLinEq[0][1] = hi[1] / (2.0 * (hi[0] + hi[1])); SysLinEq[0][indJ - 1] = (3.0 * ((points[2].Item2 - points[1].Item2) / hi[1] - (points[1].Item2 - points[0].Item2) / hi[0])) / (2.0 * (hi[0] + hi[1])); }
                    else if (i != 0 && i != indI - 1) { SysLinEq[i][ShiftMatr] = hi[ShiftMatr + 1]; SysLinEq[i][ShiftMatr + 1] = 2.0 * (hi[ShiftMatr + 1] + hi[ShiftMatr + 2]); SysLinEq[i][ShiftMatr + 2] = hi[ShiftMatr + 2]; SysLinEq[i][indJ - 1] = 3.0 * ((points[i + 2].Item2 - points[i + 1].Item2) / hi[i + 1] - (points[i + 1].Item2 - points[i].Item2) / hi[i]); ShiftMatr++; }
                    else if (i == indI - 1) { SysLinEq[i][i] = 1; SysLinEq[i][i - 1] = hi[i] / (2.0 * (hi[i] + hi[i + 1])); SysLinEq[i][indJ - 1] = (3.0 * ((points[i + 2].Item2 - points[i + 1].Item2) / hi[i + 1] - (points[i + 1].Item2 - points[i].Item2) / hi[i])) / (2.0 * (hi[i] + hi[i + 1])); }
                }
            }


            for (int i = 0; i < indI; i++)
            {
                for (int j = 0; j < indJ; j++)
                {
                    Console.Write(SysLinEq[i][j] + " ");
                }
                Console.WriteLine();
            }

            coefA[0] = -SysLinEq[0][1];       //Вписываем значения первых А и Б, далее считаем следующие
            coefB[0] = SysLinEq[0][indJ-1];
            for (int i = 1; i < indI-1; i++)
            {
                if ((SysLinEq[i][i - 1] * coefA[i - 1] + SysLinEq[i][i]) == 0) { MessageBox.Show("Случилось деление на 0, введите другие точки"); DelIn0 = true; }
                else
                {
                    coefA[i] = -SysLinEq[i][i + 1] / (SysLinEq[i][i - 1] * coefA[i - 1] + SysLinEq[i][i]);
                    coefB[i] = (SysLinEq[i][indJ - 1] - SysLinEq[i][i - 1] * coefB[i - 1]) / (SysLinEq[i][i - 1] * coefA[i - 1] + SysLinEq[i][i]);
                }
            }
            if (DelIn0) //выход из программы, если ошибка / на 0
            {
                Environment.Exit(0);
            }

           for (int j = 0; j < indI-1; j++)
           {
               Console.Write(coefA[j] + " " + coefB[j] + " | ");
           }
           Console.WriteLine();

            //считаем Ci
            masCi[points.Count - 2] = (SysLinEq[indI - 1][indJ - 1] - SysLinEq[indI - 1][indJ - 3] * coefB[indI - 2]) / ( 1.0 + SysLinEq[indI - 1][indJ - 3] * coefA[indI - 2] );
            Console.WriteLine((SysLinEq[indI - 1][indJ - 1] - SysLinEq[indI - 1][indJ - 3] * coefB[indI - 2]) + " * " + (1.0 + SysLinEq[indI - 1][indJ - 3] * coefA[indI - 2]));

            int NumC = points.Count - 3;
            int Ind = indI - 2;
            while (Ind >= 0)
            {
                masCi[NumC] = coefA[Ind] * masCi[NumC + 1] + coefB[Ind];
                Console.WriteLine(masCi[NumC] + " " + coefA[Ind] + " * " + masCi[NumC + 1] + "  " + coefB[Ind]);
                NumC--;
                Ind--;
            }
            label1.Text = "";
            for (int j = 0; j < points.Count; j++) //вывод в консоль Ci
            {
                //label1.Text += Convert.ToString(masCi[j] + '\n');
                Console.WriteLine(masCi[j] + " || ");
            }

            for (int i = 0; i < points.Count - 1; i++) //считаем bi и di
            {
                Bi[i] = (points[i + 1].Item2 - points[i].Item2) / hi[i] - masCi[i] * hi[i] - ((masCi[i + 1] - masCi[i]) / 3.0) * hi[i];
                Di[i] = (masCi[i + 1] - masCi[i]) / (3.0 * hi[i]);
            }

            for (int i = 0; i < points.Count - 1; i++) //выводим канонические уравнения в label
            {
                label1.Text += "P" + (i + 2) + " = " + " + " + " ( " + Convert.ToString(Math.Round(masA[i], 4)) + " ) " + " " + Convert.ToString(Math.Round(Bi[i], 4))  + "( x - (" + Convert.ToString(Math.Round(points[i].Item1, 4)) + ") ) " + " + " + "( " + Convert.ToString(Math.Round(masCi[i], 4)) + " ) " + "( x - (" + Convert.ToString(Math.Round(points[i].Item1, 4)) + ") )^2 " + " + " + " ( " +Convert.ToString(Math.Round(Di[i], 4)) + " ) " + " (x - (" + Convert.ToString(Math.Round(points[i].Item1, 4)) + ") )^3 " + '\n';
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {

            Pen pen1 = new Pen(Color.Black, 2);

            Bitmap BitmapGr = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics g = Graphics.FromImage(BitmapGr);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            //Получение центра оси координат Х,Y
            int centerX = pictureBox1.ClientSize.Width / 2;
            int centerY = pictureBox1.Height / 2;

            //Посчитаем нужный масштаб для нашего рисунка по точкам
            double maxDistX; //максимальное расстояние по Х (по модулю)
            double maxDistY; //максимальное расстояние по Y (по модулю)
            double minY1 = points[0].Item2;
            double maxY1 = 0, maxY2 = 0;
            maxDistX = Math.Abs(points[0].Item1) + Math.Abs(points[points.Count - 1].Item1);
            for (int i = 0; i < points.Count; i++)
            {
                if (maxY1 < points[i].Item2) { maxY1 = points[i].Item2; }
                if (minY1 > points[i].Item2) { minY1 = points[i].Item2; }
            }
            for (int i = 0; i < points.Count; i++)
            {
                if (maxY2 < points[i].Item2 && points[i].Item2 != maxY1) { maxY2 = points[i].Item2; }
            }

            maxDistY = Math.Abs(maxY1) + Math.Abs(maxY2);

           
            //Работа с масштабом ...
            if (minY1 > 0) { centerY += (int)ScaleLines * (int)Math.Abs(minY1) + 1; }//опускаем нижнюю часть, если большая часть графика вверху


            if (points[0].Item1 >= -4 && points[points.Count - 1].Item1 <= 4 && minY1 >= -3 && maxY1 <= 3) { ScaleLines = 55; }
            else if (points[0].Item1 >= -5 && points[points.Count - 1].Item1 <= 5 && minY1 >= -4 && maxY1 <= 4) { ScaleLines = 50; }
            else if (points[0].Item1 >= -6 && points[points.Count - 1].Item1 <= 6 && minY1 >= -4 && maxY1 <= 4) { ScaleLines = 45; }
            else if (points[0].Item1 >= -7 && points[points.Count - 1].Item1 <= 7 && minY1 >= -5 && maxY1 <= 5) { ScaleLines = 40; }
            else if (points[0].Item1 >= -8 && points[points.Count - 1].Item1 <= 8 && minY1 >= -6 && maxY1 <= 6) { ScaleLines = 35; }
            else if (points[0].Item1 >= -9 && points[points.Count - 1].Item1 <= 9 && minY1 >= -7 && maxY1 <= 7) { ScaleLines = 30; }
            else if (points[0].Item1 >= -10 && points[points.Count - 1].Item1 <= 10 && minY1 >= -8 && maxY1 <= 8) { ScaleLines = 25; }
            else if (points[0].Item1 >= -11 && points[points.Count - 1].Item1 <= 11 && minY1 >= -9 && maxY1 <= 9) { ScaleLines = 20; }
            else ScaleLines = 10;
            // ...


            //Прорисовка оси
            g.Clear(Color.White);
            g.DrawLine(pen1, 0, centerY, centerX * 2, centerY);//Ось OX
            g.DrawLine(pen1, centerX, 0, centerX, centerY * 2);//Ось OY
            for (int i = 1; i < centerX / ScaleLines + 1; i++) //единичные отрезки для оси x
            {
                g.DrawLine(pen1, centerX + i * ScaleLines, centerY + 5, centerX + i * ScaleLines, centerY - 5);
                g.DrawLine(pen1, centerX - i * ScaleLines, centerY + 5, centerX - i * ScaleLines, centerY - 5);
            }
            for (int i = 1; i < centerY / ScaleLines + 1; i++) //единичные отрезки для оси y
            {
                g.DrawLine(pen1, centerX + 5, centerY + i * ScaleLines, centerX - 5, centerY + i * ScaleLines);
                g.DrawLine(pen1, centerX + 5, centerY - i * ScaleLines, centerX - 5, centerY - i * ScaleLines);
            }


            // откуда до куда будем строить
            const int minX = -25;
            const int maxX = 25;

            // массив точек, которые надо отрисовать
            PointF[] points2 = new PointF[500];
            for (int k = 0; k < points.Count - 1; k++)
            {
                if (k == 0) { pen1.Color = Color.Aqua; }
                if (k == 1) { pen1.Color = Color.Blue; }
                if (k == 2) { pen1.Color = Color.Green; }
                if (k == 3) { pen1.Color = Color.Orange; }
                if (k == 4) { pen1.Color = Color.IndianRed; }
                if (k == 5) { pen1.Color = Color.Olive; }
                if (k == 6) { pen1.Color = Color.Yellow; }
                if (k == 7) { pen1.Color = Color.Violet; }

                float x = -25;
                    for (int i = 0; i < points2.Length; i++)
                    {
                        x += (float)0.1;
                        float y = (float)masA[k] + (float)Bi[k] * (x - (float)points[k].Item1) + (float)masCi[k] * (float)Math.Pow((x - (float)points[k].Item1), 2) + (float)Di[k] * (float)Math.Pow((x - (float)points[k].Item1), 3);
                        points2[i] = new PointF(x, y); //формируем точки для графика

                    }

                    for (int i = 0; i < points2.Length; i++) //подгоняем точки под наш масштаб
                    {
                        points2[i].X = centerX + ScaleLines * points2[i].X;
                        points2[i].Y = centerY - ScaleLines * points2[i].Y;
                    }
                if (kanonPolin[k] == true)
                {
                    g.DrawLines(pen1, points2); //рисуем график
                }
            }

            pen1.Color = Color.Red;
            Color brushcol = Color.Red;
            for (int i = 0; i < points.Count; i++) //рисуем полученные точки
            {                                  //коэффициент для подгонки точки по х            коэффициент для подгонки точки по y
                g.DrawEllipse(pen1, (float)(centerX - 4.3 + ScaleLines * (float)points[i].Item1), (float)(centerY - 3 - ScaleLines * (float)points[i].Item2), 6, 6);
                g.FillEllipse(new SolidBrush(brushcol), (float)(centerX - 4.3 + ScaleLines * (float)points[i].Item1), (float)(centerY - 3 - ScaleLines * (float)points[i].Item2), 6, 6);

            }
 

            PointF[] points3;
            int NumPoints = 0;
            int t = 0;
            for (int i = 0; i < points.Count-1; i++)    //рисуем сплайн
            {
                NumPoints = 0;
                for (float j = (float)points[i].Item1; j <= points[i + 1].Item1+0.05; j += (float)0.05) // j в роли х
                {
                   NumPoints++; //посчитали количество точек
                }
                points3 = new PointF[NumPoints];
                t = 0;
                for (float j = (float)points[i].Item1; j <= points[i + 1].Item1+0.05; j += (float)0.05) // j в роли х
                {
                    float y = (float)masA[i] + (float)Bi[i] * (j - (float)points[i].Item1) + (float)masCi[i] * (float)Math.Pow((j - (float)points[i].Item1), 2) + (float)Di[i] * (float)Math.Pow((j - (float)points[i].Item1), 3);
                    
                    points3[t] = new PointF(j, y); //формируем точки для графика
                    t++;
                    
                }

                for (int k = 0; k < NumPoints; k++) //подгоняем точки под наш масштаб
                {
                    Console.Write(" k = " + k + " : " + points3[k].X + " " + points3[k].Y + " | ");
                }
                Console.WriteLine();

                for (int k = 0; k < NumPoints; k++) //подгоняем точки под наш масштаб
                {
                    points3[k].X = centerX + ScaleLines * points3[k].X;
                    points3[k].Y = centerY - ScaleLines * points3[k].Y;
                }

                g.DrawLines(pen1, points3);
            }

            
                pictureBox1.Image = BitmapGr;

        }

        private void сохранитьКаноническиеУравненияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog spd = new SaveFileDialog();
            if (spd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using (StreamWriter stream = new StreamWriter(spd.FileName))
                {
                    for (int i = 0; i < points.Count-1; i++)
                    {
                        stream.Write("P" + (i + 2) + " = " + " + " + " ( " + Convert.ToString(Math.Round(masA[i], 4)) + " ) " + " " + Convert.ToString(Math.Round(Bi[i], 4)) + "( x - (" + Convert.ToString(Math.Round(points[i].Item1, 4)) + ") ) " + " + " + "( " + Convert.ToString(Math.Round(masCi[i], 4)) + " ) " + "( x - (" + Convert.ToString(Math.Round(points[i].Item1, 4)) + ") )^2 " + " + " + " ( " + Convert.ToString(Math.Round(Di[i], 4)) + " ) " + " (x - (" + Convert.ToString(Math.Round(points[i].Item1, 4)) + ") )^3 ") ;
                        stream.WriteLine();
                    }
                }

            }
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            char text = e.KeyChar;
            if (!Char.IsDigit(text) && text != 8 && text != '-' && text != ',') //Если символ - не цифра (IsDigit),
            {
                e.Handled = true;// то событие не обрабатывается 
            }
        }

        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            char text = e.KeyChar;

            if (!Char.IsDigit(text) && text != 8  && text != '-' && text != ',') //Если символ - не цифра (IsDigit),
            {
                e.Handled = true;// то событие не обрабатывается 
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            kanonPolin[0] = checkBox1.Checked;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            kanonPolin[1] = checkBox2.Checked;
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            kanonPolin[2] = checkBox3.Checked;
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if (points.Count >= 5) { kanonPolin[3] = checkBox4.Checked; }
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            if (points.Count >= 6) { kanonPolin[4] = checkBox5.Checked; }
        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            if (points.Count >= 7) { kanonPolin[5] = checkBox6.Checked; }
        }

        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {
            if (points.Count >= 8) { kanonPolin[6] = checkBox7.Checked; }
        }


        //Обновление рисунка
        private void button4_Click(object sender, EventArgs e) 
        {

            Pen pen1 = new Pen(Color.Black, 2);

            Bitmap BitmapGr = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics g = Graphics.FromImage(BitmapGr);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            //Получение центра оси координат Х,Y
            int centerX = pictureBox1.ClientSize.Width / 2;
            int centerY = pictureBox1.Height / 2;

            //Посчитаем нужный масштаб для нашего рисунка по точкам
            double maxDistX; //максимальное расстояние по Х (по модулю)
            double maxDistY; //максимальное расстояние по Y (по модулю)
            double minY1 = points[0].Item2;
            double maxY1 = 0, maxY2 = 0;
            maxDistX = Math.Abs(points[0].Item1) + Math.Abs(points[points.Count - 1].Item1);
            for (int i = 0; i < points.Count; i++)
            {
                if (maxY1 < points[i].Item2) { maxY1 = points[i].Item2; }
                if (minY1 > points[i].Item2) { minY1 = points[i].Item2; }
            }
            for (int i = 0; i < points.Count; i++)
            {
                if (maxY2 < points[i].Item2 && points[i].Item2 != maxY1) { maxY2 = points[i].Item2; }
            }

            maxDistY = Math.Abs(maxY1) + Math.Abs(maxY2);


            //Работа с масштабом ...
            if (minY1 > 0) { centerY += (int)ScaleLines * (int)Math.Abs(minY1) + 1; }//опускаем нижнюю часть, если большая часть графика вверху
                                                                                     

            if (points[0].Item1 >= -4 && points[points.Count - 1].Item1 <= 4 && minY1 >= -3 && maxY1 <= 3) { ScaleLines = 55; }
            else if (points[0].Item1 >= -5 && points[points.Count - 1].Item1 <= 5 && minY1 >= -4 && maxY1 <= 4) { ScaleLines = 50; }
            else if (points[0].Item1 >= -6 && points[points.Count - 1].Item1 <= 6 && minY1 >= -4 && maxY1 <= 4) { ScaleLines = 45; }
            else if (points[0].Item1 >= -7 && points[points.Count - 1].Item1 <= 7 && minY1 >= -5 && maxY1 <= 5) { ScaleLines = 40; }
            else if (points[0].Item1 >= -8 && points[points.Count - 1].Item1 <= 8 && minY1 >= -6 && maxY1 <= 6) { ScaleLines = 35; }
            else if (points[0].Item1 >= -9 && points[points.Count - 1].Item1 <= 9 && minY1 >= -7 && maxY1 <= 7) { ScaleLines = 30; }
            else if (points[0].Item1 >= -10 && points[points.Count - 1].Item1 <= 10 && minY1 >= -8 && maxY1 <= 8) { ScaleLines = 25; }
            else if (points[0].Item1 >= -11 && points[points.Count - 1].Item1 <= 11 && minY1 >= -9 && maxY1 <= 9) { ScaleLines = 20; }
            else ScaleLines = 10;
            // ...
            

            //Прорисовка оси
            g.Clear(Color.White);
            g.DrawLine(pen1, 0, centerY, centerX * 2, centerY);//Ось OX
            g.DrawLine(pen1, centerX, 0, centerX, centerY * 2);//Ось OY
            for (int i = 1; i < centerX / ScaleLines + 1; i++) //единичные отрезки для оси x
            {
                g.DrawLine(pen1, centerX + i * ScaleLines, centerY + 5, centerX + i * ScaleLines, centerY - 5);
                g.DrawLine(pen1, centerX - i * ScaleLines, centerY + 5, centerX - i * ScaleLines, centerY - 5);
            }
            for (int i = 1; i < centerY / ScaleLines + 1; i++) //единичные отрезки для оси y
            {
                g.DrawLine(pen1, centerX + 5, centerY + i * ScaleLines, centerX - 5, centerY + i * ScaleLines);
                g.DrawLine(pen1, centerX + 5, centerY - i * ScaleLines, centerX - 5, centerY - i * ScaleLines);
            }

            //g.DrawLine(pen1, centerX - 10, centerY - 10, centerX - 20, centerY - 20);



            // откуда до куда будем строить
            const int minX = -25;
            const int maxX = 25;

            // массив точек, которые надо отрисовать
            PointF[] points2 = new PointF[500];
            for (int k = 0; k < points.Count - 1; k++)
            {
                if (k == 0) { pen1.Color = Color.Aqua; }
                if (k == 1) { pen1.Color = Color.Blue; }
                if (k == 2) { pen1.Color = Color.Green; }
                if (k == 3) { pen1.Color = Color.Orange; }
                if (k == 4) { pen1.Color = Color.IndianRed; }
                if (k == 5) { pen1.Color = Color.Olive; }
                if (k == 6) { pen1.Color = Color.Yellow; }
                if (k == 7) { pen1.Color = Color.Violet; }

                float x = -25;
                for (int i = 0; i < points2.Length; i++)
                {
                    x += (float)0.1;
                    
                    float y = (float)masA[k] + (float)Bi[k] * (x - (float)points[k].Item1) + (float)masCi[k] * (float)Math.Pow((x - (float)points[k].Item1), 2) + (float)Di[k] * (float)Math.Pow((x - (float)points[k].Item1), 3);
                    points2[i] = new PointF(x, y); //формируем точки для графика

                }

                for (int i = 0; i < points2.Length; i++) //подгоняем точки под наш масштаб
                {
                    points2[i].X = centerX + ScaleLines * points2[i].X;
                    points2[i].Y = centerY - ScaleLines * points2[i].Y;
                }
                if (kanonPolin[k] == true)
                {
                    g.DrawLines(pen1, points2); //рисуем график
                }
            }

            pen1.Color = Color.Red;
            Color brushcol = Color.Red;
            for (int i = 0; i < points.Count; i++) //рисуем полученные точки
            {                                    //коэффициент для подгонки точки по х                    коэффициент для подгонки точки по y                        
                g.DrawEllipse(pen1, (float)(centerX - 4.3 + ScaleLines * (float)points[i].Item1), (float)(centerY - 3 - ScaleLines * (float)points[i].Item2), 6, 6);
                g.FillEllipse(new SolidBrush(brushcol), (float)(centerX - 4.3 + ScaleLines * (float)points[i].Item1), (float)(centerY - 3 - ScaleLines * (float)points[i].Item2), 6, 6);

            }

            
            PointF[] points3;
            int NumPoints = 0;
            int t = 0;
            for (int i = 0; i < points.Count - 1; i++)    //рисуем сплайн
            {
                NumPoints = 0;
                for (float j = (float)points[i].Item1; j <= points[i + 1].Item1 + 0.05; j += (float)0.05) // j в роли х
                {
                    NumPoints++; //посчитали количество точек
                }
                points3 = new PointF[NumPoints];
                t = 0;
                for (float j = (float)points[i].Item1; j <= points[i + 1].Item1 + 0.05; j += (float)0.05) // j в роли х
                {
                    float y = (float)masA[i] + (float)Bi[i] * (j - (float)points[i].Item1) + (float)masCi[i] * (float)Math.Pow((j - (float)points[i].Item1), 2) + (float)Di[i] * (float)Math.Pow((j - (float)points[i].Item1), 3);

                    points3[t] = new PointF(j, y); //формируем точки для графика
                    t++;

                }

                for (int k = 0; k < NumPoints; k++) //подгоняем точки под наш масштаб
                {
                    Console.Write(" k = " + k + " : " + points3[k].X + " " + points3[k].Y + " | ");
                }
                Console.WriteLine();

                for (int k = 0; k < NumPoints; k++) //подгоняем точки под наш масштаб
                {
                    points3[k].X = centerX + ScaleLines * points3[k].X;
                    points3[k].Y = centerY - ScaleLines * points3[k].Y;
                }

                g.DrawLines(pen1, points3);
            }

            pictureBox1.Image = BitmapGr;

        }

        
    }
}


