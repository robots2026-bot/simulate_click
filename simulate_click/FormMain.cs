using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace simulate_click
{
    public partial class FormMain : Form
    {
        private const string DefaultHost = "192.168.4.50";
        private const int DefaultPort = 22;
        private const string DefaultUser = "root";
        private const string DefaultPass = "lf123!";
        private const int DeviceWidth = 1280;
        private const int DeviceHeight = 800;
        private const string FramebufferPath = "/dev/fb0";
        private const string RemoteRawPath = "/tmp/fb0.raw";
        private const string TouchCommandPath = "/home/app/uinput_touchctl";

        private readonly System.Windows.Forms.Timer _refreshTimer;
        private bool _isRefreshing;
        private SshClient? _ssh;
        private ScpClient? _scp;

        public FormMain()
        {
            InitializeComponent();
            _refreshTimer = new System.Windows.Forms.Timer();
            _refreshTimer.Tick += async (_, _) => await RefreshFrameAsync();

            InitUiDefaults();
            EnsureConsole();
        }

        private void InitUiDefaults()
        {
            txtHost.Text = DefaultHost;
            txtPort.Text = DefaultPort.ToString();
            txtUser.Text = DefaultUser;
            txtPass.Text = DefaultPass;

            cboRefresh.Items.AddRange(new object[] { "0", "0.5", "1", "2", "5" });
            cboRefresh.SelectedIndex = 2;

            btnConnect.Click += (_, _) => EnsureConnected();
            btnRefresh.Click += async (_, _) => await RefreshFrameAsync();
            cboRefresh.SelectedIndexChanged += (_, _) => UpdateRefreshTimer();
            picScreen.MouseClick += async (_, e) => await HandleScreenClickAsync(e.Location);
            FormClosing += (_, _) => DisposeClients();

            UpdateRefreshTimer();
        }

        private void UpdateRefreshTimer()
        {
            if (!decimal.TryParse(cboRefresh.SelectedItem?.ToString(), out var seconds))
            {
                _refreshTimer.Stop();
                return;
            }

            if (seconds <= 0)
            {
                _refreshTimer.Stop();
                return;
            }

            _refreshTimer.Interval = (int)(seconds * 1000m);
            _refreshTimer.Start();
        }

        private void EnsureConnected()
        {
            try
            {
                Log("EnsureConnected: begin");
                if (_ssh is { IsConnected: true } && _scp is { IsConnected: true })
                {
                    SetStatus("Connected");
                    Log("EnsureConnected: already connected");
                    return;
                }

                DisposeClients();

                if (!TryGetConnectionInfo(out var host, out var port, out var user, out var pass))
                {
                    SetStatus("Invalid connection info.");
                    Log("EnsureConnected: invalid connection info");
                    return;
                }

                Log($"EnsureConnected: connecting to {host}:{port} user={user}");
                var connectionInfo = new PasswordConnectionInfo(host, port, user, pass);

                _ssh = new SshClient(connectionInfo);
                _scp = new ScpClient(connectionInfo);
                _ssh.HostKeyReceived += (_, e) => { e.CanTrust = true; };
                _scp.HostKeyReceived += (_, e) => { e.CanTrust = true; };

                _ssh.Connect();
                _scp.Connect();

                SetStatus("Connected");
                Log("EnsureConnected: connected");
            }
            catch (SshException ex)
            {
                SetStatus($"SSH error: {ex.Message}");
                Log($"EnsureConnected: SSH error {ex}");
                DisposeClients();
            }
            catch (Exception ex)
            {
                SetStatus($"Connect failed: {ex.Message}");
                Log($"EnsureConnected: failed {ex}");
                DisposeClients();
            }
        }

        private bool TryGetConnectionInfo(out string host, out int port, out string user, out string pass)
        {
            host = txtHost.Text.Trim();
            user = txtUser.Text.Trim();
            pass = txtPass.Text;
            port = DefaultPort;

            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(user))
            {
                return false;
            }

            if (!int.TryParse(txtPort.Text.Trim(), out port))
            {
                return false;
            }

            return true;
        }

        private async Task RefreshFrameAsync()
        {
            if (_isRefreshing)
            {
                Log("RefreshFrame: skipped (busy)");
                return;
            }

            _isRefreshing = true;
            try
            {
                EnsureConnected();
                if (_ssh is not { IsConnected: true } || _scp is not { IsConnected: true })
                {
                    Log("RefreshFrame: not connected");
                    return;
                }

                SetStatus("Capturing...");
                Log("RefreshFrame: capture start");
                await Task.Run(() =>
                {
                    var count = DeviceWidth * DeviceHeight;
                    var cmd = $"dd if={FramebufferPath} of={RemoteRawPath} bs=4 count={count}";
                    _ssh.RunCommand(cmd);
                });

                SetStatus("Downloading...");
                Log("RefreshFrame: download start");
                byte[] raw;
                using (var ms = new MemoryStream())
                {
                    await Task.Run(() => _scp.Download(RemoteRawPath, ms));
                    raw = ms.ToArray();
                }

                if (raw.Length < DeviceWidth * DeviceHeight * 4)
                {
                    SetStatus($"Frame size mismatch: {raw.Length} bytes.");
                    Log($"RefreshFrame: size mismatch {raw.Length}");
                    return;
                }

                var useRgba = chkRgba.Checked;
                var bmp = CreateBitmapFromRaw(raw, DeviceWidth, DeviceHeight, useRgba);

                var old = picScreen.Image;
                picScreen.Image = bmp;
                old?.Dispose();

                SetStatus($"Updated {DateTime.Now:HH:mm:ss}");
                Log("RefreshFrame: done");
            }
            catch (SshException ex)
            {
                SetStatus($"SSH error: {ex.Message}");
                Log($"RefreshFrame: SSH error {ex}");
            }
            catch (Exception ex)
            {
                SetStatus($"Refresh failed: {ex.Message}");
                Log($"RefreshFrame: failed {ex}");
            }
            finally
            {
                _isRefreshing = false;
            }
        }

        private async Task HandleScreenClickAsync(Point clickPoint)
        {
            if (picScreen.Image is null)
            {
                Log("HandleClick: no image");
                return;
            }

            var imgRect = GetImageDisplayRectangle(picScreen);
            if (!imgRect.Contains(clickPoint))
            {
                Log("HandleClick: outside image bounds");
                return;
            }

            var img = picScreen.Image;
            var xInImage = (int)((clickPoint.X - imgRect.X) * img.Width / (double)imgRect.Width);
            var yInImage = (int)((clickPoint.Y - imgRect.Y) * img.Height / (double)imgRect.Height);

            var xDevice = (int)(xInImage * (double)DeviceWidth / img.Width);
            var yDevice = (int)(yInImage * (double)DeviceHeight / img.Height);

            xDevice = Math.Clamp(xDevice, 0, DeviceWidth - 1);
            yDevice = Math.Clamp(yDevice, 0, DeviceHeight - 1);

            Log($"HandleClick: click=({clickPoint.X},{clickPoint.Y}) imgRect=({imgRect.X},{imgRect.Y},{imgRect.Width},{imgRect.Height}) img=({img.Width},{img.Height}) device=({xDevice},{yDevice})");
            ShowTapMarker(xDevice, yDevice);
            SetClickStatus(xDevice, yDevice);

            await SendTapAsync(xDevice, yDevice);
        }

        private async Task SendTapAsync(int x, int y)
        {
            try
            {
                EnsureConnected();
                if (_ssh is not { IsConnected: true })
                {
                    Log("SendTap: not connected");
                    return;
                }

                SetStatus($"Tap {x},{y}...");
                Log($"SendTap: {x},{y}");
                var cmd = await Task.Run(() => _ssh.RunCommand($"{TouchCommandPath} {x} {y}"));
                Log($"SendTap: exit={cmd.ExitStatus} stdout='{cmd.Result?.Trim()}' stderr='{cmd.Error?.Trim()}'");
                SetStatus($"Tap sent {x},{y}");
            }
            catch (SshException ex)
            {
                SetStatus($"Tap error: {ex.Message}");
                Log($"SendTap: SSH error {ex}");
            }
            catch (Exception ex)
            {
                SetStatus($"Tap failed: {ex.Message}");
                Log($"SendTap: failed {ex}");
            }
        }

        private static Bitmap CreateBitmapFromRaw(byte[] raw, int width, int height, bool isRgba)
        {
            byte[] buffer = raw;
            if (isRgba)
            {
                buffer = new byte[width * height * 4];
                for (int i = 0; i < buffer.Length; i += 4)
                {
                    buffer[i + 0] = raw[i + 2];
                    buffer[i + 1] = raw[i + 1];
                    buffer[i + 2] = raw[i + 0];
                    buffer[i + 3] = raw[i + 3];
                }
            }

            var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            var data = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, bmp.PixelFormat);
            try
            {
                System.Runtime.InteropServices.Marshal.Copy(buffer, 0, data.Scan0, width * height * 4);
            }
            finally
            {
                bmp.UnlockBits(data);
            }

            return bmp;
        }

        private static Rectangle GetImageDisplayRectangle(PictureBox pictureBox)
        {
            if (pictureBox.Image == null || pictureBox.SizeMode != PictureBoxSizeMode.Zoom)
            {
                return pictureBox.ClientRectangle;
            }

            var img = pictureBox.Image;
            var imgRatio = img.Width / (double)img.Height;
            var boxRatio = pictureBox.ClientSize.Width / (double)pictureBox.ClientSize.Height;

            if (boxRatio > imgRatio)
            {
                var height = pictureBox.ClientSize.Height;
                var width = (int)(height * imgRatio);
                var x = (pictureBox.ClientSize.Width - width) / 2;
                return new Rectangle(x, 0, width, height);
            }

            var w = pictureBox.ClientSize.Width;
            var h = (int)(w / imgRatio);
            var y = (pictureBox.ClientSize.Height - h) / 2;
            return new Rectangle(0, y, w, h);
        }

        private void DisposeClients()
        {
            try
            {
                _scp?.Disconnect();
                _scp?.Dispose();
            }
            catch
            {
                // Ignore dispose errors.
            }

            try
            {
                _ssh?.Disconnect();
                _ssh?.Dispose();
            }
            catch
            {
                // Ignore dispose errors.
            }

            _scp = null;
            _ssh = null;
        }

        private void SetStatus(string message)
        {
            statusLabel.Text = message;
            Log($"Status: {message}");
        }

        private void SetClickStatus(int x, int y)
        {
            statusLabelClick.Text = $"Click: {x},{y}";
        }

        private void ShowTapMarker(int xDevice, int yDevice)
        {
            if (picScreen.Image is not Bitmap source)
            {
                return;
            }

            try
            {
                var marked = new Bitmap(source);
                using var g = Graphics.FromImage(marked);
                using var pen = new Pen(Color.Red, 3);
                using var brush = new SolidBrush(Color.FromArgb(120, Color.Red));
                const int radius = 10;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.FillEllipse(brush, xDevice - radius, yDevice - radius, radius * 2, radius * 2);
                g.DrawEllipse(pen, xDevice - radius, yDevice - radius, radius * 2, radius * 2);

                var old = picScreen.Image;
                picScreen.Image = marked;
                old?.Dispose();
            }
            catch (Exception ex)
            {
                Log($"ShowTapMarker failed: {ex.Message}");
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AllocConsole();

        private static void EnsureConsole()
        {
            try
            {
                AllocConsole();
                Log("Console allocated");
            }
            catch
            {
                // Ignore console allocation failures.
            }
        }

        private static void Log(string message)
        {
            var line = $"{DateTime.Now:HH:mm:ss.fff} {message}";
            Console.WriteLine(line);
            System.Diagnostics.Debug.WriteLine(line);
        }
    }
}
