﻿using Coffeeland.Messaging.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Coffeeland.Messaging.Queries.Queries
{
    public class GetPersonalDataQuery : IQuery
    {
        public string sessionToken;
    }
}