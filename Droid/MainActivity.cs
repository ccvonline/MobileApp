﻿using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Util;
using Android.Gms.Maps;
using App.Shared.Config;
using Rock.Mobile.PlatformSpecific.Android.Graphics;
using Android.Graphics;
using Com.Localytics.Android;
using App.Shared.PrivateConfig;
using Android.Hardware;
using Android.Media;
using Android.Content.Res;
using Rock.Mobile.Math;

namespace Droid
{
    [Application( Name="com.ccvonline.CCVMobileApp" )]
    [MetaData ("LOCALYTICS_APP_KEY", Value=GeneralConfig.Droid_Localytics_Key)]
    public class MainApplication : Application
    {
        public MainApplication( System.IntPtr whatever, Android.Runtime.JniHandleOwnership jniHandle ) : base( whatever, jniHandle )
        {
        }
        
        public override void OnCreate()
        {
            base.OnCreate();

            Rock.Mobile.PlatformSpecific.Android.Core.Context = this;

#if !DEBUG
            LocalyticsActivityLifecycleCallbacks callback = new LocalyticsActivityLifecycleCallbacks( this );
            RegisterActivityLifecycleCallbacks( callback );
#endif
        }
    }
    
    [Activity( Label = GeneralConfig.AndroidAppName, NoHistory = true, LaunchMode = Android.Content.PM.LaunchMode.SingleTask, MainLauncher = true, Icon = "@drawable/icon", ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize )]
    public class Splash : Activity
    {
        AspectScaledImageView SplashImage { get; set; }
        Bitmap SplashBitmap { get; set; }

        protected override void OnCreate( Bundle bundle )
        {
            base.OnCreate( bundle );

            // see if this device will support wide landscape (like, if it's a tablet)
            if ( MainActivity.SupportsLandscapeWide( this ) && Rock.Mobile.PlatformSpecific.Android.Core.IsOrientationUnlocked( this ) )
            {
                RequestedOrientation = Android.Content.PM.ScreenOrientation.FullSensor;
            }
            else
            {
                RequestedOrientation = Android.Content.PM.ScreenOrientation.Portrait;
            }

            Window.AddFlags( WindowManagerFlags.Fullscreen );

            Rock.Mobile.Util.URL.Override.SetAppUrlOverrides( PrivateGeneralConfig.App_URL_Overrides );

            // Set our view from the "main" layout resource
            SetContentView( Resource.Layout.Splash );

            // load our image.
            RelativeLayout layout = FindViewById<RelativeLayout>(Resource.Id.fragment_container);

            SplashImage = new AspectScaledImageView( this );
            SplashImage.LayoutParameters = new RelativeLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent );
            SplashImage.SetScaleType( ImageView.ScaleType.CenterCrop );
            layout.AddView( SplashImage );

            SplashBitmap = BitmapFactory.DecodeResource( Rock.Mobile.PlatformSpecific.Android.Core.Context.Resources, Resource.Drawable.splash_combo_android );
            SplashImage.SetImageBitmap( SplashBitmap );

            System.Timers.Timer splashTimer = new System.Timers.Timer();
            splashTimer.Interval = 500;
            splashTimer.AutoReset = false;
            splashTimer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e) => 
                {

                    RunOnUiThread( delegate
                        {
                            // launch create order intent, which should be a FORM
                            Intent intent = new Intent(this, typeof(MainActivity));
                            StartActivity(intent);
                        });
                };

            splashTimer.Start( );
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if ( SplashImage != null && SplashImage.Drawable != null )
            {
                SplashImage.Drawable.Dispose( );
                SplashImage.SetImageBitmap( null );
            }

