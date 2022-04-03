using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrandPrixRadioRemote
{
    public class GetRequestData
    {
        public ContentTypes contentType;
        public string data;

        public GetRequestData(ContentTypes contentType, string data)
        {
            this.contentType = contentType;
            this.data = data;
        }
    }
}
