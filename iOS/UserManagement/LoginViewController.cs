using System;
using Foundation;
using UIKit;
using System.CodeDom.Compiler;
using Rock.Mobile.Network;
using MobileApp.Shared.Network;
using System.IO;
using MobileApp.Shared.Config;
using Rock.Mobile.UI;
using MobileApp.Shared.Strings;
using MobileApp.Shared;
using Rock.Mobile.Threading;
using Rock.Mobile.PlatformSpecific.iOS.UI;
using System.Collections.Generic;
using Rock.Mobile.Animation;
using CoreGraphics;
using MobileApp.Shared.UI;
using Rock.Mobile.PlatformSpecific.Util;
using MobileApp.Shared.Analytics;
using MobileApp.Shared.PrivateConfig;

namespace iOS
{
	class LoginViewController : UIViewController
	{
        /// <summary>
        /// Reference to the parent springboard for returning apon completion
        /// </summary>
        /// <value>The springboard.</value>
        public SpringboardViewController Springboard { get; set; }

        /// <summary>
        /// Timer to allow a small delay before returning to the springboard after a successful login.
        /// </summary>
        /// <value>The login successful timer.</value>
        System.Timers.Timer LoginSuccessfulTimer { get; set; }

        UIBlockerView BlockerView { get; set; }

        WebLayout WebLayout { get; set; }

        UIScrollViewWrapper ScrollView { get; set; }

		public LoginViewController ( ) : base ( )
		{
            // setup our timer
            LoginSuccessfulTimer = new System.Timers.Timer();
            LoginSuccessfulTimer.AutoReset = false;
            LoginSuccessfulTimer.Interval = 2000;
		}

        protected enum LoginState
        {
            Out,
            Trying,

            // Deprecated state
            In
        };
        LoginState State { get; set; }

        UIImageView LogoView { get; set; }

        UIImageView FBImageView { get; set; }

        StyledTextField UserNameField { get; set; }

        StyledTextField PasswordField { get; set; }

        UIButton LoginButton { get; set; }

        UILabel AdditionalOptions { get; set; }
        UIButton RegisterButton { get; set; }
        UILabel OrSpacerLabel { get; set; }
        UIButton FacebookLogin { get; set; }

        UIButton CancelButton { get; set; }

        StyledTextField LoginResult { get; set; }

        UIView HeaderView { get; set; }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            BlockerView = new UIBlockerView( View, View.Frame.ToRectF( ) );

            View.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BackgroundColor );

            ScrollView = new UIScrollViewWrapper();
            ScrollView.Parent = this;
            ScrollView.Layer.AnchorPoint = CGPoint.Empty;
            ScrollView.Bounds = View.Bounds;
            View.AddSubview( ScrollView );

            UserNameField = new StyledTextField();
            ScrollView.AddSubview( UserNameField.Background );

