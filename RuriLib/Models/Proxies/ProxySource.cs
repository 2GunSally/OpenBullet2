﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Models.Proxies;

public abstract class ProxySource : IDisposable
{
    protected readonly Random random = new();
    public ProxyType DefaultType { get; set; } = ProxyType.Http;
    public string DefaultUsername { get; set; } = string.Empty;
    public string DefaultPassword { get; set; } = string.Empty;
    public int UserId { get; set; } = 0;

    public virtual void Dispose()
    {
    }

    public virtual Task<IEnumerable<Proxy>> GetAllAsync(CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}