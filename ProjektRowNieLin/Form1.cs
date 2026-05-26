using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BIBLIOTEKA_NR1.Algebra_Nieliniowa;
using System.Windows.Forms.DataVisualization.Charting;
using System.Globalization;

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

        // Stałe dla 2-członowego ramienia robota [m]
        private const double L1 = 3.0;
        private const double L2 = 2.0;
        private const double REACH = L1 + L2; // Maksymalny zasięg robota

        /// <summary>
        /// Deklaracja obiektu do rozwiązywania układu równań nieliniowych
        /// jako pole klasy formularza- delaruje pojektant
        /// </summary>
        Metoda_Newtona RNL;

        /// <summary>
        /// Wektory do zadawania warunków początkowych 
        /// i rozwiązania układu równań nieliniowych - delaruje pojektant
        /// </summary>
        double[] X0, X;

        /// <summary>
        /// Pole przechowujące docelową pozycję (X_cel, Y_cel) dla kinematyki odwrotnej
        /// </summary>
        private double X_cel = 3.5;
        private double Y_cel = 2.0;

        /// <summary>
        /// Flaga do wyboru trybu pracy: true = kinematyka odwrotna, false = przecięcie
        /// </summary>
        private bool modeKinematics = false;

        /// <summary>
        /// Metoda definiująca metodę, która dowolnemu wektorowi X
        /// przyporądkowuje wektor F do zapisu układu 
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
        /// Nowa metoda definiująca układ równań dla kinematyki odwrotnej 2-członowego ramienia robota
        /// Równania: 
        /// F[1] = L1*cos(theta1) + L2*cos(theta1+theta2) - X_cel
        /// F[2] = L1*sin(theta1) + L2*sin(theta1+theta2) - Y_cel
        /// </summary>
        /// <param name="X">Wektor kątów: X[1] = theta1, X[2] = theta2 (w radianach)</param>
        /// <returns>Wektor residuów F</returns>
        public double[] FunRobot(double[] X)
        {
            double[] F = new double[3];
            double theta1 = X[1];
            double theta2 = X[2];
            double sum_angle = theta1 + theta2;

            // Pozycja efektora (x, y) końca ramienia robota
            double x = L1 * Math.Cos(theta1) + L2 * Math.Cos(sum_angle);
            double y = L1 * Math.Sin(theta1) + L2 * Math.Sin(sum_angle);

            // Residua równań (zmienna minus cel)
            F[1] = x - X_cel;
            F[2] = y - Y_cel;

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

            // Obsługa kliknięcia myszą na chart1 do pobierania współrzędnych docelowych
            chart1.MouseClick += Chart1_MouseClick;

            // Inicjalizacja domyślnych wartości celu
            // Use culture-aware parsing to avoid FormatException on locales using ',' as decimal separator
            if (!double.TryParse(textBoxXcel.Text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out X_cel))
            {
                // fallback: accept '.' as decimal separator even on cultures that use ','
                string alt = textBoxXcel.Text.Replace('.', CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0]);
                if (!double.TryParse(alt, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out X_cel))
                {
                    // final fallback to invariant culture
                    double.TryParse(textBoxXcel.Text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out X_cel);
                }
            }

            if (!double.TryParse(textBoxYcel.Text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out Y_cel))
            {
                string alt = textBoxYcel.Text.Replace('.', CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0]);
                if (!double.TryParse(alt, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out Y_cel))
                {
                    double.TryParse(textBoxYcel.Text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out Y_cel);
                }
            }

            //Wywołanie metody do wykresu 
            button2_Click(sender, e);
        }//Form1_Load

        /// <summary>
        /// Obsługa zdarzenia kliknięcia na chart1 do pobierania współrzędnych docelowych
        /// </summary>
        private void Chart1_MouseClick(object sender, MouseEventArgs e)
        {
            if (modeKinematics)
            {
                var hitTestResult = chart1.HitTest(e.X, e.Y);
                if (hitTestResult.ChartElementType == ChartElementType.PlottingArea)
                {
                    var xAxis = chart1.ChartAreas[0].AxisX;
                    var yAxis = chart1.ChartAreas[0].AxisY;

                    // Konwersja współrzędnych pikseli na wartości na wykresie
                    double chartX = xAxis.PixelPositionToValue(e.X);
                    double chartY = yAxis.PixelPositionToValue(e.Y);

                    // Sprawdzenie, czy kliknięty punkt jest w zasięgu robota
                    double distance = Math.Sqrt(chartX * chartX + chartY * chartY);
                    if (distance <= REACH)
                    {
                        X_cel = chartX;
                        Y_cel = chartY;
                        
                        // Zaktualizuj TextBoxy
                        textBoxXcel.Text = X_cel.ToString("F2");
                        textBoxYcel.Text = Y_cel.ToString("F2");
                    }
                    else
                    {
                        MessageBox.Show($"Cel ({chartX:F2}, {chartY:F2}) znajduje się poza zasięgiem robota ({REACH:F2}m)!",
                                       "Cel poza zasięgiem", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

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
            
            // Odczyt warunków początkowych
            try
            {
                X0[1] = double.Parse(dataGridView1[1, 0].Value?.ToString() ?? "0.1");
                X0[2] = double.Parse(dataGridView1[1, 1].Value?.ToString() ?? "0.1");
            }
            catch
            {
                X0[1] = 0.1;
                X0[2] = 0.1;
            }

            //Czyszczenie wykresu punktowego dla formatki chart1
            chart1.Series[2].Points.Clear();
            //Odczyt parametru eps określających dokładność iteracji w metodzie Newtona
            double eps = double.Parse(textBox2.Text);

            if (modeKinematics)
            {
                // Odczyt współrzędnych docelowych z TextBoxów
                try
                {
                    X_cel = double.Parse(textBoxXcel.Text);
                    Y_cel = double.Parse(textBoxYcel.Text);
                }
                catch
                {
                    MessageBox.Show("Błąd: Niepoprawne wartości X_cel lub Y_cel!", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Tryb kinematyki odwrotnej
                // Sprawdzenie, czy cel znajduje się w zasięgu robota
                double distToGoal = Math.Sqrt(X_cel * X_cel + Y_cel * Y_cel);
                if (distToGoal > REACH)
                {
                    MessageBox.Show($"Cel ({X_cel:F2}, {Y_cel:F2}) znajduje się poza zasięgiem robota ({REACH:F2}m)!",
                                   "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    button2.Enabled = false;
                    return;
                }

                // Inicjalizacja obiektu RNL z nowym delegatem FunRobot
                RNL = new MetodaNewtona(FunRobot, 2, X0, eps, 1E-8, 1E-20, 100);
            }
            else
            {
                // Tryb przecięcia okręgu i elipsy
                RNL = new MetodaNewtona(FunW, 2, X0, eps, 1E-8, 1E-20, 100);
            }

            //Wywołanie metody rozwiązywania układu równań nieliniowych
            blad = RNL.Rozwiaz();
            if (blad == 0)
            {
                for (int i = 1; i <= 2; i++)
                {
                    X[i] = RNL.X[i];
                    //Zapis rozwiązania w pierwszej kolumnie tabeli
                    dataGridView1[0, i - 1].Value = X[i].ToString("F16");
                }
                //Zapis ilości iteracji Newtona
                textBox1.Text = "Nite= " + RNL.Nite.ToString();
                
                //Ilustracja graficzna procesu iteracyjnego na komponencie chart1
                for (int j = 0; j <= RNL.Nite; j++)
                    chart1.Series[2].Points.AddXY(RNL.XX[j][1], RNL.XX[j][2]);

                // Jeśli tryb przecięcia - pokazanie czterech punktów rozwiązań
                if (!modeKinematics)
                {
                    chart1.Series[2].Points.AddXY(5, 1);
                    chart1.Series[2].Points.AddXY(-5, 1);
                    chart1.Series[2].Points.AddXY(5, -1);
                    chart1.Series[2].Points.AddXY(-5, -1);
                }

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
            if (modeKinematics)
            {
                DrawRobotArm();
            }
            else
            {
                DrawIntersection();
            }
        }//button2_Click

        /// <summary>
        /// Metoda rysująca ramię robota na podstawie wyliczonych kątów
        /// </summary>
        private void DrawRobotArm()
        {
            chart1.Series[0].Points.Clear();
            chart1.Series[1].Points.Clear();
            chart1.Series[2].Points.Clear();

            chart1.ChartAreas[0].AxisX.Interval = 1;
            chart1.ChartAreas[0].AxisY.Interval = 1;
            chart1.ChartAreas[0].AxisX.Minimum = -6;
            chart1.ChartAreas[0].AxisX.Maximum = 6;
            chart1.ChartAreas[0].AxisY.Minimum = -6;
            chart1.ChartAreas[0].AxisY.Maximum = 6;

            // Rysowanie zasięgu robota jako koła
            double dt = 2 * Math.PI / 100;
            for (int i = 0; i <= 100; i++)
            {
                double t = i * dt;
                double xCircle = REACH * Math.Cos(t);
                double yCircle = REACH * Math.Sin(t);
                chart1.Series[0].Points.AddXY(xCircle, yCircle);
            }

            // Jeśli rozwiązanie zostało znaleione, rysuj ramię robota
            if (RNL != null && RNL.Nite >= 0)
            {
                double theta1 = RNL.X[1];
                double theta2 = RNL.X[2];

                // Koniec pierwszego segmentu (połączenie)
                double x1 = L1 * Math.Cos(theta1);
                double y1 = L1 * Math.Sin(theta1);

                // Koniec drugiego segmentu (efektor)
                double x2 = x1 + L2 * Math.Cos(theta1 + theta2);
                double y2 = y1 + L2 * Math.Sin(theta1 + theta2);

                // Rysowanie pierwszego segmentu ramienia
                chart1.Series[1].Points.AddXY(0, 0);
                chart1.Series[1].Points.AddXY(x1, y1);

                // Rysowanie drugiego segmentu ramienia
                chart1.Series[1].Points.AddXY(x1, y1);
                chart1.Series[1].Points.AddXY(x2, y2);

                // Rysowanie punktu docelowego jako marker
                int pointIndex = chart1.Series[2].Points.AddXY(X_cel, Y_cel);
                chart1.Series[2].Points[pointIndex].MarkerStyle = MarkerStyle.Diamond;
                chart1.Series[2].Points[pointIndex].MarkerSize = 10;
                chart1.Series[2].Points[pointIndex].MarkerColor = Color.Red;
            }
        }

        /// <summary>
        /// Metoda rysująca przecięcie okręgu i elipsy (tryb oryginalny)
        /// </summary>
        private void DrawIntersection()
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
                //chart1.Series[1] wybrano jako typ Spline wykresu ciągłego
                chart1.Series[1].Points.AddXY(x1, x2);
            }
        }

        /// <summary>
        /// Obsługa zdarzenia zmiany trybu pracy w ComboBox
        /// </summary>
        private void ComboBoxMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            modeKinematics = (comboBoxMode.SelectedIndex == 1);

            // Pokaż/ukryj UI elementy dla kinematyki
            labelXcel.Visible = modeKinematics;
            labelYcel.Visible = modeKinematics;
            textBoxXcel.Visible = modeKinematics;
            textBoxYcel.Visible = modeKinematics;

            // Odświeżenie wykresu
            button2_Click(sender, e);
        }

    }//Koniec public partial class Form1
}//namespace ProjektRowNieLin
