// FormDashboard.cs
using System;
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using SocketIOClient;
using Newtonsoft.Json;

namespace AttendanceClient
{
    public class FormDashboard : Form
    {
        private readonly string empNo;
        private readonly string empName;
        private SocketIOClient.SocketIO socket;

        // UI
        private Panel pnlMain;
        private Button btnMenuAbsen;
        private Button btnMenuRiwayat;
        private Button btnLogout;
        private Label lblStatus;

        // Kontrol page Absen
        private Button btnAbsenMasuk;
        private Button btnAbsenPulang;

        // Kontrol page Riwayat
        private ListView lvHistory;

        public FormDashboard(string empNo, string empName)
        {
            this.empNo = empNo;
            this.empName = empName;

            this.Text = "Dashboard Absensi (Dark Modern)";
            this.Size = new Size(1000, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(25, 25, 30);

            InitUI();

            this.Load += async (s, e) =>
            {
                await InitSocket();
                ShowAbsenPage(); // default halaman pertama
            };
        }

        private void InitUI()
        {
            // Sidebar
            Panel sidebar = new Panel()
            {
                Dock = DockStyle.Left,
                Width = 220,
                BackColor = Color.FromArgb(20, 20, 25)
            };

            Label lblUser = new Label()
            {
                Text = $"👤 {empName}",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Height = 60,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top
            };
            sidebar.Controls.Add(lblUser);

            btnMenuAbsen = CreateSidebarButton("🕘 Absen");
            btnMenuRiwayat = CreateSidebarButton("📄 Riwayat");
            btnLogout = CreateSidebarButton("🚪 Logout");

            btnMenuAbsen.Click += (s, e) => ShowAbsenPage();
            btnMenuRiwayat.Click += (s, e) => ShowRiwayatPage();
            btnLogout.Click += (s, e) =>
            {
                socket?.DisconnectAsync();
                new FormLogin().Show();
                this.Close();
            };

            sidebar.Controls.Add(btnLogout);
            sidebar.Controls.Add(btnMenuRiwayat);
            sidebar.Controls.Add(btnMenuAbsen);

            // Main panel
            pnlMain = new Panel()
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(30, 30, 36),
                Padding = new Padding(16)
            };

            lblStatus = new Label()
            {
                Text = "Koneksi: Menghubungkan...",
                Dock = DockStyle.Bottom,
                Height = 28,
                ForeColor = Color.LightGray
            };

            this.Controls.Add(pnlMain);
            this.Controls.Add(sidebar);
            this.Controls.Add(lblStatus);
        }

        private Button CreateSidebarButton(string text)
        {
            return new Button()
            {
                Text = text,
                Height = 50,
                Dock = DockStyle.Top,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(40, 40, 48),
                Font = new Font("Segoe UI", 10),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0),
                FlatAppearance = { BorderSize = 0 }
            };
        }

        // ================= SOCKET.IO =================
        private async Task InitSocket()
        {
            socket = new SocketIOClient.SocketIO("http://localhost:5000");

            socket.OnConnected += (s, e) => this.Invoke(() => lblStatus.Text = "Koneksi: Terhubung ✅");
            socket.OnDisconnected += (s, e) => this.Invoke(() => lblStatus.Text = "Koneksi: Terputus ❌");

            socket.On("absen_update", response =>
            {
                var data = response.GetValue<dynamic>();
                this.Invoke(() =>
                {
                    if ((string)data.employee_no != empNo) return;

                    string tipe = ((string)data.type).ToUpper();
                    ShowNotif($"Absen {tipe} berhasil", true);

                    // Tambahkan ke lvHistory jika riwayat page aktif
                    if (lvHistory != null && lvHistory.Visible)
                    {
                        var item = new ListViewItem(new[]
                        {
                            (string)data.date,
                            (string)data.time,
                            empNo,
                            tipe,
                            "Tercatat"
                        });
                        item.ForeColor = tipe == "MASUK" ? Color.LightGreen : Color.Orange;
                        lvHistory.Items.Insert(0, item);
                    }

                    // Update tombol absen
                    UpdateAbsenButtons();
                });
            });

            await socket.ConnectAsync();
        }

