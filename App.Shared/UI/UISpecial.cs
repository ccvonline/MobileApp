using System;
using Rock.Mobile.UI;
using System.Drawing;
using MobileApp.Shared.Config;
using MobileApp.Shared.Strings;
using Rock.Mobile.Animation;
using System.IO;
using System.Collections.Generic;

namespace MobileApp.Shared.UI
{
    public class UISpecial
    {
        public static bool Trigger( string trigger, object arg1, object arg2, object arg3, object arg4 )
        {
            bool didTrigger = false;

            switch( trigger )
            {
                case "there are no easter eggs":
                {
                    didTrigger = true;

                    #if __ANDROID__
                    Android.Widget.RelativeLayout relativeLayout = ((Android.Views.View)arg1).FindViewById<Android.Widget.RelativeLayout>( Droid.Resource.Id.relative_background );
                    RectangleF bounds = new System.Drawing.RectangleF( 0, 0, Droid.NavbarFragment.GetCurrentContainerDisplayWidth( ), ((Android.App.Fragment)arg2).Resources.DisplayMetrics.HeightPixels );

                    UICredits credits = new UICredits();
                    credits.Create( relativeLayout, false, bounds, delegate
                    {
                        credits.View.RemoveAsSubview( relativeLayout );
                    } );
                    credits.LayoutChanged( bounds );
                    #endif

                    #if __IOS__
                    RectangleF rectf = new RectangleF( (float)((UIKit.UIView)arg1).Frame.Left, (float)((UIKit.UIView)arg1).Frame.Top, (float)((UIKit.UIView)arg1).Frame.Width, (float)((UIKit.UIView)arg1).Frame.Height );

                    UICredits credits = new UICredits();
                    credits.Create( (UIKit.UIScrollView)arg2, true, rectf, delegate { credits.View.RemoveAsSubview( (UIKit.UIScrollView)arg2 ); });
                    credits.LayoutChanged( new System.Drawing.RectangleF( 0, 0, (float)((UIKit.UIView)arg1).Bounds.Width, (float)((UIKit.UIScrollView)arg2).ContentSize.Height ) );

                    ((UIKit.UIScrollView)arg2).ContentOffset = CoreGraphics.CGPoint.Empty;
                    ((UIKit.UIScrollView)arg2).ContentSize = new CoreGraphics.CGSize( ((UIKit.UIView)arg1).Frame.Width, credits.View.Frame.Height );
                    #endif
                    break;
                }

                case "i want a pony":
                {
                    didTrigger = true;

                    DoPicture( trigger, arg1, arg2, "pony.png" );
                    break;
                }
            }

            return didTrigger;
        }

        public static void DoPicture( string trigger, object arg1, object arg2, string imageName )
        {
            #if __ANDROID__
            Android.Widget.RelativeLayout relativeLayout = ((Android.Views.View)arg1).FindViewById<Android.Widget.RelativeLayout>( Droid.Resource.Id.relative_background );
            RectangleF bounds = new System.Drawing.RectangleF( 0, 0, Droid.NavbarFragment.GetCurrentContainerDisplayWidth( ), ((Android.App.Fragment)arg2).Resources.DisplayMetrics.HeightPixels );

            UIPony special = new UIPony();
            special.Create( relativeLayout, false, imageName, bounds, delegate
                {
                    special.View.RemoveAsSubview( relativeLayout );
                } );
            special.LayoutChanged( bounds );
            #endif
            #if __IOS__
            RectangleF rectf = new RectangleF( (float)((UIKit.UIView)arg1).Frame.Left, (float)((UIKit.UIView)arg1).Frame.Top, (float)((UIKit.UIView)arg1).Frame.Width, (float)((UIKit.UIView)arg1).Frame.Height );

            UIPony special = new UIPony();
            special.Create( (UIKit.UIScrollView)arg2, true, imageName, rectf, delegate { special.View.RemoveAsSubview( (UIKit.UIScrollView)arg2 ); });
            special.LayoutChanged( new System.Drawing.RectangleF( 0, 0, (float)((UIKit.UIView)arg1).Bounds.Width, (float)((UIKit.UIScrollView)arg2).ContentSize.Height ) );

            ((UIKit.UIScrollView)arg2).ContentOffset = CoreGraphics.CGPoint.Empty;
            ((UIKit.UIScrollView)arg2).ContentSize = new CoreGraphics.CGSize( ((UIKit.UIView)arg1).Frame.Width, special.View.Frame.Height );
            #endif
        }

