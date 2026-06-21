using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace CRUDMahasiswaADO
{
    public partial class Form1 : Form
    {
        private BindingSource bindingSource = new BindingSource();
        private DataTable dtMahasiswa = new DataTable();
        private readonly SqlConnection conn;
        private readonly string connectionString =
            "Data Source=DEVA\\DEPA15;Initial Catalog=DBAkademikADO;Integrated Security=True";

        public Form1()
        {
            InitializeComponent();
            conn = new SqlConnection(connectionString);
        }

        private void ConnectDatabase()
        {
            try
            {
                if (conn.State == System.Data.ConnectionState.Closed)
                {
                    conn.Open();
                }

                MessageBox.Show("Koneksi Berhasil");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Koneksi Gagal: " + ex.Message);
            }

        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            ConnectDatabase();
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        private void btninsert_Click(object sender, EventArgs e)
        {
            SqlConnection conn =
                 new SqlConnection(connectionString);

            conn.Open();

            SqlTransaction trans =         
                conn.BeginTransaction();

            try
            {
                    SqlCommand cmd =
                        new SqlCommand(
                        "sp_InsertMahasiswa",
                        conn,
                        trans);
                    
                        cmd.CommandType = 
                            CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue(
                            "@NIM",
                            txtnim.Text);

                        cmd.Parameters.AddWithValue(
                            "@Nama",                            
                            txtnama.Text);

                        cmd.Parameters.AddWithValue(
                            "@JenisKelamin",
                            cmbJK.Text);

                        cmd.Parameters.AddWithValue(
                            "@TanggalLahir", 
                            dtpTanggalLahir.Value.Date);

                        cmd.Parameters.AddWithValue(
                            "@Alamat", 
                            txtAlamat.Text);

                        cmd.Parameters.AddWithValue(
                            "@KodeProdi", 
                            txtKodeProdi.Text);

                        cmd.Parameters.AddWithValue(
                            "@TanggalDaftar", 
                            DateTime.Now);

                        cmd.ExecuteNonQuery();
                            
                        SqlCommand cmdlog =
                            new SqlCommand(
                            @"INSERT INTO LogAktivitasSalah
                    (Aktivitas,waktu)
                    VALUES
                    (@aktivitas,GETDATE())",
                         conn, 
                         trans);

                        cmdlog.Parameters.AddWithValue(
                            "@aktivitas", 
                            "INSERT MAHASISWA : " + 
                            txtnim.Text);

                        cmdlog.ExecuteNonQuery();

                        trans.Commit();

                        MessageBox.Show
                            ("Data berhasil disimpan!");

                        LoadData();
            }
            catch (SqlException ex)
            {
                trans.Rollback();

                SimpanLog(
                    "ROLLBACK INSERT : " + 
                    ex.Message);

                MessageBox.Show(
                    ex.Message);
            }
            catch (Exception ex)
            {
                trans.Rollback();

                SimpanLog(
                    "GENERAL ERROR : " +
                    ex.Message)
                    ;
                MessageBox.Show(
                    ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                SqlConnection conn = new SqlConnection(connectionString);
                {
                    using (SqlCommand cmd = new SqlCommand("sp_UpdateMahasiswa", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@NIM", txtnim.Text);
                        cmd.Parameters.AddWithValue("@Nama", txtnama.Text);
                        cmd.Parameters.AddWithValue("@JenisKelamin", cmbJK.Text);
                        cmd.Parameters.AddWithValue("@TanggalLahir", dtpTanggalLahir.Value.Date);
                        cmd.Parameters.AddWithValue("@Alamat", txtAlamat.Text);
                        cmd.Parameters.AddWithValue("@KodeProdi", txtKodeProdi.Text);

                        conn.Open();
                        cmd.ExecuteNonQuery();

                        MessageBox.Show("Data berhasil diupdate!");
                    }
                } 
            }
            catch (Exception ex)
            {
                MessageBox.Show("Terjadi Kesalahan: " + ex.Message);
            }
        }
        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_DeleteMahasiswa", conn);
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@NIM", SqlDbType.Char, 11).Value = txtnim.Text;
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
            catch (Exception ex)
            {
                MessageBox.Show("Gagal menghapus data: " + ex.Message);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                txtnim.Text = row.Cells["NIM"].Value.ToString();
                txtnama.Text = row.Cells["Nama"].Value.ToString();
                cmbJK.Text = row.Cells["JenisKelamin"].Value.ToString();
                dtpTanggalLahir.Value = Convert.ToDateTime(row.Cells["TanggalLahir"].Value);
                txtAlamat.Text = row.Cells["Alamat"].Value.ToString();
                txtKodeProdi.Text = row.Cells["KodeProdi"].Value.ToString();
            }
        }

        private void Formm1_Load(object sender, EventArgs e)
        {

            cmbJK.DataSource = new string[] { "L", "P" };

            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            bindingNavigator1.BindingSource = bindingSource;

            LoadData();
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

            HitungTotal();
        }

        private void BindControls()
        {
            txtnim.DataBindings.Clear();
            txtnama.DataBindings.Clear();
            cmbJK.DataBindings.Clear();
            dtpTanggalLahir.DataBindings.Clear();
            txtAlamat.DataBindings.Clear();
            txtKodeProdi.DataBindings.Clear();

            txtnim.DataBindings.Add("Text", bindingSource, "NIM");
            txtnama.DataBindings.Add("Text", bindingSource, "Nama");
            cmbJK.DataBindings.Add("Text", bindingSource, "JenisKelamin");
            dtpTanggalLahir.DataBindings.Add("Value", bindingSource, "TanggalLahir");
            txtAlamat.DataBindings.Add("Text", bindingSource, "Alamat");
            txtKodeProdi.DataBindings.Add("Text", bindingSource, "KodeProdi");
        }

        private void btnResetData_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"IF OBJECT_ID('dbo.Mahasiswa_Backup') IS NOT NULL
                            BEGIN
                                DELETE FROM dbo.Mahasiswa;
                                INSERT INTO dbo.Mahasiswa SELECT * FROM dbo.Mahasiswa_Backup;
                            END";
                    using (SqlCommand cmd = new SqlCommand(query, conn)) { cmd.ExecuteNonQuery(); }
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
            {
                try
                {
                    using (SqlConnection conn =
                    new SqlConnection(connectionString))
                    {
                        string query =
                        "UPDATE Mahasiswa SET Nama='" +
                        txtnama.Text +
                        "' WHERE NIM='" +
                        txtnim.Text + "'";

                        SqlCommand cmd =
                        new SqlCommand(query, conn);

                        conn.Open();

                        cmd.ExecuteNonQuery();

                        MessageBox.Show("Update berhasil");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void HitungTotal()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_CountMahasiswa", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        SqlParameter OutputParam = new SqlParameter("@Total", SqlDbType.Int);
                        OutputParam.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(OutputParam);

                        conn.Open();
                        cmd.ExecuteNonQuery();

                        lblTotal.Text = "Total Mahasiswa: " + OutputParam.Value.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal menghitung total: " + ex.Message);
            }
        }


        private void SimpanLog(string pesan)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"INSERT INTO LogError
                         VALUES(GETDATE(), @pesan)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@pesan", pesan);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void btnRekap_Click(object sender, EventArgs e)
        {
            Form2 fm2 = new Form2();
            fm2.Show();
            this.Hide();
        }

        private void bindingNavigator1_RefreshItems(object sender, EventArgs e)
        {

        }
    }
}
