﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NuclearWinter.Collections;

namespace NuclearWinter.UI
{
    //--------------------------------------------------------------------------
    public class TreeViewNode: Widget
    {
        //----------------------------------------------------------------------
        public Texture2D            Icon
        {
            get { return mImage.Texture; }
            set {
                mImage.Texture = value;
                mLabel.Padding = mImage.Texture != null ? new Box( 10, 10, 10, 0 ) : new Box( 10 );
            }
        }

        string mstrText = "";
        public string               Text
        {
            get { return mstrText; }
            set {
                mstrText = value;
                UpdateLabel();
            }
        }

        void UpdateLabel()
        {
            mLabel.Text = Collapsed ? string.Format( "{0} ({1})", mstrText, ContainedNodeCount ) : mstrText;
        }

        public object               Tag;

        public ObservableList<TreeViewNode>     Children            { get; private set; }
        public int                              ContainedNodeCount  { get; private set; }

        public bool                 DisplayAsContainer;
        public bool                 Collapsed {
            get { return mbCollapsed; }
            set {
                mbCollapsed = value;
                UpdateLabel();
                UpdateContentSize();
            }
        }

        //----------------------------------------------------------------------
        TreeView                    mTreeView;

        Label                       mLabel;
        Image                       mImage;

        bool                        mbCollapsed;
        bool                        mbIsLast;

        //----------------------------------------------------------------------
        public TreeViewNode( TreeView _treeView, string _strText, Texture2D _icon, object _tag )
        : base( _treeView.Screen )
        {
            mTreeView   = _treeView;
            Children    = new ObservableList<TreeViewNode>();

            Children.ListChanged += delegate( object _source, ObservableList<TreeViewNode>.ListChangedEventArgs _args )
            {
                _args.Item.Parent = _args.Added ? this : null;

                if( _args.Added )
                {
                    OnNodeAdded();
                }
                else
                {
                    OnNodeRemoved();
                }

                UpdateLabel();
                UpdateContentSize();
            };

            Children.ListCleared += delegate( object _source, EventArgs _args )
            {
                ContainedNodeCount = 0;
                UpdateLabel();
                UpdateContentSize();
            };

            mstrText = _strText;
            mLabel      = new Label( Screen, _strText, Anchor.Start, Screen.Style.DefaultTextColor );
            mImage      = new Image( Screen );
            mImage.Padding = new Box( 0, 5, 0, 10 );

            Icon = _icon;

            UpdateContentSize();
        }

        public TreeViewNode( TreeView _treeView, string _strText, Texture2D _icon )
        : this( _treeView, _strText, _icon, null )
        {

        }

        public TreeViewNode( TreeView _treeView, string _strText, object _tag )
        : this( _treeView, _strText, null, _tag )
        {

        }

        public TreeViewNode( TreeView _treeView, string _strText )
        : this( _treeView, _strText, null, null )
        {
        }

        //----------------------------------------------------------------------
        internal override void UpdateContentSize()
        {
            ContentHeight = mTreeView.NodeHeight + mTreeView.NodeSpacing;
            if( Children.Count > 0 && ! mbCollapsed )
            {
                foreach( TreeViewNode child in Children )
                {
                    ContentHeight += child.ContentHeight;
                }
            }

            if( Parent is TreeViewNode )
            {
                ((TreeViewNode)Parent).ChildSizeChanged();
            }

            base.UpdateContentSize();
        }

        //----------------------------------------------------------------------
        void ChildSizeChanged()
        {
            UpdateContentSize();
        }

        void OnNodeAdded()
        {
            ContainedNodeCount++;

            if( Parent is TreeViewNode )
            {
                ((TreeViewNode)Parent).OnNodeAdded();
            }
        }

        void OnNodeRemoved()
        {
            ContainedNodeCount--;

            if( Parent is TreeViewNode )
            {
                ((TreeViewNode)Parent).OnNodeRemoved();
            }
        }


