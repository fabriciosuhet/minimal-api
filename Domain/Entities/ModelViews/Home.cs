using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace minimal_api.Domain.Entities.ModelViews
{
    public struct Home
    {
        public readonly string? Message { get => "Bem vindo a API  de veículos - Minimal APi"; }

        public readonly string? Documentation => "/swagger";
    }
}