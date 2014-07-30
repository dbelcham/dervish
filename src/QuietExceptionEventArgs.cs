using System;

namespace dervish
{
    public class QuietExceptionEventArgs:EventArgs
    {
        public Exception RaisedException{ get; set; }

        public QuietExceptionEventArgs(Exception raisedException)
        {
            RaisedException = raisedException;
        }
    }
}