        //----------------------------------------------------------------------
        internal override void DoLayout( Rectangle _rect )
        {
            Position = _rect.Location;
            Size = new Point( _rect.Width, _rect.Height );
            HitBox = _rect;
            
            int iLabelX = ( Children.Count > 0 || DisplayAsContainer ) ? mTreeView.NodeBranchWidth : 0;

            if( mImage.Texture != null )
            {
                mImage.DoLayout( new Rectangle( Position.X + iLabelX, Position.Y, mImage.ContentWidth, mTreeView.NodeHeight ) );
                iLabelX += mImage.ContentWidth;
            }

            mLabel.DoLayout( new Rectangle( Position.X + iLabelX, Position.Y, Size.X - iLabelX, mTreeView.NodeHeight ) );

            int iX = Position.X;
            int iY = Position.Y + mTreeView.NodeHeight + mTreeView.NodeSpacing;
            foreach( TreeViewNode child in Children )
            {
                child.DoLayout( new Rectangle( iX + mTreeView.NodeBranchWidth, iY, Size.X - mTreeView.NodeBranchWidth, child.ContentHeight ) );
                iY += child.ContentHeight;
            }

            if( Parent != null )
            {
                TreeViewNode parent = ((TreeViewNode)Parent);
                mbIsLast = parent.Children[ parent.Children.Count - 1 ] == this;
            }
        }

        //----------------------------------------------------------------------
        internal override void Draw()
        {
            if( Parent != null )
            {
                Screen.Game.SpriteBatch.Draw( mbIsLast ? Screen.Style.TreeViewBranchLast : Screen.Style.TreeViewBranch, new Vector2( Position.X - mTreeView.NodeBranchWidth, Position.Y ), Color.White );
            }

            Rectangle nodeRect = new Rectangle( Position.X, Position.Y, Size.X, mTreeView.NodeHeight );

            if( Children.Count == 0 && ! DisplayAsContainer )
            {
                Screen.DrawBox( mTreeView.SelectedNode == this ? Screen.Style.GridBoxFrameSelected : Screen.Style.GridBoxFrame, nodeRect, Screen.Style.GridBoxFrameCornerSize, Color.White );
            }
            else
            {
                Texture2D tex = Children.Count == 0 ? Screen.Style.TreeViewBranchOpenEmpty : Screen.Style.TreeViewBranchOpen;

                if( Collapsed )
                {
                    tex = Screen.Style.TreeViewBranchClosed;
                }

                Screen.Game.SpriteBatch.Draw( tex, new Vector2( Position.X, Position.Y ), Color.White );
            }

            if( mImage.Texture != null )
            {
                mImage.Draw();
            }

            mLabel.Draw();

            if( mTreeView.HasFocus && mTreeView.FocusedNode == this )
            {
                if( mTreeView.SelectedNode != this )
                {
                    Screen.DrawBox( Screen.Style.GridBoxFrameFocus, nodeRect, Screen.Style.GridBoxFrameCornerSize, Color.White );
                }
                else
                if( Screen.Style.GridBoxFrameSelectedFocus != null )
                {
                    Screen.DrawBox( Screen.Style.GridBoxFrameSelectedFocus, nodeRect, Screen.Style.GridBoxFrameCornerSize, Color.White );
                }
            }

            if( mTreeView.HoveredNode == this )
            {
                if( mTreeView.SelectedNode != this )
                {
                    Screen.DrawBox( Screen.Style.GridBoxFrameHover, nodeRect, Screen.Style.GridBoxFrameCornerSize, Color.White );
                }
                else
                if( Screen.Style.GridBoxFrameSelectedHover != null )
                {
                    Screen.DrawBox( Screen.Style.GridBoxFrameSelectedHover, nodeRect, Screen.Style.GridBoxFrameCornerSize, Color.White );
                }
            }

            if( ! mbCollapsed )
            {
                foreach( TreeViewNode child in Children )
                {
                    child.Draw();
                }
            }
        }
    }

    //--------------------------------------------------------------------------
    public class TreeView: Widget
    {
        //----------------------------------------------------------------------
        public List<TreeViewNode>           Nodes               { get; private set; }

        public int                          NodeHeight      = 40;
        public int                          NodeSpacing     = 0;
        public int                          NodeBranchWidth = 25;

        //----------------------------------------------------------------------
        public Action<TreeView>             ValidateHandler;
        public TreeViewNode                 SelectedNode    = null;

        //----------------------------------------------------------------------
        internal TreeViewNode               HoveredNode     = null;
        internal TreeViewNode               FocusedNode     = null;

        //----------------------------------------------------------------------
        bool                                mbIsHovered;
        Point                               mHoverPoint;

        int                                 miScrollOffset;
        int                                 miScrollMax;

