using System;
using System.Collections.Generic;

namespace AstrhoApp.API.DTOs
{
    public class DashboardOverviewDto
    {
        public RealTimeStatsDto RealTimeStats { get; set; } = new RealTimeStatsDto();
        public SummaryStatsDto SummaryStats { get; set; } = new SummaryStatsDto();
        public List<ChartDataDto> RevenueChart { get; set; } = new List<ChartDataDto>();
        public List<ChartDataDto> AppointmentsChart { get; set; } = new List<ChartDataDto>();
        public List<ClientTypeChartDto> ClientsChart { get; set; } = new List<ClientTypeChartDto>();
        public List<ChartDataDto> ProductsChart { get; set; } = new List<ChartDataDto>();
        public List<UpcomingAppointmentDto> UpcomingAppointments { get; set; } = new List<UpcomingAppointmentDto>();
        public List<TopServiceDto> TopServices { get; set; } = new List<TopServiceDto>();
    }

    public class RealTimeStatsDto
    {
        public int ActiveClients { get; set; }
        public int TodayAppointments { get; set; }
        public int PendingOrders { get; set; }
        public int LowStockItems { get; set; }
    }

    public class SummaryStatsDto
    {
        public decimal Revenue { get; set; }
        public int Appointments { get; set; }
        public int Clients { get; set; }
        public int ProductsSold { get; set; }
        public int ServicesCompleted { get; set; }
        public int NewClients { get; set; }
    }

    public class ChartDataDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal Value { get; set; }
    }

    public class ClientTypeChartDto
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
        public string Color { get; set; } = string.Empty;
    }

    public class UpcomingAppointmentDto
    {
        public int Id { get; set; }
        public string Client { get; set; } = string.Empty;
        public string Service { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Employee { get; set; } = string.Empty;
    }

    public class TopServiceDto
    {
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Revenue { get; set; }
        public int Percentage { get; set; }
    }
}