        public static void DoVideo( string trigger, object arg1, object arg2, object arg3, object arg4, string url )
        {
            string awesomeUrl = url;
            #if __ANDROID__
            Droid.Tasks.Notes.NotesWatchFragment watchFrag = new Droid.Tasks.Notes.NotesWatchFragment();
            watchFrag.ParentTask = ((Droid.Tasks.Task)arg3);
            watchFrag.MediaUrl = awesomeUrl;
            watchFrag.Name = trigger;
            ((Droid.Tasks.Task)arg3).PresentFragment( watchFrag, true );
            #endif
            #if __IOS__
            iOS.NotesWatchUIViewController viewController = new iOS.NotesWatchUIViewController( );
            viewController.MediaUrl = awesomeUrl;
            viewController.Name = trigger;
            viewController.HideProgressIndicator = true;

            ((iOS.Task)arg4).PerformSegue( (UIKit.UIViewController)arg3, viewController );
            #endif
        }
    }

    public class UIPony
    {
        public PlatformView View { get; set; }
        public PlatformImageView PonyImage { get; set; }

        public PlatformButton CloseButton { get; set; }

        public UIPony( )
        {
        }

        public delegate void OnCompletion( );
        OnCompletion OnCompletionCallback;

        public void Create( object masterView, bool scaleImage, string imageName, RectangleF frame, OnCompletion onCompletion )
        {
            View = PlatformView.Create( );
            View.BackgroundColor = ControlStylingConfig.BackgroundColor;
            View.Frame = frame;
            View.AddAsSubview( masterView );

            MemoryStream logoStream = Rock.Mobile.IO.AssetConvert.AssetToStream( imageName );
            logoStream.Position = 0;
            PonyImage = PlatformImageView.Create( );
            PonyImage.AddAsSubview( View.PlatformNativeObject );
            PonyImage.Image = logoStream;
            PonyImage.ImageScaleType = PlatformImageView.ScaleType.ScaleAspectFit;
            logoStream.Dispose( );

            CloseButton = PlatformButton.Create( );
            CloseButton.AddAsSubview( View.PlatformNativeObject );
            CloseButton.Text = "Prance! Dance! It's Magic Pony Time!";
            CloseButton.BackgroundColor = PrayerConfig.PrayedForColor;
            CloseButton.TextColor = ControlStylingConfig.Button_TextColor;
            CloseButton.CornerRadius = ControlStylingConfig.Button_CornerRadius;
            CloseButton.SizeToFit( );
            CloseButton.ClickEvent = delegate(PlatformButton button) 
                {
                    OnCompletionCallback( );
                };

            OnCompletionCallback = onCompletion;
        }

        public void Destroy( )
        {
            // clean up resources (looking at you, Android)
            PonyImage.Destroy( );
        }


        bool Animating { get; set; }

        public void LayoutChanged( RectangleF frame )
        {
            View.Frame = new RectangleF( frame.Left, frame.Top, frame.Width, frame.Height );

            float currentYPos = Rock.Mobile.Graphics.Util.UnitToPx( 250 );
            PonyImage.Frame = new RectangleF( ( ( View.Frame.Width - PonyImage.Frame.Width ) / 2 ), currentYPos, PonyImage.Bounds.Width, PonyImage.Frame.Height );

            currentYPos = PonyImage.Frame.Bottom + Rock.Mobile.Graphics.Util.UnitToPx( 25 );
            CloseButton.Frame = new RectangleF( ( ( View.Frame.Width - CloseButton.Frame.Width ) / 2 ), currentYPos, CloseButton.Bounds.Width, CloseButton.Bounds.Height );

            View.Frame = new RectangleF( frame.Left, frame.Top, frame.Width, frame.Height );

            StartAnimation( );
        }

        void StartAnimation( )
        {
            if ( Animating == false )
            {
                Animating = true;

                float startingYPos = PonyImage.Position.Y;

                SimpleAnimator_Float animJump = new SimpleAnimator_Float( startingYPos, 50, 1.00f, 
                    delegate(float percent, object value )
                    {
                        PonyImage.Position = new PointF( PonyImage.Position.X, (float)value );
                    }, 
                    delegate
                    {
                        SimpleAnimator_Float animFall = new SimpleAnimator_Float( PonyImage.Position.Y, startingYPos, 1.00f, 
                            delegate(float percent, object value )
                            {
                                PonyImage.Position = new PointF( PonyImage.Position.X, (float)value );
                            }, 
                            delegate
                            {
                                Animating = false;
                                StartAnimation( );
                            });

                        animFall.Start( SimpleAnimator.Style.CurveEaseIn );
                    } );
                animJump.Start( SimpleAnimator.Style.CurveEaseOut );
            }
        }
    }

    public class UICredits
    {
        public PlatformView View { get; set; }

        public PlatformButton CloseButton { get; set; }

