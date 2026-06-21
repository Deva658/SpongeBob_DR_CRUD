using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRUDMahasiswaADO
{
    internal class Class1
    {
        public string Nama { get; set; }
        public string JenisKelamin { get; set; }
        public string Alamat { get; set; }
        public string NamaProdi { get; set; }
        public DateTime TanggalDaftar { get; set; }

        static string connectionString = "Data Source=DEVA\\DEPA15;Initial Catalog=DBAkademikADO;Integrated Security=True";

        SqlConnection conn = new SqlConnection(connectionString);
        SqlDataAdapter da;
        DataTable dtMahasiswa;

        string prodi { get; set; }
        DateTime tglMasuk { get; set; }
    }
}
