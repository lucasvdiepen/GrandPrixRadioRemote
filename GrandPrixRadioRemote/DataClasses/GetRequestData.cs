using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrandPrixRadioRemote.DataClasses
{
    public class GetRequestData
    {
        public string contentType;
        public string data;

        public GetRequestData(string contentType, string data)
        {
            this.contentType = contentType;
            this.data = data;
        }
    }
}