            if ( SplashBitmap != null )
            {
                SplashBitmap.Recycle( );
                SplashBitmap.Dispose( );
                SplashBitmap = null;
            }
        }
    }

    [Activity( Label = GeneralConfig.AndroidAppName, Icon = "@drawable/icon", LaunchMode = Android.Content.PM.LaunchMode.SingleTask, ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize )]
    //This defines support for a URL scheme in the format "ccvmobile://go" that will re-launch this activity.
    [IntentFilter (new[]{Intent.ActionView}, 
        Categories=new[]{Intent.CategoryDefault, Intent.CategoryBrowsable},
        DataScheme="ccvmobile",DataHost="go")]
    public class MainActivity : Activity, ISensorEventListener
    {
        Springboard Springboard { get; set; }
        SensorManager SensorManager { get; set; }
        static readonly object _syncLock = new object();

        protected override void OnCreate( Bundle bundle )
        {
            base.OnCreate( bundle );

#if !DEBUG
            Xamarin.Insights.Initialize( GeneralConfig.Droid_Xamarin_Insights_Key, this );
#endif

            Window.AddFlags(WindowManagerFlags.Fullscreen);

            Rock.Mobile.PlatformSpecific.Android.Core.Context = this;

            // default our app to protrait mode, and let the notes change it.
            if ( SupportsLandscapeWide( ) && Rock.Mobile.PlatformSpecific.Android.Core.IsOrientationUnlocked( ) )
            {
                RequestedOrientation = Android.Content.PM.ScreenOrientation.FullSensor;
            }
            else
            {
                RequestedOrientation = Android.Content.PM.ScreenOrientation.Portrait;
            }

            DisplayMetrics metrics = Resources.DisplayMetrics;
            Rock.Mobile.Util.Debug.WriteLine( string.Format( "Android Device detected dpi: {0}", metrics.DensityDpi ) );

            // Set our view from the "main" layout resource
            SetContentView( Resource.Layout.Main );

            // get the active task frame and give it to the springboard
            FrameLayout layout = FindViewById<FrameLayout>(Resource.Id.activetask);

            Rock.Mobile.UI.PlatformBaseUI.Init( );
            MapsInitializer.Initialize( this );

            Springboard = FragmentManager.FindFragmentById(Resource.Id.springboard) as Springboard;
            Springboard.SetActiveTaskFrame( layout );


            SensorManager = (SensorManager) GetSystemService( Context.SensorService );
        }

        /// <summary>
        /// Returns true if the device CAN display in landscape wide mode. This doesn't
        /// necessarily mean it IS in landscape wide mode.
        /// </summary>
        public static bool SupportsLandscapeWide( Context contextArg = null )
        {
            Context currContext = contextArg == null ? Rock.Mobile.PlatformSpecific.Android.Core.Context : contextArg;
            
            // get the current device configuration
            Android.Content.Res.Configuration currConfig = currContext.Resources.Configuration;

            if ( ( currConfig.ScreenLayout & Android.Content.Res.ScreenLayout.SizeMask ) >= Android.Content.Res.ScreenLayout.SizeLarge )
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true if the device is CURRENTLY IN landscape wide mode.
        /// </summary>
        public static bool IsLandscapeWide( )
        {
            Android.Content.Res.Configuration currConfig = Rock.Mobile.PlatformSpecific.Android.Core.Context.Resources.Configuration;

            // if it has the capacity for landscape wide, and is currently in landscape
            if ( MainActivity.SupportsLandscapeWide( ) == true && (currConfig.ScreenWidthDp > currConfig.ScreenHeightDp) == true )
            {
                // then yes, we're in landscape wide.
                return true;
            }

            return false;
        }

        public static bool IsLandscape( )
        {
            Android.Content.Res.Configuration currConfig = Rock.Mobile.PlatformSpecific.Android.Core.Context.Resources.Configuration;

            // is our width greater?
            if ( currConfig.ScreenWidthDp > currConfig.ScreenHeightDp == true )
            {
                
                return true;
            }

            return false;
        }

        public static bool IsPortrait( )
        {
            Android.Content.Res.Configuration currConfig = Rock.Mobile.PlatformSpecific.Android.Core.Context.Resources.Configuration;

            // is our width less?
            if ( currConfig.ScreenWidthDp < currConfig.ScreenHeightDp == true )
            {

                return true;
            }

            return false;
        }

        protected override void OnRestart()
        {
            base.OnRestart();

            // restore our context
            Rock.Mobile.PlatformSpecific.Android.Core.Context = this;
        }

        protected override void OnResume()
        {
            base.OnResume();

            OverridePendingTransition( Resource.Animation.abc_fade_in, Resource.Animation.abc_fade_out );

            // restore our context
            Rock.Mobile.PlatformSpecific.Android.Core.Context = this;

            SensorManager.RegisterListener( this, SensorManager.GetDefaultSensor( SensorType.Accelerometer ), SensorDelay.Normal );
        }

        protected override void OnPause ()
        {
            base.OnPause();

            SensorManager.UnregisterListener( this );
        }

        public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
        {
            // We don't want to do anything here.
        }

        Vector3 LastPhonePosition = null;

        public void OnSensorChanged(SensorEvent e)
        {
            lock (_syncLock)
            {
                if( LastPhonePosition == null )
                {
                    LastPhonePosition = new Vector3( ) { X = e.Values[ 0 ], Y = e.Values[ 1 ], Z = e.Values[ 2 ] };
                }
                else
                {
                    Vector3 currPos = new Vector3( ) { X = e.Values[ 0 ], Y = e.Values[ 1 ], Z = e.Values[ 2 ] };

                    Vector3 delta = LastPhonePosition - currPos;

                    float changeRate = Vector3.Magnitude( delta );

                    if( changeRate > 4.0f )
                    {
                        LastPhonePosition = currPos;
                    
                        Console.WriteLine( string.Format("x={0:f}, y={1:f}, y={2:f}", e.Values[0], e.Values[1], e.Values[2]) );
                    }
                }
            }
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);

            Intent = intent;
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            // restore our context
            Rock.Mobile.PlatformSpecific.Android.Core.Context = this;
        }

        public override void OnBackPressed()
        {
            // only allow Back if the springboard OKs it.
            if ( Springboard.CanPressBack( ) )
            {
                base.OnBackPressed( );
            }
        }
    }
}
