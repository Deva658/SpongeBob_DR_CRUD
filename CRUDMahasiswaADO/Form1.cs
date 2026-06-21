using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace CRUDMahasiswaADO
{
    public partial class Form1 : Form
    {
        DAL dbLogic = new DAL();
        private BindingSource bindingSource = new BindingSource();
        private DataTable dtMahasiswa = new DataTable();
        private readonly string connectionString = "Data Source=DEVA\\DEPA15;Initial Catalog=DBAkademikADO;Integrated Security=True";

        public Form1()
        {
            InitializeComponent();
        }

        private void SimpanLog(string pesan)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"INSERT INTO LogError VALUES (GETDATE(), @pesan)";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@pesan", pesan);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Konfigurasi ComboBox dan Grid sesuai standar keamanan modul
            cmbJK.DataSource = new string[] { "L", "P" };

            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Integrasi BindingNavigator
            bindingNavigator1.BindingSource = bindingSource;

            LoadData();
        }

        private void HitungTotal()
        {
            try
            {
                int total = dbLogic.CountMhs();
                lblTotal.Text = "Total Mahasiswa: " + total;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal menghitung total: " + ex.Message);
            }
        }

        private void LoadData()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetMahasiswa", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        dtMahasiswa = new DataTable();
                        da.Fill(dtMahasiswa);

                        bindingSource.DataSource = dtMahasiswa;
                        dataGridView1.DataSource = bindingSource;

                        BindControls();
                    }
                }
            }

            HitungTotal(); // <- tambahkan ini
        }

        private void BindControls()
        {
            // Membersihkan binding lama sebelum membuat yang baru
            txtNIM.DataBindings.Clear();
            txtNama.DataBindings.Clear();
            cmbJK.DataBindings.Clear();
            dtpTanggalLahir.DataBindings.Clear();
            txtAlamat.DataBindings.Clear();
            txtKodeProdi.DataBindings.Clear();

            // Menghubungkan control ke BindingSource
            txtNIM.DataBindings.Add("Text", bindingSource, "NIM");
            txtNama.DataBindings.Add("Text", bindingSource, "Nama");
            cmbJK.DataBindings.Add("Text", bindingSource, "JenisKelamin");
            dtpTanggalLahir.DataBindings.Add("Value", bindingSource, "TanggalLahir");
            txtAlamat.DataBindings.Add("Text", bindingSource, "Alamat");
            txtKodeProdi.DataBindings.Add("Text", bindingSource, "KodeProdi");
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    MessageBox.Show("Koneksi berhasil");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Koneksi gagal: " + ex.Message);
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        private void btnInsert_Click(object sender, EventArgs e)
        {
            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            SqlTransaction trans = conn.BeginTransaction();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_InsertMahasiswa", conn, trans);
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@NIM", txtNIM.Text);
                    cmd.Parameters.AddWithValue("@Nama", txtNama.Text);
                    cmd.Parameters.AddWithValue("@JenisKelamin", cmbJK.Text);
                    cmd.Parameters.AddWithValue("@TanggalLahir", dtpTanggalLahir.Value.Date);
                    cmd.Parameters.AddWithValue("@Alamat", txtAlamat.Text);
                    cmd.Parameters.AddWithValue("@KodeProdi", txtKodeProdi.Text);
                    cmd.Parameters.AddWithValue("@TanggalDaftar", DateTime.Now);
                    cmd.ExecuteNonQuery();
                    SqlCommand cmdLog = new SqlCommand(@"INSERT INTO LogAktivitasSalah (aktivitas,waktu) VALUES (@aktivitas,GETDATE())", conn, trans);
                    cmdLog.Parameters.AddWithValue("@aktivitas", "INSERT MAHASISWA: " + txtNIM.Text);
                    cmdLog.ExecuteNonQuery();
                    trans.Commit();
                    MessageBox.Show("Data berhasil ditambahkan!");
                    LoadData();
                }
            }
            catch (SqlException ex)
            {
                trans.Rollback();
                SimpanLog("ROLLBACK INSERT: " + ex.Message);
                MessageBox.Show(ex.Message);
            }
            catch (Exception ex)
            {
                trans.Rollback();
                SimpanLog("GENERAL ERROR: " + ex.Message);
                MessageBox.Show("General Error: " + ex.Message);
            }
            finally
            { 
                conn.Close();
            }
        }

        private void btnResetData_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                        IF OBJECT_ID('dbo.Mahasiswa_Backup') IS NOT NULL
                        BEGIN
                            DELETE FROM dbo.Mahasiswa;
                            INSERT INTO dbo.Mahasiswa 
                            SELECT * FROM dbo.Mahasiswa_Backup;
                        END";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("Data berhasil direset");
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Reset gagal: " + ex.Message);
            }
        }

        private void btnTestInjection_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "UPDATE Mahasiswa SET Nama='" + txtNama.Text + "' WHERE NIM='" + txtNIM.Text + "'";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Update Berhasil");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ClearForm()
        {
            txtNIM.Clear();
            txtNama.Clear();
            cmbJK.SelectedIndex = -1;
            dtpTanggalLahir.Value = DateTime.Now;
            txtAlamat.Clear();
            txtKodeProdi.Clear();
            txtNIM.Focus();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_UpdateMahasiswa", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@NIM", txtNIM.Text);
                    cmd.Parameters.AddWithValue("@Nama", txtNama.Text);
                    cmd.Parameters.AddWithValue("@JenisKelamin", cmbJK.Text);
                    cmd.Parameters.AddWithValue("@TanggalLahir", dtpTanggalLahir.Value.Date);
                    cmd.Parameters.AddWithValue("@Alamat", txtAlamat.Text);
                    cmd.Parameters.AddWithValue("@KodeProdi", txtKodeProdi.Text);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                DialogResult resultConfire = MessageBox.Show(
                    "akin ingin menghapus data?", 
                    "Konfirmasi", 
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Question);
                if (resultConfire == DialogResult.Yes)
                {
                  
                    using (SqlCommand cmd = new SqlCommand("sp_DeleteMahasiswa", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@NIM", SqlDbType.Char, 11).Value = txtNIM.Text;
                        conn.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected < 0)
                            MessageBox.Show("Data berhasil dihapus");
                        else
                            MessageBox.Show("Data tidak ditemukan");

                        if (conn.State == ConnectionState.Open)
                        {
                            conn.Close();
                        }
                    }
                }
            }
        }

        private void btnRekapData_Click(object sender, EventArgs e)
        {
            Form2 fm2 = new Form2();
            fm2.Show();
            this.Hide();
        }
    }
}