        // ===================== PAGES =====================
        private void ShowAbsenPage()
        {
            pnlMain.Controls.Clear();

            Label header = new Label()
            {
                Text = "Absen Hari Ini",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                Height = 50
            };

            btnAbsenMasuk = new Button()
            {
                Text = "🕘 Absen Masuk",
                Height = 50,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(60, 179, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            btnAbsenMasuk.FlatAppearance.BorderSize = 0;

            btnAbsenPulang = new Button()
            {
                Text = "🏁 Absen Pulang",
                Height = 50,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(255, 165, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            btnAbsenPulang.FlatAppearance.BorderSize = 0;

            btnAbsenMasuk.Click += async (s, e) => await KirimAbsen("masuk");
            btnAbsenPulang.Click += async (s, e) => await KirimAbsen("pulang");

            pnlMain.Controls.Add(btnAbsenPulang);
            pnlMain.Controls.Add(btnAbsenMasuk);
            pnlMain.Controls.Add(header);

            UpdateAbsenButtons();
        }

        private void ShowRiwayatPage()
{
    pnlMain.Controls.Clear();

    Label header = new Label()
    {
        Text = "Riwayat Absensi",
        Font = new Font("Segoe UI", 16, FontStyle.Bold),
        ForeColor = Color.White,
        Dock = DockStyle.Top,
        Height = 50
    };

    lvHistory = new ListView()
    {
        View = View.Details,
        Dock = DockStyle.Fill,
        BackColor = Color.FromArgb(40, 40, 46),
        ForeColor = Color.White,
        Font = new Font("Segoe UI", 10),
        FullRowSelect = true,
        GridLines = true, // tampilkan garis grid
        HideSelection = false
    };

    // Kolom tabel
    lvHistory.Columns.Add("Tanggal", 120, HorizontalAlignment.Center);
    lvHistory.Columns.Add("Jam", 80, HorizontalAlignment.Center);
    lvHistory.Columns.Add("No Karyawan", 120, HorizontalAlignment.Center);
    lvHistory.Columns.Add("Tipe", 100, HorizontalAlignment.Center);
    lvHistory.Columns.Add("Keterangan", 250, HorizontalAlignment.Left);

    pnlMain.Controls.Add(lvHistory);
    pnlMain.Controls.Add(header);

    _ = LoadHistoryAsync();
}

private async Task LoadHistoryAsync()
{
    if (lvHistory == null) return;

    try
    {
        using var client = new HttpClient();
        var url = $"http://localhost:5000/api/attendance/history/{empNo}";
        var response = await client.GetStringAsync(url);
        // Diasumsikan history API mengembalikan array object dengan field date, time_in, time_out
        var list = JsonConvert.DeserializeObject<dynamic[]>(response); 

        lvHistory.Items.Clear();

        foreach (var item in list)
        {
            // Ambil string waktu dan tanggal langsung dari object item
            string strDate = (string)item.date;
            string strTimeIn = (string)item.time_in;
            string strTimeOut = (string)item.time_out;

            // MASUK
            var masuk = new ListViewItem(new[]
            {
                strDate,        // Tanggal: yyyy-MM-dd
                strTimeIn.Substring(0, 5), // Jam: HH:mm (ambil 5 karakter pertama, contoh: "08:00:00" -> "08:00")
                empNo,
                "MASUK",
                "Tercatat"
            });
            masuk.ForeColor = Color.LightGreen;
            lvHistory.Items.Add(masuk);

            // PULANG
            if (!string.IsNullOrEmpty(strTimeOut)) // Cek apakah time_out ada/tidak null
            {
                var pulang = new ListViewItem(new[]
                {
                    strDate, 
                    strTimeOut.Substring(0, 5), // Jam: HH:mm
                    empNo,
                    "PULANG",
                    "Tercatat"
                });
                pulang.ForeColor = Color.Orange;
                lvHistory.Items.Add(pulang);
            }
        }

        UpdateAbsenButtons();
    }
    catch (Exception ex)
    {
        ShowNotif("Gagal load riwayat: " + ex.Message, false);
    }
}


        // ================= KIRIM ABSEN =================
        private async Task KirimAbsen(string tipe)
        {
            if (socket == null || !socket.Connected)
            {
                ShowNotif("Belum terhubung ke server realtime!", false);
                return;
            }

            // Cek apakah sudah absen hari ini
            bool sudahAbsen = false;
            if (lvHistory != null)
            {
                foreach (ListViewItem item in lvHistory.Items)
                {
                    if (item.SubItems[0].Text == DateTime.Now.ToString("yyyy-MM-dd") &&
                        item.SubItems[3].Text.ToUpper() == tipe.ToUpper())
                    {
                        sudahAbsen = true;
                        break;
                    }
                }
            }

            if (sudahAbsen)
            {
                ShowNotif($"Sudah absen {tipe} hari ini!", false);
                return;
            }

            await socket.EmitAsync("absen", new { employee_no = empNo, type = tipe });
        }

        private void UpdateAbsenButtons()
        {
            if (btnAbsenMasuk != null)
                btnAbsenMasuk.Enabled = !HasAbsen("MASUK");

            if (btnAbsenPulang != null)
                btnAbsenPulang.Enabled = !HasAbsen("PULANG");
        }

        private bool HasAbsen(string tipe)
        {
            if (lvHistory == null) return false;
            foreach (ListViewItem item in lvHistory.Items)
            {
                if (item.SubItems[0].Text == DateTime.Now.ToString("yyyy-MM-dd") &&
                    item.SubItems[3].Text.ToUpper() == tipe.ToUpper())
                    return true;
            }
            return false;
        }

        // ================= NOTIF =================
        private void ShowNotif(string message, bool sukses)
        {
            MessageBox.Show(message, sukses ? "Sukses" : "Gagal",
                MessageBoxButtons.OK,
                sukses ? MessageBoxIcon.Information : MessageBoxIcon.Error);
        }
    }
}
