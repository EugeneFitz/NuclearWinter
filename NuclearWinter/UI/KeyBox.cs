﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace NuclearWinter.UI
{
    public class KeyBox: Widget
    {
        UIFont mFont;
        public UIFont       Font
        {
            get { return mFont; }
            
            set
            {
                mFont = value;
                UpdateContentSize();
            }
        }

        Keys mKey;
        public Keys                 Key
        {
            get { return mKey; }

            set
            {
                mKey = value;
                UpdateContentSize();
            }
        }

        public string               DisplayedKey
        {
            get {
                return Key.ToString();
            }
        }

        Point                       mpTextPosition;
        int                         miTextWidth;

        public Func<Keys,bool>      ChangeHandler;
        public Action<KeyBox>       FocusHandler;
        public Action<KeyBox>       BlurHandler;

        bool                        mbIsHovered;

        //----------------------------------------------------------------------
        public KeyBox( Screen _screen, Keys _key )
        : base( _screen )
        {
            mKey        = _key;
            mFont       = _screen.Style.MediumFont;
            mPadding    = new Box( 15 );

            UpdateContentSize();
        }

        //----------------------------------------------------------------------
        internal override void UpdateContentSize()
        {
            ContentHeight = (int)( Font.LineSpacing * 0.9f ) + Padding.Top + Padding.Bottom;
            ContentWidth = 0;
            miTextWidth = (int)Font.MeasureString( DisplayedKey ).X;

            base.UpdateContentSize();
        }

        //----------------------------------------------------------------------
        internal override void DoLayout( Rectangle _rect )
        {
            base.DoLayout( _rect );
            HitBox = LayoutRect;

            mpTextPosition = new Point(
                LayoutRect.X + Padding.Left,
                LayoutRect.Center.Y - ContentHeight / 2 + Padding.Top
            );
        }

        //----------------------------------------------------------------------
        internal override void OnMouseEnter( Point _hitPoint )
        {
            base.OnMouseEnter( _hitPoint );
            mbIsHovered = true;
        }

        internal override void OnMouseMove( Point _hitPoint )
        {
        }

        internal override void OnMouseOut( Point _hitPoint )
        {
            base.OnMouseOut( _hitPoint );
            mbIsHovered = false;
        }

        //----------------------------------------------------------------------
        internal override void OnMouseDown( Point _hitPoint, int _iButton )
        {
            if( _iButton != Screen.Game.InputMgr.PrimaryMouseButton ) return;

            Screen.Focus( this );
            OnActivateDown();
        }

        internal override void OnMouseUp( Point _hitPoint, int _iButton )
        {
            if( _iButton != Screen.Game.InputMgr.PrimaryMouseButton ) return;

            if( HitTest( _hitPoint ) == this )
            {
                OnActivateUp();
            }
        }

        //----------------------------------------------------------------------
        internal override void OnFocus()
        {
            if( FocusHandler != null ) FocusHandler( this );
        }

        internal override void OnBlur()
        {
            if( BlurHandler != null ) BlurHandler( this );
        }

        internal override void OnPadMove( Direction _direction )
        {
            // Nothing
        }

        internal override void OnWindowsKeyPress( System.Windows.Forms.Keys _key )
        {
            // Nothing
        }

        //----------------------------------------------------------------------
        internal override void OnKeyPress( Keys _key )
        {
            Keys newKey = ( _key != Keys.Back ) ? _key : Keys.None;

            if( ChangeHandler( newKey ) )
            {
                Key = newKey;
            }
        }

        //----------------------------------------------------------------------
        internal override void Draw()
        {
            Screen.DrawBox( Screen.Style.EditBoxFrame, LayoutRect, Screen.Style.EditBoxCornerSize, Color.White );

            if( Screen.IsActive && mbIsHovered )
            {
                Screen.DrawBox( Screen.Style.ButtonPress, LayoutRect, Screen.Style.EditBoxCornerSize, Color.White );
            }

            Screen.PushScissorRectangle( new Rectangle( LayoutRect.X + Padding.Left, LayoutRect.Y, LayoutRect.Width - Padding.Horizontal, LayoutRect.Height ) );

            Screen.Game.DrawBlurredText( Screen.Style.BlurRadius, mFont, DisplayedKey, new Vector2( mpTextPosition.X , mpTextPosition.Y + mFont.YOffset ), Color.White );

            Screen.PopScissorRectangle();

            if( HasFocus )
            {
                Rectangle selectionRectangle = new Rectangle( mpTextPosition.X, LayoutRect.Y + Padding.Top, miTextWidth, LayoutRect.Height - Padding.Vertical );
                Screen.Game.SpriteBatch.Draw( Screen.Game.WhitePixelTex, selectionRectangle, Color.White * 0.3f );
            }
        }
    }
}