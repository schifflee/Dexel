﻿using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace Dexel.Editor.Views.DragAndDrop
{
    public class FrameworkElementDragBehavior : Behavior<FrameworkElement>
    {
        private bool _isMouseClicked;
        public static bool DragDropInProgressFlag = false;

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseLeftButtonDown += AssociatedObject_MouseLeftButtonDown;
            AssociatedObject.MouseLeftButtonUp += AssociatedObject_MouseLeftButtonUp;
            AssociatedObject.MouseLeave += AssociatedObject_MouseLeave;
        }

        void AssociatedObject_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragDropInProgressFlag = true;
            _isMouseClicked = true;
        }

        void AssociatedObject_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {           
            DragDropInProgressFlag = false;
            _isMouseClicked = false;
        }

        void AssociatedObject_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_isMouseClicked)
            {
               
                //set the item's DataContext as the data to be transferred
                var datacontext = AssociatedObject.DataContext;
                var dragObject = datacontext as IDragable;
                if (dragObject != null)
                {
                    DataObject data = new DataObject();
                    data.SetData(dragObject.DataType, AssociatedObject.DataContext);

                    DragDrop.DoDragDrop(AssociatedObject, data, DragDropEffects.Move);
                    DragDropInProgressFlag = false;
                }
            }
            _isMouseClicked = false;
        }
    }
}
