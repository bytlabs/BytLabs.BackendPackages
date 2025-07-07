using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BytLabs.FivePaisa
{
    internal interface IWebsocket
    {

        bool IsConnected();
        void ConnectForFeed(string jwttoken, string clientcode);
        void FetchMarketFeed(WebsocketConnect list);
        void Send(string Message);
        void Close(bool Abort = false);



    }
}
