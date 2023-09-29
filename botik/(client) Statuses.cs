using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botik
{
    internal enum clientStatus
    {
       notStarted = 0,
       Started = 1,
       enteringPhoneNumber = 2,
       enteringMenu = 3,
       enteringDestination = 4,
       enteringOrderToVhenge = 5,
       statusPreparing = 6,
       statusReady = 7,
       statusEnRoute = 8,
       statusDelivered = 9
    }
}