        //----------------------------------------------------------------------
        public TreeView( Screen _screen )
        : base( _screen )
        {
            Nodes = new List<TreeViewNode>();
        }

        //----------------------------------------------------------------------
        internal override void UpdateContentSize()
        {
            ContentWidth    = Padding.Left + Padding.Right;
            ContentHeight   = Padding.Top + Padding.Bottom;

            base.UpdateContentSize();
        }

        //----------------------------------------------------------------------
        internal override void DoLayout( Rectangle _rect )
        {
            Position = _rect.Location;
            Size = new Point( _rect.Width, _rect.Height );
            HitBox = _rect;

            int iX = Position.X + 10;
            int iY = Position.Y + 10;
            int iHeight = 0;
            foreach( TreeViewNode node in Nodes )
            {
                node.DoLayout( new Rectangle( iX, iY + iHeight - miScrollOffset, Size.X - 20, node.ContentHeight ) );
                iHeight += node.ContentHeight;
            }

            miScrollMax = Math.Max( 0, ( iHeight ) - ( Size.Y - 20 ) );
        }

        //----------------------------------------------------------------------
        internal override void OnMouseEnter( Point _hitPoint )
        {
            mbIsHovered = true;
            mHoverPoint = _hitPoint;
            UpdateHoveredNode();
        }

        internal override void OnMouseMove(Point _hitPoint)
        {
            mHoverPoint = _hitPoint;
            UpdateHoveredNode();
        }

        internal override void OnMouseOut( Point _hitPoint )
        {
            mbIsHovered = false;
            UpdateHoveredNode();
        }

        void UpdateHoveredNode()
        {
            HoveredNode = null;
            if( mbIsHovered )
            {
                int iNodeIndex = ( mHoverPoint.Y - ( Position.Y + 10 ) + miScrollOffset ) / ( NodeHeight + NodeSpacing );

                HoveredNode = FindHoveredNode( Nodes, iNodeIndex, 0 );
            }
        }

        TreeViewNode FindHoveredNode( IList<TreeViewNode> _children, int _iNodeIndex, int _iNodeOffset )
        {
            foreach( TreeViewNode node in _children )
            {
                if( _iNodeOffset == _iNodeIndex )
                {
                    return node;
                }
                else
                if( node.Collapsed )
                {
                    _iNodeOffset += 1;
                }
                else
                if( _iNodeOffset + node.ContainedNodeCount >= _iNodeIndex )
                {
                    return FindHoveredNode( node.Children, _iNodeIndex, _iNodeOffset + 1 );
                }
                else
                {
                    _iNodeOffset += 1 + node.ContainedNodeCount;
                }
            }

            return null;
        }

        //----------------------------------------------------------------------
        internal override void OnMouseDown( Point _hitPoint )
        {
            Screen.Focus( this );
            FocusedNode = HoveredNode;
        }

        internal override void OnMouseUp( Point _hitPoint )
        {
            if( HoveredNode != null && FocusedNode == HoveredNode )
            {
                if( HoveredNode.DisplayAsContainer )
                {
                    SelectedNode = null;
                    HoveredNode.Collapsed = ! HoveredNode.Collapsed;
                }
                else
                {
                    SelectedNode = HoveredNode;
                }
            }
        }

        //----------------------------------------------------------------------
        internal override void OnMouseDoubleClick( Point _hitPoint )
        {
            if( SelectedNode != null && ValidateHandler != null ) ValidateHandler( this );
        }

        //----------------------------------------------------------------------
        internal override void OnMouseWheel( Point _hitPoint, int _iDelta )
        {
            int iNewScrollOffset = (int)MathHelper.Clamp( miScrollOffset - ( _iDelta / 120 * 3 * ( NodeHeight + NodeSpacing ) ), 0, Math.Max( 0, miScrollMax ) );
            miScrollOffset = iNewScrollOffset;
            UpdateHoveredNode();
        }

        //----------------------------------------------------------------------
        internal override void Draw()
        {
            Screen.DrawBox( Screen.Style.GridFrame, new Rectangle( Position.X, Position.Y, Size.X, Size.Y ), 30, Color.White );

            Screen.PushScissorRectangle( new Rectangle( Position.X + 10, Position.Y + 10, Size.X - 20, Size.Y - 20 ) );
            foreach( TreeViewNode node in Nodes )
            {
                node.Draw();
            }
            Screen.PopScissorRectangle();
        }

    }
}