            UserNameField.Field.AutocapitalizationType = UITextAutocapitalizationType.None;
            UserNameField.Field.AutocorrectionType = UITextAutocorrectionType.No;
            ControlStyling.StyleTextField( UserNameField.Field, LoginStrings.UsernamePlaceholder, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            ControlStyling.StyleBGLayer( UserNameField.Background );
            UserNameField.Field.ShouldReturn += (textField) => 
                {
                    textField.ResignFirstResponder();

                    TryRockBind();
                    return true;
                };

            PasswordField = new StyledTextField();
            ScrollView.AddSubview( PasswordField.Background );
            PasswordField.Field.AutocorrectionType = UITextAutocorrectionType.No;
            PasswordField.Field.SecureTextEntry = true;

            ControlStyling.StyleTextField( PasswordField.Field, LoginStrings.PasswordPlaceholder, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            ControlStyling.StyleBGLayer( PasswordField.Background );
            PasswordField.Field.ShouldReturn += (textField) => 
                {
                    textField.ResignFirstResponder();

                    TryRockBind();
                    return true;
                };

            // obviously attempt a login if login is pressed
            LoginButton = UIButton.FromType( UIButtonType.System );
            ScrollView.AddSubview( LoginButton );
            ControlStyling.StyleButton( LoginButton, LoginStrings.LoginButton, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            LoginButton.SizeToFit( );
            LoginButton.TouchUpInside += (object sender, EventArgs e) => 
                {
                    if( RockMobileUser.Instance.LoggedIn == true )
                    {
                        RockMobileUser.Instance.LogoutAndUnbind( );

                        SetUIState( LoginState.Out );
                    }
                    else
                    {
                        TryRockBind();
                    }
                };

            AdditionalOptions = new UILabel( );
            ScrollView.AddSubview( AdditionalOptions );
            ControlStyling.StyleUILabel( AdditionalOptions, ControlStylingConfig.Font_Regular, ControlStylingConfig.Small_FontSize );
            AdditionalOptions.Text = LoginStrings.AdditionalOptions;
            AdditionalOptions.TextColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor );
            AdditionalOptions.SizeToFit( );

            OrSpacerLabel = new UILabel( );
            ScrollView.AddSubview( OrSpacerLabel );
            ControlStyling.StyleUILabel( OrSpacerLabel, ControlStylingConfig.Font_Regular, ControlStylingConfig.Small_FontSize );
            OrSpacerLabel.TextColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor );
            OrSpacerLabel.Text = LoginStrings.OrString;
            OrSpacerLabel.SizeToFit( );

            RegisterButton = UIButton.FromType( UIButtonType.System );
            ScrollView.AddSubview( RegisterButton );
            ControlStyling.StyleButton( RegisterButton, LoginStrings.RegisterButton, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            //RegisterButton.BackgroundColor = UIColor.Clear;
            RegisterButton.SizeToFit( );
            RegisterButton.TouchUpInside += (object sender, EventArgs e ) =>
                {
                    Springboard.RegisterNewUser( );
                };

            // setup the result
            LoginResult = new StyledTextField( );
            ScrollView.AddSubview( LoginResult.Background );

            ControlStyling.StyleTextField( LoginResult.Field, "", ControlStylingConfig.Font_Regular, ControlStylingConfig.Small_FontSize );
            ControlStyling.StyleBGLayer( LoginResult.Background );
            LoginResult.Field.UserInteractionEnabled = false;
            LoginResult.Field.TextAlignment = UITextAlignment.Center;

            // setup the facebook button
            FacebookLogin = new UIButton( );
            ScrollView.AddSubview( FacebookLogin );
            string imagePath = NSBundle.MainBundle.BundlePath + "/" + "facebook_login.png";
            FBImageView = new UIImageView( new UIImage( imagePath ) );

            FacebookLogin.SetTitle( "", UIControlState.Normal );
            FacebookLogin.AddSubview( FBImageView );
            FacebookLogin.Layer.CornerRadius = 4;
            FBImageView.Layer.CornerRadius = 4;

            FacebookLogin.TouchUpInside += (object sender, EventArgs e) => 
                {
                    TryFacebookBind();
                };

            // If cancel is pressed, notify the springboard we're done.
            CancelButton = UIButton.FromType( UIButtonType.System );
            ScrollView.AddSubview( CancelButton );
            CancelButton.SetTitleColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.Label_TextColor ), UIControlState.Normal );
            CancelButton.SetTitle( GeneralStrings.Cancel, UIControlState.Normal );
            CancelButton.SizeToFit( );
            CancelButton.TouchUpInside += (object sender, EventArgs e) => 
                {
                    // don't allow canceling while we wait for a web request.
                    if( LoginState.Trying != State )
                    {
                        Springboard.ResignModelViewController( this, null );
                    }
                };

