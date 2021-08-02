using PeanutButter.INI;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace game_of_life
{
    public partial class Form1 : Form
    {
        public Form1()
        {

            InitializeComponent();
        }

        // начальные значения
        private IINIFile _loadedConfig;
        private Graphics graphics;
        private int currentGeneration = 0;
        private int rows = 100;
        private int columns = 100;
        private bool[,] field;
        int size_rect;
        private string path = (Directory.GetCurrentDirectory() + "\\Settings\\");
        Brush[] colors = { Brushes.Red, Brushes.Blue, Brushes.Green }; // цвета клеток

        //основной таймер генерации поколения
        private void timer1_Tick(object sender, EventArgs e)
        {
            nextGeneration();
        }

        //начальный генератор клеток
        private void random_generation()
        {
            field = new bool[columns, rows];
            Random random = new Random();
            for (int x = 0; x < columns; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    field[x, y] = random.Next((int)nudDensity.Value) == 0;
                }
            }
        }
        
        private void btnStart_Click(object sender, EventArgs e)
        {
            
            // проверка на уже запущенную игру
            if (timer1.Enabled)
                return;

            currentGeneration = 0;
            rtbLog.Text = "";
            if (cbRandom.Checked)
                random_generation();
            timer1.Start();
        }

        //функция генерации клеток
        private void nextGeneration()
        {
            graphics.Clear(Color.White);

            var newField = new bool[columns, rows];

            for (int x = 0; x < columns; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    var neighboursCount = countNeighbours(x, y);
                    var hasLife = field[x, y];

                    if (!hasLife && neighboursCount == 3) // сохранение жизни клетки/создание новой
                    {
                        newField[x, y] = true;
                    }
                    else if (hasLife && (neighboursCount < 2 || neighboursCount > 3)) // переполнение клеток
                    {
                        newField[x, y] = false;
                    }
                    else
                    {
                        newField[x, y] = field[x, y];
                    }

                    if (hasLife)
                    {
                        // отрисовка прямоугольника
                        graphics.FillRectangle(get_color(), x * size_rect, y * size_rect, size_rect, size_rect);
                    }
                }
            }
            // вывод данных в log
            rtbLog.AppendText(String.Format("Текущее поколение:{0} | Живых клеток:{1} | Цвет клеток:{2}\n", currentGeneration,get_number_of_living(rows,columns),cbColor.SelectedItem));
            rtbLog.ScrollToCaret();
            currentGeneration++;
            this.Text = "Поколение:" + currentGeneration;
            field = newField;
            pictureBox1.Refresh();
        }

        // функция получения количества живых клеток на поле
        private int get_number_of_living(int rows, int columns)
        {
            int count = 0;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    if (field[j, i])
                        count++;
            }
                }
            return count;
        }
        
        // функция получения цвета
        private Brush get_color()
        {
            return colors[cbColor.SelectedIndex];
        }

        //функция подсчета соседних клеток
        private int countNeighbours(int x, int y)//
        {
            int count = 0;

            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    int col = (x + i + columns) % columns;
                    int row = (y + j + rows) % rows;

                    var isSelfChecking = col == x && row == y;
                    var hasLife = field[col, row];                  
                    if (hasLife && !isSelfChecking)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        private void stopGame()
        {
            if (!timer1.Enabled)
                return;
            timer1.Stop();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            stopGame();
        }

        //создание(удаление) клетки на лкм/пкм
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var x = e.Location.X / size_rect;
                var y = e.Location.Y / size_rect;
                if (validateMousePosition(x, y))
                {
                    field[x, y] = true;
                    graphics.FillRectangle(get_color(), x * size_rect, y * size_rect, size_rect, size_rect);
                    pictureBox1.Refresh();
                }
            }

            if (e.Button == MouseButtons.Right)
            {
                var x = e.Location.X / size_rect;
                var y = e.Location.Y / size_rect;
                if (validateMousePosition(x, y))
                {
                    field[x, y] = false;
                    graphics.FillRectangle(Brushes.Black, x * size_rect, y * size_rect, size_rect, size_rect);
                    pictureBox1.Refresh();
                }
            }
        }


        //функция проверки положения мыши относительно рабочего окна игры
        private bool validateMousePosition(int x, int y)
        {
            return x >= 0 && y >= 0 && y < columns && x < rows;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cbColor.SelectedIndex = 0;
            size_rect = pictureBox1.Height / rows;
            pictureBox1.Image = new Bitmap(splitContainer1.Panel2.Width, splitContainer1.Panel2.Height);
            graphics = Graphics.FromImage(pictureBox1.Image);
            graphics.Clear(Color.White);
            field = new bool[rows, columns];
        }

        private void btnRead_Click(object sender, EventArgs e)
        {
            try
            {
                clear_field();
                _loadedConfig = new INIFile(Directory.GetCurrentDirectory() + "\\Settings\\settings.ini");
                nudDensity.Value = _loadedConfig.HasSetting("UserSettings", "density") ? int.Parse(_loadedConfig["UserSettings"]["density"]) : 2;
                cbColor.SelectedIndex = _loadedConfig.HasSetting("UserSettings", "color_index") ? int.Parse(_loadedConfig["UserSettings"]["color_index"]) : -1;
                read_field("field.txt");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            _loadedConfig = new INIFile(path + "settings.ini");
            _loadedConfig["UserSettings"]["density"] = nudDensity.Value.ToString();
            _loadedConfig["UserSettings"]["color_index"] = cbColor.SelectedIndex.ToString();

            _loadedConfig.Persist();
            save_field();
        }

        private void save_field()
        {
            using (StreamWriter sw = new StreamWriter(path + "field.txt", false, System.Text.Encoding.Default))
            {
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < columns; j++)
                    {
                        sw.Write(field[i,j] ? 1 : 0);
                    }
                    sw.WriteLine();
                }
            }
        }

        private void read_field(string name_field)
        {
            using (StreamReader sr = new StreamReader(path + name_field, System.Text.Encoding.Default))
            {
                string line;
                for (int i = 0; i < rows; i++)
                {
                    line = sr.ReadLine(); // считывание строки
                    for (int j = 0; j < columns; j++)
                    {
                        try
                        {
                            field[i, j] = line[j] == '1' ? true : false; // 
                            if (field[i, j])
                                graphics.FillRectangle(get_color(), i * size_rect, j * size_rect, size_rect, size_rect);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                            return;
                        }
                    }
                }
                pictureBox1.Refresh();
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            clear_field();
        }

        private void clear_field()
        {
            field = new bool[rows, columns];
            graphics.Clear(Color.White);
            pictureBox1.Refresh();
        }

        // масштабирование поля
        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            size_rect = pictureBox1.Height / rows;
            pictureBox1.Image = new Bitmap(splitContainer1.Panel2.Width, splitContainer1.Panel2.Height);
            graphics = Graphics.FromImage(pictureBox1.Image);
            graphics.Clear(Color.White);
        }

        // считывание поля из файла
        private void btnReadField_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = path;
                openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    clear_field();
                    string name_field = Path.GetFileName(openFileDialog.FileName);
                    read_field(name_field);
                }
            }
        }        
    }
}