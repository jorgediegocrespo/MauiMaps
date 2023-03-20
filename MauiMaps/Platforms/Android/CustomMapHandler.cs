using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Graphics.Drawables;
using Microsoft.Maui.Maps;
using Microsoft.Maui.Maps.Handlers;
using Microsoft.Maui.Platform;
using IMap = Microsoft.Maui.Maps.IMap;

namespace MauiMaps;

public class CustomMapHandler : MapHandler
{
    private List<Marker>? _markers;

    public static void CustomMapPins(IMapHandler handler, IMap map)
    {
        if (!(handler is CustomMapHandler mapHandler))
            return;

        if (mapHandler._markers == null)
        {
            mapHandler.AddPins(map.Pins);
            return;
        }


        foreach (var marker in mapHandler._markers)
            marker.Remove();

        mapHandler._markers = null;
        mapHandler.AddPins(map.Pins);

    }

    private void AddPins(IEnumerable<IMapPin> mapPins)
    {
        if (Map is null || MauiContext is null)
            return;


        _markers ??= new List<Marker>();
        foreach (var pin in mapPins)
        {
            var pinHandler = pin.ToHandler(MauiContext);
            if (!(pinHandler is IMapPinHandler mapPinHandler))
                continue;

            var markerOption = mapPinHandler.PlatformView;
            if (!(pin is CustomPin cp))
            {
                AddMarker(Map, pin, _markers, markerOption);
                continue;
            }

            cp.ImageSource.LoadImage(MauiContext, result =>
            {
                if (result?.Value is BitmapDrawable bitmapDrawable)
                    markerOption.SetIcon(BitmapDescriptorFactory.FromBitmap(bitmapDrawable.Bitmap));

                AddMarker(Map, pin, _markers, markerOption);
            });
        }
    }

    private void AddMarker(GoogleMap map, IMapPin pin, List<Marker> markers, MarkerOptions markerOption)
    {
        var marker = map.AddMarker(markerOption);
        pin.MarkerId = marker.Id;
        markers.Add(marker);
    }
}
