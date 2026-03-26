using System;
using System.Threading.Tasks;

namespace Extenity.ParallelToolbox
{
    public static class AsyncTools
    {
        public static async void FireAndForget(this Task task)
        {
            try
            {
                await task;
            }
            catch (Exception exception)
            {
                // This try-catch block in async void method is essential
                // - to prevent crashes due to exceptions thrown in async void method,
                // - and to display error logs that would otherwise be swallowed.
                // This is a template code that is enforced to all async void methods.
                try
                {
                    // Do the logging, and do nothing else in here.
                    // If a custom exception handling is needed, add another try-catch above.
                    Log.With("AsyncTools").Error(exception);
                }
                catch
                {
                    // Do absolutely nothing in this catch block. Reaching here means
                    // the application is in pretty bad shape. But it increases the chances
                    // of error catching systems to report the error to cloud.
                }
            }
        }

        public static async void FireAndForgetAndContinue(this Task task, Action continueWith)
        {
            try
            {
                await task;
                continueWith();
            }
            catch (Exception exception)
            {
                // This try-catch block in async void method is essential
                // - to prevent crashes due to exceptions thrown in async void method,
                // - and to display error logs that would otherwise be swallowed.
                // This is a template code that is enforced to all async void methods.
                try
                {
                    // Do the logging, and do nothing else in here.
                    // If a custom exception handling is needed, add another try-catch above.
                    Log.With("AsyncTools").Error(exception);
                }
                catch
                {
                    // Do absolutely nothing in this catch block. Reaching here means
                    // the application is in pretty bad shape. But it increases the chances
                    // of error catching systems to report the error to cloud.
                }
            }
        }
    }
}