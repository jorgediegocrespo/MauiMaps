using CoreLocation;
using MapKit;
using Microsoft.Maui.Maps.Handlers;
using Microsoft.Maui.Maps.Platform;
using Microsoft.Maui.Platform;
using UIKit;

namespace MauiMaps;

public class CustomMapHandler : MapHandler
{
	protected override void ConnectHandler(MauiMKMapView platformView)
	{
		base.ConnectHandler(platformView);
		platformView.GetViewForAnnotation = GetViewForAnnotations;
	}

    public static void CustomMapPins(IMapHandler handler, Microsoft.Maui.Maps.IMap view)
    {
        MauiMKMapView mauiMKMapView = handler.PlatformView;
        System.Collections.IList pins = (System.Collections.IList)view.Pins;

        if (handler?.MauiContext == null)
            return;

        if (mauiMKMapView.Annotations?.Length > 0)
            mauiMKMapView.RemoveAnnotations(mauiMKMapView.Annotations);

        foreach (Microsoft.Maui.Maps.IMapPin pin in pins)
        {
            if (!(pin.ToHandler(handler.MauiContext).PlatformView is IMKAnnotation annotation))
                continue;

            if (!(pin is CustomPin cp))
            {
                pin.MarkerId = annotation;
                mauiMKMapView.AddAnnotation(annotation);
                continue;
            }

            cp.ImageSource.LoadImage(handler.MauiContext, result =>
            {
                var markerOption = new CustomAnnotation()
                {
                    Identifier = cp.Id,
                    Image = result?.Value,
                    Title = pin.Label,
                    Subtitle = pin.Address,
                    Coordinate = new CLLocationCoordinate2D(pin.Location.Latitude, pin.Location.Longitude),
                    Pin = cp
                };

                pin.MarkerId = markerOption;
                mauiMKMapView.AddAnnotation(markerOption);
            });
        }
    }

    private MKAnnotationView GetViewForAnnotations(MKMapView mapView, IMKAnnotation annotation)
    {
        MKAnnotationView annotationView = null;
        if (annotation is CustomAnnotation customAnnotation)
        {
            annotationView = mapView.DequeueReusableAnnotation(customAnnotation.Identifier.ToString()) ?? new MKAnnotationView(annotation, customAnnotation.Identifier.ToString());
            annotationView.Image = customAnnotation.Image;
            annotationView.CanShowCallout = true;
        }
        else if (annotation is MKPointAnnotation)
        {
            const string defaultPinId = "defaultPin";
#pragma warning disable CA1416, CA1422
            annotationView = mapView.DequeueReusableAnnotation(defaultPinId) ?? new MKPinAnnotationView(annotation, defaultPinId);
#pragma warning restore CA1416, CA1422
            annotationView.CanShowCallout = true;
        }

        return annotationView;
    }    
}
