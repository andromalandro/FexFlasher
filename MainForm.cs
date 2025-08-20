using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Windows.Forms;

namespace FexFlasher
{
    public class MainForm : Form
    {
        private Button flashButton;
        private TextBox logBox;
        private OpenFileDialog openFileDialog;
        private ComboBox driveSelector;
        private Button refreshButton;

        public MainForm()
        {
            Text = "Fex Flasher";
            Width = 650;
            Height = 450;

            driveSelector = new ComboBox
            {
                Dock = DockStyle.Top,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Height = 30
            };

            refreshButton = new Button
            {
                Text = "Refresh Drives",
                Dock = DockStyle.Top,
                Height = 30
            };
            refreshButton.Click += (s, e) => PopulateDrives();

            flashButton = new Button
            {
                Text = "Select .fex and Flash",
                Dock = DockStyle.Top,
                Height = 40
            };
            flashButton.Click += FlashButton_Click;

            logBox = new TextBox
            {
                Multiline = true,
                Dock = DockStyle.Fill,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true
            };

            Controls.Add(logBox);
            Controls.Add(flashButton);
            Controls.Add(refreshButton);
            Controls.Add(driveSelector);

            openFileDialog = new OpenFileDialog
            {
                Filter = "FEX files (*.fex)|*.fex|All files (*.*)|*.*"
            };

            PopulateDrives();
        }

        private void PopulateDrives()
        {
            driveSelector.Items.Clear();

            foreach (var drive in DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Removable && d.IsReady))
            {
                string display = $"{drive.Name} ({FormatSize(drive.TotalSize)})";
                driveSelector.Items.Add(display);
            }

            if (driveSelector.Items.Count > 0)
                driveSelector.SelectedIndex = 0;
        }

        private void FlashButton_Click(object? sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (driveSelector.SelectedItem == null)
                {
                    Log("No drive selected.");
                    return;
                }

                string selected = driveSelector.SelectedItem.ToString()!;
                string driveLetter = selected.Substring(0, 2); // e.g. "F:"

                // Confirm before writing
                var confirm = MessageBox.Show(
                    $"⚠️ WARNING ⚠️\n\nYou are about to overwrite raw data on:\n\n" +
                    $"{selected}\n\nThis will destroy existing data on the SD card.\n\nProceed?",
                    "Confirm Flash",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (confirm != DialogResult.Yes)
                {
                    Log("Flash cancelled by user.");
                    return;
                }

                string? physicalDrive = GetPhysicalDriveForLetter(driveLetter);
                if (physicalDrive == null)
                {
                    Log("Failed to resolve physical drive.");
                    return;
                }

                Log($"Flashing {openFileDialog.FileName} to {physicalDrive} ...");

                try
                {
                    // Equivalent to: dd if=file.fex of=drive bs=1k seek=16400
                    using FileStream input = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read);
                    using FileStream output = new FileStream(physicalDrive, FileMode.Open, FileAccess.ReadWrite);

                    const int blockSize = 1024;
                    long seek = 16400L * blockSize;

                    output.Seek(seek, SeekOrigin.Begin);

                    byte[] buffer = new byte[blockSize];
                    int bytesRead;
                    long totalWritten = 0;
                    long fileSize = input.Length;
                    int lastPercent = -1;

                    while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        output.Write(buffer, 0, bytesRead);
                        totalWritten += bytesRead;

                        int percent = (int)((totalWritten * 100) / fileSize);
                        if (percent != lastPercent && percent % 5 == 0) // update every 5%
                        {
                            Log($"Progress: {percent}%");
                            lastPercent = percent;
                        }
                    }

                    Log($"✅ Done! Wrote {totalWritten} bytes.");
                }
                catch (Exception ex)
                {
                    Log("❌ Error: " + ex.Message);
                }
            }
        }

        private string? GetPhysicalDriveForLetter(string letter)
        {
            string driveLetter = letter.TrimEnd('\\');

            using var searcher = new ManagementObjectSearcher(
                $"ASSOCIATORS OF {{Win32_LogicalDisk.DeviceID='{driveLetter}'}} WHERE AssocClass=Win32_LogicalDiskToPartition");

            foreach (ManagementObject partition in searcher.Get())
            {
                using var diskSearcher = new ManagementObjectSearcher(
                    $"ASSOCIATORS OF {{Win32_DiskPartition.DeviceID='{partition["DeviceID"]}'}} WHERE AssocClass=Win32_DiskDriveToDiskPartition");

                foreach (ManagementObject disk in diskSearcher.Get())
                {
                    return disk["DeviceID"]?.ToString(); // e.g. "\\\\.\\PHYSICALDRIVE2"
                }
            }
            return null;
        }

        private void Log(string msg)
        {
            logBox.AppendText(msg + Environment.NewLine);
        }

        private string FormatSize(long size)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = size;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}
