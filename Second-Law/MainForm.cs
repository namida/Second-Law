﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace SecondLaw {
	public partial class MainForm : Form {
		private readonly Hardware _hardware = new Hardware();
		private readonly SupportedDevices _supportedDevices = new SupportedDevices();
		
		public MainForm() {
			InitializeComponent();

			_hardware.DeviceInterfaceChanged += Hardware_DeviceInterfaceChanged;
			_hardware.RegisterNotifications(this);

			tslStatus.Text = "Waiting for device...";
			ScanForUsbDevices();
		}

		private void Hardware_DeviceInterfaceChanged(object sender, DeviceInterfaceChangedArgs e) {
			ScanForUsbDevices();
		}

		protected override void WndProc(ref Message m) {
			base.WndProc(ref m);
			_hardware.WndProc(ref m);
		}

		private void mniFileScanForDevices_Click(object sender, System.EventArgs e) {
			ScanForUsbDevices();
		}

		private void ScanForUsbDevices() {
			tslStatus.Text = "Scanning...";
			pnlScanning.Visible = true;
			pnlDevice.Visible = false;
			prgScanning.Visible = true;

			SupportedDevice supportedDevice = null;
			var devices = _hardware.EnumerateUsbDevices();
			foreach (var usbDevice in devices) {
				// Load the device icons
				string imageKey = usbDevice.PhysicalDeviceObjectName;
				LoadImage(imageKey, usbDevice.LargeIcon, imlLargeIcons);
				LoadImage(imageKey, usbDevice.SmallIcon, imlSmallIcons);

				supportedDevice = _supportedDevices.GetDevice(usbDevice.VendorId, usbDevice.ProductId, usbDevice.Revision, usbDevice.InterfaceId);
				if (supportedDevice != null) {
					break;
				}
			}

			prgScanning.Visible = false;
			if (supportedDevice != null) {
				tslStatus.Text = "Found a " + supportedDevice.ProductName;
				DisplayDeviceInformation(supportedDevice);
				pnlScanning.Visible = false;
				pnlDevice.Visible = true;
			} else {
				tslStatus.Text = "Waiting for device...";
			}
		}

		private void DisplayDeviceInformation(SupportedDevice device) {
			SetLink(lnkDeviceName, device.DeviceName, device.ProductPage);
			lblSystemVersion.Text = AdbDaemon.GetSystemVersion();
			SetLink(lnkVendor, device.VendorName, device.SupportPage);
			SetLink(lnkManufacturer, device.ManufacturerName, device.ManufacturerPage);
			picDevice.Image = device.DeviceImage;
		}

		private void SetLink(LinkLabel link, string caption, Uri uri) {
			link.Text = caption;
			link.Links.Clear();
			if ((caption != null) && (uri != null)) {
				link.Links.Add(0, caption.Length, uri);
				toolTip.SetToolTip(link, uri.ToString());
				link.Enabled = true;
			} else {
				toolTip.SetToolTip(link, null);
				link.Enabled = false;
			}
		}

		private static void LoadImage(string key, Bitmap image, ImageList imageList) {
			if (!imageList.Images.ContainsKey(key) && (image != null)) {
				imageList.Images.Add(key, image);
			}
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
			Application.Exit();
		}

		private void viewlogToolStripMenuItem_Click(object sender, EventArgs e) {
			AdbDaemon.LaunchLogCat();
		}

		private void systemInformationToolStripMenuItem_Click(object sender, EventArgs e) {
			MessageBox.Show(AdbDaemon.GetSystemInformation(), "System Information");
		}

		private void normalToolStripMenuItem_Click(object sender, System.EventArgs e) {
			var output = AdbDaemon.Reboot() ?? "Device is rebooting";
			MessageBox.Show(output, "Reboot - Normal");
		}

		private void recoveryToolStripMenuItem_Click(object sender, System.EventArgs e) {
			var output = AdbDaemon.Reboot(AdbDaemon.RebootMode.Recovery) ?? "Device is rebooting";
			MessageBox.Show(output, "Reboot - Recovery");
		}

		private void bootloaderToolStripMenuItem_Click(object sender, System.EventArgs e) {
			var output = AdbDaemon.Reboot(AdbDaemon.RebootMode.Bootloader) ?? "Device is rebooting";
			MessageBox.Show(output, "Reboot - Bootloader");
		}

		private void Link_Clicked(object sender, LinkLabelLinkClickedEventArgs e) {
			var url = (Uri) e.Link.LinkData;
			if (url != null) {
				Process.Start(url.ToString());
			}
		}
	}
}
