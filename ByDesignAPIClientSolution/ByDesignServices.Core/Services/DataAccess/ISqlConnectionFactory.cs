﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace ByDesignServices.Core.Services.DataAccess
{
    public interface ISqlConnectionFactory
    {
        IDbConnection GetOpenConnection();
    }
}
