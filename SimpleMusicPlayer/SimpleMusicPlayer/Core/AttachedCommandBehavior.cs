using System;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace SimpleMusicPlayer.Core
{
    /// <summary>
    /// thx to Sacha Barber  http://sachabarber.net
    /// </summary>
    public static class AttachedCommandBehavior
    {
        #region TheCommandToRun

        /// <summary>
        /// TheCommandToRun : The actual ICommand to run
        /// </summary>
        public static readonly DependencyProperty TheCommandToRunProperty =
          DependencyProperty.RegisterAttached("TheCommandToRun",
                                              typeof(ICommand),
                                              typeof(AttachedCommandBehavior),
                                              new FrameworkPropertyMetadata((ICommand)null));

        /// <summary>
        /// Gets the TheCommandToRun property.  
        /// </summary>
        public static ICommand GetTheCommandToRun(DependencyObject d)
        {
            return (ICommand)d.GetValue(TheCommandToRunProperty);
        }

        /// <summary>
        /// Sets the TheCommandToRun property.  
        /// </summary>
        public static void SetTheCommandToRun(DependencyObject d, ICommand value)
        {
            d.SetValue(TheCommandToRunProperty, value);
        }

        #endregion

        #region RoutedEventName

        /// <summary>
        /// RoutedEventName : The event that should actually execute the
        /// ICommand
        /// </summary>
        public static readonly DependencyProperty RoutedEventNameProperty =
          DependencyProperty.RegisterAttached("RoutedEventName",
                                              typeof(String),
                                              typeof(AttachedCommandBehavior),
                                              new FrameworkPropertyMetadata((String)String.Empty, new PropertyChangedCallback(OnRoutedEventNameChanged)));

        /// <summary>
        /// Gets the RoutedEventName property.  
        /// </summary>
        public static String GetRoutedEventName(DependencyObject d)
        {
            return (String)d.GetValue(RoutedEventNameProperty);
        }

        /// <summary>
        /// Sets the RoutedEventName property.  
        /// </summary>
        public static void SetRoutedEventName(DependencyObject d, String value)
        {
            d.SetValue(RoutedEventNameProperty, value);
        }

        /// <summary>
        /// Hooks up a Dynamically created EventHandler (by using the 
        /// <see cref="EventHooker">EventHooker</see> class) that when
        /// run will run the associated ICommand
        /// </summary>
        private static void OnRoutedEventNameChanged(DependencyObject d,
                                                     DependencyPropertyChangedEventArgs e)
        {
            String routedEvent = (String)e.NewValue;

            //If the RoutedEvent string is not null, create a new
            //dynamically created EventHandler that when run will execute
            //the actual bound ICommand instance (usually in the ViewModel)
            if (!String.IsNullOrEmpty(routedEvent))
            {
                EventHooker eventHooker = new EventHooker();
                eventHooker.ObjectWithAttachedCommand = d;

                EventInfo eventInfo = d.GetType().GetEvent(routedEvent, BindingFlags.Public | BindingFlags.Instance);

                //Hook up Dynamically created event handler
                if (eventInfo != null)
                {
                    eventInfo.AddEventHandler(d, eventHooker.GetNewEventHandlerToRunCommand(eventInfo));
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// Contains the event that is hooked into the source RoutedEvent
    /// that was specified to run the ICommand
    /// </summary>
    internal sealed class EventHooker
    {
        #region Public Methods/Properties

        /// <summary>
        /// The DependencyObject, that holds a binding to the actual
        /// ICommand to execute
        /// </summary>
        public DependencyObject ObjectWithAttachedCommand { get; set; }

        /// <summary>
        /// Creates a Dynamic EventHandler that will be run the ICommand
        /// when the user specified RoutedEvent fires
        /// </summary>
        /// <param name="eventInfo">The specified RoutedEvent EventInfo</param>
        /// <returns>An Delegate that points to a new EventHandler
        /// that will be run the ICommand</returns>
        public Delegate GetNewEventHandlerToRunCommand(EventInfo eventInfo)
        {
            Delegate del = null;

            if (eventInfo == null)
            {
                throw new ArgumentNullException("eventInfo");
            }

            if (eventInfo.EventHandlerType == null)
            {
                throw new ArgumentException("EventHandlerType is null");
            }

            if (del == null)
            {
                del = Delegate.CreateDelegate(eventInfo.EventHandlerType, this,
                                              this.GetType().GetMethod("OnEventRaised", BindingFlags.NonPublic | BindingFlags.Instance));
            }

            return del;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Runs the ICommand when the requested RoutedEvent fires
        /// </summary>
        private void OnEventRaised(object sender, EventArgs e)
        {
            ICommand command = (ICommand)(sender as DependencyObject).GetValue(AttachedCommandBehavior.TheCommandToRunProperty);

            if (command != null)
            {
                command.Execute(null);
            }
        }

        #endregion
    }
}