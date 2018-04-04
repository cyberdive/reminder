using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Forms = System.Windows.Forms;
using System.Windows.Media;
using Drawing = System.Drawing;
using System.Windows.Controls;
//using Reminder.Core.Desktop;
//using System.Linq;
//using System.Windows.Controls.Primitives;

namespace Reminder.ControlUI
{
    public class NotifyIcon : FrameworkElement
    {
        private Forms.NotifyIcon _notifyIcon;

        public override void BeginInit()
        {
            base.BeginInit();

            _notifyIcon = new Forms.NotifyIcon
            {
                //Icon = FromImageSource(Icon),
                //Text = Text,
                Visible = Visibility == Visibility.Visible
            };

            _notifyIcon.MouseClick += OnMouseClick;
            _notifyIcon.MouseUp += OnMouseUp;
        }
        
        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            if (oldParent != null)
            {
                AttachToWindowClose();
            }
            
            base.OnVisualParentChanged(oldParent);
        }

        public void AttachToWindowClose()
        {
            var window = Window.GetWindow(this);
            if (window != null)
            {
                window.Closed += (s, a) => _notifyIcon.Dispose();
            }
        }
        private static MouseButtonEventArgs CreateMouseButtonEventArgs(RoutedEvent handler, Forms.MouseButtons button)
        {
            return new MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, ToMouseButton(button))
            {
                RoutedEvent = handler
            };
        }

        private static MouseButton ToMouseButton(Forms.MouseButtons button)
        {
            switch (button)
            {
                case Forms.MouseButtons.Left:
                    return MouseButton.Left;

                case Forms.MouseButtons.Right:
                    return MouseButton.Right;

                case Forms.MouseButtons.Middle:
                    return MouseButton.Middle;

                case Forms.MouseButtons.XButton1:
                    return MouseButton.XButton1;

                case Forms.MouseButtons.XButton2:
                    return MouseButton.XButton2;
            }

            throw new InvalidOperationException();
        }


        #region Events
        internal static readonly RoutedEvent MouseClickEvent = EventManager.RegisterRoutedEvent("MouseClick", RoutingStrategy.Bubble, typeof(MouseButtonEventHandler), typeof(NotifyIcon));

        /// <summary>
        /// Raised when we click on the notify icon in the task bar.
        /// </summary>
        internal event MouseButtonEventHandler MouseClick
        {
            add { AddHandler(MouseClickEvent, value); }
            remove { RemoveHandler(MouseClickEvent, value); }
        }

        private void OnMouseClick(object sender, Forms.MouseEventArgs e)
        {
            RaiseEvent(CreateMouseButtonEventArgs(MouseClickEvent, e.Button));
        }
 
        #endregion


        #region ContextMenu
        private void OnMouseUp(object sender, Forms.MouseEventArgs e)
        {
            if (e.Button == Forms.MouseButtons.Right)
            {
                ShowContextMenu();
            }

            RaiseEvent(CreateMouseButtonEventArgs(MouseUpEvent, e.Button));
        }

        /// <summary>
        /// Show the contextual menu
        /// </summary>
        private void ShowContextMenu()
        {
            if (ContextMenu != null)
            {
                //var mousePosition =new  NativeMethods.Win32Point();
                //NativeMethods.GetCursorPos(ref mousePosition);
                //var activeScreen = Forms.Screen.FromPoint(new Drawing.Point(mousePosition.X, mousePosition.Y));
                //var screen = SystemInfoHelper.GetAllScreenInfos().Single(s => s.DeviceName == activeScreen.DeviceName);

                //ContextMenu.Placement = PlacementMode.Absolute;
                //ContextMenu.HorizontalOffset = (mousePosition.X / (screen.Scale / 100.0)) - 2;
                //ContextMenu.VerticalOffset = (mousePosition.Y / (screen.Scale / 100.0)) - 2;

                //ContextMenu.Opened += OnContextMenuOpened;
                //ContextMenu.Closed += OnContextMenuClosed;
                ContextMenu.IsOpen = true;

                //HwndSource source = (HwndSource)PresentationSource.FromVisual(ContextMenu);
                //if (source != null && source.Handle != IntPtr.Zero)
                //{
                //    //activate the context menu or the message window to track deactivation - otherwise, the context menu
                //    //does not close if the user clicks somewhere else. With the message window
                //    //fallback, the context menu can't receive keyboard events - should not happen though
                //    NativeMethods.SetForegroundWindow(source.Handle);
                //}
            }
        }
        #endregion


        #region Icon
        private static Drawing.Icon FromImageSource(ImageSource icon)
        {
            if (icon == null)
            {
                return null;
            }

            var iconUri = new Uri(icon.ToString());
            var resourceStream = Application.GetResourceStream(iconUri);

            return new Drawing.Icon(resourceStream.Stream);
        }

        /// <summary>
        /// Gets or sets the icon of the <see cref="NotifyIcon"/>
        /// </summary>
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(ImageSource), typeof(NotifyIcon), new FrameworkPropertyMetadata(OnIconChanged));

        /// <summary>
        /// Gets or sets the icon of the <see cref="NotifyIcon"/>
        /// </summary>
        public ImageSource Icon
        {
            get { return (ImageSource)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        private static void OnIconChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(target))
            {
                var control = (NotifyIcon)target;
                control._notifyIcon.Icon = FromImageSource(control.Icon);
            }
        }
        #endregion

        #region Text
        /// <summary>
        /// Gets or sets the text of the <see cref="NotifyIcon"/>
        /// </summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(NotifyIcon), new PropertyMetadata(OnTextChanged));

        /// <summary>
        /// Gets or sets the text of the <see cref="NotifyIcon"/>
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }


        private static void OnTextChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            var control = (NotifyIcon)target;
            control._notifyIcon.Text = control.Text;
        }
        #endregion

    }
}
