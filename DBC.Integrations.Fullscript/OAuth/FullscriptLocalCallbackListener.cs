using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DBC.Integrations.Fullscript.OAuth
{
    public class FullscriptLocalCallbackResult
    {
        public string Code { get; set; }

        public string State { get; set; }

        public string Error { get; set; }

        public string ErrorDescription { get; set; }

        public bool TimedOut { get; set; }

        public bool HasCode()
        {
            return !string.IsNullOrWhiteSpace(Code);
        }

        public bool HasError()
        {
            return !string.IsNullOrWhiteSpace(Error);
        }
    }

    public class FullscriptLocalCallbackListener
    {
        private readonly string _prefix;

        public FullscriptLocalCallbackListener()
            : this("http://localhost:5000/fullscript/oauth/callback/")
        {
        }

        public FullscriptLocalCallbackListener(string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {
                throw new ArgumentException("Listener prefix is required.", "prefix");
            }

            if (!prefix.EndsWith("/"))
            {
                prefix += "/";
            }

            _prefix = prefix;
        }

        public async Task<FullscriptLocalCallbackResult> WaitForCallbackAsync()
        {
            using (var listener = new HttpListener())
            {
                listener.Prefixes.Add(_prefix);
                listener.Start();

                var context = await listener.GetContextAsync().ConfigureAwait(false);
                var result = BuildResultFromContext(context);

                WriteBrowserResponse(context.Response, result);

                listener.Stop();

                return result;
            }
        }

        public async Task<FullscriptLocalCallbackResult> WaitForCallbackAsync(TimeSpan timeout)
        {
            using (var listener = new HttpListener())
            {
                listener.Prefixes.Add(_prefix);
                listener.Start();

                var callbackTask = listener.GetContextAsync();
                var timeoutTask = Task.Delay(timeout);

                var completedTask = await Task.WhenAny(callbackTask, timeoutTask).ConfigureAwait(false);

                if (completedTask == timeoutTask)
                {
                    listener.Stop();

                    return new FullscriptLocalCallbackResult
                    {
                        TimedOut = true,
                        Error = "timeout",
                        ErrorDescription = "No Fullscript callback was received before the timeout expired."
                    };
                }

                var context = await callbackTask.ConfigureAwait(false);
                var result = BuildResultFromContext(context);

                WriteBrowserResponse(context.Response, result);

                listener.Stop();

                return result;
            }
        }

        private static FullscriptLocalCallbackResult BuildResultFromContext(HttpListenerContext context)
        {
            var request = context.Request;

            Console.WriteLine("Callback received:");
            Console.WriteLine(request.Url.ToString());

            return new FullscriptLocalCallbackResult
            {
                Code = request.QueryString["code"],
                State = request.QueryString["state"],
                Error = request.QueryString["error"],
                ErrorDescription = request.QueryString["error_description"]
            };
        }

        private static void WriteBrowserResponse(HttpListenerResponse response, FullscriptLocalCallbackResult result)
        {
            string message;

            if (result.HasCode())
            {
                message = "Fullscript authorization received. You may return to the DBC test application.";
            }
            else if (result.HasError())
            {
                message = "Fullscript authorization returned an error: " + WebUtility.HtmlEncode(result.Error);
            }
            else
            {
                message = "Fullscript callback received, but no authorization code was found.";
            }

            var html = "<html><head><title>Fullscript Authorization</title></head><body>" +
                       "<h2>" + message + "</h2>" +
                       "</body></html>";

            var buffer = Encoding.UTF8.GetBytes(html);

            response.ContentType = "text/html";
            response.ContentEncoding = Encoding.UTF8;
            response.ContentLength64 = buffer.Length;

            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.OutputStream.Close();
        }
    }
}