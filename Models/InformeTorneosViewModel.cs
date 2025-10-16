using System;
using System.Collections.Generic;

namespace ProyectoMDGSharksWeb.Models
{
    /// <summary>
    /// ViewModel para el Informe de Torneos y Participación de Clientes
    /// </summary>
    public class InformeTorneosViewModel
    {
        // Filtros de rango de fechas
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }

        // KPIs principales
        public int TotalClientes { get; set; }
        public int ClientesConCompras { get; set; }
        public int ClientesEnTorneos { get; set; }
        public int ClientesGanadores { get; set; }

        public string ClienteTopParticipaciones { get; set; } = "N/A";
        public string ClienteTopGanador { get; set; } = "N/A";
        public string DiaMasPopular { get; set; } = "N/A";

        public decimal IngresoPorEntradas { get; set; }

        // Datos para gráficos
        public List<TorneoPorDiaDTO> ParticipacionesPorDia { get; set; } = new List<TorneoPorDiaDTO>();
        public List<TorneoIngresoMensualDTO> IngresosMensuales { get; set; } = new List<TorneoIngresoMensualDTO>();
        public List<TopClienteDTO> TopParticipantes { get; set; } = new List<TopClienteDTO>();
        public List<TopClienteDTO> TopGanadores { get; set; } = new List<TopClienteDTO>();
    }

    /// <summary>
    /// DTO para participaciones agrupadas por día de la semana
    /// </summary>
    public class TorneoPorDiaDTO
    {
        public string Dia { get; set; } = string.Empty;
        public int Participaciones { get; set; }
    }

    /// <summary>
    /// DTO para ingresos agrupados por mes
    /// </summary>
    public class TorneoIngresoMensualDTO
    {
        public string Mes { get; set; } = string.Empty;
        public decimal TotalIngreso { get; set; }
    }

    /// <summary>
    /// DTO para top de clientes (participaciones o victorias)
    /// </summary>
    public class TopClienteDTO
    {
        public string NombreCompleto { get; set; } = string.Empty;
        public int Total { get; set; }
    }
}
