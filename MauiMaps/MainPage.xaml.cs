using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace MauiMaps;

public partial class MainPage : ContentPage
{
    private double _zoomLevel;
    
    public MainPage()
    {
        InitializeComponent();
        
        _zoomLevel = Math.Sqrt(360 / 0.01);
        
        map.IsShowingUser = true;
        map.IsScrollEnabled = true;
        map.IsTrafficEnabled = true;
        map.IsZoomEnabled = true;
        map.MapType = MapType.Street;
    }

    private void SpanButton_OnClicked(object sender, EventArgs e)
    {
        var location = new Location(40.416931, -3.703328); //Madrid
        MapSpan mapSpan = MapSpan.FromCenterAndRadius(location, Distance.FromKilometers(0.5));
        map.MoveToRegion(mapSpan);
    }
    
    private void ZoomInButton_OnClicked(object sender, EventArgs e)
    {
        var newValue = map.VisibleRegion.Radius.Meters / 2;
        if (map.VisibleRegion != null)
            map.MoveToRegion(MapSpan.FromCenterAndRadius(map.VisibleRegion.Center, Distance.FromMeters(newValue)));
    }
    
    private void ZoomOutButton_OnClicked(object sender, EventArgs e)
    {
        var newValue = 2 * map.VisibleRegion.Radius.Meters;
        if (map.VisibleRegion != null)
            map.MoveToRegion(MapSpan.FromCenterAndRadius(map.VisibleRegion.Center, Distance.FromMeters(newValue)));
    }
    
    private void CreatePins_OnClicked(object sender, EventArgs e)
    {
        CustomPin officePin = new CustomPin
        {
            Location = new Location(40.417759, -3.705920),
            Label = "Office",
            Address = "Office Street, 10",
            Type = PinType.Place,
            ImageSource = new FontImageSource() { FontFamily = "MaterialDesignIcons", Glyph = MauiMaps.Resources.Fonts.MaterialDesignIcon.OfficeBuilding, Color = Colors.Green }
        };
        officePin.MarkerClicked += MarkerPinClicked;
        
        CustomPin homePin = new CustomPin
        {
            Location = new Location(40.417335, -3.701926),
            Label = "Home",
            Address = "Home Street, 1",
            Type = PinType.SavedPin,
            ImageSource = new FontImageSource() { FontFamily = "MaterialDesignIcons", Glyph = MauiMaps.Resources.Fonts.MaterialDesignIcon.Home, Color = Colors.Blue }
        };
        homePin.MarkerClicked += MarkerPinClicked;
        
        CustomPin schoolPin = new CustomPin
        {
            Location = new Location(40.416142, -3.703687),
            Label = "School",
            Address = "School Av, 32",
            Type = PinType.Generic,
            ImageSource = new FontImageSource() { FontFamily = "MaterialDesignIcons", Glyph = MauiMaps.Resources.Fonts.MaterialDesignIcon.School, Color = Colors.Red }
        };
        schoolPin.MarkerClicked += MarkerPinClicked;

        Pin restaurantPin = new Pin
        {
            Location = new Location(40.417351, -3.700939),
            Label = "Restaurant",
            Address = "Restaurant Av, 14",
            Type = PinType.SearchResult,
        };
        restaurantPin.MarkerClicked += MarkerPinClicked;

        map.Pins.Add(officePin);
        map.Pins.Add(homePin);
        map.Pins.Add(schoolPin);
        map.Pins.Add(restaurantPin);
        
        var location = new Location(40.417335, -3.701926);
        MapSpan mapSpan = MapSpan.FromCenterAndRadius(location, Distance.FromKilometers(1));
        map.MoveToRegion(mapSpan);
    }

    private void MarkerPinClicked(object sender, PinClickedEventArgs e)
    {
        //Do something when pin clicked
    }

    private void ChangeUser_OnClicked(object sender, EventArgs e)
    {
        map.IsShowingUser = !map.IsShowingUser;
        btChangeUser.Text = $"User {(map.IsShowingUser ? "On" : "Off")}";
    }

    private void ChangeScroll_OnClicked(object sender, EventArgs e)
    {
        map.IsScrollEnabled = !map.IsScrollEnabled;
        btChangeScroll.Text = $"Scroll {(map.IsScrollEnabled ? "On" : "Off")}";
    }

    private void ChangeTraffic_OnClicked(object sender, EventArgs e)
    {
        map.IsTrafficEnabled = !map.IsTrafficEnabled;
        btChangeTraffic.Text = $"Traffic {(map.IsTrafficEnabled ? "On" : "Off")}";
    }

    private void ChangeMapType_OnClicked(object sender, EventArgs e)
    {
        map.MapType = map.MapType switch
        {
            MapType.Hybrid => MapType.Satellite,
            MapType.Satellite => MapType.Street,
            _ => MapType.Hybrid
        };
    }

    private void ChangeZoom_OnClicked(object sender, EventArgs e)
    {
        map.IsZoomEnabled = !map.IsZoomEnabled;
        btChangeZoom.Text = $"Zoom {(map.IsZoomEnabled ? "On" : "Off")}";
    }
}