        class Credit
        {
            public Credit( string message, string image, bool scaleImage, RectangleF frame, PlatformView parent )
            {
                MemoryStream logoStream = Rock.Mobile.IO.AssetConvert.AssetToStream( image );
                logoStream.Position = 0;
                Image = PlatformImageView.Create( );
                Image.AddAsSubview( parent.PlatformNativeObject );
                Image.Image = logoStream;
                Image.ImageScaleType = PlatformImageView.ScaleType.ScaleAspectFit;
                logoStream.Dispose( );

                Label = PlatformLabel.Create( );
                Label.Text = message;
                Label.BackgroundColor = ControlStylingConfig.BG_Layer_Color;
                Label.BorderColor = ControlStylingConfig.BG_Layer_BorderColor;
                Label.BorderWidth = ControlStylingConfig.BG_Layer_BorderWidth;
                Label.TextColor = ControlStylingConfig.Label_TextColor;
                Label.Bounds = new RectangleF( 0, 0, frame.Width * .75f, 0 );
                Label.SizeToFit( );
                Label.AddAsSubview( parent.PlatformNativeObject );
            }
            
            public PlatformImageView Image { get; set; }
            public PlatformLabel Label { get; set; }    
        }
        List<Credit> Credits { get; set; }

        public UICredits( )
        {
        }

        public delegate void OnCompletion( );
        OnCompletion OnCompletionCallback;

        public void Create( object masterView, bool scaleImage, RectangleF frame, OnCompletion onCompletion )
        {
            View = PlatformView.Create( );
            View.BackgroundColor = ControlStylingConfig.BackgroundColor;
            View.Frame = frame;
            View.AddAsSubview( masterView );

            Credits = new List<Credit>();
            Credits.Add( new Credit( "Hey you found me! I'm Jered, the mobile app developer here at CCV. Making this app was a ton of work, and couldn't have happened without the support of a ton of people!\n\nThanks so much to:\n" +
                "Jenni, my beautiful wife and very patient app tester!\n\n" +
                "Jon & David, for developing this app's backbone, Rock.\n\n" + 
                "Mason & Mike for testing, end points, and endless puns! (See what I did there?)\n\n" + 
                "Nick & Kyle for all the great artwork and design you see in this app.\n\n" +
                "Dan & Kris Simpson for HUGE feedback, testing, and movie nights!!\n\n" +
                "The IT boyz: Jim, `Stopher and Chris, for being extremely willing Android guinea pigs.\n\n" +
                "Matt, Emily, Robin, Jill and Bree, for their testing and feedback.\n\n\n" +
                "And thanks of course to all of you out there using the app and making CCV the awesome church that it is!", 
                "me.png", scaleImage, frame, View ) );


            CloseButton = PlatformButton.Create( );
            CloseButton.AddAsSubview( View.PlatformNativeObject );
            CloseButton.Text = "Got It!";
            CloseButton.BackgroundColor = PrayerConfig.PrayedForColor;
            CloseButton.TextColor = ControlStylingConfig.Button_TextColor;
            CloseButton.CornerRadius = ControlStylingConfig.Button_CornerRadius;
            CloseButton.SizeToFit( );
            CloseButton.ClickEvent = delegate(PlatformButton button) 
                {
                    OnCompletionCallback( );
                };

            OnCompletionCallback = onCompletion;
        }

        public void Destroy( )
        {
            // clean up resources (looking at you, Android)
            foreach ( Credit credit in Credits )
            {
                credit.Image.Destroy( );
            }
        }

        public void LayoutChanged( RectangleF frame )
        {
            View.Frame = new RectangleF( frame.Left, frame.Top, frame.Width, frame.Height );

            float currentYPos = Rock.Mobile.Graphics.Util.UnitToPx( 40 );
            foreach( Credit credit in Credits )
            {
                credit.Image.Frame = new RectangleF( ( ( View.Frame.Width - credit.Image.Frame.Width ) / 2 ), currentYPos, credit.Image.Bounds.Width, credit.Image.Frame.Height );

                credit.Label.Frame = new RectangleF( ( ( View.Frame.Width - credit.Label.Frame.Width ) / 2 ), credit.Image.Frame.Bottom + Rock.Mobile.Graphics.Util.UnitToPx( 50 ), credit.Label.Bounds.Width, credit.Label.Bounds.Height );

                currentYPos = credit.Label.Frame.Bottom + Rock.Mobile.Graphics.Util.UnitToPx( 25 );
            }

            CloseButton.Frame = new RectangleF( ( ( View.Frame.Width - CloseButton.Frame.Width ) / 2 ), currentYPos, CloseButton.Bounds.Width, CloseButton.Bounds.Height );

            View.Frame = new RectangleF( frame.Left, frame.Top, frame.Width, currentYPos + Rock.Mobile.Graphics.Util.UnitToPx( 150 ) );
        }
    }
}
