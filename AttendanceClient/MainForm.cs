using System;
using System.Windows.Forms;

namespace AttendanceClient
{
    public partial class MainForm : Form
    {
        private Label lblWelcome;
        private Button btnAbsenDatang;
        private Button btnAbsenPulang;
        private Button btnRiwayat;

        public MainForm(string employeeName)
        {
            // Hapus InitializeComponent(); karena tidak ada Designer file
            this.Text = "Sistem Absensi Karyawan";
            this.Width = 400;
            this.Height = 300;

            // Label selamat datang
            lblWelcome = new Label()
            {
                Text = $"Selamat datang, {employeeName}",
                Left = 20,
                Top = 20,
                Width = 350,
                Font = new System.Drawing.Font("Segoe UI", 12, System.Drawing.FontStyle.Bold)
            };

            // Tombol Absen Datang
            btnAbsenDatang = new Button()
            {
                Text = "Absen Datang",
                Left = 20,
                Top = 70,
                Width = 150
            };
            btnAbsenDatang.Click += BtnAbsenDatang_Click;

            // Tombol Absen Pulang
            btnAbsenPulang = new Button()
            {
                Text = "Absen Pulang",
                Left = 200,
                Top = 70,
                Width = 150
            };
            btnAbsenPulang.Click += BtnAbsenPulang_Click;

            // Tombol Riwayat Absen
            btnRiwayat = new Button()
            {
                Text = "Lihat Riwayat Absen",
                Left = 20,
                Top = 130,
                Width = 330
            };
            btnRiwayat.Click += BtnRiwayat_Click;

            // Tambahkan semua kontrol ke form
            Controls.Add(lblWelcome);
            Controls.Add(btnAbsenDatang);
            Controls.Add(btnAbsenPulang);
            Controls.Add(btnRiwayat);
        }

        private void BtnAbsenDatang_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Absen datang berhasil!");
        }

        private void BtnAbsenPulang_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Absen pulang berhasil!");
        }

        private void BtnRiwayat_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Menampilkan riwayat absen...");
        }
    }
}
