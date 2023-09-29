using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botik
{
    internal enum orderStatuses
    {
        notAccepted = 0,
        Accepted = 1,
        Preparing = 2,
        Ready = 3,
        enRoute = 4,
        Delivered = 5
    }

}
