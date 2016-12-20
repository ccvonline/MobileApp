using System;
using MobileApp.Shared;
using Rock.Mobile.PlatformSpecific.Util;
using MobileApp.Shared.Config;
using UIKit;
using Foundation;
using MobileApp.Shared.UI;
using CoreGraphics;
using System.Drawing;
using Rock.Mobile.PlatformSpecific.iOS.UI;

namespace iOS
{
    public class GroupFinderJoinViewController : TaskUIViewController
    {
        public string GroupTitle { get; set; }
        public string Distance { get; set; }
        public string MeetingTime { get; set; }
        public int GroupID { get; set; }

        UIJoinGroup JoinGroupView { get; set; }

        UIScrollViewWrapper ScrollView { get; set; }
        UITextField CellPhoneTextField { get; set; }


        KeyboardAdjustManager KeyboardAdjustManager { get; set; }
        
        public GroupFinderJoinViewController( )
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BackgroundColor );

            ScrollView = new UIScrollViewWrapper();
            ScrollView.Layer.AnchorPoint = CGPoint.Empty;
            ScrollView.Parent = this;
            ScrollView.Bounds = View.Bounds;
            View.AddSubview( ScrollView );

            KeyboardAdjustManager = new KeyboardAdjustManager( View );

            JoinGroupView = new UIJoinGroup();
            JoinGroupView.Create( ScrollView, View.Frame.ToRectF( ) );

            UITextField spouseName = (UITextField)JoinGroupView.SpouseName.PlatformNativeObject;
            spouseName.Delegate = new KeyboardAdjustManager.TextFieldDelegate( );

            UITextField emailName = (UITextField)JoinGroupView.Email.PlatformNativeObject;
            emailName.Delegate = new KeyboardAdjustManager.TextFieldDelegate( );

            // since we're using the platform UI, we need to manually hook up the phone formatting delegate,
            // because that isn't implemented in platform abstracted code.
            CellPhoneTextField = (UITextField)JoinGroupView.CellPhone.PlatformNativeObject;
            CellPhoneTextField.Delegate = new Rock.Mobile.PlatformSpecific.iOS.UI.PhoneNumberFormatterDelegate();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            KeyboardAdjustManager.Activate( );

            // setup the values
            JoinGroupView.DisplayView( GroupTitle, Distance, MeetingTime, GroupID );

            // force the cell phone field to update itself so it contains proper formatting
            CellPhoneTextField.Delegate.ShouldChangeCharacters( CellPhoneTextField, new NSRange( CellPhoneTextField.Text.Length, 0 ), "" );
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            KeyboardAdjustManager.Deactivate( );
        }

        public override void LayoutChanged( )
        {
            base.LayoutChanged( );

            ScrollView.Bounds = View.Bounds;

            nfloat controlBottom = JoinGroupView.GetControlBottom( ) + ( View.Bounds.Height * .25f );
            ScrollView.ContentSize = new CGSize( 0, (nfloat) Math.Max( controlBottom, View.Bounds.Height * 1.05f ) );

            RectangleF joinBounds = new RectangleF( 0, 0, (float)View.Bounds.Width, (float)ScrollView.ContentSize.Height );
            JoinGroupView.LayoutChanged( joinBounds );
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);

            JoinGroupView.TouchesEnded( );
        }
    }
}
