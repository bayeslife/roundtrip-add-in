using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoundTripAddIn
{
    public class Profiler
    {

        Logger logger = null;

        public void setLogger(Logger logger)
        {
            this.logger = logger;
        }

        System.Diagnostics.Stopwatch watch = null;
        String context = null;
        public void start(String context)
        {        
            this.watch = System.Diagnostics.Stopwatch.StartNew();
            this.context = context;
        }

        public void stop()
        {
            this.watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            logger.log(this.context + elapsedMs);
        }
    }
}
