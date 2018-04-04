using System;
using System.Runtime.InteropServices;

namespace Reminder.Toast
{
    /// <summary>
    /// Inherited class of notification activator (for Action Center of Windows 10)
    /// </summary>
    /// <remarks>The CLSID of this class must be unique for each application.</remarks>
    [Guid("F8B398FA-6831-4104-A5D5-3A850A5DA37E"), ComVisible(true), ClassInterface(ClassInterfaceType.None)]
	[ComSourceInterfaces(typeof(INotificationActivationCallback))]
	public class NotificationActivator : NotificationActivatorBase
	{ }
}