            // setup the fake header
            HeaderView = new UIView( );
            View.AddSubview( HeaderView );
            HeaderView.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BackgroundColor );

            // set the title image for the bar if there's no safe area defined. (A safe area is like, say, the notch for iPhone X)
            if ( UIApplication.SharedApplication.KeyWindow.SafeAreaInsets.Top == 0 )
            {
                imagePath = NSBundle.MainBundle.BundlePath + "/" + PrivatePrimaryNavBarConfig.LogoFile_iOS;
                LogoView = new UIImageView( new UIImage( imagePath ) );
                LogoView.SizeToFit( );
                LogoView.Layer.AnchorPoint = CGPoint.Empty;

                HeaderView.AddSubview( LogoView );
            }
        }

        public override void ViewDidLayoutSubviews( )
        {
            base.ViewDidLayoutSubviews( );

            ScrollView.Layer.Position = new CGPoint( 0, HeaderView.Frame.Bottom );
            ScrollView.Bounds = View.Bounds;

            UserNameField.SetFrame( new CGRect( -10, View.Frame.Height * .10f, View.Frame.Width + 20, StyledTextField.StyledFieldHeight ) );
            PasswordField.SetFrame( new CGRect( UserNameField.Background.Frame.Left, UserNameField.Background.Frame.Bottom, View.Frame.Width + 20, StyledTextField.StyledFieldHeight ) );

            // use the facebook image's button width, as it looks good.
            nfloat buttonWidth = FBImageView.Bounds.Width;

            LoginButton.Frame = new CGRect( ( ScrollView.Bounds.Width - buttonWidth) / 2, PasswordField.Background.Frame.Bottom + 20, buttonWidth, ControlStyling.ButtonHeight );
            LoginResult.SetFrame( new CGRect( UserNameField.Background.Frame.Left, LoginButton.Frame.Bottom + 20, View.Frame.Width + 20, StyledTextField.StyledFieldHeight ) );



            AdditionalOptions.Frame = new CGRect( (View.Bounds.Width - AdditionalOptions.Bounds.Width) / 2, LoginResult.Background.Frame.Bottom + 10, AdditionalOptions.Bounds.Width, ControlStyling.ButtonHeight );


            // setup the "Register or Facebook"
            RegisterButton.Frame = new CGRect( (View.Bounds.Width - buttonWidth) / 2, AdditionalOptions.Frame.Bottom + 5, buttonWidth, ControlStyling.ButtonHeight );
            OrSpacerLabel.Frame = new CGRect( (View.Bounds.Width - OrSpacerLabel.Bounds.Width) / 2, RegisterButton.Frame.Bottom + 5, OrSpacerLabel.Bounds.Width, FBImageView.Bounds.Height );
            FacebookLogin.Frame = new CGRect( (View.Bounds.Width - FBImageView.Bounds.Width) / 2, OrSpacerLabel.Frame.Bottom + 5, FBImageView.Bounds.Width, FBImageView.Bounds.Height );

            //

            CancelButton.Frame = new CGRect( ( View.Frame.Width - CancelButton.Frame.Width ) / 2, FacebookLogin.Frame.Bottom + 20, CancelButton.Frame.Width, CancelButton.Frame.Height );

            HeaderView.Frame = new CGRect( View.Frame.Left, View.Frame.Top, View.Frame.Width, StyledTextField.StyledFieldHeight );

            // setup the header shadow
            UIBezierPath shadowPath = UIBezierPath.FromRect( HeaderView.Bounds );
            HeaderView.Layer.MasksToBounds = false;
            HeaderView.Layer.ShadowColor = UIColor.Black.CGColor;
            HeaderView.Layer.ShadowOffset = new CoreGraphics.CGSize( 0.0f, .0f );
            HeaderView.Layer.ShadowOpacity = .23f;
            HeaderView.Layer.ShadowPath = shadowPath.CGPath;

            // the logo may not exist if we're on a display with a notch
            if( LogoView != null )
            {
                LogoView.Layer.Position = new CoreGraphics.CGPoint( (HeaderView.Bounds.Width - LogoView.Bounds.Width) / 2, 0 );
            }

            FBImageView.Layer.Position = new CoreGraphics.CGPoint( FacebookLogin.Bounds.Width / 2, FacebookLogin.Bounds.Height / 2 );

            if ( WebLayout != null )
            {
                WebLayout.LayoutChanged( View.Frame );
            }

            BlockerView.SetBounds( View.Frame.ToRectF( ) );

            ScrollView.ContentSize = new CGSize( View.Bounds.Width, Math.Max( View.Bounds.Height * 1.02f, CancelButton.Frame.Bottom + 20 + HeaderView.Bounds.Height ) );
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            LoginResult.Background.Layer.Opacity = 0.00f;

            // clear these only on the appearance of the view. (As opposed to also 
            // when the state becomes LogOut.) This way, if they do something like mess
            // up their password, it won't force them to retype it all in.
            UserNameField.Field.Text = string.Empty;
            PasswordField.Field.Text = string.Empty;

            UserNameField.Background.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color );
            PasswordField.Background.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color );

            ScrollView.ContentOffset = CGPoint.Empty;
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            // restore the buttons
            CancelButton.Hidden = false;
            LoginButton.Hidden = false;
            RegisterButton.Hidden = false;

            // if we're logged in, the UI should be slightly different
            if( RockMobileUser.Instance.LoggedIn )
            {
                // populate them with the user's info
                UserNameField.Field.Text = RockMobileUser.Instance.UserID;
                PasswordField.Field.Text = RockMobileUser.Instance.RockPassword;

                SetUIState( LoginState.In );
            }
            else
            {
                SetUIState( LoginState.Out );
            }
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);

            // if they tap somewhere outside of the text fields, 
            // hide the keyboard
            UserNameField.Field.ResignFirstResponder( );
            PasswordField.Field.ResignFirstResponder( );
        }

        public override bool ShouldAutorotate()
        {
            return Springboard.ShouldAutorotate();
        }

        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations()
        {
            // insist they stay in portait on iPhones
            return Springboard.GetSupportedInterfaceOrientations( );
        }

        public override bool PrefersStatusBarHidden()
        {
            return Springboard.PrefersStatusBarHidden();
        }

        public void TryRockBind()
        {
            if( ValidateInput( ) )
            {
                SetUIState( LoginState.Trying );

                RockMobileUser.Instance.BindRockAccount( UserNameField.Field.Text, PasswordField.Field.Text, BindComplete );

                ProfileAnalytic.Instance.Trigger( ProfileAnalytic.Login, "Rock" );
            }
        }

        bool ValidateInput( )
        {
            bool inputValid = true;

            uint targetColor = ControlStylingConfig.BG_Layer_Color;
            if ( string.IsNullOrEmpty( UserNameField.Field.Text ) == true )
            {
                targetColor = ControlStylingConfig.BadInput_BG_Layer_Color;
                inputValid = false;
            }
            Rock.Mobile.PlatformSpecific.iOS.UI.Util.AnimateViewColor( targetColor, UserNameField.Background );

            targetColor = ControlStylingConfig.BG_Layer_Color;
            if ( string.IsNullOrEmpty( PasswordField.Field.Text ) == true )
            {
                targetColor = ControlStylingConfig.BadInput_BG_Layer_Color;
                inputValid = false;
            }
            Rock.Mobile.PlatformSpecific.iOS.UI.Util.AnimateViewColor( targetColor, PasswordField.Background );

            return inputValid;
        }

        public void TryFacebookBind( )
        {
            SetUIState( LoginState.Trying );

            // have our rock mobile user begin the facebook bind process
            RockMobileUser.Instance.BindFacebookAccount( delegate(string fromUri, Facebook.FacebookClient session) 
            {
                    // it's ready, so create a webView that will take them to the FBLogin page
                    WebLayout = new WebLayout( View.Frame );
                    WebLayout.DeleteCacheAndCookies( );

                    View.AddSubview( WebLayout.ContainerView );

                    // set it totally transparent so we can fade it in
                    WebLayout.ContainerView.BackgroundColor = UIColor.Black;
                    WebLayout.ContainerView.Layer.Opacity = 0.00f;
                    WebLayout.SetCancelButtonColor( ControlStylingConfig.TextField_PlaceholderTextColor );

                    // do a nice fade-in
                    SimpleAnimator_Float floatAnimator = new SimpleAnimator_Float( 0.00f, 1.00f, .25f, 
                        delegate(float percent, object value) 
                        {
                            WebLayout.ContainerView.Layer.Opacity = (float)value;
                        },
                        delegate 
                        {
                            // once faded in, begin loading the page
                            WebLayout.ContainerView.Layer.Opacity = 1.00f;

                            WebLayout.LoadUrl( fromUri, delegate(WebLayout.Result result, string url) 
                                {
                                    BlockerView.Hide( );

                                    // if fail/success comes in
                                    if( result != WebLayout.Result.Cancel )
                                    {
                                        // see if it's a valid facebook response

                                        // if an empty url was returned, it's NOT. Fail.
                                        if( string.IsNullOrEmpty( url ) == true )
                                        {
                                            WebLayout.ContainerView.RemoveFromSuperview( );
                                            BindComplete( false );
                                        }
                                        // otherwise, try to parse the response and move forward
                                        else if ( RockMobileUser.Instance.HasFacebookResponse( url, session ) )
                                        {
                                            // it is, continue the bind process
                                            WebLayout.ContainerView.RemoveFromSuperview( );
                                            RockMobileUser.Instance.FacebookCredentialResult( url, session, BindComplete );

                                            ProfileAnalytic.Instance.Trigger( ProfileAnalytic.Login, "Facebook" );
                                        }
                                    }
                                    else
                                    {
                                        // they pressed cancel, so simply cancel the attempt
                                        WebLayout.ContainerView.RemoveFromSuperview( );
                                        LoginComplete( System.Net.HttpStatusCode.ResetContent, "" );
                                    }
                                } );
                        });

                    floatAnimator.Start( );
            });
        }

        public void BindComplete( bool success )
        {
            if ( success )
            {
                // However we chose to bind, we can now login with the bound account
                RockMobileUser.Instance.Login( LoginComplete );
            }
            else
            {
                LoginComplete( System.Net.HttpStatusCode.BadRequest, "" );
            }
        }

        protected void SetUIState( LoginState state )
        {
            // reset the result label
            LoginResult.Field.Text = "";

            switch( state )
            {
                case LoginState.Out:
                {
                    UserNameField.Field.Enabled = true;
                    PasswordField.Field.Enabled = true;
                    LoginButton.Enabled = true;
                    CancelButton.Enabled = true;
                    RegisterButton.Hidden = false;
                    RegisterButton.Enabled = true;

                    LoginButton.SetTitle( LoginStrings.LoginButton, UIControlState.Normal );

                    break;
                }

                case LoginState.Trying:
                {
                    FadeLoginResult( false );
                    BlockerView.Show( null );
                    BlockerView.BringToFront( );

                    UserNameField.Field.Enabled = false;
                    PasswordField.Field.Enabled = false;
                    LoginButton.Enabled = false;
                    CancelButton.Enabled = false;
                    RegisterButton.Enabled = false;

                    LoginButton.SetTitle( LoginStrings.LoginButton, UIControlState.Normal );

                    break;
                }

                // Deprecated state
                case LoginState.In:
                {
                    UserNameField.Field.Enabled = false;
                    PasswordField.Field.Enabled = false;
                    LoginButton.Enabled = true;
                    CancelButton.Enabled = true;
                    RegisterButton.Hidden = true;
                    RegisterButton.Enabled = false;

                    LoginButton.SetTitle( "Logout", UIControlState.Normal );

                    break;
                }
            }

            State = state;
        }

        public void LoginComplete( System.Net.HttpStatusCode statusCode, string statusDescription )
        {
            switch ( statusCode )
            {
                // if we received No Content, we're logged in
                case System.Net.HttpStatusCode.NoContent:
                {
                    RockMobileUser.Instance.GetPersonData( 
                        delegate(System.Net.HttpStatusCode code, string desc)
                        {
                            Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                                {
                                    UIThread_LoginComplete( code, desc );
                                } );
                        });
                    break;
                }

                case System.Net.HttpStatusCode.Unauthorized:
                {
                    BlockerView.Hide( delegate
                        {
                            // allow them to attempt logging in again
                            SetUIState( LoginState.Out );

                            // wrong user name / password
                            FadeLoginResult( true );
                            LoginResult.Field.Text = LoginStrings.Error_Credentials;
                        } );
                    break;
                }

                case System.Net.HttpStatusCode.ResetContent:
                {
                    // consider this a cancellation
                    BlockerView.Hide( delegate
                        {
                            // allow them to attempt logging in again
                            SetUIState( LoginState.Out );

                            LoginResult.Field.Text = "";
                        } );

                    break;
                }

                default:
                {
                    BlockerView.Hide( delegate
                        {
                            // allow them to attempt logging in again
                            SetUIState( LoginState.Out );

                            // failed to login for some reason
                            FadeLoginResult( true );
                            LoginResult.Field.Text = LoginStrings.Error_Unknown;
                        } );
                    break;
                }
            }
        }

        void UIThread_LoginComplete( System.Net.HttpStatusCode code, string desc ) 
        {
            BlockerView.Hide( delegate
                {
                    switch ( code )
                    {
                        case System.Net.HttpStatusCode.OK:
                        {
                            // see if we should set their viewing campus
                            if( RockMobileUser.Instance.PrimaryFamily.CampusId.HasValue == true )
                            {
                                RockMobileUser.Instance.ViewingCampus = RockMobileUser.Instance.PrimaryFamily.CampusId.Value;
                            }
                            
                            // if they have a profile picture, grab it.
                            RockMobileUser.Instance.TryDownloadProfilePicture( PrivateGeneralConfig.ProfileImageSize, ProfileImageComplete );

                            // update the UI
                            FadeLoginResult( true );
                            LoginResult.Field.Text = string.Format( LoginStrings.Success, RockMobileUser.Instance.PreferredName( ) );

                            // start the timer, which will notify the springboard we're logged in when it ticks.
                            LoginSuccessfulTimer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e ) =>
                                {
                                    // when the timer fires, notify the springboard we're done.
                                    Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                                        {
                                            Springboard.ResignModelViewController( this, null );
                                        } );
                                };

                            LoginSuccessfulTimer.Start( );

                            break;
                        }

                        default:
                        {
                            // if we couldn't get their profile, that should still count as a failed login.
                            SetUIState( LoginState.Out );

                            // failed to login for some reason
                            FadeLoginResult( true );
                            LoginResult.Field.Text = LoginStrings.Error_Unknown;

                            RockMobileUser.Instance.LogoutAndUnbind( );
                            break;
                        }
                    }
                } );
        }

        public void ProfileImageComplete( System.Net.HttpStatusCode code, string desc )
        {
            switch( code )
            {
                case System.Net.HttpStatusCode.OK:
                {
                    // sweet! make the UI update.
                    Rock.Mobile.Threading.Util.PerformOnUIThread( delegate { Springboard.UpdateProfilePic( ); } );
                    break;
                }

                default:
                {
                    // bummer, we couldn't get their profile picture. Doesn't really matter...
                    break;
                }
            }
        }

        void FadeLoginResult( bool fadeIn )
        {
            UIView.Animate( .33f, 0, UIViewAnimationOptions.CurveEaseInOut, 
                new Action( 
                    delegate 
                    { 
                        LoginResult.Background.Layer.Opacity = fadeIn == true ? 1.00f : 0.00f;
                    })

                , new Action(
                    delegate
                    {
                    })
            );
        }
	}
}
