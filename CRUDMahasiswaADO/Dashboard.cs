using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace CRUDMahasiswaADO
{
    public partial class Dashboard : Form
    {
        DAL dbLogic = new DAL();
        bool isInitializing = true;
        DataTable dt;
        int button = 0;
        public Dashboard()
        {
            InitializeComponent();
        }

        private void Dashboard_Load(object sender, EventArgs e)
        {
            dtpTanggalMasuk.MinDate = new DateTime(2000, 1, 1);
            dtpTanggalMasuk.Format = DateTimePickerFormat.Custom;
            dtpTanggalMasuk.CustomFormat = "yyyy";
            dtpTanggalMasuk.ShowUpDown = true;
            dtpTanggalMasuk.MaxDate = DateTime.Now;

            cmbTipe.DropDownStyle = ComboBoxStyle.DropDownList;
            var items = new List<KeyValuePair<String, SeriesChartType>>
            {
                new KeyValuePair<string, SeriesChartType>("Kolom", SeriesChartType.Column),
                new KeyValuePair<string, SeriesChartType>("Pie", SeriesChartType.Pie),
            };

            isInitializing = true;
            cmbTipe.DataSource = items;
            cmbTipe.DisplayMember = "Key";
            cmbTipe.ValueMember = "Value";
            cmbTipe.SelectedIndex = 0;

            isInitializing = false;
        }
        public void LoadDataChart()
        {
            chartProdi.Series.Clear();
            chartProdi.ChartAreas.Clear();
            chartProdi.Legends.Clear();
            chartProdi.Titles.Clear();

            ChartArea ca = new ChartArea("Main Area");
            ca.AxisX.Title = "Program Studi";
            ca.AxisY.Title = "Jumlah Mahasiswa";
            ca.AxisX.LabelStyle.Angle = -45;
            chartProdi.ChartAreas.Add(ca);
            try
            {
                if (button == 1)
                {
                    dt = dbLogic.getDataChartByTahun(dtpTanggalMasuk.Value);
                }
                else
                {
                    dt = dbLogic.getAllDataChart();
                }

                SeriesChartType tipe = (SeriesChartType)cmbTipe.SelectedValue;
                if (tipe == SeriesChartType.Column)
                {
                    Series s = new Series("Mahasiswa");
                    s.ChartType = SeriesChartType.Column;
                    foreach (DataRow row in dt.Rows)
                    {
                        string prodi = row["namaprodi"].ToString();
                        int jumlah = Convert.ToInt32((long)row["JmlhMhs"]);
                        s.Points.AddXY(prodi, jumlah);
                    }
                    chartProdi.Series.Add(s);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal Load Data: " + ex.Message);
            }
            Title title = new Title("Jumlah Mahasiswa per Program Studi", Docking.Top, new Font("Arial", 14, FontStyle.Bold), Color.DarkBlue);
            chartProdi.Titles.Add(title);
            Legend legend = new Legend("Main Legend");
            legend.Docking = Docking.Bottom;
            chartProdi.Legends.Add(legend);
        }
    }
}
