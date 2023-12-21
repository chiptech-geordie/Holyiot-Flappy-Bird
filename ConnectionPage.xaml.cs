using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;

namespace ChristmasCodingChallange;

public partial class ConnectionPage : ContentPage
{

    private readonly IAdapter adapter;
    private IDevice SelectedDevice;

    public ConnectionPage()
    {
        InitializeComponent();
        adapter = CrossBluetoothLE.Current.Adapter;
        ScanDevices();
    }

    private async void ScanDevices(object? sender = null, EventArgs? e = null)
    {
        SetSearchVisibility(true);

        List<IDevice> devices = new List<IDevice>();

        adapter.DeviceDiscovered += (s, a) => { devices.Add(a.Device); };
        await adapter.StartScanningForDevicesAsync();

        DevicesListView.ItemsSource = devices;

        SetSearchVisibility(false);
    }

    private void SetSearchVisibility(bool visible)
    {
        SearchLabel.IsVisible = visible;
        DevicesListView.IsVisible = !visible;
        OptionButtons.IsVisible = !visible;
        ConnectionButton.IsEnabled = false;
    }

    private void ItemSelectedEvent(object sender, SelectedItemChangedEventArgs e)
    {
        ConnectionButton.IsEnabled = true;
        SelectedDevice = (IDevice)e.SelectedItem;
    }

    private async void ConnectDeviceEvent(object sender, EventArgs e)
    {
        await adapter.ConnectToDeviceAsync(SelectedDevice);
        await Navigation.PushAsync(new GamePage(SelectedDevice));
    }
}