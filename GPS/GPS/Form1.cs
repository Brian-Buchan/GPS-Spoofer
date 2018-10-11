using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using iMobileDevice;
using iMobileDevice.iDevice;
using iMobileDevice.Lockdown;

namespace GPS
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            NativeLibraries.Load();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            map.MapProvider = GMapProviders.GoogleMap;
            map.DragButton = MouseButtons.Right;
            map.MinZoom = 5;
            map.MaxZoom = 100;
            map.Zoom = 10;
            map.MarkersEnabled = true;
            // Position at my house
            changeMapPosition(43.005174, -85.640573);
            map.OnPositionChanged += Map_OnPositionChanged;
        }

        private void Map_OnPositionChanged(PointLatLng point)
        {
            txt_Lat_Select.Text = point.Lat.ToString();
            txt_Long_Select.Text = point.Lng.ToString();
        }

        private void changeMapPosition(double latitude, double longitude)
        {
            map.Position = new PointLatLng(latitude, longitude);
            txt_Lat_Current.Text = latitude.ToString();
            txt_Long_Current.Text = longitude.ToString();
            GMapMarker mark = new GMarkerGoogle(map.Position, GMarkerGoogleType.blue);
            GMapOverlay markers = new GMapOverlay("markers");
            markers.Markers.Add(mark);
            map.Overlays.Clear();
            map.Overlays.Add(markers);
        }

        private void btnTele_Click(object sender, EventArgs e)
        {
            changeMapPosition(map.Position.Lat, map.Position.Lng);
        }

        private void cmb_Connected_Devices_DropDown(object sender, EventArgs e)
        {
            cmb_Connected_Devices.Items.Clear();
            ReadOnlyCollection<string> udids = GetDevices();
            foreach (string udid in udids)
            {
                cmb_Connected_Devices.Items.Add(udid);
            }
        }

        private ReadOnlyCollection<string> GetDevices()
        {
            ReadOnlyCollection<string> udids;
            int count = 0;

            var idevice = LibiMobileDevice.Instance.iDevice;
            var lockdown = LibiMobileDevice.Instance.Lockdown;

            var ret = idevice.idevice_get_device_list(out udids, ref count);

            if (ret == iDeviceError.NoDevice)
            {
                // Not actually an error in our case
                //return;
            }

            ret.ThrowOnError();

            // Get the device name
            foreach (var udid in udids)
            {
                iDeviceHandle deviceHandle;
                idevice.idevice_new(out deviceHandle, udid).ThrowOnError();

                LockdownClientHandle lockdownHandle;
                lockdown.lockdownd_client_new_with_handshake(deviceHandle, out lockdownHandle, "Quamotion").ThrowOnError();

                string deviceName;
                lockdown.lockdownd_get_device_name(lockdownHandle, out deviceName).ThrowOnError();

                deviceHandle.Dispose();
                lockdownHandle.Dispose();
            }

            return udids;
        }
    }
}
