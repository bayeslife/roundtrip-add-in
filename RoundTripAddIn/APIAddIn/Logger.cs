using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoundTripAddIn
{
    public class Logger
    {
        
        bool toggle = false;//false

        EA.Repository repository = null;

        public void setRepository(EA.Repository r){
            this.repository = r;
        }

        public void toggleLogging(EA.Repository r)
        {
            this.toggle = !this.toggle;
            this.repository = r;
            if (this.toggle)
            {
                enable(r);                            
            }
                
        }

        public void enable(EA.Repository r)
        {
            this.toggle = true;
            this.repository = r;
            if (this.toggle)
            {
                repository.CreateOutputTab(RoundTripAddInClass.ADDIN_NAME);
                repository.EnsureOutputVisible(RoundTripAddInClass.ADDIN_NAME);
                repository.ClearOutput(RoundTripAddInClass.ADDIN_NAME);
                log("Logger is enabled");
                string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                log(RoundTripAddInClass.ADDIN_NAME+ " version " + version);
            }
        }

        public void log(string msg)
        {
            if (toggle)
                repository.WriteOutput(RoundTripAddInClass.ADDIN_NAME, msg, 0);
        }
    }

   
}
