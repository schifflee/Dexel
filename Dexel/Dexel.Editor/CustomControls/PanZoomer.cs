﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Dexel.Editor.ViewModels;

namespace Dexel.Editor.CustomControls
{
    public class PanZoomer : ScrollViewer
    {
        private const MouseButton PanButton = MouseButton.Middle;
        private UIElement child = null;
        private Point origin;
        private Point start;

        public Point BeforeContextMenuPoint;

        private TranslateTransform GetTranslateTransform(UIElement element)
        {
            return (TranslateTransform)((TransformGroup)element.RenderTransform)
              .Children.First(tr => tr is TranslateTransform);
        }

        private ScaleTransform GetScaleTransform(UIElement element)
        {
            return (ScaleTransform)((TransformGroup)element.RenderTransform)
              .Children.First(tr => tr is ScaleTransform);
        }


        protected override void OnInitialized(EventArgs e)
        {
            Initialize((UIElement)Content);
        }






        //public override UIElement Child
        //{
        //    get { return base.Child; }
        //    set
        //    {
        //        if (value != null && value != this.Child)
        //            this.Initialize(value);
        //        base.Child = value;
        //    }
        //}

        public void Initialize(UIElement element)
        {
            this.child = element;
            if (this.child != null)
            {
                TransformGroup group = new TransformGroup();
                ScaleTransform st = new ScaleTransform();
                group.Children.Add(st);
                TranslateTransform tt = new TranslateTransform();
                group.Children.Add(tt);
                child.RenderTransform = group;
                child.RenderTransformOrigin = new Point(0.0, 0.0);
                this.PreviewMouseWheel += child_MouseWheel;
                this.PreviewMouseDown += child_MouseLeftButtonDown;
                this.PreviewMouseUp += child_MouseLeftButtonUp;
                this.PreviewMouseMove += child_MouseMove;
                //this.PreviewMouseRightButtonDown += new MouseButtonEventHandler(
                //  child_PreviewMouseRightButtonDown);
                this.PreviewMouseRightButtonDown += new MouseButtonEventHandler(
                  savePositionBeforeContextMenu);

            }
        }


        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            e.Handled = true;
        }




        private void savePositionBeforeContextMenu(object sender, MouseButtonEventArgs e)
        {
            BeforeContextMenuPoint = e.GetPosition(this);
        }

        public void Reset()
        {
            ScrollToHorizontalOffset(0);
            ScrollToVerticalOffset(0);

            if (child != null)
            {
                // reset zoom
                var st = GetScaleTransform(child);
                st.ScaleX = 1;
                st.ScaleY = 1;

                // reset pan
                var tt = GetTranslateTransform(child);
                tt.X = 0.0;
                tt.Y = 0.0;  
            }
        }

        #region Child Events

        private void child_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (child == null) return;
            e.Handled = true;

            var st = GetScaleTransform(child);
            var tt = GetTranslateTransform(child);

            double zoom = e.Delta > 0 ? .2 : -.2;

            
            if (!(e.Delta > 0) && (st.ScaleX < .3 || st.ScaleY < .3))
                return;

            Point relative = e.GetPosition(child);

            var abosuluteX = relative.X * st.ScaleX + tt.X;
            var abosuluteY = relative.Y * st.ScaleY + tt.Y;

            st.ScaleX += zoom;
            st.ScaleY += zoom;

            tt.X = abosuluteX - relative.X * st.ScaleX;
            tt.Y = abosuluteY - relative.Y * st.ScaleY;
        }

        private void child_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != PanButton)
            {
                child.ReleaseMouseCapture();
                this.Cursor = Cursors.Arrow;
                return;

            }
            if (child != null)
            {
                var tt = GetTranslateTransform(child);
                start = e.GetPosition(this);
                origin = new Point(tt.X, tt.Y);
                this.Cursor = Cursors.Hand;
                child.CaptureMouse();
            }
        }

        private void child_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

            if (e.ChangedButton != PanButton)
            {
                return;
            }
            if (child != null)
            {
                child.ReleaseMouseCapture();
                this.Cursor = Cursors.Arrow;
            }
        }

        void child_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Reset();
        }

        private void child_MouseMove(object sender, MouseEventArgs e)
        {
            if (child != null)
            {
                if (child.IsMouseCaptured)
                {
                    var tt = GetTranslateTransform(child);
                    Vector v = start - e.GetPosition(this);
                    tt.X = origin.X - v.X;
                    tt.Y = origin.Y - v.Y;
                }
            }
        }

        #endregion
    }
}
