using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSC_Monitor
{

    class outputResponse
    {
        public string Function;
        public JObject Args = new JObject();
        public JObject Response = new JObject();

        public outputResponse(string inFunc, JObject inArg, JObject inResponse)
        {
            this.Function = inFunc;
            this.Args = inArg;
            this.Response = inResponse;

        }
    }
}
