# dotNet MAUI Maps
Una de las novedades que se ha lanzado ultimamente en *dotNet MAUI* es la posibilidad de utilizar mapas. Por lo que estos días he estado jugando un poco con todas las posibilidades que nos ofrecen estos mapas.

## Preconfiguración par autilizar mapas
Para poder utilizar mapas en nuestra aplicación, es necesario hacer algunas acciones previas que detallo a continuación:
- Instalar el nuget *Microsoft.Maui.Controls.Maps*
- Ejecutar, desde el método *CreateMauiApp* de la clase *MauiProgram*, *builder.UseMauiMaps*
~~~
public static MauiApp CreateMauiApp()
{
    var builder = MauiApp.CreateBuilder();
    builder
        ...
        .UseMauiMaps()
        ...
    return builder.Build();
}
~~~
- En iOS, modificar el archivo *info.plist* para informar al usuario que se va a utilizar su localización
~~~
<key>NSLocationAlwaysAndWhenInUseUsageDescription</key>
<string>Can we use your location at all times?</string>
<key>NSLocationWhenInUseUsageDescription</key>
<string>Can we use your location when your app is being used?</string>
~~~
- En Android, es necesario modificar el archivo *AndroidManifest.xml* para incluir nuestro *API key* de *Google Maps* (en esta web se indica cómo obtenerlo https://developers.google.com/maps/documentation/android-sdk/get-api-key) y permisos.
~~~
<application ...>
  <meta-data android:name="com.google.android.geo.API_KEY" android:value="YOUR-API-KEY" />
  <meta-data android:name="com.google.android.gms.version" android:value="@integer/google_play_services_version" />
</application>
...
<uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" />
<uses-permission android:name="android.permission.ACCESS_COARSE_LOCATION" />
<uses-permission android:name="android.permission.ACCESS_COARSE_LOCATION" />
...
~~~

## Explorando las funcionalidades de los mapas 
He creado un ejemplo en el que muestro las propiedades de los mapas que más útiles me han parecido y detallo a continuación:
- **IShowingUser:** Indica si muestra la posición actual del usuario.
- **IsScrollEnabled:** Utilizada para habilitar/deshabilitar el *scroll* en el mapa, tanto horizontal como vertical.
- **IsTrafficEnabled:** Sirve para mostrar u ocultar el tráfico dentro del mapa.
- **MapType:** Indica el tipo de mapa, pudiendo elegir entre *SatelLite*, *Hybrid* y *Street*
- **Pins:** Colección de pins que aparecen en el mapa. Permite indicar algunos valores descriptivos, como son  *Location*, *Label* y *Address*. NOTA: Aunque existe una propiedad *Type*, ésta no cambia el aspecto del pin. De hecho, para cambiar el aspecto de un pin, es necesario hacer un *handler* personalizado, tal y como veremos más adelante.

## Moviendo el mapa
En el ejemplo que he creado, he añadido un botón, con el texto *Span* que mueve el mapa al centro de Madrid. Para conseguirlo, he utilizado el método *MoveToRegion* del mapa, al que hay que pasarle un *MapSpan*, creado a partir de una localización (latitud y longitud) y una distancia, que puede ser creada a partir de kilómetros, metros o millas.

~~~
var location = new Location(40.416931, -3.703328); //Madrid
MapSpan mapSpan = MapSpan.FromCenterAndRadius(location, Distance.FromKilometers(0.5));
map.MoveToRegion(mapSpan);
~~~

## Jugando con el zoom
Para acercarnos o alejarnos a un punto del mapa, tenemos 2 opciones. Permitir el zoom y dejar al usuario que se mueva por el mapa utilizando los gestos pertinentes, o implementar el zoom programáticamente. En el ejemplo, he añadido dos botones para jugar con el zoom. Para conseguir acercarnos/alejarnos a un punto, he vuelto a utilizar el método *MoveToRegion*, manteniendo la localización del mapa y cambiando la distancia, ampliandola o reduciendola para reducir o ampliar el zoom, respectivamente.

~~~
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
~~~

## Pins personalizados
Con las propiedades que nos ofrecen los mapas, no tenemos opciones de personalizar el pin de un mapa con una imagen alternativa. No obstante, podemos conseguirlo de una forma sencilla modificando el método utilizado para mapear los pins a los controles nativos. 
El primer paso, es ampliar las propiedades de un pin, para poder especificar una imagen, tal y como se muestra a continuación:

~~~
public class CustomPin : Pin
{
    public static readonly BindableProperty ImageSourceProperty = BindableProperty.Create(nameof(ImageSource), typeof(ImageSource), typeof(CustomPin));

    public ImageSource ImageSource
    {
        get => (ImageSource)GetValue(ImageSourceProperty);
        set => SetValue(ImageSourceProperty, value);
    }
}
~~~

El siguiente paso sería modificar el mapeo de estos pines para cada una de las plataformas, iOS y Android en el ejemplo que nos ocupa. Además, en ambas plataformas es necesario sobreescribir el *handler* pora poder añadir cierta funcionalidad periférica al mapeo de estos pines. 

En iOS se sobreescribe para modificar el método que retorna la *View* para las *Annotations* del mapa. 

~~~
public class CustomAnnotation : MKPointAnnotation
{
  public Guid Identifier { get; init; }
  public UIImage? Image { get; init; }
  public required IMapPin Pin { get; init; }
}

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
~~~

En Android, se hace para tener una refencia a los *Markers* del mapa, ya que está refencia no es accesible en un *MapHandler*.

~~~
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
~~~

Por último, es necesario indicarle al *builder* el *handler* que se va a utilizar para renderizar un mapa, así cómo modificar el *mapper* de los pins. Para ello, se añaden las siguientes líneas en el método *CreateMauiApp* del *MauiProgram*

~~~
builder
  .UseMauiApp<App>()
  ...
  .ConfigureMauiHandlers(handlers =>
  {
  #if IOS || MACCATALYST
    handlers.AddHandler<Microsoft.Maui.Controls.Maps.Map, CustomMapHandler>();
    Microsoft.Maui.Maps.Handlers.MapHandler.Mapper.ModifyMapping(nameof(Microsoft.Maui.Maps.IMap.Pins), (handler, view, action) =>
      CustomMapHandler.CustomMapPins(handler, view));
  #elif ANDROID
    handlers.AddHandler<Microsoft.Maui.Controls.Maps.Map, CustomMapHandler>();
    Microsoft.Maui.Maps.Handlers.MapHandler.Mapper.ModifyMapping(nameof(Microsoft.Maui.Maps.IMap.Pins), (handler, view, action) =>
      CustomMapHandler.CustomMapPins(handler, view));

    handlers.AddHandler<Microsoft.Maui.Controls.Maps.Map, CustomMapHandler>();
#endif
~~~
