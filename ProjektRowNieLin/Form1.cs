using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BIBLIOTEKA_NR1.Algebra_Nieliniowa;

namespace ProjektRowNieLin
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// Konstruktor klasy formularza generowany automatycznie
        /// </summary>
        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Deklaracja obiektu do rozwiązywania układu równań nieliniowych
        /// jako pole klasy formularza- delaruje pojektant
        /// </summary>
        Metoda_Newtona RNL;

        /// <summary>
        /// Wektory do zadawania warunków początkowych 
        /// i rozwiązania układu równań nieliniowych - delaruje pojektant
        /// </summary>
        double[] X0,X;

        /// <summary>
        /// Metoda definiująca metodę, która dowolnemu wektorowi X
        /// przyporządkowuje wektor F do zapisu układu 
        /// dwóch równań nieliniowych - delaruje pojektant
        /// </summary>
        /// <param name="X">Argument funkcji jako wekto X</param>
        /// <returns>Zwraca wektor</returns>
        public double[] FunW(double[] X)
        {
            double x1k, x2k;
            double[] F = new double[3];
            x1k = X[1] * X[1];
            x2k = X[2] * X[2];
            //Na płaszczyźnie x10x2 przedstawia okrąg
            F[1] = x1k + x2k - 26.0; 
            //Przedstawia elipsę przecinającą okrąg w czterech punktach
            F[2] = 3 * x1k + 25 * x2k - 100.0; 
            return F;
        }

        /// <summary>
        /// Metoda ustalająca wstępne parametry projektu 
        /// wywoływana automatycznie po uruchomieniu programu
        /// Nagłówek metody generowany automatycznie-nie kopiować
        /// Instrukcje w metodzie pisze projektant
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            X0 = new double[3];
            X = new double[3];
            //Wstępnie ustalone warunki początkowe iteracji
            X0[1] = -6.0; X0[2] = 6.0;
            //Ustalenie wymiarów tabeli
            dataGridView1.RowCount = 2;
            dataGridView1.ColumnCount = 2;
            dataGridView1.Rows[0].HeaderCell.Value = "x1 =";
            dataGridView1.Rows[1].HeaderCell.Value = "x2 =";
            //Opsis pierwszej i drugiej kolumny tabeli
            dataGridView1.Columns[0].HeaderCell.Value = "Rozwiązanie";
            dataGridView1.Columns[1].HeaderCell.Value = "Warunek początkowy";
            //Lokalizacja wstępnych warunków początkowych
            //w drugiej kolumnie komponentu dataGridView1
            dataGridView1[1, 0].Value = X0[1].ToString();
            dataGridView1[1, 1].Value = X0[2].ToString();
            //Wywołanie metody do wykresu 
            button2_Click(sender, e);
        }//Form1_Load

        /// <summary>
        /// Metoda podpięta pod zdarzenie Click przycisku button1
        /// realizująca rozwiązywanie układu równaań nieliniowych
        /// Nagłówek metody generowany automatycznie-nie kopiować
        /// Instrukcje w metodzie pisze projektant
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            int blad = 0;
            //Odczyt dowolnie ustalonych warunków początkowych iteracji 
            //z drugiej kolumny tabeli dataGridView1 
            X0[1] = double.Parse(dataGridView1[1, 0].Value.ToString());
            X0[2] = double.Parse(dataGridView1[1, 1].Value.ToString());
            //Czyszczenie wykresu punktowego dla formatki chart1
            chart1.Series[2].Points.Clear();
            //Odczyt parametru eps określających dokładność iteracji w metodzie Newtona
            double eps = double.Parse(textBox2.Text);
            //Inicjalizacja obiektu RNL typu RowNieLinMetodaNewtona do 
            //rozwiązywania układu równań nieliniowych
            RNL = new MetodaNewtona(FunW, 2, X0, eps, 1E-8, 1E-20, 100);
            //Wywołanie metody rozwiązywania układu równań nieliniowych  przekazanych 
            //do obiektu za pomocą parametru FunW wskazującego układ równań nieliniowych
            //zdefiniowanych jako metoda w klasie formularza od 23 wiersz projektu.
            blad = RNL.Rozwiaz();
            if (blad == 0)
            {
                for (int i = 1; i <= 2; i++)
                {
                    X[i] = RNL.X[i];
                    //Zapis rozwiązania w pierwszej kolumnie(zerowy indeks) tabelki dataGridView1 
                    dataGridView1[0, i - 1].Value = X[i].ToString("F16");
                }
                //Zapis ilości iteracji Newtona, ktOre zwraca obiekt RNL 
                textBox1.Text ="Nite= "+ RNL.Nite.ToString();
                //Ilustracja graficzna procesu iteracyjnego w postaci punktów na komponencie chart1
                //Pole XX obiektu RNL typu RowNieLinMetodaNewtona zawiera wyniki obliczeń 
                //poszczególnych iteracji procesu Newtona
                //chart1.Series[2] jest utalony na typ Point wykresu punktowego
                for (int j = 0; j <= RNL.Nite; j++)
                    chart1.Series[2].Points.AddXY(RNL.XX[j][1], RNL.XX[j][2]);
                //Pokazanie punktów możliwych rozwiązań zależnych 
                //od wyboru punktu początkowego iteracji na komponenciie chart1
                chart1.Series[2].Points.AddXY(5, 1);
                chart1.Series[2].Points.AddXY(-5, 1);
                chart1.Series[2].Points.AddXY(5, -1);
                chart1.Series[2].Points.AddXY(-5, -1);
                button2.Enabled = true;
            }
            else RNL.PiszKomunikat(blad);
        }//button1_Click

        /// <summary>
        /// Metoda do ilustracji położenia możliwych punktów rozwiązania
        /// w zależności od wybranego punktu początkowego iteracji 
        /// Nagłówek metody generowany automatycznie-nie kopiować
        /// Instrukcje w metodzie pisze projektant
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            double t, dt, x1, x2, r, a, b;
            int N = 100;
            r = Math.Sqrt(26.0);
            //Parametry elipsy a, b
            a = 10.0 / Math.Sqrt(3.0);
            b = 2.0;
            //Krok obliczeń dla wykresu ciągłego
            dt = 2 * Math.PI / N;

            chart1.Series[0].Points.Clear();
            chart1.Series[1].Points.Clear();
            chart1.ChartAreas[0].AxisX.Interval = 1;
            chart1.ChartAreas[0].AxisY.Interval = 1;
            //Ustalenie na komponencie chart1 przedziałów zmienności na osi X oraz Y 
            chart1.ChartAreas[0].AxisX.Minimum = -6;
            chart1.ChartAreas[0].AxisX.Maximum = 6;
            chart1.ChartAreas[0].AxisY.Minimum = -6;
            chart1.ChartAreas[0].AxisY.Maximum = 6;
            for (int i = 0; i <= N; i++)
            {
                t = i * dt;//Parametr do zapisu parametrycznego okręgu i elipsy
                //Zapis parametryczny elipsy
                x1 = a * Math.Cos(t);
                x2 = b * Math.Sin(t);
                //chart1.Series[0] wybrano jako typ Spline wykresu ciągłego 
                chart1.Series[0].Points.AddXY(x1, x2);
                //Zapis parametryczny okręgu
                x1 = r * Math.Cos(t);
                x2 = r * Math.Sin(t);
                //chart1.Series[0] wybrano jako typ Spline wykresu ciągłego
                chart1.Series[1].Points.AddXY(x1, x2);
            }
        }//button2_Click

    }//Koniec public partial class Form1
}//namespace ProjektRowNieLin
