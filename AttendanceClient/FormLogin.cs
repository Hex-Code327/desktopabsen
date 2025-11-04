// FormLogin.cs
using System;
using System.Drawing;
using System.Net.Http;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace AttendanceClient
{
    public class FormLogin : Form
    {
        TextBox txtNo, txtPass;
        Button btnLogin;
        Label lblInfo;

        public FormLogin()
        {
            this.Text = "Login - Absensi (Dark Mode)";
            this.Size = new Size(420, 320);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(30, 30, 36);

            Label title = new Label()
            {
                Text = "SISTEM ABSENSI",
                ForeColor = Color.FromArgb(220, 220, 220),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 70
            };
            Controls.Add(title);

            Label l1 = new Label() { Text = "No Karyawan", ForeColor = Color.FromArgb(200,200,200), Left = 40, Top = 90, Width = 100 };
            txtNo = new TextBox() { Left = 150, Top = 85, Width = 220, Font = new Font("Segoe UI", 10), BackColor = Color.FromArgb(45,45,50), ForeColor = Color.White };

            Label l2 = new Label() { Text = "Password", ForeColor = Color.FromArgb(200,200,200), Left = 40, Top = 130, Width = 100 };
            txtPass = new TextBox() { Left = 150, Top = 125, Width = 220, Font = new Font("Segoe UI", 10), BackColor = Color.FromArgb(45,45,50), ForeColor = Color.White, PasswordChar = '●' };

            btnLogin = new Button()
            {
                Text = "Masuk",
                Left = 150,
                Top = 170,
                Width = 220,
                Height = 40,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;

            lblInfo = new Label() { Left = 40, Top = 225, Width = 330, ForeColor = Color.OrangeRed };

            Controls.Add(l1); Controls.Add(txtNo);
            Controls.Add(l2); Controls.Add(txtPass);
            Controls.Add(btnLogin); Controls.Add(lblInfo);
        }

        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            lblInfo.Text = "";
            var no = txtNo.Text.Trim();
            var pass = txtPass.Text.Trim();

            if (no == "" || pass == "")
            {
                lblInfo.Text = "Isi No Karyawan dan Password.";
                return;
            }

            try
            {
                using (var http = new HttpClient())
                {
                    var json = $"{{\"employee_no\":\"{no}\",\"password\":\"{pass}\"}}";
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var res = await http.PostAsync("http://localhost:5000/api/auth/login", content);
                    var body = await res.Content.ReadAsStringAsync();

                    if (res.IsSuccessStatusCode)
                    {
                        var obj = JObject.Parse(body);
                        string name = (string)obj["user"]["name"];

                        var dashboard = new FormDashboard(no, name);
                        dashboard.Show();
                        this.Hide();
                    }
                    else
                    {
                        lblInfo.Text = "Login gagal: periksa kembali.";
                    }
                }
            }
            catch (Exception ex)
            {
                lblInfo.Text = "Error: " + ex.Message;
            }
        }
    }
}
