using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
public class inputCommand
{
    public string Function;
    public JObject Args = new JObject();
}
